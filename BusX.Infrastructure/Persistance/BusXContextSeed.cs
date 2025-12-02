using BusX.Domain.Entities;
using BusX.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusX.Infrastructure.Persistence;

public static class BusXContextSeed
{
    public static async Task SeedAsync(BusXDbContext context)
    {
        if (await context.Stations.AnyAsync()) return;

        var stations = new List<Station>
        {
            new() { City = "İstanbul", Name = "Esenler Otogarı" },
            new() { City = "Ankara", Name = "Aşti" },
            new() { City = "İzmir", Name = "İzotaş" },
            new() { City = "Antalya", Name = "Antalya Otogarı" }
        };
        context.Stations.AddRange(stations);
        await context.SaveChangesAsync();

        var istId = stations.First(s => s.City == "İstanbul").Id;
        var ankId = stations.First(s => s.City == "Ankara").Id;
        var izmId = stations.First(s => s.City == "İzmir").Id;
        var antId = stations.First(s => s.City == "Antalya").Id;

        var journeys = new List<Journey>
        {
            new()
            {
                FromStationId = istId,
                ToStationId = ankId,
                Departure = DateTime.UtcNow.AddDays(14).AddHours(10),
                BasePrice = 650,
                Provider = "Ist Turizm"
            },

            new()
            {
                FromStationId = istId,
                ToStationId = ankId,
                Departure = DateTime.UtcNow.AddDays(14).AddHours(14),
                BasePrice = 750,
                Provider = "Ankara Yol Turizm"
            },

            new()
            {
                FromStationId = istId,
                ToStationId = ankId,
                Departure = DateTime.UtcNow.AddDays(14).AddHours(14),
                BasePrice = 850,
                Provider = "Atlas Turizm"
            }
        };
        context.Journeys.AddRange(journeys);
        await context.SaveChangesAsync();

        foreach (var journey in journeys)
        {
            await CreateSeatsForJourney(context, journey);
        }
    }

    private static async Task CreateSeatsForJourney(BusXDbContext context, Journey journey)
    {
        var seats = new List<Seat>();
        bool is2plus1 = journey.Provider == "ProviderA";
        int seatNo = 1;
        int totalRows = 12;

        for (int row = 1; row <= totalRows; row++)
        {
            if (is2plus1)
            {
                seats.Add(CreateSeat(journey.Id, row, 1, seatNo++));
                seats.Add(CreateSeat(journey.Id, row, 2, seatNo++));
                seats.Add(CreateSeat(journey.Id, row, 4, seatNo++));
            }
            else
            {
                seats.Add(CreateSeat(journey.Id, row, 1, seatNo++));
                seats.Add(CreateSeat(journey.Id, row, 2, seatNo++));
                seats.Add(CreateSeat(journey.Id, row, 4, seatNo++));
                seats.Add(CreateSeat(journey.Id, row, 5, seatNo++));
            }
        }

        if (seats.Count > 5)
        {
            seats[0].Status = SeatStatus.Sold;
            seats[1].GenderLock = Gender.Female;
            seats[1].Status = SeatStatus.Sold;
        }

        context.Seats.AddRange(seats);
        await context.SaveChangesAsync();
    }

    private static Seat CreateSeat(int journeyId, int row, int col, int no)
    {
        return new Seat
        {
            JourneyId = journeyId,
            Row = row,
            Col = col,
            No = no,
            Status = SeatStatus.Available,
            GenderLock = Gender.None,
            Version = 1
        };
    }
}