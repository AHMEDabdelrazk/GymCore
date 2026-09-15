using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GymCore.API.Data;
using GymCore.API.Models;
using GymCore.API.Models.Entities;
using GymCore.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BackEnd.Tests;

public class MultiTenancyIsolationTests
{
    private GymDbContext CreateDbContext(int tenantId, bool isGlobalAdmin = false)
    {
        var tenantMock = new Mock<ITenantProvider>();
        tenantMock.Setup(t => t.GetCurrentTenantId()).Returns(tenantId);
        tenantMock.Setup(t => t.IsGlobalAdmin()).Returns(isGlobalAdmin);

        var options = new DbContextOptionsBuilder<GymDbContext>()
            .UseInMemoryDatabase(databaseName: $"GymCore_MT_Test_{Guid.NewGuid():N}")
            .Options;

        return new GymDbContext(options, tenantMock.Object);
    }

    [Fact]
    public async Task QueryFilter_ShouldIsolateData_BetweenDifferentTenants()
    {
        // Arrange: DbContext for Tenant 1
        using var contextTenant1 = CreateDbContext(tenantId: 1);

        var memberTenant1 = new Member
        {
            TenantId = 1,
            FullName = "Tenant 1 Member",
            Email = "t1@example.com",
            MemberCode = "MEM-101",
            IsActive = true
        };

        var memberTenant2 = new Member
        {
            TenantId = 2,
            FullName = "Tenant 2 Member",
            Email = "t2@example.com",
            MemberCode = "MEM-201",
            IsActive = true
        };

        contextTenant1.Members.AddRange(memberTenant1, memberTenant2);
        await contextTenant1.SaveChangesAsync();

        // Act: Query as Tenant 1
        var resultsTenant1 = await contextTenant1.Members.ToListAsync();

        // Assert: Tenant 1 should only see member from Tenant 1
        resultsTenant1.Should().HaveCount(1);
        resultsTenant1.First().FullName.Should().Be("Tenant 1 Member");
    }

    [Fact]
    public async Task QueryFilter_ShouldAllowGlobalAdmin_ToAccessAllTenants()
    {
        // Arrange
        using var context = CreateDbContext(tenantId: 1, isGlobalAdmin: true);

        context.Members.AddRange(
            new Member { TenantId = 1, FullName = "Member T1", Email = "t1@gym.com", MemberCode = "M1", IsActive = true },
            new Member { TenantId = 2, FullName = "Member T2", Email = "t2@gym.com", MemberCode = "M2", IsActive = true }
        );
        await context.SaveChangesAsync();

        // Act: Query with Global Admin rights
        var allMembers = await context.Members.ToListAsync();

        // Assert
        allMembers.Should().HaveCount(2);
    }

    [Fact]
    public async Task SaveChanges_ShouldAutomaticallyStampTenantId_WhenNotExplicitlyProvided()
    {
        // Arrange
        using var context = CreateDbContext(tenantId: 5);

        var newPlan = new MembershipPlan
        {
            TenantId = 0, // Not explicitly set
            Name = "Gold Access",
            Price = 99.99m,
            DurationInDays = 30,
            IsActive = true
        };

        context.MembershipPlans.Add(newPlan);
        await context.SaveChangesAsync();

        // Assert: TenantId should be automatically stamped with 5
        newPlan.TenantId.Should().Be(5);
    }
}
