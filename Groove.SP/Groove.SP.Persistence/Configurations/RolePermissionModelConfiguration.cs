using Groove.SP.Application.Authorization;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Groove.SP.Persistence.Configurations
{
    public class RolePermissionModelConfiguration : IEntityTypeConfiguration<RolePermissionModel>
    {
        public void Configure(EntityTypeBuilder<RolePermissionModel> builder)
        {
            builder.HasKey(t => new { t.RoleId, t.PermissionId });

            builder.HasOne(pt => pt.Role)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(pt => pt.RoleId);

            builder.HasOne(pt => pt.Permission)
                .WithMany(t => t.RolePermissions)
                .HasForeignKey(pt => pt.PermissionId);

            #region Permission for role Administrator RoleId = 1 - Full Permissions

            var createdDate = new DateTime(2019, 01, 01);
            var appPermissions = PermissionHelper.GetFieldInfoPermissions();
            var permissionsOfAdmin = new List<RolePermissionModel>();
            var permissionOrderDictionary = new AppPermissionOrders();

            int permissionId = 1;
            appPermissions.ForEach(p =>
            {
                var permissionName = p.GetRawConstantValue() as string;
                
                var ignorePermissions = new string[] {
                    AppPermissions.Product,
                    AppPermissions.Product_List,
                    AppPermissions.Product_Detail,
                    AppPermissions.Product_Detail_Add,
                    AppPermissions.Product_Detail_Edit,
                    AppPermissions.Dashboard_Top10CarrierThisWeek
                };

                // If obsolete then not need to seed
                // If these above six permission, ignore also
                if (permissionOrderDictionary.PermissionOrderDictionary.ContainsKey(permissionName) && !ignorePermissions.Contains(permissionName))
                {
                    permissionsOfAdmin.Add(new RolePermissionModel
                    {
                        RoleId = 1,
                        PermissionId = permissionId,
                        CreatedDate = createdDate,
                        CreatedBy = AppConstant.SYSTEM_USERNAME
                    });
                }
                permissionId++;
            });
            
            builder.HasData(permissionsOfAdmin);
            #endregion

            // We do not set default permission for other roles as it may seriously affect to current data on RolePermissions table
        }
    }
}
