using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.OrganizationRoles.ViewModels;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.OrganizationRoles.Services.Interfaces
{
    public interface IOrganizationRoleService : IServiceBase<OrganizationRoleModel, OrganizationRoleViewModel>
    {
        Task<OrganizationRoleViewModel> GetAsync(long id);
    }
}
