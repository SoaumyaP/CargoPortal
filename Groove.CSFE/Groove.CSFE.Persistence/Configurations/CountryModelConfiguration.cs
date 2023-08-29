using Groove.CSFE.Core.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.CSFE.Persistence.Configurations
{
    public class CountryModelConfiguration : IEntityTypeConfiguration<CountryModel>
    {
        public void Configure(EntityTypeBuilder<CountryModel> builder)
        {
            builder.Property(e => e.Code).HasColumnType("nvarchar(4)");
            builder.Property(e => e.Name).HasColumnType("nvarchar(128)");
        }
    }
}
