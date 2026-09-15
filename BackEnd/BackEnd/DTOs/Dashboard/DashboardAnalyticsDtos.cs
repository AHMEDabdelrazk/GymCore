using System;
using System.Collections.Generic;

namespace GymCore.API.DTOs.Dashboard;

public class DashboardMetricsDto
{
    public int ActiveMembersCount { get; set; }
    public decimal MonthlyRecurringRevenue { get; set; }
    public int TodayCheckInsCount { get; set; }
    public double ClassCapacityUtilizationPercentage { get; set; }
    public int UpcomingClassesCount { get; set; }
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class RecentActivityDto
{
    public string Type { get; set; } = string.Empty; // CheckIn, Booking, Payment, Audit
    public string Description { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
}
