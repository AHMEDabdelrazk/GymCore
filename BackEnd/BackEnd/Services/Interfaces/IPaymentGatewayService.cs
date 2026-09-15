using System.Threading.Tasks;
using GymCore.API.DTOs.Billing;

namespace GymCore.API.Services.Interfaces;

public class WebhookProcessingResult
{
    public bool Success { get; set; }
    public bool IsDuplicate { get; set; }
    public string Message { get; set; } = string.Empty;
}

public interface IPaymentGatewayService
{
    bool VerifyStripeSignature(string payload, string signatureHeader, string webhookSecret);
    Task<WebhookProcessingResult> ProcessWebhookEventAsync(string eventId, string eventType, StripeObjectDto data, string rawPayload);
    Task<InvoiceDto> CreateInvoiceAsync(int memberId, decimal amount, string currency = "USD");
}
