using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class OrgContactPreferenceModelConfiguration : IEntityTypeConfiguration<OrgContactPreferenceModel>
    {
        public void Configure(EntityTypeBuilder<OrgContactPreferenceModel> builder)
        {
            builder.HasIndex(e => new { e.CompanyName, e.OrganizationId }).IsUnique();

            builder.Property(e => e.CompanyName).HasColumnType("NVARCHAR(100)").HasMaxLength(100).IsRequired();
            builder.Property(e => e.Address).HasColumnType("NVARCHAR(250)").HasMaxLength(250);
            builder.Property(e => e.AddressLine2).HasColumnType("NVARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.AddressLine3).HasColumnType("NVARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.AddressLine4).HasColumnType("NVARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.ContactName).HasColumnType("NVARCHAR(100)").HasMaxLength(100);
            builder.Property(e => e.ContactNumber).HasColumnType("NVARCHAR(100)").HasMaxLength(100);
            builder.Property(e => e.ContactEmail).HasColumnType("NVARCHAR(100)").HasMaxLength(100);
            builder.Property(e => e.WeChatOrWhatsApp).HasColumnType("NVARCHAR(32)");
        }
    }
}
