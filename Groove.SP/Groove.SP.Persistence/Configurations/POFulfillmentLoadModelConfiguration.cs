using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class POFulfillmentLoadModelConfiguration : IEntityTypeConfiguration<POFulfillmentLoadModel>
    {
        public void Configure(EntityTypeBuilder<POFulfillmentLoadModel> builder)
        {
            builder.Property(p => p.LoadReferenceNumber).HasColumnType("NVARCHAR(12)").HasMaxLength(12);

            builder.Property(p => p.ContainerNumber).HasColumnType("NVARCHAR(50)").HasMaxLength(50);
            builder.Property(p => p.SealNumber).HasColumnType("NVARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.SealNumber2).HasColumnType("NVARCHAR(50)").HasMaxLength(50);
            builder.Property(p => p.TotalGrossWeight).HasColumnType("DECIMAL(18, 2)");
            builder.Property(p => p.TotalNetWeight).HasColumnType("DECIMAL(18, 2)");

            builder.Property(p => p.PlannedNetWeight).HasColumnType("DECIMAL(18, 4)");
            builder.Property(p => p.PlannedVolume).HasColumnType("DECIMAL(18, 4)");
            builder.Property(p => p.SubtotalNetWeight).HasColumnType("DECIMAL(18, 4)");
            builder.Property(p => p.SubtotalGrossWeight).HasColumnType("DECIMAL(18, 4)");
            builder.Property(p => p.SubtotalVolume).HasColumnType("DECIMAL(18, 4)");

            builder.HasMany(p => p.Details)
                .WithOne(d => d.PoFulfillmentLoad)
                .HasForeignKey(d => d.POFulfillmentLoadId);
        }
    }
}