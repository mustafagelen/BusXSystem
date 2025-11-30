using BusX.Domain.Common;
using BusX.Domain.Enums;

namespace BusX.Domain.Entities;

public class Ticket : BaseEntity
{
    public string Pnr { get; set; } = string.Empty;
    public int JourneyId { get; set; }
    public int SeatId { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Journey Journey { get; set; }
}