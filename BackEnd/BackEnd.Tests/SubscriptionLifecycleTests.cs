using System;
using System.Threading.Tasks;
using FluentAssertions;
using GymCore.API.Data;
using GymCore.API.DTOs.CheckIn;
using GymCore.API.Models;
using GymCore.API.Models.Entities;
using GymCore.API.Services;
using GymCore.API.Services.BackgroundJobs;
using GymCore.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BackEnd.Tests;

public class SubscriptionLifecycleTests
{
    private GymDbContext CreateInMemoryContext()
    {
        var tenantMock = new Mock<ITenantProvider>();
        tenantMock.Setup(t => t.GetCurrentTenantId()).Returns(1);
        tenantMock.Setup(t => t.IsGlobalAdmin()).Returns(true);

        var options = new DbContextOptionsBuilder<GymDbContext>()
            .UseInMemoryDatabase(databaseName: $"GymCore_Lifecycle_Test_{Guid.NewGuid():N}")
            .Options;

        return new GymDbContext(options, tenantMock.Object);
    }

    [Fact]
    public async Task ProcessSubscriptionAuditAsync_ShouldTransitionPastDueActiveToGracePeriod()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var sub = new MemberSubscription
        {
            TenantId = 1,
            MemberId = 10,
            MembershipPlanId = 1,
            StartDateUtc = DateTime.UtcNow.AddDays(-35),
            EndDateUtc = DateTime.UtcNow.AddDays(-2), // 2 days past due
            Status = SubscriptionStatus.Active,
            PricePaid = 49.99m
        };
        context.MemberSubscriptions.Add(sub);
        await context.SaveChangesAsync();

        // Setup service provider for worker
        var services = new ServiceCollection();
        services.AddSingleton(context);
        var serviceProvider = services.BuildServiceProvider();

        var worker = new SubscriptionLifecycleWorker(serviceProvider, NullLogger<SubscriptionLifecycleWorker>.Instance);

        // Act
        await worker.ProcessSubscriptionAuditAsync();

        // Assert
        var updatedSub = await context.MemberSubscriptions.FindAsync(sub.Id);
        updatedSub.Should().NotBeNull();
        updatedSub!.Status.Should().Be(SubscriptionStatus.GracePeriod);
    }

    [Fact]
    public async Task CheckInService_WithExpiredSubscription_ShouldDenyAccess()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var checkInService = new CheckInService(context);

        var member = new Member
        {
            Id = 55,
            TenantId = 1,
            MemberCode = "MEM-5555",
            FullName = "Expired Sub Member",
            IsActive = true
        };

        var expiredSub = new MemberSubscription
        {
            TenantId = 1,
            MemberId = 55,
            MembershipPlanId = 1,
            StartDateUtc = DateTime.UtcNow.AddMonths(-3),
            EndDateUtc = DateTime.UtcNow.AddDays(-20),
            Status = SubscriptionStatus.Expired
        };

        context.Members.Add(member);
        context.MemberSubscriptions.Add(expiredSub);
        await context.SaveChangesAsync();

        // Act
        var result = await checkInService.ProcessCheckInAsync(new CheckInRequestDto
        {
            MemberCodeOrId = "MEM-5555"
        });

        // Assert
        result.AccessGranted.Should().BeFalse();
        result.Message.Should().Contain("expired");
    }

    [Fact]
    public async Task CheckInService_WithActiveSubscription_ShouldGrantAccess()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var checkInService = new CheckInService(context);

        var member = new Member
        {
            Id = 77,
            TenantId = 1,
            MemberCode = "MEM-7777",
            FullName = "Active Sub Member",
            IsActive = true
        };

        var activeSub = new MemberSubscription
        {
            TenantId = 1,
            MemberId = 77,
            MembershipPlanId = 1,
            StartDateUtc = DateTime.UtcNow.AddDays(-5),
            EndDateUtc = DateTime.UtcNow.AddDays(25),
            Status = SubscriptionStatus.Active
        };

        context.Members.Add(member);
        context.MemberSubscriptions.Add(activeSub);
        await context.SaveChangesAsync();

        var testMember = await context.Members
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.MemberCode == "MEM-7777");

        testMember.Should().NotBeNull();

        // Act
        var result = await checkInService.ProcessCheckInAsync(new CheckInRequestDto
        {
            MemberCodeOrId = "MEM-7777"
        });

        // Assert
        result.AccessGranted.Should().BeTrue();
        result.Message.Should().Contain("Access Granted");
    }
}
