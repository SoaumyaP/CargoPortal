using Groove.SP.Core.Entities.Cruise;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class CruiseOrderContactModelConfiguration : IEntityTypeConfiguration<CruiseOrderContactModel>
    {
        public void Configure(EntityTypeBuilder<CruiseOrderContactModel> builder)
        {
            builder.Property(x => x.OrganizationId).IsRequired().HasColumnType("BIGINT");
            builder.Property(x => x.OrganizationRole).IsRequired().HasColumnType("VARCHAR(50)");
            builder.Property(x => x.CompanyName).IsRequired().HasColumnType("NVARCHAR(100)");
            builder.Property(x => x.Address).HasColumnType("NVARCHAR(250)");
            builder.Property(x => x.ContactName).HasColumnType("NVARCHAR(250)");
            builder.Property(x => x.ContactNumber).HasColumnType("NVARCHAR(100)");
            builder.Property(x => x.ContactEmail).HasColumnType("NVARCHAR(100)");

            builder.HasOne(x => x.Order)
                    .WithMany(x => x.Contacts)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("CruiseOrderContacts", "cruise");
        }
    }
}
