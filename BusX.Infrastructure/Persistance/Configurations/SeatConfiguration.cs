using BusX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusX.Infrastructure.Persistence.Configurations;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.Property(s => s.Version)
               .IsConcurrencyToken()
               .HasDefaultValue(1);

        builder.HasIndex(s => new { s.JourneyId, s.No }).IsUnique();

        builder.HasOne(s => s.Journey)
               .WithMany(j => j.Seats)
               .HasForeignKey(s => s.JourneyId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}