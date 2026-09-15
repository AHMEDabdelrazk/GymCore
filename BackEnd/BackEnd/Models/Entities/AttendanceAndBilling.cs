using System;
using GymCore.API.Models.Interfaces;

namespace GymCore.API.Models.Entities;

public enum CheckInStatus
{
    Success = 1,
    Denied = 2
}

public class CheckInRecord : ITenantEntity
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public DateTime CheckInTimeUtc { get; set; } = DateTime.UtcNow;

    public CheckInStatus Status { get; set; } = CheckInStatus.Success;

    public string? DenialReason { get; set; }

    public string AccessMethod { get; set; } = "Kiosk_QR"; // Kiosk_QR, RFID_Badge, Manual_Desk
}

public enum InvoiceStatus
{
    Pending = 1,
    Paid = 2,
    Failed = 3,
    Refunded = 4
}

public class Invoice : ITenantEntity
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public int? SubscriptionId { get; set; }
    public MemberSubscription? Subscription { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "USD";

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

    public DateTime DueDateUtc { get; set; }

    public DateTime? PaidAtUtc { get; set; }

    public string? StripePaymentIntentId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Enforces idempotency for external webhook callbacks (e.g. Stripe) to prevent duplicate processing.
/// </summary>
public class ProcessedWebhookEvent
{
    public int Id { get; set; }

    public string EventId { get; set; } = string.Empty; // e.g. evt_3NxY29...

    public string EventType { get; set; } = string.Empty; // e.g. invoice.payment_succeeded

    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = "Processed"; // Processed, Failed, Ignored

    public string? PayloadSummary { get; set; }
}

/// <summary>
/// Immutable audit log capturing entity modifications and administrative actions with state diffs.
/// </summary>
public class AuditLog
{
    public long Id { get; set; }

    public int? TenantId { get; set; }

    public string? UserId { get; set; }

    public string? UserEmail { get; set; }

    public string Action { get; set; } = string.Empty; // Create, Update, Delete, CheckIn, WebhookProcessed

    public string EntityName { get; set; } = string.Empty;

    public string EntityId { get; set; } = string.Empty;

    public string? OldValuesJson { get; set; }

    public string? NewValuesJson { get; set; }

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public string? IpAddress { get; set; }
}
