using System;

using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class RoleModelConfiguration : IEntityTypeConfiguration<RoleModel>
    {
        public void Configure(EntityTypeBuilder<RoleModel> builder)
        {
            var createdDate = new DateTime(2019, 01, 01);
            builder.Property(e => e.Name).IsRequired();
            builder.Property(e => e.IsOfficial).IsRequired();
            builder.Property(e => e.IsInternal).IsRequired();
            builder.HasData
            (
                new RoleModel() { Id = 1, Name = "System Admin", IsInternal = true, CreatedBy = AppConstant.SYSTEM_USERNAME, CreatedDate = createdDate, Status = RoleStatus.Active, IsOfficial = true },
                new RoleModel() { Id = 2, Name = "CSR", IsInternal = true, CreatedBy = AppConstant.SYSTEM_USERNAME, CreatedDate = createdDate, Status = RoleStatus.Active, IsOfficial = true },
                new RoleModel() { Id = 3, Name = "Sale", IsInternal = true, CreatedBy = AppConstant.SYSTEM_USERNAME, CreatedDate = createdDate, Status = RoleStatus.Inactive, IsOfficial = true },
                new RoleModel() { Id = 4, Name = "Agent", IsInternal = false, CreatedBy = AppConstant.SYSTEM_USERNAME, CreatedDate = createdDate, Status = RoleStatus.Active, IsOfficial = true },
                new RoleModel() { Id = 5, Name = "Registered User", IsInternal = false, CreatedBy = AppConstant.SYSTEM_USERNAME, CreatedDate = createdDate, Status = RoleStatus.Active, IsOfficial = true },
                new RoleModel() { Id = 6, Name = "Guest", IsInternal = false, CreatedBy = AppConstant.SYSTEM_USERNAME, CreatedDate = createdDate, Status = RoleStatus.Active, IsOfficial = false },
                new RoleModel() { Id = 7, Name = "Pending", IsInternal = true, CreatedBy = AppConstant.SYSTEM_USERNAME, CreatedDate = createdDate, Status = RoleStatus.Active, IsOfficial = false },
                new RoleModel() { Id = 8, Name = "Principal", IsInternal = false, CreatedBy = AppConstant.SYSTEM_USERNAME, CreatedDate = createdDate, Status = RoleStatus.Active, IsOfficial = true },
                new RoleModel() { Id = 9, Name = "Shipper", IsInternal = false, CreatedBy = AppConstant.SYSTEM_USERNAME, CreatedDate = createdDate, Status = RoleStatus.Active, IsOfficial = true },
                new RoleModel() { Id = 10, Name = "Cruise Agent", IsInternal = false, CreatedBy = AppConstant.SYSTEM_USERNAME, CreatedDate = createdDate, Status = RoleStatus.Active, IsOfficial = true },
                new RoleModel() { Id = 11, Name = "Cruise Principal", IsInternal = false, CreatedBy = AppConstant.SYSTEM_USERNAME, CreatedDate = createdDate, Status = RoleStatus.Active, IsOfficial = true },
                new RoleModel() { Id = 12, Name = "Warehouse", IsInternal = false, CreatedBy = AppConstant.SYSTEM_USERNAME, CreatedDate = createdDate, Status = RoleStatus.Active, IsOfficial = true },
                new RoleModel() { Id = 13, Name = "Factory", IsInternal = false, CreatedBy = AppConstant.SYSTEM_USERNAME, CreatedDate = createdDate, Status = RoleStatus.Active, IsOfficial = true }
            );
        }
    }
}
