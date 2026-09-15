using System;
using System.Collections.Generic;

namespace GymCore.API.Models.Entities;

/// <summary>
/// Represents a gym branch or enterprise tenant in the multi-tenant architecture.
/// </summary>
public class Tenant
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string TimeZone { get; set; } = "UTC";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation collections
    public ICollection<Member> Members { get; set; } = new List<Member>();
    public ICollection<MembershipPlan> Plans { get; set; } = new List<MembershipPlan>();
    public ICollection<ClassSession> ClassSessions { get; set; } = new List<ClassSession>();
}
