using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GymCore.API.Models.Interfaces;

namespace GymCore.API.Models.Entities;

public enum BookingStatus
{
    Confirmed = 1,
    Waitlisted = 2,
    Attended = 3,
    Canceled = 4
}

public class ClassType : ITenantEntity
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = "HIIT"; // HIIT, Yoga, Strength, Spin, Pilates

    public int DefaultDurationMinutes { get; set; } = 60;

    public int DefaultCapacity { get; set; } = 20;

    public bool IsActive { get; set; } = true;
}

public class ClassSession : ITenantEntity
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    public int ClassTypeId { get; set; }
    public ClassType ClassType { get; set; } = null!;

    public string TrainerId { get; set; } = string.Empty;
    public ApplicationUser? Trainer { get; set; }

    public string RoomName { get; set; } = "Studio A";

    public DateTime StartTimeUtc { get; set; }

    public DateTime EndTimeUtc { get; set; }

    public int Capacity { get; set; } = 20;

    public int ReservedSpots { get; set; } = 0;

    public bool IsCanceled { get; set; } = false;

    // Concurrency control token
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public ICollection<ClassBooking> Bookings { get; set; } = new List<ClassBooking>();
}

public class ClassBooking : ITenantEntity
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    public int ClassSessionId { get; set; }
    public ClassSession ClassSession { get; set; } = null!;

    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    public int? WaitlistPosition { get; set; }

    public DateTime BookedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CanceledAtUtc { get; set; }
}
