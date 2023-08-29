using GGroove.CSFE.Application;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Locations.ViewModels;
using Groove.CSFE.Core.Entities;
using Groove.CSFE.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.Locations.Services.Interfaces
{
    public interface ILocationService : IServiceBase<LocationModel, LocationViewModel>
    {
        Task<IEnumerable<LocationViewModel>> GetAsync(IEnumerable<long> countryIds);
        Task<LocationViewModel> GetAsync(long id);
        Task<LocationViewModel> GetByCodeAsync(string code);
        Task<LocationViewModel> GetByDescriptionAsync(string description);
        Task<LocationViewModel> GetByDescriptionAsync(string description, long countryId);
        Task<IEnumerable<LocationViewModel>> GetByCodesAsync(IEnumerable<string> codes);
        Task<IEnumerable<dynamic>> GetDropDownByCountryIdAsync(long countryId);
        Task<IEnumerable<DropDownModel<long>>> GetLocationSameCountryDropDownAsync(long locationId);
    }
}
