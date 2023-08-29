using Groove.CSFE.Core.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.CSFE.Persistence.Configurations
{
    public class CurrencyModelConfiguration : IEntityTypeConfiguration<CurrencyModel>
    {
        public void Configure(EntityTypeBuilder<CurrencyModel> builder)
        {
            builder.Property(e => e.Code).HasColumnType("nvarchar(16)");
            builder.Property(e => e.Name).HasColumnType("nvarchar(128)");
            builder.Property(e => e.Symbol).HasColumnType("nvarchar(8)");
            builder.Property(e => e.ExchangeRate).HasColumnType("DECIMAL(18, 4)");
            builder.Property(e => e.CurrencyPrecision).HasColumnType("tinyint");
            builder.Property(e => e.Status).HasColumnType("tinyint");
        }
    }
}
