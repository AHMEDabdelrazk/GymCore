using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymCore.API.Data;
using GymCore.API.DTOs.Tenant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymCore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly GymDbContext _context;

    public TenantsController(GymDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TenantDto>>> GetAll()
    {
        var tenants = await _context.Tenants
            .Where(t => t.IsActive)
            .Select(t => new TenantDto
            {
                Id = t.Id,
                Name = t.Name,
                Code = t.Code,
                Address = t.Address,
                PhoneNumber = t.PhoneNumber,
                TimeZone = t.TimeZone,
                IsActive = t.IsActive
            })
            .ToListAsync();

        return Ok(tenants);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TenantDto>> GetById(int id)
    {
        var tenant = await _context.Tenants
            .Where(t => t.Id == id)
            .Select(t => new TenantDto
            {
                Id = t.Id,
                Name = t.Name,
                Code = t.Code,
                Address = t.Address,
                PhoneNumber = t.PhoneNumber,
                TimeZone = t.TimeZone,
                IsActive = t.IsActive
            })
            .FirstOrDefaultAsync();

        if (tenant == null)
            return NotFound();

        return Ok(tenant);
    }
}
