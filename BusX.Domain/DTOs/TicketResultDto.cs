using BusX.Domain.Entities;

namespace BusX.Domain.DTOs;

public class TicketResultDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string PnrCode { get; set; } = string.Empty;
    public Seat Data { get; set; }
}