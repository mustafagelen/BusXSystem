using BusX.Domain.Entities;
using BusX.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusX.Infrastructure.Persistence;

public static class BusXContextSeed
{
    public static async Task SeedAsync(BusXDbContext context)
    {
        if (!await context.Stations.AnyAsync())
        {
            var stations = new List<Station>
            {
                new() { City = "İstanbul", Name = "Esenler Otogarı" },
                new() { City = "Ankara", Name = "Aşti" },
                new() { City = "İzmir", Name = "İzotaş" },
                new() { City = "Antalya", Name = "Antalya Otogarı" },
                new() { City = "Bursa", Name = "Bursa Terminali" }
            };
            context.Stations.AddRange(stations);
            await context.SaveChangesAsync();
        }

        if (await context.Journeys.AnyAsync()) return;

        var istId = (await context.Stations.FirstAsync(s => s.City == "İstanbul")).Id;
        var ankId = (await context.Stations.FirstAsync(s => s.City == "Ankara")).Id;

        var journeys = new List<Journey>();

        var today = DateTime.UtcNow.Date;

        var providers = new[] { "Lüks İstanbul", "Metro VIP", "Kamil Koç", "Pamukkale", "Varan" };
        var random = new Random();

        for (int i = 0; i < 15; i++)
        {
            int dayOffset = i / 5;
            int hour = 9 + ((i % 5) * 2);

            var departureTime = today.AddDays(dayOffset).AddHours(hour);

            journeys.Add(new Journey
            {
                FromStationId = istId,
                ToStationId = ankId,
                Departure = departureTime,
                BasePrice = random.Next(500, 900),
                Provider = providers[i % 5]
            });
        }

        context.Journeys.AddRange(journeys);
        await context.SaveChangesAsync();


        foreach (var journey in journeys)
        {

            bool isVip = journey.Provider.Contains("VIP") || journey.Provider.Contains("Lüks");
            await CreateSeatsForJourney(context, journey, isVip);
        }
    }

    private static async Task CreateSeatsForJourney(BusXDbContext context, Journey journey, bool is2plus1)
    {
        var seats = new List<Seat>();
        int seatNo = 1;
        int totalRows = 12;

        for (int row = 1; row <= totalRows; row++)
        {
            if (is2plus1)
            {
                seats.Add(CreateSeat(journey.Id, row, 1, seatNo++));
                seats.Add(CreateSeat(journey.Id, row, 3, seatNo++));
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

        var random = new Random();
        int soldCount = random.Next(2, 5);
        for (int k = 0; k < soldCount; k++)
        {
            var s = seats[random.Next(seats.Count)];
            if (s.Status == SeatStatus.Available)
            {
                s.Status = SeatStatus.Sold;
                s.GenderLock = random.Next(2) == 0 ? Gender.Female : Gender.Male;
            }
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