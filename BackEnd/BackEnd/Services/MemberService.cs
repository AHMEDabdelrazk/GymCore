using GymCore.API.Data;
using GymCore.API.DTOs.Member;
using GymCore.API.Models;
using GymCore.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymCore.API.Services;

public class MemberService : IMemberService
{
    private readonly GymDbContext _context;

    public MemberService(GymDbContext context)
    {
        _context = context;
    }

    public async Task<List<MemberDto>> GetAllAsync()
    {
        return await _context.Members
            .Include(m => m.MembershipPlan)
            .Select(m => new MemberDto
            {
                Id = m.Id,
                FullName = m.FullName,
                Email = m.Email,
                PhoneNumber = m.PhoneNumber,
                DateOfBirth = m.DateOfBirth,
                Gender = m.Gender,
                Address = m.Address,
                MembershipPlanId = m.MembershipPlanId,
                MembershipPlanName = m.MembershipPlan.Name,
                IsActive = m.IsActive
            })
            .ToListAsync();
    }

    public async Task<MemberDto?> GetByIdAsync(int id)
    {
        return await _context.Members
            .Include(m => m.MembershipPlan)
            .Where(m => m.Id == id)
            .Select(m => new MemberDto
            {
                Id = m.Id,
                FullName = m.FullName,
                Email = m.Email,
                PhoneNumber = m.PhoneNumber,
                DateOfBirth = m.DateOfBirth,
                Gender = m.Gender,
                Address = m.Address,
                MembershipPlanId = m.MembershipPlanId,
                MembershipPlanName = m.MembershipPlan.Name,
                IsActive = m.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int> CreateAsync(CreateMemberDto dto)
    {
        var member = new Member
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            Address = dto.Address,
            MembershipPlanId = dto.MembershipPlanId,
            JoinDate = DateTime.UtcNow,
            IsActive = true
        };

        _context.Members.Add(member);

        await _context.SaveChangesAsync();

        return member.Id;
    }

    public async Task UpdateAsync(
    int id,
    UpdateMemberDto dto)
    {
        var member = await _context.Members
            .FirstOrDefaultAsync(x => x.Id == id);

        if (member == null)
            throw new Exception("Member not found");

        member.FullName = dto.FullName;
        member.Email = dto.Email;
        member.PhoneNumber = dto.PhoneNumber;
        member.DateOfBirth = dto.DateOfBirth;
        member.Gender = dto.Gender;
        member.Address = dto.Address;
        member.MembershipPlanId = dto.MembershipPlanId;

        await _context.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        var member = await _context.Members
            .FirstOrDefaultAsync(x => x.Id == id);

        if (member == null)
            throw new Exception("Member not found");

        member.IsActive = false;

        await _context.SaveChangesAsync();
    }
}