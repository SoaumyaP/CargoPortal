using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class GlobalIdMasterDialogModelConfiguration : IEntityTypeConfiguration<GlobalIdMasterDialogModel>
    {
        public void Configure(EntityTypeBuilder<GlobalIdMasterDialogModel> builder)
        {
            builder.HasKey(e => new { e.GlobalId, e.MasterDialogId });

            builder.HasOne(e => e.ReferenceEntity)
                    .WithMany(e => e.GlobalIdMasterDialogs)
                    .HasForeignKey(e => e.GlobalId)
                    .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.MasterDialog)
                    .WithMany(e => e.GlobalIdMasterDialogs)
                    .HasForeignKey(e => e.MasterDialogId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
