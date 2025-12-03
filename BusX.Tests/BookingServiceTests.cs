using BusX.Domain.DTOs;
using BusX.Domain.Entities;
using BusX.Domain.Enums;
using BusX.Infrastructure.Persistence;
using BusX.Infrastructure.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
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
        var seat = new Seat
        {
            Id = 10,
            JourneyId = 1,
            No = 5,
            Status = SeatStatus.Sold, 
            Version = 1,
            GenderLock = Gender.None
        };

        context.Journeys.Add(journey);
        context.Seats.Add(seat);
        await context.SaveChangesAsync();

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var mockLogger = new Mock<ILogger<BusBookingService>>();

        var mockValidator = new Mock<IValidator<CheckoutRequestDto>>();
        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CheckoutRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult()); 

        var service = new BusBookingService(context, memoryCache, mockValidator.Object, mockLogger.Object);

        var request = new CheckoutRequestDto
        {
            JourneyId = 1,
            SeatIds = new List<int> { 10 },
            Gender = Gender.Male,
            PassengerName = "Ahmet Yılmaz",
            IdentityNumber = "12345678901",
            CreditCardNumber = "1234-5678-9012-3456"
        };

        var result = await service.BuyTicketAsync(request);

        Assert.False(result.IsSuccess, "İşlem başarısız olmalıydı çünkü koltuk satılmış.");
        Assert.NotNull(result.ErrorMessage);
    }
}