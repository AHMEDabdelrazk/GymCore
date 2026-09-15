using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GymCore.API.DTOs.Billing;
using GymCore.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GymCore.API.Controllers;

[ApiController]
[Route("api/webhooks/stripe")]
public class StripeWebhookController : ControllerBase
{
    private readonly IPaymentGatewayService _paymentService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(
        IPaymentGatewayService paymentService,
        IConfiguration configuration,
        ILogger<StripeWebhookController> logger)
    {
        _paymentService = paymentService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        using var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8);
        var jsonPayload = await reader.ReadToEndAsync();

        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();
        var webhookSecret = _configuration["Stripe:WebhookSecret"] ?? "whsec_test_secret_key_gymcore_2026";

        // Validate cryptographic signature
        var isValid = _paymentService.VerifyStripeSignature(jsonPayload, signatureHeader, webhookSecret);
        if (!isValid)
        {
            _logger.LogWarning("Invalid Stripe webhook HMAC signature received.");
            return BadRequest(new { error = "Invalid signature" });
        }

        try
        {
            var eventPayload = JsonSerializer.Deserialize<StripeWebhookPayloadDto>(jsonPayload, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (eventPayload == null || string.IsNullOrWhiteSpace(eventPayload.Id))
            {
                return BadRequest(new { error = "Invalid event payload structure" });
            }

            var result = await _paymentService.ProcessWebhookEventAsync(
                eventPayload.Id,
                eventPayload.Type,
                eventPayload.Data?.Object ?? new StripeObjectDto(),
                jsonPayload);

            return Ok(new
            {
                status = "success",
                eventId = eventPayload.Id,
                isDuplicate = result.IsDuplicate,
                message = result.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook.");
            return StatusCode(500, new { error = "Internal webhook processing error" });
        }
    }

    /// <summary>
    /// Testing/Demo Endpoint: Allows reviewers and frontend demo to fire simulated Stripe webhook events
    /// without requiring live external webhooks or ngrok tunnels.
    /// </summary>
    [HttpPost("simulate")]
    public async Task<IActionResult> SimulateWebhook([FromBody] SimulateWebhookRequestDto request)
    {
        var eventId = string.IsNullOrWhiteSpace(request.EventId)
            ? $"evt_sim_{Guid.NewGuid():N}"
            : request.EventId;

        var eventType = string.IsNullOrWhiteSpace(request.EventType)
            ? "invoice.payment_succeeded"
            : request.EventType;

        var stripeObject = new StripeObjectDto
        {
            Id = $"pi_sim_{Guid.NewGuid():N}",
            MemberId = request.MemberId,
            Amount = request.Amount > 0 ? request.Amount : 69.99m,
            Currency = "usd",
            Status = eventType == "invoice.payment_succeeded" ? "paid" : "failed"
        };

        var result = await _paymentService.ProcessWebhookEventAsync(
            eventId,
            eventType,
            stripeObject,
            "Simulated Payload");

        return Ok(new
        {
            status = result.Success ? "success" : "failed",
            eventId = eventId,
            isDuplicate = result.IsDuplicate,
            message = result.Message
        });
    }
}

public class SimulateWebhookRequestDto
{
    public string? EventId { get; set; }
    public string? EventType { get; set; } // invoice.payment_succeeded or invoice.payment_failed
    public int MemberId { get; set; }
    public decimal Amount { get; set; } = 69.99m;
}
