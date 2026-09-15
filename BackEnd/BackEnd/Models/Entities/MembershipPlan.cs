using System;
using System.Collections.Generic;
using GymCore.API.Models.Interfaces;

namespace GymCore.API.Models.Entities;

public class MembershipPlan : ITenantEntity
{
    public int Id { get; set; }

    public int TenantId { get; set; } = 1;

    public string Name { get; set; } = string.Empty;

    public string Tier { get; set; } = "Standard"; // Basic, Pro, Elite

    public decimal Price { get; set; }

    public int DurationInDays { get; set; }

    public int MaxClassesPerWeek { get; set; } = 3;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Tenant? Tenant { get; set; }

    public ICollection<Member> Members { get; set; } = new List<Member>();
}