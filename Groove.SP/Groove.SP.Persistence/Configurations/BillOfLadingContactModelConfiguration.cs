using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class BillOfLadingContactModelConfiguration : IEntityTypeConfiguration<BillOfLadingContactModel>
    {
        public void Configure(EntityTypeBuilder<BillOfLadingContactModel> builder)
        {
            builder.Property(e => e.OrganizationRole).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.CompanyName).HasColumnType("NVARCHAR(MAX)");
            builder.Property(e => e.Address).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ContactName).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ContactNumber).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ContactEmail).HasColumnType("NVARCHAR(256)").HasMaxLength(256);

            builder.HasOne(e => e.BillOfLading)
                .WithMany(e => e.Contacts)
                .HasForeignKey(e => e.BillOfLadingId);
        }
    }
}
