using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.CSFE.Persistence.Configurations
{
    public class VesselModelConfiguration : IEntityTypeConfiguration<VesselModel>
    {
        public void Configure(EntityTypeBuilder<VesselModel> builder)
        {
            builder.Property(e => e.Code).HasColumnType("NVARCHAR(64)");
            builder.Property(e => e.Name).HasColumnType("NVARCHAR(512)").IsRequired();
            builder.Property(e => e.IsRealVessel).HasColumnType("BIT").IsRequired();
        }
    }
}
