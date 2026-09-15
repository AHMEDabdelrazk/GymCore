using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymCore.API.Data;
using GymCore.API.DTOs.CheckIn;
using GymCore.API.Models;
using GymCore.API.Models.Entities;
using GymCore.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymCore.API.Services;

public class CheckInService : ICheckInService
{
    private readonly GymDbContext _context;

    public CheckInService(GymDbContext context)
    {
        _context = context;
    }

    public async Task<CheckInResultDto> ProcessCheckInAsync(CheckInRequestDto request)
    {
        var query = _context.Members
            .Include(m => m.MembershipPlan)
            .Include(m => m.Subscriptions)
            .AsQueryable();

        // Search by MemberCode (e.g. MEM-1001) or Id
        Member? member = null;
        if (int.TryParse(request.MemberCodeOrId, out var id))
        {
            member = await query.FirstOrDefaultAsync(m => m.Id == id || m.MemberCode == request.MemberCodeOrId);
        }
        else
        {
            member = await query.FirstOrDefaultAsync(m => m.MemberCode.ToLower() == request.MemberCodeOrId.ToLower());
        }

        if (member == null)
        {
            return new CheckInResultDto
            {
                AccessGranted = false,
                Message = "Access Denied: Member not found",
                TimestampUtc = DateTime.UtcNow
            };
        }

        if (!member.IsActive)
        {
            await RecordCheckInLog(member.Id, CheckInStatus.Denied, "Member profile is deactivated", request.AccessMethod);
            return new CheckInResultDto
            {
                AccessGranted = false,
                MemberName = member.FullName,
                Message = "Access Denied: Member account is deactivated",
                TimestampUtc = DateTime.UtcNow
            };
        }

        // Check active subscription
        var latestSub = member.Subscriptions
            .OrderByDescending(s => s.EndDateUtc)
            .FirstOrDefault();

        var subStatus = latestSub?.Status ?? SubscriptionStatus.Active;
        var planName = latestSub?.MembershipPlan?.Name ?? member.MembershipPlan?.Name ?? "General Membership";

        if (latestSub != null && latestSub.Status == SubscriptionStatus.Expired)
        {
            await RecordCheckInLog(member.Id, CheckInStatus.Denied, "Membership subscription expired", request.AccessMethod);
            return new CheckInResultDto
            {
                AccessGranted = false,
                MemberName = member.FullName,
                SubscriptionStatus = "Expired",
                PlanName = planName,
                Message = "Access Denied: Subscription has expired. Please renew at the front desk.",
                TimestampUtc = DateTime.UtcNow
            };
        }

        if (latestSub != null && latestSub.Status == SubscriptionStatus.Canceled)
        {
            await RecordCheckInLog(member.Id, CheckInStatus.Denied, "Membership canceled", request.AccessMethod);
            return new CheckInResultDto
            {
                AccessGranted = false,
                MemberName = member.FullName,
                SubscriptionStatus = "Canceled",
                PlanName = planName,
                Message = "Access Denied: Subscription canceled.",
                TimestampUtc = DateTime.UtcNow
            };
        }

        // Check for GracePeriod warning
        string message = "Access Granted. Welcome!";
        if (latestSub != null && latestSub.Status == SubscriptionStatus.GracePeriod)
        {
            message = "Access Granted (Grace Period): Payment is overdue. Please settle invoice.";
        }

        await RecordCheckInLog(member.Id, CheckInStatus.Success, null, request.AccessMethod);

        return new CheckInResultDto
        {
            AccessGranted = true,
            MemberName = member.FullName,
            SubscriptionStatus = subStatus.ToString(),
            PlanName = planName,
            Message = message,
            TimestampUtc = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<CheckInHistoryDto>> GetRecentCheckInsAsync(int limit = 50)
    {
        return await _context.CheckInRecords
            .Include(c => c.Member)
            .OrderByDescending(c => c.CheckInTimeUtc)
            .Take(limit)
            .Select(c => new CheckInHistoryDto
            {
                Id = c.Id,
                MemberId = c.MemberId,
                MemberName = c.Member != null ? c.Member.FullName : "Unknown",
                CheckInTimeUtc = c.CheckInTimeUtc,
                Status = c.Status.ToString(),
                DenialReason = c.DenialReason,
                AccessMethod = c.AccessMethod
            })
            .ToListAsync();
    }

    private async Task RecordCheckInLog(int memberId, CheckInStatus status, string? reason, string accessMethod)
    {
        var record = new CheckInRecord
        {
            MemberId = memberId,
            TenantId = _context.CurrentTenantId,
            Status = status,
            DenialReason = reason,
            AccessMethod = string.IsNullOrWhiteSpace(accessMethod) ? "Kiosk_QR" : accessMethod,
            CheckInTimeUtc = DateTime.UtcNow
        };

        _context.CheckInRecords.Add(record);
        await _context.SaveChangesAsync();
    }
}
