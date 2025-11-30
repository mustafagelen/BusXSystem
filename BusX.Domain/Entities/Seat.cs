using BusX.Domain.Common;
using BusX.Domain.Enums;

namespace BusX.Domain.Entities;

public class Seat : BaseEntity
{
    public int JourneyId { get; set; }
    public int Floor { get; set; } = 1;
    public int Row { get; set; }
    public int Col { get; set; }
    public int No { get; set; } 

    public SeatStatus Status { get; set; } = SeatStatus.Available;
    public Gender GenderLock { get; set; } = Gender.None; 

    public uint Version { get; set; }
    public Journey Journey { get; set; }
}