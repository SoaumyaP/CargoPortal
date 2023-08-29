using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.CSFE.Persistence.Configurations
{
    public class WarehouseLocationModelConfiguration : IEntityTypeConfiguration<WarehouseLocationModel>
    {
        public void Configure(EntityTypeBuilder<WarehouseLocationModel> builder)
        {
            builder.Property(e => e.Code).IsRequired().HasColumnType("nvarchar(128)");
            builder.Property(e => e.Name).IsRequired().HasColumnType("nvarchar(128)");
            builder.Property(e => e.AddressLine1).IsRequired().HasColumnType("nvarchar(256)");
            builder.Property(e => e.AddressLine2).HasColumnType("nvarchar(256)");
            builder.Property(e => e.AddressLine3).HasColumnType("nvarchar(256)");
            builder.Property(e => e.AddressLine4).HasColumnType("nvarchar(256)");
            builder.Property(e => e.ContactPerson).HasColumnType("nvarchar(256)");
            builder.Property(e => e.ContactPhone).HasColumnType("nvarchar(32)");
            builder.Property(e => e.ContactEmail).HasColumnType("nvarchar(128)");
            builder.Property(e => e.WorkingHours).HasColumnType("nvarchar(512)");
            builder.Property(e => e.Remarks).HasColumnType("nvarchar(512)");


            builder.HasOne(w => w.Location)
                .WithMany(l => l.WarehouseLocations)
                .HasForeignKey(w => w.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(w => w.Organization)
                .WithMany(o => o.Warehouses)
                .HasForeignKey(w => w.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}