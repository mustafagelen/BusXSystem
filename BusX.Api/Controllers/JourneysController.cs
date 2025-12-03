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

    [HttpGet("stations")]
    public async Task<IActionResult> GetStations()
    {
        var stations = await _service.GetStationsAsync();
        return Ok(stations);
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] int fromId, [FromQuery] int toId, [FromQuery] DateTime date)
    {
        var results = await _service.SearchJourneysAsync(fromId, toId, date);

        if (results == null || !results.Any())
        {
            return NotFound(new { message = "Uygun sefer bulunamadı" });
        }

        return Ok(results);
    }


    [HttpGet("{id}/seats")]
    public async Task<IActionResult> GetSeats(int id)
    {
        var journeyDetail = await _service.GetJourneyDetailsAsync(id);

        if (journeyDetail == null)
            return NotFound("Aradığınız sefer bulunamadı.");
     
        return Ok(journeyDetail);
    }
}