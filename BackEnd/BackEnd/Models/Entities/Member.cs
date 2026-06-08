using GymCore.API.Models.Entities;

namespace GymCore.API.Models;

public class Member
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string Gender { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public DateTime JoinDate { get; set; }

    public bool IsActive { get; set; } = true;

    public int MembershipPlanId { get; set; }

    public MembershipPlan MembershipPlan { get; set; } = null!;
}