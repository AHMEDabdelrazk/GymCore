using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GymCore.API.Data;
using GymCore.API.Models;
using GymCore.API.Models.Entities;
using GymCore.API.Services;
using GymCore.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BackEnd.Tests;

public class ClassBookingConcurrencyTests
{
    private GymDbContext CreateInMemoryContext()
    {
        var tenantMock = new Mock<ITenantProvider>();
        tenantMock.Setup(t => t.GetCurrentTenantId()).Returns(1);
        tenantMock.Setup(t => t.IsGlobalAdmin()).Returns(true);

        var options = new DbContextOptionsBuilder<GymDbContext>()
            .UseInMemoryDatabase(databaseName: $"GymCore_Booking_Test_{Guid.NewGuid():N}")
            .Options;

        return new GymDbContext(options, tenantMock.Object);
    }

    [Fact]
    public async Task BookClassAsync_WhenCapacityReached_ShouldPlaceMemberOnWaitlist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var bookingService = new ClassBookingService(context);

        var classType = new ClassType { Id = 1, TenantId = 1, Name = "HIIT", DefaultCapacity = 2 };
        var session = new ClassSession
        {
            Id = 1,
            TenantId = 1,
            ClassTypeId = 1,
            StartTimeUtc = DateTime.UtcNow.AddHours(2),
            EndTimeUtc = DateTime.UtcNow.AddHours(3),
            Capacity = 2,
            ReservedSpots = 0
        };

        var m1 = new Member { Id = 1, TenantId = 1, FullName = "Member 1", IsActive = true };
        var m2 = new Member { Id = 2, TenantId = 1, FullName = "Member 2", IsActive = true };
        var m3 = new Member { Id = 3, TenantId = 1, FullName = "Member 3", IsActive = true };

        context.ClassTypes.Add(classType);
        context.ClassSessions.Add(session);
        context.Members.AddRange(m1, m2, m3);
        await context.SaveChangesAsync();

        // Act: Book spot 1 (Confirmed)
        var result1 = await bookingService.BookClassAsync(session.Id, m1.Id);
        // Act: Book spot 2 (Confirmed)
        var result2 = await bookingService.BookClassAsync(session.Id, m2.Id);
        // Act: Book spot 3 (Waitlisted)
        var result3 = await bookingService.BookClassAsync(session.Id, m3.Id);

        // Assert
        result1.Status.Should().Be("Confirmed");
        result2.Status.Should().Be("Confirmed");
        result3.Status.Should().Be("Waitlisted");
        result3.WaitlistPosition.Should().Be(1);

        session.ReservedSpots.Should().Be(2);
    }

    [Fact]
    public async Task CancelBookingAsync_WhenConfirmedBookingCanceled_ShouldAutoPromoteTopWaitlistedMember()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var bookingService = new ClassBookingService(context);

        var classType = new ClassType { Id = 1, TenantId = 1, Name = "Spin", DefaultCapacity = 1 };
        var session = new ClassSession
        {
            Id = 10,
            TenantId = 1,
            ClassTypeId = 1,
            StartTimeUtc = DateTime.UtcNow.AddHours(1),
            EndTimeUtc = DateTime.UtcNow.AddHours(2),
            Capacity = 1,
            ReservedSpots = 0
        };

        var m1 = new Member { Id = 10, TenantId = 1, FullName = "Confirmed Member", IsActive = true };
        var m2 = new Member { Id = 20, TenantId = 1, FullName = "Waitlisted Member", IsActive = true };

        context.ClassTypes.Add(classType);
        context.ClassSessions.Add(session);
        context.Members.AddRange(m1, m2);
        await context.SaveChangesAsync();

        // Fill capacity
        var book1 = await bookingService.BookClassAsync(session.Id, m1.Id);
        var book2 = await bookingService.BookClassAsync(session.Id, m2.Id);

        book1.Status.Should().Be("Confirmed");
        book2.Status.Should().Be("Waitlisted");

        // Act: Member 1 cancels booking
        var cancelSuccess = await bookingService.CancelBookingAsync(book1.BookingId);

        // Assert
        cancelSuccess.Should().BeTrue();

        var promotedBooking = await context.ClassBookings.FindAsync(book2.BookingId);
        promotedBooking.Should().NotBeNull();
        promotedBooking!.Status.Should().Be(BookingStatus.Confirmed);
        promotedBooking.WaitlistPosition.Should().BeNull();
        session.ReservedSpots.Should().Be(1);
    }
}
