using Groove.SP.Core.Entities;
using Groove.SP.Core.Entities.Cruise;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class CruiseOrderModelConfiguration : IEntityTypeConfiguration<CruiseOrderModel>
    {
        public void Configure(EntityTypeBuilder<CruiseOrderModel> builder)
        {
            builder.Property(x => x.ActualDeliveryDate).HasColumnType("DATETIME2(7)");
            builder.Property(x => x.ActualShipDate).HasColumnType("DATETIME2(7)");
            builder.Property(x => x.ApprovalStatus).HasColumnType("NVARCHAR(20)");
            builder.Property(x => x.ApprovedDate).HasColumnType("DATETIME2(7)");
            builder.Property(x => x.Approver).HasColumnType("NVARCHAR(100)");
            builder.Property(x => x.BudgetAccount).HasColumnType("NVARCHAR(100)");
            builder.Property(x => x.BudgetId).HasColumnType("NVARCHAR(20)");
            builder.Property(x => x.BudgetPeriod).HasColumnType("INT");
            builder.Property(x => x.BudgetYear).HasColumnType("INT");
            builder.Property(x => x.CertificateId).HasColumnType("NVARCHAR(20)");
            builder.Property(x => x.CertificateNumber).HasColumnType("NVARCHAR(20)");
            builder.Property(x => x.CreationUser).HasColumnType("NVARCHAR(100)");
            builder.Property(x => x.Delivered).HasColumnType("NVARCHAR(5)");
            builder.Property(x => x.DeliveryMeans).HasColumnType("NVARCHAR(20)");
            builder.Property(x => x.Department).HasColumnType("NVARCHAR(30)");
            builder.Property(x => x.FirstReceivingPoint).HasColumnType("NVARCHAR(50)");
            builder.Property(x => x.Invoiced).HasColumnType("DECIMAL(18,3)");
            builder.Property(x => x.MaintenanceObject).HasColumnType("NVARCHAR(100)");
            builder.Property(x => x.Maker).HasColumnType("NVARCHAR(50)");
            builder.Property(x => x.POCause).HasColumnType("NVARCHAR(20)");
            builder.Property(x => x.POId).HasColumnType("NVARCHAR(512)");
            builder.Property(x => x.POType).HasColumnType("NVARCHAR(20)");
            builder.Property(x => x.POPriority).HasColumnType("NVARCHAR(20)");
            builder.Property(x => x.RequestApprovedDate).HasColumnType("DATETIME2(7)");
            builder.Property(x => x.RequestDate).HasColumnType("DATETIME2(7)");
            builder.Property(x => x.RequestPriority).HasColumnType("NVARCHAR(20)");
            builder.Property(x => x.RequestType).HasColumnType("NVARCHAR(500)");
            builder.Property(x => x.RequestType2).HasColumnType("NVARCHAR(500)");
            builder.Property(x => x.RequestType3).HasColumnType("NVARCHAR(500)");
            builder.Property(x => x.Requestor).HasColumnType("NVARCHAR(50)");
            builder.Property(x => x.Ship).HasColumnType("NVARCHAR(20)");
            builder.Property(x => x.WithWO).HasColumnType("NVARCHAR(5)");
            builder.Property(x => x.EstimatedDeliveryDate).HasColumnType("DATETIME2(7)");
            builder.Property(x => x.PONumber).IsRequired().HasColumnType("NVARCHAR(512)");
            builder.Property(x => x.POStatus).HasColumnType("NVARCHAR(50)");
            builder.Property(x => x.POSubject).HasColumnType("NVARCHAR(100)");
            builder.Property(x => x.PODate).HasColumnType("DATETIME2(7)");

            builder.ToTable("CruiseOrders", "cruise");
        }
    }
}
