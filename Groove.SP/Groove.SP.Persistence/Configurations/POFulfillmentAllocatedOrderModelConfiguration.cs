using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class POFulfillmentAllocatedOrderModelConfiguration : IEntityTypeConfiguration<POFulfillmentAllocatedOrderModel>
    {
        public void Configure(EntityTypeBuilder<POFulfillmentAllocatedOrderModel> builder)
        {
            builder.Property(p => p.Volume).HasColumnType("DECIMAL(18, 4)");
        }
    }
}