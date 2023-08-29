using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class RoutingOrderContactModelConfiguration : IEntityTypeConfiguration<RoutingOrderContactModel>
    {
        public void Configure(EntityTypeBuilder<RoutingOrderContactModel> builder)
        {
            builder.Property(e => e.OrganizationRole).IsRequired().HasColumnType("VARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.OrganizationCode).IsRequired().HasColumnType("VARCHAR(35)").HasMaxLength(35);
            builder.Property(e => e.CompanyName).IsRequired().HasColumnType("NVARCHAR(100)").HasMaxLength(100);
            builder.Property(e => e.AddressLine1).HasColumnType("NVARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.AddressLine2).HasColumnType("NVARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.AddressLine3).HasColumnType("NVARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.AddressLine4).HasColumnType("NVARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.ContactName).HasColumnType("NVARCHAR(30)").HasMaxLength(30);
            builder.Property(e => e.ContactNumber).HasColumnType("NVARCHAR(30)").HasMaxLength(30);
            builder.Property(e => e.ContactEmail).HasColumnType("NVARCHAR(100)").HasMaxLength(100);

            builder.HasIndex(e => e.OrganizationId);
        }
    }
}
