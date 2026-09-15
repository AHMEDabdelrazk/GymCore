using System;

namespace GymCore.API.DTOs.CheckIn;

public class CheckInRequestDto
{
    public string MemberCodeOrId { get; set; } = string.Empty;
    public string AccessMethod { get; set; } = "Kiosk_QR"; // Kiosk_QR, RFID_Badge, Manual_Desk
}

public class CheckInResultDto
{
    public bool AccessGranted { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? MemberName { get; set; }
    public string? SubscriptionStatus { get; set; }
    public string? PlanName { get; set; }
    public DateTime TimestampUtc { get; set; }
}

public class CheckInHistoryDto
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public DateTime CheckInTimeUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? DenialReason { get; set; }
    public string AccessMethod { get; set; } = string.Empty;
}
