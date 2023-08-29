using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class PermissionModelConfiguration : IEntityTypeConfiguration<PermissionModel>
    {
        public void Configure(EntityTypeBuilder<PermissionModel> builder)
        {
            builder.Property(x => x.Name).IsRequired();
            builder.HasData(PermissionHelper.SeedDefaultPermissions());
        }
    }
}
