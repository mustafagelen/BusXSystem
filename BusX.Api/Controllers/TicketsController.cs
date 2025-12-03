using BusX.Domain.Common;
using BusX.Domain.DTOs;
using BusX.Domain.Exceptions;
using BusX.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        var result = await _service.BuyTicketAsync(request);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        if (result.Exception is SeatUnavailableException ||
            result.Exception is DbUpdateConcurrencyException)
        {
            return Conflict(new { message = result.ErrorMessage });
        }

        if (result.Exception is PaymentFailedException)
        {
            return StatusCode(402, new { message = result.ErrorMessage });
        }

        if (result.Exception is GenderMismatchException)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return BadRequest(new { message = result.ErrorMessage });
    }
}