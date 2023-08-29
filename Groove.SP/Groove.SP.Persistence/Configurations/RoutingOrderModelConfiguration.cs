using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class RoutingOrderModelConfiguration : IEntityTypeConfiguration<RoutingOrderModel>
    {
        public void Configure(EntityTypeBuilder<RoutingOrderModel> builder)
        {
            builder.Property(p => p.RoutingOrderNumber).IsRequired().HasColumnType("VARCHAR(20)").HasMaxLength(20);
            builder.Property(e => e.VesselName).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.VoyageNo).HasColumnType("NVARCHAR(512)").HasMaxLength(512);

            builder.HasMany(p => p.Contacts)
                .WithOne(c => c.RoutingOrder)
                .HasForeignKey(c => c.RoutingOrderId);

            builder.HasMany(p => p.LineItems)
                .WithOne(o => o.RoutingOrder)
                .HasForeignKey(o => o.RoutingOrderId);

            builder.HasMany(p => p.Containers)
                .WithOne(o => o.RoutingOrder)
                .HasForeignKey(o => o.RoutingOrderId);

            builder.HasMany(p => p.Invoices)
                .WithOne(l => l.RoutingOrder)
                .HasForeignKey(l => l.RoutingOrderId);

            builder.HasIndex(e => e.RoutingOrderNumber).IsUnique();
        }
    }
}
