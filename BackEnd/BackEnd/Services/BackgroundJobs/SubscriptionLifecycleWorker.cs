using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GymCore.API.Data;
using GymCore.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GymCore.API.Services.BackgroundJobs;

/// <summary>
/// Background worker that periodically checks subscription lifecycles, handles grace periods,
/// marks expired subscriptions, and generates automated billing renewals.
/// </summary>
public class SubscriptionLifecycleWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SubscriptionLifecycleWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(30);

    public SubscriptionLifecycleWorker(
        IServiceProvider serviceProvider,
        ILogger<SubscriptionLifecycleWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("GymCore Subscription Lifecycle Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessSubscriptionAuditAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during subscription lifecycle audit.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("GymCore Subscription Lifecycle Worker stopped.");
    }

    public async Task ProcessSubscriptionAuditAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GymDbContext>();

        var now = DateTime.UtcNow;

        // 1. Transition past-due Active subscriptions into GracePeriod
        var pastDueActive = await context.MemberSubscriptions
            .IgnoreQueryFilters() // Worker processes all tenants
            .Where(s => s.Status == SubscriptionStatus.Active && s.EndDateUtc < now)
            .ToListAsync(cancellationToken);

        foreach (var sub in pastDueActive)
        {
            sub.Status = SubscriptionStatus.GracePeriod;
            _logger.LogInformation("Subscription {SubId} for Member {MemberId} shifted to GracePeriod.", sub.Id, sub.MemberId);
        }

        // 2. Transition past-grace-period subscriptions to Expired (e.g. 5 days after EndDate)
        var gracePeriodThreshold = now.AddDays(-5);
        var expiredSubscriptions = await context.MemberSubscriptions
            .IgnoreQueryFilters()
            .Where(s => s.Status == SubscriptionStatus.GracePeriod && s.EndDateUtc < gracePeriodThreshold)
            .ToListAsync(cancellationToken);

        foreach (var sub in expiredSubscriptions)
        {
            sub.Status = SubscriptionStatus.Expired;
            _logger.LogInformation("Subscription {SubId} for Member {MemberId} has Expired.", sub.Id, sub.MemberId);
        }

        if (pastDueActive.Count > 0 || expiredSubscriptions.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Subscription lifecycle worker processed {GraceCount} grace periods and {ExpiredCount} expirations.",
                pastDueActive.Count, expiredSubscriptions.Count);
        }
    }
}
