namespace BusX.Domain.DTOs;

public class JourneyDto
{
    public int Id { get; set; }
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Price { get; set; }
    public string Provider { get; set; } = string.Empty;
}