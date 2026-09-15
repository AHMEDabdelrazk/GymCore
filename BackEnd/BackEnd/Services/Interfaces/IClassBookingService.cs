using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GymCore.API.DTOs.Class;

namespace GymCore.API.Services.Interfaces;

public interface IClassBookingService
{
    Task<IEnumerable<ClassSessionDto>> GetUpcomingSessionsAsync(DateTime? fromDate = null);
    Task<BookingResultDto> BookClassAsync(int sessionId, int memberId);
    Task<bool> CancelBookingAsync(int bookingId);
    Task<IEnumerable<ClassRosterItemDto>> GetSessionRosterAsync(int sessionId);
    Task<int> CreateSessionAsync(int classTypeId, string trainerId, string roomName, DateTime startTimeUtc, int capacity);
}
