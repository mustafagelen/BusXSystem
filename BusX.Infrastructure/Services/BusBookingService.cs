using BusX.Domain.Common;
using BusX.Domain.DTOs;
using BusX.Domain.Entities;
using BusX.Domain.Exceptions;
using BusX.Domain.Interfaces;
using BusX.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BusX.Infrastructure.Services;

public class BusBookingService : IBusBookingService
{
    private readonly BusXDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly IValidator<CheckoutRequestDto> _validator;
    private readonly ILogger<BusBookingService> _logger;

    public BusBookingService(
        BusXDbContext context,
        IMemoryCache cache,
        IValidator<CheckoutRequestDto> validator,
        ILogger<BusBookingService> logger)
    {
        _context = context;
        _cache = cache;
        _validator = validator;
        _logger = logger;
    }
    public async Task<List<JourneyDto>> SearchJourneysAsync(int fromId, int toId, DateTime date)
    {
        string cacheKey = $"search_{fromId}_{toId}_{date:yyyyMMdd}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);

            var now = DateTime.UtcNow;

            return await _context.Journeys
                .AsNoTracking()
                .Where(j =>
                    j.FromStationId == fromId &&
                    j.ToStationId == toId &&
                    j.Departure.Date == date.Date &&
                    j.Departure > now
                )
                .Include(j => j.FromStation)
                .Include(j => j.ToStation)
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

    public async Task<List<Station>> GetStationsAsync()
    {
        return await _context.Stations.AsNoTracking().ToListAsync();
    }
    public async Task<Result<TicketResultDto>> BuyTicketAsync(CheckoutRequestDto request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Invalid checkout request: {Errors}", errors);
            return Result<TicketResultDto>.Failure(errors);
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var seats = await _context.Seats
                .Where(s => request.SeatIds.Contains(s.Id) && s.JourneyId == request.JourneyId)
                .ToDictionaryAsync(s => s.Id, s => s);

            var invalidSeats = request.SeatIds.Where(id => !seats.ContainsKey(id)).ToList();
            if (invalidSeats.Any())
            {
                _logger.LogWarning("Invalid seat IDs: {SeatIds}", string.Join(",", invalidSeats));
                return Result<TicketResultDto>.Failure($"{string.Join(", ", invalidSeats)} ID'li koltuk(lar) bulunamadı.");
            }

            var unavailableSeats = seats.Values.Where(s => s.Status == Domain.Enums.SeatStatus.Sold).ToList();
            if (unavailableSeats.Any())
            {
                var seatNos = string.Join(", ", unavailableSeats.Select(s => s.No));
                throw new SeatUnavailableException($"Koltuk(lar) {seatNos} maalesef az önce satıldı.");
            }

            var genderMismatches = seats.Values
                .Where(s => s.GenderLock != Domain.Enums.Gender.None && s.GenderLock != request.Gender)
                .ToList();

            if (genderMismatches.Any())
            {
                var seatNos = string.Join(", ", genderMismatches.Select(s => s.No));
                throw new GenderMismatchException($"Koltuk(lar) {seatNos} seçtiğiniz cinsiyete ({request.Gender}) uygun değil.");
            }

            string pnr = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            var ticketsToAdd = new List<Ticket>();

            foreach (var seat in seats.Values)
            {
                seat.Status = Domain.Enums.SeatStatus.Sold;
                seat.GenderLock = request.Gender;

                ticketsToAdd.Add(new Ticket
                {
                    JourneyId = request.JourneyId,
                    SeatId = seat.Id,
                    PassengerName = request.PassengerName,
                    Gender = request.Gender,
                    Pnr = pnr,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (new Random().Next(10) == 0)
            {
                throw new PaymentFailedException("Ödeme alınamadı (Yetersiz Bakiye/Banka Hatası).");
            }

            _context.Tickets.AddRange(ticketsToAdd);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Tickets purchased successfully for PNR: {Pnr}", pnr);

            return Result<TicketResultDto>.Success(new TicketResultDto
            {
                IsSuccess = true,
                Message = "İşlem Başarılı",
                PnrCode = pnr
            });
        }
        catch (SeatUnavailableException ex)
        {
            await transaction.RollbackAsync();
            return Result<TicketResultDto>.Failure(ex.Message, ex);
        }
        catch (GenderMismatchException ex)
        {
            await transaction.RollbackAsync();
            return Result<TicketResultDto>.Failure(ex.Message, ex);
        }
        catch (PaymentFailedException ex)
        {
            await transaction.RollbackAsync();
            return Result<TicketResultDto>.Failure(ex.Message, ex);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            return Result<TicketResultDto>.Failure("ÇAKIŞMA: Koltuklardan biri işlem sırasında başkası tarafından alındı.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Unexpected error");
            return Result<TicketResultDto>.Failure("Sistem hatası oluştu.");
        }
    }
}