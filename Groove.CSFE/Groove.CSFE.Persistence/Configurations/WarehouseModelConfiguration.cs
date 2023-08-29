using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.CSFE.Persistence.Configurations
{
    public class WarehouseModelConfiguration : IEntityTypeConfiguration<WarehouseModel>
    {
        public void Configure(EntityTypeBuilder<WarehouseModel> builder)
        {
            builder.Property(e => e.WarehouseCode).IsRequired().HasColumnType("nvarchar(128)");
            builder.Property(e => e.WarehouseName).HasColumnType("nvarchar(128)");
            builder.Property(e => e.Address).HasColumnType("nvarchar(256)");


            builder.HasOne(w => w.Location)
                .WithMany(l => l.Warehouses)
                .HasForeignKey(w => w.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
