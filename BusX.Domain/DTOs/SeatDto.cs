namespace BusX.Domain.DTOs;

public class SeatDto
{
    public int Id { get; set; }
    public int No { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public int Status { get; set; }
    public int Gender { get; set; }
    public decimal Price { get; set; }
}