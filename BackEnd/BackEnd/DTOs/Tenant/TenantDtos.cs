using System;

namespace GymCore.API.DTOs.Tenant;

public class TenantDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string TimeZone { get; set; } = "UTC";
    public bool IsActive { get; set; }
}
