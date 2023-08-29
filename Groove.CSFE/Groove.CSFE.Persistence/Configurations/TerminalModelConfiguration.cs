using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.CSFE.Persistence.Configurations
{
    public class TerminalModelConfiguration : IEntityTypeConfiguration<TerminalModel>
    {
        public void Configure(EntityTypeBuilder<TerminalModel> builder)
        {
            builder.Property(e => e.TerminalCode).IsRequired().HasColumnType("nvarchar(128)");
            builder.Property(e => e.TerminalName).HasColumnType("nvarchar(128)");
            builder.Property(e => e.Address).HasColumnType("nvarchar(256)");


            builder.HasOne(w => w.Location)
                .WithMany(l => l.Terminals)
                .HasForeignKey(w => w.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
