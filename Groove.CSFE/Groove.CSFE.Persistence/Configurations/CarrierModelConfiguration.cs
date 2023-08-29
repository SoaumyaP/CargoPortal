using Groove.CSFE.Core;
using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.CSFE.Persistence.Configurations
{
    public class CarrierModelConfiguration : IEntityTypeConfiguration<CarrierModel>
    {
        public void Configure(EntityTypeBuilder<CarrierModel> builder)
        {
            builder.Property(e => e.Name).HasColumnType("NVARCHAR(512)");
            builder.Property(e => e.CarrierCode).HasColumnType("NVARCHAR(128)");
            builder.Property(e => e.ModeOfTransport).HasColumnType("NVARCHAR(128)");
            builder.Property(e => e.Status).HasColumnType("tinyint").HasDefaultValue(CarrierStatus.Active);
        }
    }
}
