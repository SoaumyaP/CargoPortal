using System.Collections.Generic;
using System.Threading.Tasks;

using Groove.SP.Application.Common;
using Groove.SP.Application.Users.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Users.Services.Interfaces
{
    public interface IRoleService: IServiceBase<RoleModel, RoleViewModel>
    {
        Task<RoleViewModel> GetAsync(long id);

        Task<IEnumerable<RoleViewModel>> GetOfficialAsync();

        Task<RoleViewModel> UpdateAsync(RoleViewModel viewModel, string userName);
    }
}
