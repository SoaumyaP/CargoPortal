using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class CurrencyExchangeRateModelConfiguration : IEntityTypeConfiguration<CurrencyExchangeRateModel>
    {
        public void Configure(EntityTypeBuilder<CurrencyExchangeRateModel> builder)
        {
            builder.Property(e => e.Code).HasColumnType("nvarchar(16)").IsRequired();
            builder.Property(e => e.Name).HasColumnType("nvarchar(128)");
            builder.Property(e => e.ExchangeRate).HasColumnType("DECIMAL(18,4)").IsRequired();
            builder.Property(e => e.StartDate).HasColumnType("datetime2(7)").IsRequired();

            builder.HasIndex(e => new { e.Code, e.StartDate }).IsUnique();
        }
    }
}
