using BusX.Domain.Common;

namespace BusX.Domain.Entities;

public class Journey : BaseEntity
{
    public int FromStationId { get; set; }
    public int ToStationId { get; set; }
    public DateTime Departure { get; set; }
    public string Provider { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }

    public Station FromStation { get; set; }
    public Station ToStation { get; set; }
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
}