using System.ComponentModel.DataAnnotations;

namespace GymCore.API.DTOs.Plan;

public class CreatePlanDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 100000)]
    public decimal Price { get; set; }

    [Range(1, 3650)]
    public int DurationInDays { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}