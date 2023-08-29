using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class PurchaseOrderContactModelConfiguration : IEntityTypeConfiguration<PurchaseOrderContactModel>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrderContactModel> builder)
        {
            builder.Property(e => e.OrganizationRole).IsRequired().HasColumnType("VARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.CompanyName).HasColumnType("NVARCHAR(100)").HasMaxLength(100);
            builder.Property(e => e.OrganizationCode).IsRequired().HasColumnType("NVARCHAR(35)").HasMaxLength(35);
            builder.Property(e => e.AddressLine1).HasColumnType("NVARCHAR(250)").HasMaxLength(250);
            builder.Property(e => e.AddressLine2).HasColumnType("NVARCHAR(250)").HasMaxLength(250);
            builder.Property(e => e.AddressLine3).HasColumnType("NVARCHAR(250)").HasMaxLength(250);
            builder.Property(e => e.AddressLine4).HasColumnType("NVARCHAR(250)").HasMaxLength(250);
            builder.Property(e => e.ContactName).HasColumnType("NVARCHAR(250)").HasMaxLength(250);
            builder.Property(e => e.Department).HasColumnType("NVARCHAR(250)").HasMaxLength(250);
            builder.Property(e => e.Name).HasColumnType("NVARCHAR(100)").HasMaxLength(100);
            builder.Property(e => e.References).HasColumnType("NVARCHAR(100)").HasMaxLength(100);
            builder.Property(e => e.ContactNumber).HasColumnType("NVARCHAR(100)").HasMaxLength(100);
            builder.Property(e => e.ContactEmail).HasColumnType("NVARCHAR(100)").HasMaxLength(100);

            builder.HasOne(e => e.PurchaseOrder)
                    .WithMany(e => e.Contacts)
                    .HasForeignKey(e => e.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.OrganizationCode);
            builder.HasIndex(e => e.OrganizationId);
        }
    }
}
