using System.Collections.Generic;
using System.Threading.Tasks;
using GymCore.API.DTOs.CheckIn;

namespace GymCore.API.Services.Interfaces;

public interface ICheckInService
{
    Task<CheckInResultDto> ProcessCheckInAsync(CheckInRequestDto request);
    Task<IEnumerable<CheckInHistoryDto>> GetRecentCheckInsAsync(int limit = 50);
}
