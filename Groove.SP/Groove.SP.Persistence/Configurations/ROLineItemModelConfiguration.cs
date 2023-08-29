using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class ROLineItemModelConfiguration : IEntityTypeConfiguration<ROLineItemModel>
    {
        public void Configure(EntityTypeBuilder<ROLineItemModel> builder)
        {
            builder.Property(p => p.Volume).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.PONo).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ItemNo).IsRequired().HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.DescriptionOfGoods).IsRequired().HasColumnType("NVARCHAR(MAX)");
            builder.Property(e => e.CountryCodeOfOrigin).HasColumnType("NVARCHAR(4)");
        }
    }
}
