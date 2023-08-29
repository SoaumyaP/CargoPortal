using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FtpServerModelConfiguration : IEntityTypeConfiguration<FtpServerModel>
{
    public void Configure(EntityTypeBuilder<FtpServerModel> builder)
    {
        builder.Property(e => e.HostName).IsRequired().HasColumnType("NVARCHAR(512)").HasMaxLength(512);
        builder.Property(e => e.Username).IsRequired().HasColumnType("NVARCHAR(512)").HasMaxLength(512);
    }
}