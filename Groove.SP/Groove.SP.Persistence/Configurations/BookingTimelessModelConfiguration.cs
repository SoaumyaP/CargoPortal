using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class BookingTimelessModelConfiguration : IEntityTypeConfiguration<BookingTimelessModel>
    {
        public void Configure(EntityTypeBuilder<BookingTimelessModel> builder)
        {
            builder.Property(b => b.DateForComparison).HasDefaultValue(DateForComparison.ExpectedShipDate);
        }
    }
}