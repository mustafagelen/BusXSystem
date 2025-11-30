using BusX.Domain.DTOs;
using BusX.Domain.Interfaces;
using BusX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BusX.Infrastructure.Services;

public class BusBookingService : IBusBookingService
{
    private readonly BusXDbContext _context;
    private readonly IMemoryCache _cache; 

    public BusBookingService(BusXDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<JourneyDto>> SearchJourneysAsync(string from, string to)
    {
        string cacheKey = $"search_{from.ToLower()}_{to.ToLower()}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
            var fromStation = await _context.Stations
                .FirstOrDefaultAsync(s => s.City.ToLower() == from.ToLower());
            var toStation = await _context.Stations
                .FirstOrDefaultAsync(s => s.City.ToLower() == to.ToLower());

            if (fromStation == null || toStation == null)
                return new List<JourneyDto>();
            return await _context.Journeys
                .AsNoTracking()
                .Where(j => j.FromStationId == fromStation.Id &&
                            j.ToStationId == toStation.Id &&
                            j.Departure > DateTime.UtcNow) 
                .Select(j => new JourneyDto
                {
                    Id = j.Id,
                    From = j.FromStation.City,
                    To = j.ToStation.City,
                    Date = j.Departure,
                    Price = j.BasePrice,
                    Provider = j.Provider
                })
                .ToListAsync();
        }) ?? new List<JourneyDto>();
    }

    public async Task<JourneyDetailDto?> GetJourneyDetailsAsync(int journeyId)
    {
        var journey = await _context.Journeys
            .AsNoTracking()
            .Include(j => j.Seats)
            .Include(j => j.FromStation)
            .Include(j => j.ToStation)
            .FirstOrDefaultAsync(j => j.Id == journeyId);

        if (journey == null) return null;

        return new JourneyDetailDto
        {
            JourneyId = journey.Id,
            RouteName = $"{journey.FromStation.City} > {journey.ToStation.City}",
            Seats = journey.Seats.Select(s => new SeatDto
            {
                Id = s.Id,
                No = s.No,
                Row = s.Row,
                Col = s.Col,
                Status = (int)s.Status,
                Gender = (int)s.GenderLock,
                Price = journey.BasePrice 
            }).OrderBy(s => s.No).ToList()
        };
    }

    public async Task<TicketResultDto> BuyTicketAsync(CheckoutRequestDto request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var seat = await _context.Seats
                .Include(s => s.Journey)
                .FirstOrDefaultAsync(s => s.Id == request.SeatId && s.JourneyId == request.JourneyId);

            if (seat == null)
                return new TicketResultDto { IsSuccess = false, Message = "Koltuk bulunamadı." };

            if (seat.Status == Domain.Enums.SeatStatus.Sold)
                return new TicketResultDto { IsSuccess = false, Message = "Bu koltuk maalesef satılmış." };

            if (seat.GenderLock != Domain.Enums.Gender.None && seat.GenderLock != request.Gender)
                return new TicketResultDto { IsSuccess = false, Message = "Cinsiyet kuralı hatası." };

            if (new Random().Next(10) == 0)
                return new TicketResultDto { IsSuccess = false, Message = "Ödeme alınamadı (Yetersiz Bakiye)." };

            seat.Status = Domain.Enums.SeatStatus.Sold;
            seat.GenderLock = request.Gender;

            seat.Version++;

            var ticket = new Domain.Entities.Ticket
            {
                JourneyId = request.JourneyId,
                SeatId = request.SeatId,
                PassengerName = request.PassengerName,
                Gender = request.Gender,
                Pnr = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new TicketResultDto
            {
                IsSuccess = true,
                Message = "İşlem Başarılı",
                PnrCode = ticket.Pnr
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            return new TicketResultDto { IsSuccess = false, Message = "ÇAKIŞMA: Bu koltuk başkası tarafından alındı." };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            var realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return new TicketResultDto { IsSuccess = false, Message = "Sistem hatası: " + realError };
        }
    }
}