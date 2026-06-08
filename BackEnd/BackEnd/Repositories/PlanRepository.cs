using GymCore.API.Data;
using GymCore.API.Models.Entities;
using GymCore.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymCore.API.Repositories;

public class PlanRepository : IPlanRepository
{
    private readonly GymDbContext _context;

    public PlanRepository(GymDbContext context)
    {
        _context = context;
    }

    public async Task<List<MembershipPlan>> GetAllAsync()
    {
        return await _context.MembershipPlans.ToListAsync();
    }

    public async Task<MembershipPlan?> GetByIdAsync(int id)
    {
        return await _context.MembershipPlans
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(MembershipPlan plan)
    {
        await _context.MembershipPlans.AddAsync(plan);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}