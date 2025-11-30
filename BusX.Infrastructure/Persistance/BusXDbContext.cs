using System.Reflection;
using BusX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusX.Infrastructure.Persistence;

public class BusXDbContext : DbContext
{
    public BusXDbContext(DbContextOptions<BusXDbContext> options) : base(options)
    {
    }

    public DbSet<Station> Stations { get; set; }
    public DbSet<Journey> Journeys { get; set; }
    public DbSet<Seat> Seats { get; set; }
    public DbSet<Ticket> Tickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}