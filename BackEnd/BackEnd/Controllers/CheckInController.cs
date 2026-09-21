using System.Collections.Generic;
using System.Threading.Tasks;
using GymCore.API.DTOs.CheckIn;
using GymCore.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymCore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{UserRoles.SuperAdmin},{UserRoles.FrontDeskStaff}")]
public class CheckInController : ControllerBase
{
    private readonly ICheckInService _checkInService;

    public CheckInController(ICheckInService checkInService)
    {
        _checkInService = checkInService;
    }

    [HttpPost]
    public async Task<ActionResult<CheckInResultDto>> ProcessCheckIn([FromBody] CheckInRequestDto request)
    {
        var result = await _checkInService.ProcessCheckInAsync(request);
        if (!result.AccessGranted)
        {
            return Ok(result); // Return 200 with AccessGranted = false so kiosk UI displays red card gracefully
        }
        return Ok(result);
    }

    [HttpGet("recent")]
    public async Task<ActionResult<IEnumerable<CheckInHistoryDto>>> GetRecentCheckIns([FromQuery] int limit = 50)
    {
        var history = await _checkInService.GetRecentCheckInsAsync(limit);
        return Ok(history);
    }
}
