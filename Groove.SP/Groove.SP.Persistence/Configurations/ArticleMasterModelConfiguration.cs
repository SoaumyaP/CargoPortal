using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations;
public class ArticleMasterModelConfiguration : IEntityTypeConfiguration<ArticleMasterModel>
{
    public void Configure(EntityTypeBuilder<ArticleMasterModel> builder)
    {
        builder
            .Property(e => e.Id)
            .ValueGeneratedOnAdd();

        var creationTime = builder
            .Property(e => e.Id)
            .Metadata;
        creationTime.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        creationTime.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

        builder.HasIndex(e => e.Id)
                    .IsUnique()
                    .IsClustered(false)
                    .IncludeProperties(e => new {
                        e.CompanyCode,
                        e.CompanyType,
                        e.PONo,
                        e.ItemNo,
                        e.ShipmentNo,
                        e.POSeq,
                        e.DestCode})
                    .HasDatabaseName("IX_ArticleMaster_Id");

        builder.HasKey(e => new { e.CompanyCode, e.CompanyType, e.PONo, e.ItemNo, e.ShipmentNo, e.POSeq, e.DestCode });

        builder.Property(p => p.CompanyCode).HasColumnType("VARCHAR(10)").HasMaxLength(10).IsRequired();
        builder.Property(p => p.CompanyType).HasColumnType("VARCHAR(1)").HasMaxLength(1).IsRequired();
        builder.Property(p => p.PONo).HasColumnType("VARCHAR(40)").HasMaxLength(40).IsRequired();
        builder.Property(e => e.ItemNo).HasColumnType("VARCHAR(50)").HasMaxLength(50).IsRequired();
        builder.Property(e => e.ShipmentNo).HasColumnType("VARCHAR(5)").HasMaxLength(5).IsRequired();
        builder.Property(e => e.DestCode).HasColumnType("VARCHAR(5)").HasMaxLength(5).IsRequired().HasDefaultValue(string.Empty);
        builder.Property(e => e.OrderDetailKey).HasColumnType("VARCHAR(30)").HasMaxLength(30);
        builder.Property(e => e.CategoryCode).HasColumnType("VARCHAR(30)").HasMaxLength(30);
        builder.Property(e => e.ItemDepth).HasColumnType("NUMERIC(18, 3)");
        builder.Property(e => e.ItemHeight).HasColumnType("NUMERIC(18, 3)");
        builder.Property(e => e.ItemWidth).HasColumnType("NUMERIC(18, 3)");
        builder.Property(e => e.ItemDesc).HasColumnType("VARCHAR(256)").HasMaxLength(256);
        builder.Property(e => e.UnitWeight).HasColumnType("DECIMAL(18, 3)");
        builder.Property(e => e.CartonType).HasColumnType("VARCHAR(20)").HasMaxLength(20);
        builder.Property(e => e.AssignedSupplier).HasColumnType("VARCHAR(3999)").HasMaxLength(3999);
        builder.Property(e => e.SupplierType).HasColumnType("VARCHAR(3999)").HasMaxLength(3999);
        builder.Property(e => e.Barcode).HasColumnType("VARCHAR(3999)").HasMaxLength(3999);
        builder.Property(e => e.BarcodeType).HasColumnType("VARCHAR(3999)").HasMaxLength(3999);
        builder.Property(e => e.InnerDepth).HasColumnType("NUMERIC(18, 3)");
        builder.Property(e => e.InnerHeight).HasColumnType("NUMERIC(18, 3)");
        builder.Property(e => e.InnerWidth).HasColumnType("NUMERIC(18, 3)");
        builder.Property(e => e.InnerGrossWeight).HasColumnType("NUMERIC(18, 3)");
        builder.Property(e => e.OuterDepth).HasColumnType("NUMERIC(18, 3)");
        builder.Property(e => e.OuterHeight).HasColumnType("NUMERIC(18, 3)");
        builder.Property(e => e.OuterWidth).HasColumnType("NUMERIC(18, 3)");
        builder.Property(e => e.OuterGrossWeight).HasColumnType("DECIMAL(18, 3)");
        builder.Property(e => e.DisplaySetFlat).HasColumnType("VARCHAR(30)").HasMaxLength(30);
        builder.Property(e => e.MembersQuantity).HasColumnType("VARCHAR(256)").HasMaxLength(256);
        builder.Property(e => e.MembersItemId).HasColumnType("VARCHAR(256)").HasMaxLength(256);
        builder.Property(e => e.ItemPrice).HasColumnType("NUMERIC(18, 3)");
        builder.Property(e => e.ProcurementRule).HasColumnType("VARCHAR(10)").HasMaxLength(10);
        builder.Property(e => e.Status).HasColumnType("VARCHAR(1)").HasMaxLength(1);
        builder.Property(e => e.StyleNo).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
        builder.Property(e => e.ColourCode).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
        builder.Property(e => e.Size).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
        builder.Property(e => e.StyleName).HasColumnType("NVARCHAR(256)").HasMaxLength(256);
        builder.Property(e => e.ColourName).HasColumnType("NVARCHAR(256)").HasMaxLength(256);

        builder.Property(e => e.CreatedBy).HasColumnType("VARCHAR(10)").HasMaxLength(10);
        builder.Property(e => e.UpdatedBy).HasColumnType("VARCHAR(10)").HasMaxLength(10);
        builder.Property(e => e.CreatedOn).HasColumnType("DATETIME");
        builder.Property(e => e.UpdatedOn).HasColumnType("DATETIME");
    }
}