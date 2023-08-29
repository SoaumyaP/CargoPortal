using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class UserAuditLogModelConfiguration : IEntityTypeConfiguration<UserAuditLogModel>
    {
        public void Configure(EntityTypeBuilder<UserAuditLogModel> builder)
        {       
            
            builder.Property(e => e.Email).IsRequired().HasColumnType("NVARCHAR(256)").HasMaxLength(256);
            builder.Property(e => e.OperatingSystem).HasColumnType("NVARCHAR(256)").HasMaxLength(512);
            builder.Property(e => e.Browser).HasColumnType("NVARCHAR(256)").HasMaxLength(512);
            builder.Property(e => e.ScreenSize).HasColumnType("NVARCHAR(256)").HasMaxLength(512);
            builder.Property(e => e.UserAgent).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.Feature).IsRequired().HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.AccessDateTime).IsRequired().HasColumnType("DATETIME2(7)");

            builder.ToTable("UserAuditLogs");
        }
    }
}
