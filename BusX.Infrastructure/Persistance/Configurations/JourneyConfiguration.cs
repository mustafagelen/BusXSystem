using BusX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusX.Infrastructure.Persistence.Configurations;

public class JourneyConfiguration : IEntityTypeConfiguration<Journey>
{
    public void Configure(EntityTypeBuilder<Journey> builder)
    {
        builder.Property(j => j.BasePrice).HasPrecision(18, 2);

        builder.HasOne(j => j.FromStation)
               .WithMany()
               .HasForeignKey(j => j.FromStationId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.ToStation)
               .WithMany()
               .HasForeignKey(j => j.ToStationId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}