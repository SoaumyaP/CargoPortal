using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class BuyerApprovalModelConfiguration : IEntityTypeConfiguration<BuyerApprovalModel>
    {
        public void Configure(EntityTypeBuilder<BuyerApprovalModel> builder)
        {
            builder.Property(e => e.Reference).HasColumnType("VARCHAR(13)").HasMaxLength(12);
        }
    }
}