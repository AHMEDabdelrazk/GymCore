namespace GymCore.API.DTOs.Plan;

public class PlanDto
{
    public int Id { get; set; }

    public int TenantId { get; set; } = 1;

    public string Name { get; set; } = string.Empty;

    public string Tier { get; set; } = "Standard";

    public decimal Price { get; set; }

    public int DurationInDays { get; set; }

    public int MaxClassesPerWeek { get; set; } = 3;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}