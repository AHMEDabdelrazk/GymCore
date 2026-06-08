using GymCore.API.Data;
using GymCore.API.DTOs.Plan;
using GymCore.API.Models.Entities;
using GymCore.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymCore.API.Services;

public class PlanService : IPlanService
{
    private readonly GymDbContext _context;

    public PlanService(GymDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PlanDto>> GetAllAsync()
    {
        return await _context.MembershipPlans
            .Select(p => new PlanDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                DurationInDays = p.DurationInDays,
                Description = p.Description,
                IsActive = p.IsActive
            })
            .ToListAsync();
    }

    public async Task<PlanDto?> GetByIdAsync(int id)
    {
        return await _context.MembershipPlans
            .Where(x => x.Id == id)
            .Select(p => new PlanDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                DurationInDays = p.DurationInDays,
                Description = p.Description,
                IsActive = p.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int> CreateAsync(CreatePlanDto dto)
    {
        var plan = new MembershipPlan
        {
            Name = dto.Name,
            Price = dto.Price,
            DurationInDays = dto.DurationInDays,
            Description = dto.Description
        };

        _context.MembershipPlans.Add(plan);

        await _context.SaveChangesAsync();

        return plan.Id;
    }

    public async Task UpdateAsync(int id, UpdatePlanDto dto)
    {
        var plan = await _context.MembershipPlans.FindAsync(id);

        if (plan is null)
            throw new Exception("Plan not found");

        plan.Name = dto.Name;
        plan.Price = dto.Price;
        plan.DurationInDays = dto.DurationInDays;
        plan.Description = dto.Description;

        await _context.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        var plan = await _context.MembershipPlans.FindAsync(id);

        if (plan is null)
            throw new Exception("Plan not found");

        plan.IsActive = false;

        await _context.SaveChangesAsync();
    }
}