using Groove.SP.Core.Entities.Mobile;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations.Mobile
{
    public class MobileApplicationModelConfiguration : IEntityTypeConfiguration<MobileApplicationModel>
    {
        public void Configure(EntityTypeBuilder<MobileApplicationModel> builder)
        {
            builder.Property(x => x.Version).IsRequired().HasMaxLength(52);
            builder.Property(x => x.PublishedDate).IsRequired();
            builder.Property(x => x.IsDiscontinued).HasDefaultValue(false);
            builder.Property(x => x.PackageName).IsRequired().HasMaxLength(128);
            builder.Property(x => x.PackageUrl).IsRequired().HasMaxLength(512);

            builder.ToTable("Applications", "mobile");

            builder.HasIndex(e => e.Version).IsUnique().IncludeProperties("IsDiscontinued");
            builder.HasIndex(x => x.PublishedDate).IncludeProperties("Version", "PackageUrl");

        }
    }
}
