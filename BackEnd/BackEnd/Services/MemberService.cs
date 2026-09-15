using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymCore.API.Data;
using GymCore.API.DTOs.Member;
using GymCore.API.Models;
using GymCore.API.Models.Entities;
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
        var members = await _context.Members
            .Include(m => m.MembershipPlan)
            .Include(m => m.Subscriptions)
            .ToListAsync();

        return members.Select(m =>
        {
            var latestSub = m.Subscriptions.OrderByDescending(s => s.EndDateUtc).FirstOrDefault();
            return new MemberDto
            {
                Id = m.Id,
                TenantId = m.TenantId,
                MemberCode = string.IsNullOrWhiteSpace(m.MemberCode) ? $"MEM-{m.Id + 1000}" : m.MemberCode,
                FullName = m.FullName,
                Email = m.Email,
                PhoneNumber = m.PhoneNumber,
                DateOfBirth = m.DateOfBirth,
                Gender = m.Gender,
                Address = m.Address,
                MembershipPlanId = m.MembershipPlanId ?? 0,
                MembershipPlanName = m.MembershipPlan?.Name ?? "Standard",
                SubscriptionStatus = latestSub?.Status.ToString() ?? "Active",
                IsActive = m.IsActive
            };
        }).ToList();
    }

    public async Task<MemberDto?> GetByIdAsync(int id)
    {
        var m = await _context.Members
            .Include(m => m.MembershipPlan)
            .Include(m => m.Subscriptions)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (m == null) return null;

        var latestSub = m.Subscriptions.OrderByDescending(s => s.EndDateUtc).FirstOrDefault();
        return new MemberDto
        {
            Id = m.Id,
            TenantId = m.TenantId,
            MemberCode = string.IsNullOrWhiteSpace(m.MemberCode) ? $"MEM-{m.Id + 1000}" : m.MemberCode,
            FullName = m.FullName,
            Email = m.Email,
            PhoneNumber = m.PhoneNumber,
            DateOfBirth = m.DateOfBirth,
            Gender = m.Gender,
            Address = m.Address,
            MembershipPlanId = m.MembershipPlanId ?? 0,
            MembershipPlanName = m.MembershipPlan?.Name ?? "Standard",
            SubscriptionStatus = latestSub?.Status.ToString() ?? "Active",
            IsActive = m.IsActive
        };
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
            MemberCode = $"MEM-{new Random().Next(1000, 9999)}",
            JoinDate = DateTime.UtcNow,
            IsActive = true
        };

        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        // Automatically provision an active initial subscription for the chosen plan
        var plan = await _context.MembershipPlans.FindAsync(dto.MembershipPlanId);
        var duration = plan?.DurationInDays ?? 30;
        var price = plan?.Price ?? 49.99m;

        var subscription = new MemberSubscription
        {
            TenantId = member.TenantId,
            MemberId = member.Id,
            MembershipPlanId = dto.MembershipPlanId,
            StartDateUtc = DateTime.UtcNow,
            EndDateUtc = DateTime.UtcNow.AddDays(duration),
            Status = SubscriptionStatus.Active,
            AutoRenew = true,
            NextBillingDateUtc = DateTime.UtcNow.AddDays(duration),
            LastPaymentDateUtc = DateTime.UtcNow,
            PricePaid = price
        };

        _context.MemberSubscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        return member.Id;
    }

    public async Task UpdateAsync(int id, UpdateMemberDto dto)
    {
        var member = await _context.Members.FirstOrDefaultAsync(x => x.Id == id);
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
        var member = await _context.Members.FirstOrDefaultAsync(x => x.Id == id);
        if (member == null)
            throw new Exception("Member not found");

        member.IsActive = false;
        await _context.SaveChangesAsync();
    }
}