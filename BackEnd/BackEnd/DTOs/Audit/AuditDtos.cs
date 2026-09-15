using System;

namespace GymCore.API.DTOs.Audit;

public class AuditLogDto
{
    public long Id { get; set; }
    public int? TenantId { get; set; }
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string? IpAddress { get; set; }
}
