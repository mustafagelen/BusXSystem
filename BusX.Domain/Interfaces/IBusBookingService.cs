using BusX.Domain.Common;
using BusX.Domain.DTOs;
using BusX.Domain.Entities;

namespace BusX.Domain.Interfaces;

public interface IBusBookingService
{
    Task<List<JourneyDto>> SearchJourneysAsync(int fromId, int toId, DateTime date);
    Task<JourneyDetailDto?> GetJourneyDetailsAsync(int journeyId);
    Task<Result<TicketResultDto>> BuyTicketAsync(CheckoutRequestDto request);
    Task<List<Station>> GetStationsAsync();
}