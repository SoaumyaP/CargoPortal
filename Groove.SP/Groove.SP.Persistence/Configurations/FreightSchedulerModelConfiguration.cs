using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class FreightSchedulerModelConfiguration : IEntityTypeConfiguration<FreightSchedulerModel>
    {
        public void Configure(EntityTypeBuilder<FreightSchedulerModel> builder)
        {
            builder.Property(c => c.ModeOfTransport).HasColumnType("NVARCHAR(128)").IsRequired();
            builder.Property(c => c.CarrierCode).HasColumnType("NVARCHAR(512)").IsRequired();
            builder.Property(c => c.CarrierName).HasColumnType("NVARCHAR(512)").IsRequired();

            builder.Property(c => c.VesselName).HasColumnType("NVARCHAR(512)");
            builder.Property(c => c.Voyage).HasColumnType("NVARCHAR(512)");
            builder.Property(c => c.MAWB).HasColumnType("CHAR(11)");
            builder.Property(c => c.FlightNumber).HasColumnType("NVARCHAR(512)");

            builder.Property(c => c.LocationFromCode).HasColumnType("NVARCHAR(128)").IsRequired();
            builder.Property(c => c.LocationFromName).HasColumnType("NVARCHAR(512)").IsRequired();
            builder.Property(c => c.LocationToCode).HasColumnType("NVARCHAR(128)").IsRequired();
            builder.Property(c => c.LocationToName).HasColumnType("NVARCHAR(512)").IsRequired();
            builder.Property(c => c.ETDDate).HasColumnType("DATETIME2(7)");
            builder.Property(c => c.ETADate).HasColumnType("DATETIME2(7)");
            builder.Property(c => c.ATDDate).HasColumnType("DATETIME2(7)");
            builder.Property(c => c.ATADate).HasColumnType("DATETIME2(7)");
            builder.Property(c => c.IsAllowExternalUpdate).HasDefaultValue(true).ValueGeneratedNever();

            builder.ToTable("FreightSchedulers");
        }
    }
}
