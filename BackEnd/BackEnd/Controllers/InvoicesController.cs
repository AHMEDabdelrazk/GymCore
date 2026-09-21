using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymCore.API.Data;
using GymCore.API.DTOs.Billing;
using GymCore.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymCore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{UserRoles.SuperAdmin},{UserRoles.FrontDeskStaff}")]
public class InvoicesController : ControllerBase
{
    private readonly GymDbContext _context;
    private readonly IPaymentGatewayService _paymentService;

    public InvoicesController(GymDbContext context, IPaymentGatewayService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetAll()
    {
        var invoices = await _context.Invoices
            .Include(i => i.Member)
            .OrderByDescending(i => i.DueDateUtc)
            .Select(i => new InvoiceDto
            {
                Id = i.Id,
                MemberId = i.MemberId,
                MemberName = i.Member != null ? i.Member.FullName : "Unknown",
                InvoiceNumber = i.InvoiceNumber,
                Amount = i.Amount,
                Currency = i.Currency,
                Status = i.Status.ToString(),
                DueDateUtc = i.DueDateUtc,
                PaidAtUtc = i.PaidAtUtc,
                StripePaymentIntentId = i.StripePaymentIntentId
            })
            .ToListAsync();

        return Ok(invoices);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> Create([FromBody] CreateInvoiceRequestDto request)
    {
        var result = await _paymentService.CreateInvoiceAsync(request.MemberId, request.Amount, request.Currency);
        return Ok(result);
    }
}

public class CreateInvoiceRequestDto
{
    public int MemberId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
}
