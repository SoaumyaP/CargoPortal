using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Interfaces.UnitOfWork;
using Groove.CSFE.Application.UserOffices.Services.Interfaces;
using Groove.CSFE.Application.UserOffices.ViewModels;
using Groove.CSFE.Core.Entities;
using System;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.UserOffices.Services
{
    public class UserOfficeService : ServiceBase<UserOfficeModel, UserOfficeViewModel>, IUserOfficeService
    {
        public UserOfficeService(IUnitOfWorkProvider unitOfWorkProvider)
            : base(unitOfWorkProvider)
        {
        }

        public async Task<UserOfficeViewModel> GetByLocationNameAsync(string location, long countryId)
        {
            if (string.IsNullOrWhiteSpace(location) || countryId == 0)
                return null;

            var userOffice = await Repository.GetAsNoTrackingAsync(x => x.Location.LocationDescription == location && x.Location.CountryId == countryId);

            return Mapper.Map<UserOfficeViewModel>(userOffice);
        }
    }
}