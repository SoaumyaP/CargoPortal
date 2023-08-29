using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class POFulfillmentContactModelConfiguration : IEntityTypeConfiguration<POFulfillmentContactModel>
    {
        public void Configure(EntityTypeBuilder<POFulfillmentContactModel> builder)
        {
            builder.Property(e => e.OrganizationRole).IsRequired().HasColumnType("VARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.CompanyName).IsRequired().HasColumnType("NVARCHAR(100)").HasMaxLength(100);
            builder.Property(e => e.Address).HasColumnType("NVARCHAR(250)").HasMaxLength(250);
            builder.Property(e => e.AddressLine2).HasColumnType("NVARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.AddressLine3).HasColumnType("NVARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.AddressLine4).HasColumnType("NVARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.ContactName).HasColumnType("NVARCHAR(100)").HasMaxLength(100);
            builder.Property(e => e.ContactNumber).HasColumnType("NVARCHAR(100)").HasMaxLength(100);
            builder.Property(e => e.ContactEmail).HasColumnType("NVARCHAR(100)").HasMaxLength(100);
            builder.Property(e => e.WeChatOrWhatsApp).HasMaxLength(32);

            builder.HasIndex(e => e.OrganizationId);
        }
    }
}
