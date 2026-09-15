using GymCore.API.Models.Entities;

namespace GymCore.API.Repositories.Interfaces;

public interface IPlanRepository
{
    Task<List<MembershipPlan>> GetAllAsync();

    Task<MembershipPlan?> GetByIdAsync(int id);

    Task AddAsync(MembershipPlan plan);

    Task SaveChangesAsync();
}