using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GymCore.API.Data;
using GymCore.API.DTOs.Billing;
using GymCore.API.Models.Entities;
using GymCore.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GymCore.API.Services;

public class StripePaymentGatewayService : IPaymentGatewayService
{
    private readonly GymDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentGatewayService> _logger;

    public StripePaymentGatewayService(
        GymDbContext context,
        IConfiguration configuration,
        ILogger<StripePaymentGatewayService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public bool VerifyStripeSignature(string payload, string signatureHeader, string webhookSecret)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader) || string.IsNullOrWhiteSpace(webhookSecret))
            return false;

        // Stripe signature header format: t=timestamp,v1=hash
        var items = signatureHeader.Split(',');
        string? timestamp = null;
        string? signature = null;

        foreach (var item in items)
        {
            var parts = item.Trim().Split('=');
            if (parts.Length == 2)
            {
                if (parts[0] == "t") timestamp = parts[1];
                if (parts[0] == "v1") signature = parts[1];
            }
        }

        if (timestamp == null || signature == null)
            return false;

        var signedPayload = $"{timestamp}.{payload}";
        var secretBytes = Encoding.UTF8.GetBytes(webhookSecret);

        using var hmac = new HMACSHA256(secretBytes);
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
        var computedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedSignature),
            Encoding.UTF8.GetBytes(signature.ToLowerInvariant()));
    }

    public async Task<WebhookProcessingResult> ProcessWebhookEventAsync(
        string eventId,
        string eventType,
        StripeObjectDto data,
        string rawPayload)
    {
        // 1. Idempotency Check: Prevent duplicate webhook processing
        var existingEvent = await _context.ProcessedWebhookEvents
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        if (existingEvent != null)
        {
            _logger.LogInformation("Webhook event {EventId} has already been processed (Idempotent replay ignored).", eventId);
            return new WebhookProcessingResult
            {
                Success = true,
                IsDuplicate = true,
                Message = $"Event {eventId} was already processed at {existingEvent.ReceivedAtUtc:u}"
            };
        }

        // 2. Process domain events
        switch (eventType)
        {
            case "invoice.payment_succeeded":
                await HandlePaymentSucceededAsync(data);
                break;

            case "invoice.payment_failed":
                await HandlePaymentFailedAsync(data);
                break;

            case "customer.subscription.deleted":
                await HandleSubscriptionCanceledAsync(data);
                break;

            default:
                _logger.LogInformation("Unhandled Stripe webhook event type: {EventType}", eventType);
                break;
        }

        // 3. Record processed event to enforce idempotency
        var processedRecord = new ProcessedWebhookEvent
        {
            EventId = eventId,
            EventType = eventType,
            ReceivedAtUtc = DateTime.UtcNow,
            Status = "Processed",
            PayloadSummary = $"MemberId: {data.MemberId}, Amount: {data.Amount} {data.Currency}"
        };

        _context.ProcessedWebhookEvents.Add(processedRecord);
        await _context.SaveChangesAsync();

        return new WebhookProcessingResult
        {
            Success = true,
            IsDuplicate = false,
            Message = $"Event {eventId} of type {eventType} processed successfully."
        };
    }

    private async Task HandlePaymentSucceededAsync(StripeObjectDto data)
    {
        var member = await _context.Members
            .Include(m => m.Subscriptions)
            .FirstOrDefaultAsync(m => m.Id == data.MemberId);

        if (member == null) return;

        // Find or create invoice
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.MemberId == member.Id && i.Status == InvoiceStatus.Pending);

        if (invoice == null)
        {
            invoice = new Invoice
            {
                MemberId = member.Id,
                TenantId = member.TenantId,
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                Amount = data.Amount > 0 ? data.Amount : 49.99m,
                Currency = string.IsNullOrWhiteSpace(data.Currency) ? "USD" : data.Currency.ToUpper(),
                Status = InvoiceStatus.Paid,
                DueDateUtc = DateTime.UtcNow,
                PaidAtUtc = DateTime.UtcNow,
                StripePaymentIntentId = data.Id
            };
            _context.Invoices.Add(invoice);
        }
        else
        {
            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAtUtc = DateTime.UtcNow;
            invoice.StripePaymentIntentId = data.Id;
        }

        // Extend or activate subscription
        var activeSub = member.Subscriptions
            .OrderByDescending(s => s.EndDateUtc)
            .FirstOrDefault();

        if (activeSub != null)
        {
            activeSub.Status = SubscriptionStatus.Active;
            activeSub.StartDateUtc = DateTime.UtcNow;
            activeSub.EndDateUtc = DateTime.UtcNow.AddDays(30);
            activeSub.LastPaymentDateUtc = DateTime.UtcNow;
            activeSub.NextBillingDateUtc = DateTime.UtcNow.AddDays(30);
        }
        else
        {
            var newSub = new MemberSubscription
            {
                TenantId = member.TenantId,
                MemberId = member.Id,
                MembershipPlanId = (member.MembershipPlanId.HasValue && member.MembershipPlanId.Value > 0) ? member.MembershipPlanId.Value : 1,
                StartDateUtc = DateTime.UtcNow,
                EndDateUtc = DateTime.UtcNow.AddDays(30),
                Status = SubscriptionStatus.Active,
                AutoRenew = true,
                LastPaymentDateUtc = DateTime.UtcNow,
                NextBillingDateUtc = DateTime.UtcNow.AddDays(30),
                PricePaid = invoice.Amount
            };
            _context.MemberSubscriptions.Add(newSub);
        }
    }

    private async Task HandlePaymentFailedAsync(StripeObjectDto data)
    {
        var member = await _context.Members
            .Include(m => m.Subscriptions)
            .FirstOrDefaultAsync(m => m.Id == data.MemberId);

        if (member == null) return;

        var sub = member.Subscriptions
            .OrderByDescending(s => s.EndDateUtc)
            .FirstOrDefault();

        if (sub != null)
        {
            sub.Status = SubscriptionStatus.GracePeriod;
        }

        var invoice = new Invoice
        {
            MemberId = member.Id,
            TenantId = member.TenantId,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}",
            Amount = data.Amount > 0 ? data.Amount : 49.99m,
            Currency = "USD",
            Status = InvoiceStatus.Failed,
            DueDateUtc = DateTime.UtcNow,
            StripePaymentIntentId = data.Id
        };
        _context.Invoices.Add(invoice);
    }

    private async Task HandleSubscriptionCanceledAsync(StripeObjectDto data)
    {
        var member = await _context.Members
            .Include(m => m.Subscriptions)
            .FirstOrDefaultAsync(m => m.Id == data.MemberId);

        if (member == null) return;

        var sub = member.Subscriptions
            .OrderByDescending(s => s.EndDateUtc)
            .FirstOrDefault();

        if (sub != null)
        {
            sub.Status = SubscriptionStatus.Canceled;
            sub.AutoRenew = false;
        }
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(int memberId, decimal amount, string currency = "USD")
    {
        var member = await _context.Members.FindAsync(memberId);
        if (member == null) throw new ArgumentException("Member not found");

        var invoice = new Invoice
        {
            MemberId = member.Id,
            TenantId = member.TenantId,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}",
            Amount = amount,
            Currency = currency,
            Status = InvoiceStatus.Pending,
            DueDateUtc = DateTime.UtcNow.AddDays(7)
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return new InvoiceDto
        {
            Id = invoice.Id,
            MemberId = invoice.MemberId,
            MemberName = member.FullName,
            InvoiceNumber = invoice.InvoiceNumber,
            Amount = invoice.Amount,
            Currency = invoice.Currency,
            Status = invoice.Status.ToString(),
            DueDateUtc = invoice.DueDateUtc
        };
    }
}
