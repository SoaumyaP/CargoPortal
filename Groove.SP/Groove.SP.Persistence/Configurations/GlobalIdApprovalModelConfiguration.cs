using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class GlobalIdApprovalModelConfiguration : IEntityTypeConfiguration<GlobalIdApprovalModel>
    {
        public void Configure(EntityTypeBuilder<GlobalIdApprovalModel> builder)
        {
            builder.HasKey(e => new { e.GlobalId, e.ApprovalId });

            builder.HasOne(e => e.ReferenceEntity)
                    .WithMany(e => e.GlobalIdApprovals)
                    .HasForeignKey(e => e.GlobalId)
                    .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.Approval)
                    .WithMany(e => e.GlobalIdApprovals)
                    .HasForeignKey(e => e.ApprovalId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
