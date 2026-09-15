using System;
using GymCore.API.Models.Interfaces;

namespace GymCore.API.Models.Entities;

public enum SubscriptionStatus
{
    Active = 1,
    GracePeriod = 2,
    Paused = 3,
    Expired = 4,
    Canceled = 5
}

/// <summary>
/// Encapsulates the subscription lifecycle for a member, including renewal and grace period rules.
/// </summary>
public class MemberSubscription : ITenantEntity
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public int MembershipPlanId { get; set; }
    public MembershipPlan MembershipPlan { get; set; } = null!;

    public DateTime StartDateUtc { get; set; }

    public DateTime EndDateUtc { get; set; }

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    public bool AutoRenew { get; set; } = true;

    public DateTime? NextBillingDateUtc { get; set; }

    public DateTime? LastPaymentDateUtc { get; set; }

    public decimal PricePaid { get; set; }

    public string? ExternalSubscriptionId { get; set; } // e.g. Stripe sub_xxx

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
