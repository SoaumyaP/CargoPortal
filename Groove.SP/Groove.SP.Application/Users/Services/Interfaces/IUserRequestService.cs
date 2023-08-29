using System.Threading.Tasks;

using Groove.SP.Application.Common;
using Groove.SP.Application.Users.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Users.Services.Interfaces
{
    public interface IUserRequestService: IServiceBase<UserRequestModel, UserRequestViewModel>
    {
        Task<UserRequestViewModel> GetAsync(long id);

        Task<string> ApproveAsync(long id, UserRequestViewModel viewModel, string currentUser);

        Task<string> RejectAsync(long id, UserRequestViewModel viewModel, string currentUser);
    }
}
