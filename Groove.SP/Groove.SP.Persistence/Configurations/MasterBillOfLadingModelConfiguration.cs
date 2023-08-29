using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class MasterBillOfLadingModelConfiguration : IEntityTypeConfiguration<MasterBillOfLadingModel>
    {
        public void Configure(EntityTypeBuilder<MasterBillOfLadingModel> builder)
        {
            builder.Property(e => e.CarrierContractNo).HasColumnType("VARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.MasterBillOfLadingNo).IsRequired().HasColumnType("VARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.ExecutionAgentId).IsRequired(false).HasColumnType("BIGINT");
            builder.Property(e => e.MasterBillOfLadingType).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ModeOfTransport).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.Movement).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.CarrierName).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.SCAC).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.AirlineCode).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.VesselFlight).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.Vessel).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.Voyage).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.FlightNo).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.PlaceOfReceipt).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.PortOfLoading).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.PortOfDischarge).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.PlaceOfDelivery).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.PlaceOfIssue).HasColumnType("NVARCHAR(512)").HasMaxLength(512);

            // Manually load via [CarrierContractNo] as current data not good enough to create foreign key
            builder.Ignore(e => e.ContractMaster);

            builder.HasIndex(e => new { e.OnBoardDate, e.Id });
            builder.HasIndex(e => e.MasterBillOfLadingNo).IsUnique();
        }
    }
}
