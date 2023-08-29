using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class UserProfileModelConfiguration : IEntityTypeConfiguration<UserProfileModel>
    {
        public void Configure(EntityTypeBuilder<UserProfileModel> builder)
        {
            builder.HasAlternateKey(x => x.Username);
            builder.HasAlternateKey(x => x.AccountNumber);
            builder.Property(e => e.Username).IsRequired().HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.Email).IsRequired().HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.AccountNumber).IsRequired().HasColumnType("NVARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.OrganizationCode).HasColumnType("NVARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.Phone).HasColumnType("VARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.Title).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.Department).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.ProfilePicture).HasColumnType("NVARCHAR(MAX)");
            builder.Property(e => e.OPContactEmail).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.TaxpayerId).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.OPLocationName).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.CompanyWeChatOrWhatsApp).HasColumnType("VARCHAR(32)").HasMaxLength(32);
        }
    }
}
