using BusX.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BusX.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JourneysController : ControllerBase
{
    private readonly IBusBookingService _service;

    public JourneysController(IBusBookingService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string from, [FromQuery] string to)
    {
        if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
        {
            return BadRequest("Lütfen kalkış ve varış şehirlerini giriniz.");
        }

        var results = await _service.SearchJourneysAsync(from, to);
        return Ok(results);
    }

    [HttpGet("{id}/seats")]
    public async Task<IActionResult> GetSeats(int id)
    {
        var journeyDetail = await _service.GetJourneyDetailsAsync(id);

        if (journeyDetail == null)
        {
            return NotFound("Aradığınız sefer bulunamadı.");
        }

        return Ok(journeyDetail);
    }
}