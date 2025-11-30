using BusX.Domain.DTOs;
using BusX.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BusX.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly IBusBookingService _service;

    public TicketsController(IBusBookingService service)
    {
        _service = service;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _service.BuyTicketAsync(request);

        if (!result.IsSuccess)
        {
            if (result.Message.Contains("başkası tarafından"))
            {
                return Conflict(result); 
            }

            if (result.Message.Contains("Ödeme"))
            {
                return StatusCode(402, result); 
            }

            return BadRequest(result);
        }

        return Ok(result);
    }
}