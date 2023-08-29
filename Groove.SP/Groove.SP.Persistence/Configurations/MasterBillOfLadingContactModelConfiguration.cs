using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class MasterBillOfLadingContactModelConfiguration : IEntityTypeConfiguration<MasterBillOfLadingContactModel>
    {
        public void Configure(EntityTypeBuilder<MasterBillOfLadingContactModel> builder)
        {
            builder.Property(e => e.OrganizationRole).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.CompanyName).HasColumnType("NVARCHAR(MAX)");
            builder.Property(e => e.Address).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ContactName).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ContactNumber).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ContactEmail).HasColumnType("NVARCHAR(256)").HasMaxLength(256);

            builder.HasOne(e => e.MasterBillOfLading)
                .WithMany(e => e.Contacts)
                .HasForeignKey(e => e.MasterBillOfLadingId);
        }
    }
}
