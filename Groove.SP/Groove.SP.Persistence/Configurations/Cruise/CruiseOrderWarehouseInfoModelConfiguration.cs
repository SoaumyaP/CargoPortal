using Groove.SP.Core.Entities.Cruise;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class CruiseOrderModelWarehouseInfoConfiguration : IEntityTypeConfiguration<CruiseOrderWarehouseInfoModel>
    {
        public void Configure(EntityTypeBuilder<CruiseOrderWarehouseInfoModel> builder)
        {
            builder.Property(x => x.InDate).HasColumnType("DATETIME2(7)");
            builder.Property(x => x.AgentInDate).HasColumnType("DATETIME2(7)");
            builder.Property(x => x.PackageID).HasColumnType("NVARCHAR(300)");
            builder.Property(x => x.OnHold).HasColumnType("NVARCHAR(10)");
            builder.Property(x => x.OnHoldCode).HasColumnType("NVARCHAR(100)");
            builder.Property(x => x.OSD).HasColumnType("NVARCHAR(10)");
            builder.Property(x => x.OSDReason).HasColumnType("NVARCHAR(100)");
            builder.Property(x => x.Hazardous).HasColumnType("NVARCHAR(10)");
            builder.Property(x => x.MRStatus).HasColumnType("NVARCHAR(50)");
            builder.Property(x => x.GRDate).HasColumnType("DATETIME2(7)");
            builder.Property(x => x.PackingList).HasColumnType("NVARCHAR(200)");
            builder.Property(x => x.RTPDate).HasColumnType("DATETIME2(7)");
            builder.Property(x => x.PackedDate).HasColumnType("DATETIME2(7)");
            builder.Property(x => x.PLClosingDate).HasColumnType("DATETIME2(7)");
            builder.Property(x => x.RemoteDate).HasColumnType("DATETIME2(7)");
            builder.Property(x => x.RemoteStatus).HasColumnType("NVARCHAR(500)");
            builder.Property(x => x.Bonded).HasColumnType("NVARCHAR(10)");
            builder.Property(x => x.BondNo).HasColumnType("NVARCHAR(100)");
            builder.Property(x => x.Warehouse).HasColumnType("NVARCHAR(100)");

            builder.Property(x => x.ModeOfTransport).HasColumnType("NVARCHAR(50)");
            builder.Property(x => x.ContainerId).HasColumnType("NVARCHAR(50)");
            builder.Property(x => x.ContainerNumber).HasColumnType("NVARCHAR(15)");
            builder.Property(x => x.BookedToShip).HasColumnType("NVARCHAR(50)");
            builder.Property(x => x.BookingNumber).HasColumnType("NVARCHAR(50)");
            builder.Property(x => x.HAWBMAWB).HasColumnType("NVARCHAR(50)");
            builder.Property(x => x.DeliveryTicket).HasColumnType("NVARCHAR(50)");
            builder.Property(x => x.BookingCreatedDate).HasColumnType("DATETIME2(7)");
            builder.Property(x => x.DeliveryConfirmDate).HasColumnType("DATETIME2(7)");
            builder.Property(x => x.BookingRemarks).HasColumnType("NVARCHAR(300)");

            builder.Property(x => x.RefID).HasColumnType("VARCHAR(256)")
                .IsRequired().HasDefaultValue(string.Empty);
            builder.Property(x => x.Department).HasColumnType("VARCHAR(30)");
            builder.Property(x => x.POCause).HasColumnType("VARCHAR(20)");
            builder.Property(x => x.Ship).HasColumnType("VARCHAR(20)");
            builder.Property(x => x.ReqType).HasColumnType("NVARCHAR(500)");
            builder.Property(x => x.ReqType2).HasColumnType("NVARCHAR(500)");
            builder.Property(x => x.ReqType3).HasColumnType("NVARCHAR(500)");
            builder.Property(x => x.Delivered).HasColumnType("VARCHAR(5)");
            builder.Property(x => x.UNNo).HasColumnType("VARCHAR(256)");
            builder.Property(x => x.ReqApprovedDate).HasColumnType("DATETIME2(7)");
            builder.Property(x => x.ExpLineShipDate).HasColumnType("DATETIME2(7)");

            builder.Property(x => x.UOM).HasColumnType("VARCHAR(20)");
            builder.Property(x => x.KGS).HasColumnType("NUMERIC(12,4)");
            builder.Property(x => x.Dimension).HasColumnType("VARCHAR(256)");
            builder.Property(x => x.CBM).HasColumnType("NUMERIC(12,4)");

            builder.ToTable("CruiseOrderWarehouseInfos", "cruise");

            builder.HasOne(x => x.CruiseOrderItem)
                    .WithOne(x => x.Warehouse)
                    .HasForeignKey<CruiseOrderWarehouseInfoModel>(x => x.CruiseOrderItemId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}