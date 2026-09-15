using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymCore.API.Data;
using GymCore.API.DTOs.Audit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymCore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly GymDbContext _context;

    public AuditController(GymDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetLogs([FromQuery] string? entityName, [FromQuery] int limit = 100)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            query = query.Where(a => a.EntityName.ToLower() == entityName.ToLower());
        }

        var logs = await query
            .OrderByDescending(a => a.TimestampUtc)
            .Take(limit)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                TenantId = a.TenantId,
                UserId = a.UserId,
                UserEmail = a.UserEmail,
                Action = a.Action,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                OldValuesJson = a.OldValuesJson,
                NewValuesJson = a.NewValuesJson,
                TimestampUtc = a.TimestampUtc,
                IpAddress = a.IpAddress
            })
            .ToListAsync();

        return Ok(logs);
    }
}
