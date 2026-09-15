using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymCore.API.Data;
using GymCore.API.DTOs.Class;
using GymCore.API.Models.Entities;
using GymCore.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymCore.API.Services;

public class ClassBookingService : IClassBookingService
{
    private readonly GymDbContext _context;

    public ClassBookingService(GymDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ClassSessionDto>> GetUpcomingSessionsAsync(DateTime? fromDate = null)
    {
        var threshold = fromDate ?? DateTime.UtcNow;

        return await _context.ClassSessions
            .Include(s => s.ClassType)
            .Include(s => s.Trainer)
            .Include(s => s.Bookings)
            .Where(s => !s.IsCanceled && s.EndTimeUtc >= threshold)
            .OrderBy(s => s.StartTimeUtc)
            .Select(s => new ClassSessionDto
            {
                Id = s.Id,
                ClassTypeId = s.ClassTypeId,
                ClassTypeName = s.ClassType.Name,
                Category = s.ClassType.Category,
                TrainerId = s.TrainerId,
                TrainerName = s.Trainer != null ? s.Trainer.FullName : "Staff Coach",
                RoomName = s.RoomName,
                StartTimeUtc = s.StartTimeUtc,
                EndTimeUtc = s.EndTimeUtc,
                Capacity = s.Capacity,
                ReservedSpots = s.ReservedSpots,
                WaitlistCount = s.Bookings.Count(b => b.Status == BookingStatus.Waitlisted),
                IsCanceled = s.IsCanceled
            })
            .ToListAsync();
    }

    public async Task<BookingResultDto> BookClassAsync(int sessionId, int memberId)
    {
        var session = await _context.ClassSessions
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
            throw new ArgumentException("Class session not found");

        if (session.IsCanceled)
            throw new InvalidOperationException("This class session has been canceled");

        // Verify member exists and is active
        var member = await _context.Members.FirstOrDefaultAsync(m => m.Id == memberId);
        if (member == null || !member.IsActive)
            throw new InvalidOperationException("Member not found or inactive");

        // Prevent duplicate active booking
        var existingBooking = session.Bookings.FirstOrDefault(b =>
            b.MemberId == memberId &&
            (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted));

        if (existingBooking != null)
        {
            return new BookingResultDto
            {
                BookingId = existingBooking.Id,
                Status = existingBooking.Status.ToString(),
                WaitlistPosition = existingBooking.WaitlistPosition,
                Message = $"Member is already registered with status: {existingBooking.Status}"
            };
        }

        // Concurrency-safe capacity check
        if (session.ReservedSpots < session.Capacity)
        {
            session.ReservedSpots++;

            var confirmedBooking = new ClassBooking
            {
                ClassSessionId = session.Id,
                MemberId = memberId,
                TenantId = session.TenantId,
                Status = BookingStatus.Confirmed,
                BookedAtUtc = DateTime.UtcNow
            };

            _context.ClassBookings.Add(confirmedBooking);
            await _context.SaveChangesAsync();

            return new BookingResultDto
            {
                BookingId = confirmedBooking.Id,
                Status = "Confirmed",
                WaitlistPosition = null,
                Message = "Spot successfully reserved!"
            };
        }
        else
        {
            // Class is full -> place on Waitlist
            var currentWaitlistCount = session.Bookings.Count(b => b.Status == BookingStatus.Waitlisted);
            var waitlistPosition = currentWaitlistCount + 1;

            var waitlistedBooking = new ClassBooking
            {
                ClassSessionId = session.Id,
                MemberId = memberId,
                TenantId = session.TenantId,
                Status = BookingStatus.Waitlisted,
                WaitlistPosition = waitlistPosition,
                BookedAtUtc = DateTime.UtcNow
            };

            _context.ClassBookings.Add(waitlistedBooking);
            await _context.SaveChangesAsync();

            return new BookingResultDto
            {
                BookingId = waitlistedBooking.Id,
                Status = "Waitlisted",
                WaitlistPosition = waitlistPosition,
                Message = $"Class is at capacity. Member added to waitlist at position #{waitlistPosition}."
            };
        }
    }

    public async Task<bool> CancelBookingAsync(int bookingId)
    {
        var booking = await _context.ClassBookings
            .Include(b => b.ClassSession)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null || booking.Status == BookingStatus.Canceled)
            return false;

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;
        var session = booking.ClassSession;

        booking.Status = BookingStatus.Canceled;
        booking.CanceledAtUtc = DateTime.UtcNow;
        booking.WaitlistPosition = null;

        if (wasConfirmed && session.ReservedSpots > 0)
        {
            session.ReservedSpots--;

            // Auto-promote top waitlisted member if one exists
            var nextInLine = await _context.ClassBookings
                .Where(b => b.ClassSessionId == session.Id && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition ?? int.MaxValue)
                .ThenBy(b => b.BookedAtUtc)
                .FirstOrDefaultAsync();

            if (nextInLine != null)
            {
                nextInLine.Status = BookingStatus.Confirmed;
                nextInLine.WaitlistPosition = null;
                session.ReservedSpots++;

                // Re-calculate waitlist positions for remaining waitlisted members
                var remainingWaitlist = await _context.ClassBookings
                    .Where(b => b.ClassSessionId == session.Id && b.Status == BookingStatus.Waitlisted && b.Id != nextInLine.Id)
                    .OrderBy(b => b.WaitlistPosition ?? int.MaxValue)
                    .ThenBy(b => b.BookedAtUtc)
                    .ToListAsync();

                for (int i = 0; i < remainingWaitlist.Count; i++)
                {
                    remainingWaitlist[i].WaitlistPosition = i + 1;
                }
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ClassRosterItemDto>> GetSessionRosterAsync(int sessionId)
    {
        return await _context.ClassBookings
            .Include(b => b.Member)
            .Where(b => b.ClassSessionId == sessionId && b.Status != BookingStatus.Canceled)
            .OrderBy(b => b.Status)
            .ThenBy(b => b.WaitlistPosition)
            .ThenBy(b => b.BookedAtUtc)
            .Select(b => new ClassRosterItemDto
            {
                BookingId = b.Id,
                MemberId = b.MemberId,
                MemberName = b.Member.FullName,
                MemberEmail = b.Member.Email,
                Status = b.Status.ToString(),
                WaitlistPosition = b.WaitlistPosition,
                BookedAtUtc = b.BookedAtUtc
            })
            .ToListAsync();
    }

    public async Task<int> CreateSessionAsync(int classTypeId, string trainerId, string roomName, DateTime startTimeUtc, int capacity)
    {
        var classType = await _context.ClassTypes.FindAsync(classTypeId);
        if (classType == null)
            throw new ArgumentException("ClassType not found");

        var duration = classType.DefaultDurationMinutes > 0 ? classType.DefaultDurationMinutes : 60;
        var session = new ClassSession
        {
            ClassTypeId = classTypeId,
            TrainerId = trainerId,
            RoomName = roomName,
            StartTimeUtc = startTimeUtc,
            EndTimeUtc = startTimeUtc.AddMinutes(duration),
            Capacity = capacity > 0 ? capacity : classType.DefaultCapacity,
            ReservedSpots = 0
        };

        _context.ClassSessions.Add(session);
        await _context.SaveChangesAsync();
        return session.Id;
    }
}
