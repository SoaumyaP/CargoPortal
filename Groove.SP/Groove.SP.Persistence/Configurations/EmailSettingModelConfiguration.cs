using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class EmailSettingModelConfiguration : IEntityTypeConfiguration<EmailSettingModel>
    {
        public void Configure(EntityTypeBuilder<EmailSettingModel> builder)
        {
            builder.Property(e => e.SendTo).HasColumnType("NVARCHAR(MAX)");
            builder.Property(e => e.CC).HasColumnType("NVARCHAR(MAX)");
        }
    }
}
