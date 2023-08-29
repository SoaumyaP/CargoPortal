using Groove.CSFE.Core;
using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.CSFE.Persistence.Configurations
{
    public class OrganizationModelConfiguration : IEntityTypeConfiguration<OrganizationModel>
    {
        public void Configure(EntityTypeBuilder<OrganizationModel> builder)
        {
            builder.Property(e => e.Code).HasColumnType("varchar(35)").IsRequired();
            builder.Property(e => e.Name).HasColumnType("nvarchar(250)").IsRequired();
            builder.Property(e => e.Address).HasColumnType("nvarchar(500)");
            builder.Property(e => e.AddressLine2).HasColumnType("nvarchar(50)");
            builder.Property(e => e.AddressLine3).HasColumnType("nvarchar(50)");
            builder.Property(e => e.AddressLine4).HasColumnType("nvarchar(50)");
            builder.Property(e => e.ContactEmail).HasColumnType("varchar(128)");
            builder.Property(e => e.ContactName).HasColumnType("nvarchar(256)");
            builder.Property(e => e.ContactNumber).HasColumnType("varchar(32)");
            builder.Property(e => e.WeChatOrWhatsApp).HasColumnType("varchar(32)");
            builder.Property(e => e.ParentId).HasColumnType("varchar(256)").HasDefaultValue(string.Empty);
            builder.Property(e => e.CustomerPrefix).HasColumnType("varchar(5)");
            builder.Property(e => e.LocationId).IsRequired(false);
            builder.Property(e => e.EdisonCompanyCodeId).HasColumnType("nvarchar(32)");
            builder.Property(e => e.EdisonInstanceId).HasColumnType("nvarchar(32)");
            builder.Property(e => e.AgentType).HasDefaultValue(AgentType.None);
            builder.Property(e => e.SOFormGenerationFileType).HasDefaultValue(SOFormGenerationFileType.Pdf);
            builder.Property(e => e.TaxpayerId).HasColumnType("nvarchar(50)");

            builder.HasOne(rt => rt.Location)
               .WithMany(cc => cc.Organizations)
               .HasForeignKey(rt => rt.LocationId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);

            builder.HasMany(rt => rt.CustomerRelationship)
               .WithOne(rt => rt.Supplier)
               .HasForeignKey(c => c.SupplierId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(rt => rt.EmailNotifications)
               .WithOne(rt => rt.Organization)
               .HasForeignKey(c => c.OrganizationId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(u => u.ParentId);
            builder.HasIndex(u => u.Code).IsUnique();

            builder.HasIndex(u => new { u.CreatedDate, u.Name, u.Id });
        }
    }
}
