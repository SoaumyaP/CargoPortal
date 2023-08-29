using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.CSFE.Persistence.Configurations
{
    public class PortModelConfiguration : IEntityTypeConfiguration<PortModel>
    {
        public void Configure(EntityTypeBuilder<PortModel> builder)
        {
            builder.Property(e => e.AirportCode).HasColumnType("NVARCHAR(50)");
            builder.Property(e => e.AlternativeName).HasColumnType("NVARCHAR(512)");
            builder.Property(e => e.ChineseName).HasColumnType("NVARCHAR(512)");
            builder.Property(e => e.CountryName).HasColumnType("NVARCHAR(512)");
            builder.Property(e => e.Name).IsRequired().HasColumnType("NVARCHAR(512)");
            builder.Property(e => e.SeaportCode).HasColumnType("NVARCHAR(50)");
        }
    }
}
