using System;
using System.Collections.Generic;

namespace GymCore.API.DTOs.Class;

public class ClassTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int DefaultDurationMinutes { get; set; }
    public int DefaultCapacity { get; set; }
}

public class ClassSessionDto
{
    public int Id { get; set; }
    public int ClassTypeId { get; set; }
    public string ClassTypeName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string TrainerId { get; set; } = string.Empty;
    public string TrainerName { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public int Capacity { get; set; }
    public int ReservedSpots { get; set; }
    public int AvailableSpots => Math.Max(0, Capacity - ReservedSpots);
    public int WaitlistCount { get; set; }
    public bool IsCanceled { get; set; }
}

public class BookClassRequestDto
{
    public int MemberId { get; set; }
}

public class BookingResultDto
{
    public int BookingId { get; set; }
    public string Status { get; set; } = string.Empty; // Confirmed or Waitlisted
    public int? WaitlistPosition { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ClassRosterItemDto
{
    public int BookingId { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string MemberEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? WaitlistPosition { get; set; }
    public DateTime BookedAtUtc { get; set; }
}
