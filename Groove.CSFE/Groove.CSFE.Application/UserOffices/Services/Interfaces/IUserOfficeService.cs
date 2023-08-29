using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.UserOffices.ViewModels;
using Groove.CSFE.Core.Entities;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.UserOffices.Services.Interfaces
{
    public interface IUserOfficeService : IServiceBase<UserOfficeModel, UserOfficeViewModel>
    {
        Task<UserOfficeViewModel> GetByLocationNameAsync(string location, long countryId);
    }
}