using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Exceptions;
using Groove.CSFE.Application.Interfaces.UnitOfWork;
using Groove.CSFE.Application.OrganizationRoles.Services.Interfaces;
using Groove.CSFE.Application.OrganizationRoles.ViewModels;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.OrganizationRoles.Services
{
    public class OrganizationRoleService : ServiceBase<OrganizationRoleModel, OrganizationRoleViewModel>, IOrganizationRoleService
    {
        public OrganizationRoleService(IUnitOfWorkProvider unitOfWorkProvider)
            : base(unitOfWorkProvider)
        { }

        public async Task<OrganizationRoleViewModel> GetAsync(long id)
        {
            var model = await Repository.GetAsNoTrackingAsync(s => s.Id == id);
            return Mapper.Map<OrganizationRoleModel, OrganizationRoleViewModel>(model);
        }
    }
}
