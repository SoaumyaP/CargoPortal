using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class POFulfillmentModelConfiguration : IEntityTypeConfiguration<POFulfillmentModel>
    {
        public void Configure(EntityTypeBuilder<POFulfillmentModel> builder)
        {
            builder.Property(p => p.Number).HasColumnType("NVARCHAR(20)").HasMaxLength(20);
            builder.Property(p => p.ShipFromName).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(p => p.ShipToName).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.ReceiptPort).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.DeliveryPort).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.AgentAssignmentMode).HasColumnType("NVARCHAR(32)").HasMaxLength(32).IsRequired().HasDefaultValue(AgentAssignmentMode.DEFAULT);
            builder.Property(e => e.FulfillmentType).HasColumnType("INT").IsRequired().HasDefaultValue(FulfillmentType.PO);
            builder.Property(e => e.OrderFulfillmentPolicy).HasColumnType("INT").IsRequired().HasDefaultValue(OrderFulfillmentPolicy.NotAllowMissingPO);
            builder.Property(e => e.DeliveryMode).HasMaxLength(256);
            builder.Property(e => e.VesselName).HasMaxLength(512);
            builder.Property(e => e.VoyageNo).HasMaxLength(512);
            builder.Property(e => e.SONo).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.ContainerNo).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.HAWBNo).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.NameofInternationalAccount).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.CompanyNo).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.ConfirmBy).HasMaxLength(256);
            builder.Property(e => e.Time).HasMaxLength(20);
            builder.Property(e => e.LoadingBay).HasMaxLength(512);
            builder.Property(e => e.EmailSubject).HasMaxLength(512);
            builder.Property(e => e.Forwarder).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.CYEmptyPickupTerminalCode).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.CFSWarehouseCode).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.CYEmptyPickupTerminalDescription).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.CFSWarehouseDescription).HasColumnType("NVARCHAR(512)").HasMaxLength(512);

            builder.HasMany(p => p.Contacts)
                .WithOne(c => c.POFulfillment)
                .HasForeignKey(c => c.POFulfillmentId);

            builder.HasMany(p => p.Orders)
                .WithOne(o => o.POFulfillment)
                .HasForeignKey(o => o.POFulfillmentId);

            builder.HasMany(p => p.ShortshipOrders)
                .WithOne(o => o.POFulfillment)
                .HasForeignKey(o => o.POFulfillmentId);

            builder.HasMany(p => p.Loads)
                .WithOne(l => l.PoFulfillment)
                .HasForeignKey(l => l.POFulfillmentId);

            builder.HasMany(p => p.CargoDetails)
                .WithOne(c => c.PoFulfillment)
                .HasForeignKey(c => c.POFulfillmentId);
        }
    }
}