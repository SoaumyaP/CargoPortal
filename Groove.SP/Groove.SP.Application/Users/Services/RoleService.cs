using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Groove.SP.Application.Common;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Permissions.Services.Interfaces;
using Groove.SP.Application.Users.Services.Interfaces;
using Groove.SP.Application.Users.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Groove.SP.Application.Users.Services
{
    public class RoleService : ServiceBase<RoleModel, RoleViewModel>, IRoleService
    {
        private readonly IPermissionService _permissionService;
        public RoleService(IPermissionService permissionService, IUnitOfWorkProvider uow) : base(uow)
        {
            _permissionService = permissionService;
        }

        public async Task<RoleViewModel> GetAsync(long id)
        {
            var model = await this.Repository.GetAsync(x => x.Id == id, null, FullIncludeProperties);
            var viewModel = Mapper.Map<RoleViewModel>(model);
            return viewModel;
        }

        public override async Task<IEnumerable<RoleViewModel>> GetAllAsync()
        {
            var models = await this.Repository.Query(x => x.Status == RoleStatus.Active).ToListAsync();
            return Mapper.Map<IEnumerable<RoleViewModel>>(models);
        }

        public async Task<IEnumerable<RoleViewModel>> GetOfficialAsync()
        {
            var models = await this.Repository.Query(x => x.IsOfficial == true && x.Status == RoleStatus.Active).ToListAsync();
            var viewModels = Mapper.Map<IEnumerable<RoleViewModel>>(models);

            return viewModels;
        }

        protected override Func<IQueryable<RoleModel>, IQueryable<RoleModel>> FullIncludeProperties
        {
            get
            {
                return x => x.Include(m => m.RolePermissions);
            }
        }

        public async Task<RoleViewModel> UpdateAsync(RoleViewModel viewModel, string userName)
        {
            try
            {
                viewModel.ValidateAndThrow();
                
                var model = await Repository.GetAsync(x => x.Id == viewModel.Id, null, FullIncludeProperties);
                model.Description = viewModel.Description;

                var deletedList = model.RolePermissions?.Where(x => !viewModel.PermissionIds.Any(y => y == x.PermissionId)).ToList();
                foreach (var item in deletedList)
                {
                    model.RolePermissions.Remove(item);
                }

                var addedIdList = viewModel.PermissionIds?.Where(x => !model.RolePermissions.Any(y => y.PermissionId == x));
                foreach (var id in addedIdList)
                {
                    var rolePermissionModel = new RolePermissionModel();
                    rolePermissionModel.RoleId = model.Id;
                    rolePermissionModel.PermissionId = id;
                    rolePermissionModel.Audit(userName);
                    model.RolePermissions.Add(rolePermissionModel);
                }
                model.Audit(userName);
                
                this.Repository.Update(model);
                await this.UnitOfWork.SaveChangesAsync();
                _permissionService.RemoveAllPermissionsCacheOfAllUser();
                return Mapper.Map<RoleModel, RoleViewModel>(model);
            }
            catch (Exception ex)
            {
                throw new AppException(ex.Message);
            }
        }
    }
}
