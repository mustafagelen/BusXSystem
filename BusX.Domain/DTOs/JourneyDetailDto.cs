namespace BusX.Domain.DTOs;

public class JourneyDetailDto
{
    public int JourneyId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public List<SeatDto> Seats { get; set; } = new();
}