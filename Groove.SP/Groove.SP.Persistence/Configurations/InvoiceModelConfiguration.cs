using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class InvoiceModelConfiguration : IEntityTypeConfiguration<InvoiceModel>
    {
        public void Configure(EntityTypeBuilder<InvoiceModel> builder)
        {
            builder.Property(e => e.InvoiceNo).HasColumnType("NVARCHAR(35)").IsRequired();
            builder.Property(e => e.InvoiceDate).HasColumnType("DATETIME2(7)").IsRequired();
            builder.Property(e => e.FileName).HasColumnType("NVARCHAR(512)").IsRequired();
            builder.Property(e => e.InvoiceType).HasColumnType("NVARCHAR(1)").IsRequired().HasDefaultValue('N');

            builder.Property(e => e.BillOfLadingNo).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.JobNo).HasColumnType("NVARCHAR(35)").HasMaxLength(35);
            builder.Property(e => e.BillTo).HasColumnType("NVARCHAR(250)").HasMaxLength(250);
            builder.Property(e => e.BillBy).HasColumnType("NVARCHAR(250)").HasMaxLength(250);
            builder.Property(e => e.BlobId).HasColumnType("NVARCHAR(512)").HasMaxLength(512);

            builder.Property(e => e.DateOfSubmissionToCruise).HasColumnType("DATETIME2(7)").IsRequired(false);
            builder.Property(e => e.PaymentDueDate).HasColumnType("DATETIME2(7)").IsRequired(false);

            builder.HasIndex(e => new { e.InvoiceDate, e.Id });
            builder.HasIndex(e => e.InvoiceNo).IsUnique();
        }
    }
}
