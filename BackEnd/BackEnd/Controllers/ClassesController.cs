using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GymCore.API.DTOs.Class;
using GymCore.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymCore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{UserRoles.SuperAdmin},{UserRoles.Trainer}")]
public class ClassesController : ControllerBase
{
    private readonly IClassBookingService _bookingService;

    public ClassesController(IClassBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<IEnumerable<ClassSessionDto>>> GetUpcomingSessions([FromQuery] DateTime? fromDate)
    {
        var sessions = await _bookingService.GetUpcomingSessionsAsync(fromDate);
        return Ok(sessions);
    }

    [HttpPost("sessions/{id}/book")]
    public async Task<ActionResult<BookingResultDto>> BookSession(int id, [FromBody] BookClassRequestDto request)
    {
        try
        {
            var result = await _bookingService.BookClassAsync(id, request.MemberId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("bookings/{id}/cancel")]
    public async Task<IActionResult> CancelBooking(int id)
    {
        var success = await _bookingService.CancelBookingAsync(id);
        if (!success)
            return NotFound(new { error = "Booking not found or already canceled." });

        return Ok(new { message = "Booking canceled successfully. Waitlist has been automatically updated." });
    }

    [HttpGet("sessions/{id}/roster")]
    public async Task<ActionResult<IEnumerable<ClassRosterItemDto>>> GetSessionRoster(int id)
    {
        var roster = await _bookingService.GetSessionRosterAsync(id);
        return Ok(roster);
    }

    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession([FromBody] CreateClassSessionRequestDto request)
    {
        var sessionId = await _bookingService.CreateSessionAsync(
            request.ClassTypeId,
            request.TrainerId,
            request.RoomName,
            request.StartTimeUtc,
            request.Capacity);

        return CreatedAtAction(nameof(GetUpcomingSessions), new { id = sessionId }, new { id = sessionId });
    }
}

public class CreateClassSessionRequestDto
{
    public int ClassTypeId { get; set; }
    public string TrainerId { get; set; } = string.Empty;
    public string RoomName { get; set; } = "Studio A";
    public DateTime StartTimeUtc { get; set; }
    public int Capacity { get; set; } = 20;
}
