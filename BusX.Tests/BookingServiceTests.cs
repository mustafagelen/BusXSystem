using BusX.Domain.DTOs;
using BusX.Domain.Entities;
using BusX.Domain.Enums;
using BusX.Infrastructure.Persistence;
using BusX.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace BusX.Tests;

public class BookingServiceTests
{
    private BusXDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<BusXDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var context = new BusXDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task BuyTicket_ShouldFail_WhenSeatIsAlreadySold()
    {
        var context = GetInMemoryDbContext();

        var journey = new Journey { Id = 1, BasePrice = 100, Provider = "TestProvider" };
        var seat = new Seat { Id = 10, JourneyId = 1, No = 5, Status = SeatStatus.Sold, Version = 1 };

        context.Journeys.Add(journey);
        context.Seats.Add(seat);
        await context.SaveChangesAsync();

        var mockCache = new Mock<IMemoryCache>();
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new BusBookingService(context, memoryCache);
        var request = new CheckoutRequestDto
        {
            JourneyId = 1,
            SeatId = 10,
            Gender = Gender.Male,
            PassengerName = "Ahmet Yılmaz"
        };

        var result = await service.BuyTicketAsync(request);

        Assert.False(result.IsSuccess, "Satılmış koltuk tekrar satılmamalıydı.");
        Assert.Contains("satılmış", result.Message);
    }
}