using System;
using System.Collections.Generic;
using GymCore.API.Models.Entities;
using GymCore.API.Models.Interfaces;

namespace GymCore.API.Models;

public class Member : ITenantEntity
{
    public int Id { get; set; }

    public int TenantId { get; set; } = 1;

    public string MemberCode { get; set; } = string.Empty; // e.g. MEM-1001 for QR / barcode scanner

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string Gender { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public DateTime JoinDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Direct plan association (preserved for backward compatibility with existing views)
    public int? MembershipPlanId { get; set; }
    public MembershipPlan? MembershipPlan { get; set; }

    public Tenant? Tenant { get; set; }

    // Subscriptions, Bookings, and Check-in History
    public ICollection<MemberSubscription> Subscriptions { get; set; } = new List<MemberSubscription>();
    public ICollection<ClassBooking> ClassBookings { get; set; } = new List<ClassBooking>();
    public ICollection<CheckInRecord> CheckInRecords { get; set; } = new List<CheckInRecord>();
}