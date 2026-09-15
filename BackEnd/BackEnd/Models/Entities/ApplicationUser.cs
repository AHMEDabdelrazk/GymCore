using GymCore.API.Models.Entities;
using GymCore.API.Models.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace GymCore.API.Models;

public static class UserRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string BranchManager = "BranchManager";
    public const string Trainer = "Trainer";
    public const string FrontDeskStaff = "FrontDeskStaff";
    public const string Member = "Member";
}

public class ApplicationUser : IdentityUser, ITenantEntity
{
    public int TenantId { get; set; } = 1;

    public string FullName { get; set; } = string.Empty;

    public string Role { get; set; } = UserRoles.Member;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Tenant? Tenant { get; set; }
}