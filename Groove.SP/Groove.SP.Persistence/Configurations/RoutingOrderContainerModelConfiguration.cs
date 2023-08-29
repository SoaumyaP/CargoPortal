using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class RoutingOrderContainerModelConfiguration : IEntityTypeConfiguration<RoutingOrderContainerModel>
    {
        public void Configure(EntityTypeBuilder<RoutingOrderContainerModel> builder)
        {
            builder.Property(p => p.Volume).HasColumnType("DECIMAL(18, 4)");
        }
    }
}
