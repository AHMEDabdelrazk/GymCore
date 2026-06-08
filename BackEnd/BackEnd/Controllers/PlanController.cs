using GymCore.API.DTOs.Plan;
using GymCore.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymCore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlansController : ControllerBase
{
    private readonly IPlanService _service;

    public PlansController(IPlanService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var plan = await _service.GetByIdAsync(id);

        if (plan is null)
            return NotFound();

        return Ok(plan);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePlanDto dto)
    {
        var id = await _service.CreateAsync(dto);

        return Ok(id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdatePlanDto dto)
    {
        await _service.UpdateAsync(id, dto);

        return NoContent();
    }

    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _service.DeactivateAsync(id);

        return NoContent();
    }
}