using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class ShipmentContactModelConfiguration: IEntityTypeConfiguration<ShipmentContactModel>
    {
        public void Configure(EntityTypeBuilder<ShipmentContactModel> builder)
        {
            builder.Property(e => e.OrganizationRole).IsRequired().HasColumnType("VARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.CompanyName).IsRequired().HasColumnType("NVARCHAR(100)").HasMaxLength(100);
            builder.Property(e => e.Address).HasColumnType("NVARCHAR(250)").HasMaxLength(250);
            builder.Property(e => e.ContactName).HasColumnType("NVARCHAR(250)").HasMaxLength(250);
            builder.Property(e => e.ContactNumber).HasColumnType("NVARCHAR(100)").HasMaxLength(100);
            builder.Property(e => e.ContactEmail).HasColumnType("NVARCHAR(100)").HasMaxLength(100);

            builder.HasOne(e => e.Shipment)
                    .WithMany(e => e.Contacts)
                    .HasForeignKey(e => e.ShipmentId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => e.OrganizationId);
        }
    }
}
