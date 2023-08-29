using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class POFulfillmentBookingRequestModelConfiguration : IEntityTypeConfiguration<POFulfillmentBookingRequestModel>
    {
        public void Configure(EntityTypeBuilder<POFulfillmentBookingRequestModel> builder)
        {
            builder.Property(e => e.BookingReferenceNumber).HasColumnType("NVARCHAR(25)");
            builder.Property(e => e.SONumber).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.BillOfLadingHeader).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.Warehouse).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.CYEmptyPickupTerminalCode).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.CFSWarehouseCode).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.CYEmptyPickupTerminalDescription).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.CFSWarehouseDescription).HasColumnType("NVARCHAR(512)").HasMaxLength(512);

            builder.HasIndex(e => e.BookingReferenceNumber).IsUnique();
        }
    }
}