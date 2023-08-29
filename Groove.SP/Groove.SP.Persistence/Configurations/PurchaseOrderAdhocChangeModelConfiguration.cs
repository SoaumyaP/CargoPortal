using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class PurchaseOrderAdhocChangeModelConfiguration : IEntityTypeConfiguration<PurchaseOrderAdhocChangeModel>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrderAdhocChangeModel> builder)
        {
            builder.Property(e => e.JsonCurrentData).HasColumnType("NVARCHAR(MAX)");
            builder.Property(e => e.JsonNewData).HasColumnType("NVARCHAR(MAX)");
            builder.Property(e => e.Message).HasColumnType("NVARCHAR(MAX)");

            builder.Ignore(x => x.JsonCurrentData);
            builder.Ignore(x => x.JsonNewData);


            builder.HasOne(e => e.POFulfillment)
                    .WithMany(e => e.PurchaseOrderAdhocChanges)
                    .HasForeignKey(e => e.POFulfillmentId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
