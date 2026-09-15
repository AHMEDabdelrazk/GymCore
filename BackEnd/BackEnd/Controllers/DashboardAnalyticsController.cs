using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymCore.API.Data;
using GymCore.API.DTOs.Dashboard;
using GymCore.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymCore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardAnalyticsController : ControllerBase
{
    private readonly GymDbContext _context;

    public DashboardAnalyticsController(GymDbContext context)
    {
        _context = context;
    }

    [HttpGet("metrics")]
    public async Task<ActionResult<DashboardMetricsDto>> GetMetrics()
    {
        var today = DateTime.UtcNow.Date;

        // 1. Active members
        var activeMembersCount = await _context.Members
            .CountAsync(m => m.IsActive);

        // 2. MRR (sum of active subscriptions or paid invoices in the last 30 days)
        var mrr = await _context.MemberSubscriptions
            .Where(s => s.Status == SubscriptionStatus.Active)
            .SumAsync(s => (decimal?)s.PricePaid) ?? 0m;

        if (mrr == 0)
        {
            mrr = await _context.Invoices
                .Where(i => i.Status == InvoiceStatus.Paid)
                .SumAsync(i => (decimal?)i.Amount) ?? 0m;
        }

        // 3. Today's check-ins
        var todayCheckInsCount = await _context.CheckInRecords
            .CountAsync(c => c.CheckInTimeUtc >= today);

        // 4. Class capacity utilization
        var upcomingSessions = await _context.ClassSessions
            .Where(s => !s.IsCanceled && s.EndTimeUtc >= DateTime.UtcNow)
            .ToListAsync();

        double utilizationPercentage = 0;
        if (upcomingSessions.Count > 0)
        {
            var totalCapacity = upcomingSessions.Sum(s => s.Capacity);
            var totalReserved = upcomingSessions.Sum(s => s.ReservedSpots);
            if (totalCapacity > 0)
            {
                utilizationPercentage = Math.Round(((double)totalReserved / totalCapacity) * 100, 1);
            }
        }

        // 5. Recent activities
        var activities = new List<RecentActivityDto>();

        var recentCheckIns = await _context.CheckInRecords
            .Include(c => c.Member)
            .OrderByDescending(c => c.CheckInTimeUtc)
            .Take(4)
            .ToListAsync();

        foreach (var c in recentCheckIns)
        {
            activities.Add(new RecentActivityDto
            {
                Type = "CheckIn",
                Description = $"{c.Member?.FullName ?? "Member"} checked in ({c.Status}) via {c.AccessMethod}",
                TimestampUtc = c.CheckInTimeUtc
            });
        }

        var recentInvoices = await _context.Invoices
            .Include(i => i.Member)
            .OrderByDescending(i => i.PaidAtUtc ?? i.CreatedAtUtc)
            .Take(3)
            .ToListAsync();

        foreach (var inv in recentInvoices)
        {
            activities.Add(new RecentActivityDto
            {
                Type = "Payment",
                Description = $"Invoice {inv.InvoiceNumber} ({inv.Status}): ${inv.Amount} for {inv.Member?.FullName ?? "Member"}",
                TimestampUtc = inv.PaidAtUtc ?? inv.CreatedAtUtc
            });
        }

        var sortedActivities = activities
            .OrderByDescending(a => a.TimestampUtc)
            .Take(6)
            .ToList();

        return Ok(new DashboardMetricsDto
        {
            ActiveMembersCount = activeMembersCount,
            MonthlyRecurringRevenue = mrr,
            TodayCheckInsCount = todayCheckInsCount,
            ClassCapacityUtilizationPercentage = utilizationPercentage,
            UpcomingClassesCount = upcomingSessions.Count,
            RecentActivities = sortedActivities
        });
    }
}
