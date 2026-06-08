namespace GymCore.API.DTOs.Plan;

public class PlanDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int DurationInDays { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}