using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class FreightSchedulerChangeLogModelConfiguration : IEntityTypeConfiguration<FreightSchedulerChangeLogModel>
    {
        public void Configure(EntityTypeBuilder<FreightSchedulerChangeLogModel> builder)
        {
            builder.HasOne(c => c.FreightScheduler).WithMany(c => c.ChangeLogs).HasForeignKey(c => c.ScheduleId).OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("FreightSchedulerChangeLogs");

        }
    }
}
