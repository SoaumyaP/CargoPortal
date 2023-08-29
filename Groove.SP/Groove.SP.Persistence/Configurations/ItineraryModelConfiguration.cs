using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Groove.SP.Persistence.Configurations
{
    public class ItineraryModelConfiguration : IEntityTypeConfiguration<ItineraryModel>
    {
        public void Configure(EntityTypeBuilder<ItineraryModel> builder)
        {
            builder.Property(e => e.ModeOfTransport).IsRequired().HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.CarrierName).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.SCAC).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.AirlineCode).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.VesselFlight).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.VesselName).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.Voyage).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.FlightNumber).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.LoadingPort).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.DischargePort).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ETADate).IsRequired();
            builder.Property(e => e.ETDDate).IsRequired();
            builder.Property(e => e.Status).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.RoadFreightRef).HasColumnType("NVARCHAR(512)");

            builder
                .HasOne(c => c.FreightScheduler)
                .WithMany(c => c.Itineraries)
                .HasForeignKey(c => c.ScheduleId);

            // case-insensitive comparison on database
            builder.HasQueryFilter(e => string.IsNullOrWhiteSpace(e.Status) || e.Status != "Inactive");
        }
    }
}
