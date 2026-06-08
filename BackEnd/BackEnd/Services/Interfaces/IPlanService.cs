using GymCore.API.DTOs.Plan;

namespace GymCore.API.Services.Interfaces;

public interface IPlanService
{
    Task<IEnumerable<PlanDto>> GetAllAsync();

    Task<PlanDto?> GetByIdAsync(int id);

    Task<int> CreateAsync(CreatePlanDto dto);

    Task UpdateAsync(int id, UpdatePlanDto dto);

    Task DeactivateAsync(int id);
}