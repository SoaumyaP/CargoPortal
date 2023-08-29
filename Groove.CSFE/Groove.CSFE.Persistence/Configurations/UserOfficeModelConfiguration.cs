using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class UserOfficeModelConfiguration : IEntityTypeConfiguration<UserOfficeModel>
    {
        public void Configure(EntityTypeBuilder<UserOfficeModel> builder)
        {
            builder.Property(e => e.CorpMarketingContactName).HasColumnType("nvarchar(256)");
            builder.Property(e => e.CorpMarketingContactEmail).HasColumnType("varchar(128)");
            builder.Property(e => e.OPManagementContactName).HasColumnType("nvarchar(256)");
            builder.Property(e => e.OPManagementContactEmail).HasColumnType("varchar(128)");

            builder.HasOne(rt => rt.Location)
                   .WithOne(cc => cc.UserOffices)
                   .HasForeignKey<UserOfficeModel>(c => c.LocationId);
        }
    }
}
