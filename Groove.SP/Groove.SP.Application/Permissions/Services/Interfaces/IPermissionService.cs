using Groove.SP.Application.Common;
using Groove.SP.Application.Permissions.ViewModels;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.Permissions.Services.Interfaces
{
    public interface IPermissionService : IServiceBase<PermissionModel, PermissionViewModel>
    {
        /// <summary>
        /// Check permission by username/email
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="permissionName"></param>
        /// <returns></returns>
        Task<bool> IsUserGrantedAsync(string userName, string permissionName);

        /// <summary>
        /// Check permission by username/email
        /// </summary>
        /// <param name="requiresAll"></param>
        /// <param name="userName"></param>
        /// <param name="permissionNames"></param>
        /// <returns></returns>
        Task<bool> IsUserGrantedAsync(bool requiresAll, string userName, params string[] permissionNames);

        /// <summary>
        /// Get permission by username/email
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<IEnumerable<PermissionViewModel>> GetUserPermissions(string userName);

        /// <summary>
        /// Refresh permissions by username/email
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<IEnumerable<PermissionViewModel>> InvalidatePermissionCache(string username);


        /// <summary>
        /// Check permission by role (it is in user role switch mode)
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="permissionName"></param>
        /// <returns></returns>
        Task<bool> IsRoleGrantedAsync(long roleId, string permissionName);
        /// <summary>
        /// Check permission by role (it is in user role switch mode)
        /// </summary>
        /// <param name="requiresAll"></param>
        /// <param name="roleId"></param>
        /// <param name="permissionNames"></param>
        /// <returns></returns>
        Task<bool> IsRoleGrantedAsync(bool requiresAll, long roleId, params string[] permissionNames);
        /// <summary>
        /// Get permission by username/email (it is in user role switch mode)
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        Task<IEnumerable<PermissionViewModel>> GetRolePermissions(long roleId);

        /// <summary>
        /// Refresh permissions by role (it is in user role switch mode)
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        Task<IEnumerable<PermissionViewModel>> InvalidatePermissionCache(long roleId);

        IEnumerable<PermissionViewModel> GeAllStaticPermissions();        
        void RemovePermissionsCacheOfUser(string username);
        void RemoveAllPermissionsCacheOfAllUser();
    }
}
