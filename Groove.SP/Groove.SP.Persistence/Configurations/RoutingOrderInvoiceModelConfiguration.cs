using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class RoutingOrderInvoiceModelConfiguration : IEntityTypeConfiguration<RoutingOrderInvoiceModel>
    {
        public void Configure(EntityTypeBuilder<RoutingOrderInvoiceModel> builder)
        {
            builder.Property(e => e.InvoiceType).HasColumnType("NVARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.InvoiceNumber).HasColumnType("VARCHAR(35)").HasMaxLength(35);
        }
    }
}