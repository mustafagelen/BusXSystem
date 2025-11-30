using BusX.Domain.Enums;

namespace BusX.Domain.DTOs;

public class CheckoutRequestDto
{
    public int JourneyId { get; set; }
    public int SeatId { get; set; } 
    public string PassengerName { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty; 
    public Gender Gender { get; set; } 

    public string CreditCardNumber { get; set; } = string.Empty;
}