using BusX.Domain.DTOs;

namespace BusX.Domain.Interfaces;

public interface IBusBookingService
{
    Task<List<JourneyDto>> SearchJourneysAsync(string from, string to);

    Task<JourneyDetailDto?> GetJourneyDetailsAsync(int journeyId);

    Task<TicketResultDto> BuyTicketAsync(CheckoutRequestDto request);

}