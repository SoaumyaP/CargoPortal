using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class POFulfillmentCargoReceiveModelConfiguration : IEntityTypeConfiguration<POFulfillmentCargoReceiveModel>
    {
        public void Configure(EntityTypeBuilder<POFulfillmentCargoReceiveModel> builder)
        {
            builder.Property(p => p.CRNo).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(p => p.PlantNo).IsRequired().HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(p => p.HouseNo).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
        }
    }
}
