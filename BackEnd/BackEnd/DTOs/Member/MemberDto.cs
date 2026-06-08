namespace GymCore.API.DTOs.Member;

public class MemberDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = "";

    public string Email { get; set; } = "";

    public string PhoneNumber { get; set; } = "";

    public DateTime DateOfBirth { get; set; }

    public string Gender { get; set; } = "";

    public string Address { get; set; } = "";

    public int MembershipPlanId { get; set; }

    public string MembershipPlanName { get; set; } = "";

    public bool IsActive { get; set; }
}