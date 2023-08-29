using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.CSFE.Persistence.Configurations
{
    public class CustomerRelationshipModelConfiguration : IEntityTypeConfiguration<CustomerRelationshipModel>
    {
        public void Configure(EntityTypeBuilder<CustomerRelationshipModel> builder)
        {
            builder.HasKey(tb => new { tb.SupplierId, tb.CustomerId });

            builder.Property(e => e.CustomerRefId).HasColumnType("NVARCHAR(20)").HasMaxLength(20);

            builder.HasOne(rt => rt.Customer)
              .WithMany()
              .HasForeignKey(rt => rt.CustomerId);
        }
    }
}
