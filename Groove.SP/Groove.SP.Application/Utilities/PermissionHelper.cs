using Groove.SP.Application.Authorization;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Groove.SP.Application.Utilities
{
    public static class PermissionHelper
    {
        public static List<PermissionModel> SeedDefaultPermissions()
        {
            var createdDate = new DateTime(2019, 01, 01);
            var appPermissions = GetFieldInfoPermissions();
            var permissionModels = new List<PermissionModel>();
            var permissionOrderDictionary = new AppPermissionOrders();
            int id = 1;
            appPermissions.ForEach(p =>
            {
                var permissionName = p.GetRawConstantValue() as string;
                // If obsolete then not need to seed
                if (permissionOrderDictionary.PermissionOrderDictionary.ContainsKey(permissionName))
                {
                    permissionModels.Add(new PermissionModel
                    {
                        Id = id,
                        Name = permissionName,
                        Order = permissionOrderDictionary.PermissionOrderDictionary[permissionName],
                        CreatedBy = AppConstant.SYSTEM_USERNAME,
                        CreatedDate = createdDate
                    });
                }
                id++;
            });
            return permissionModels;
        }

        public static List<FieldInfo> GetFieldInfoPermissions()
        {
            var appPermissions = typeof(AppPermissions)
                    .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
            return appPermissions;
        }
    }
}
