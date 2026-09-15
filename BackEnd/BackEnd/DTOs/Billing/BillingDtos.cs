using System;

namespace GymCore.API.DTOs.Billing;

public class InvoiceDto
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty;
    public DateTime DueDateUtc { get; set; }
    public DateTime? PaidAtUtc { get; set; }
    public string? StripePaymentIntentId { get; set; }
}

public class StripeWebhookPayloadDto
{
    public string Id { get; set; } = string.Empty; // evt_xxx
    public string Type { get; set; } = string.Empty; // invoice.payment_succeeded, etc.
    public StripeDataDto Data { get; set; } = new();
}

public class StripeDataDto
{
    public StripeObjectDto Object { get; set; } = new();
}

public class StripeObjectDto
{
    public string Id { get; set; } = string.Empty; // in_xxx or pi_xxx
    public int MemberId { get; set; }
    public int? SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public string Status { get; set; } = string.Empty;
}
