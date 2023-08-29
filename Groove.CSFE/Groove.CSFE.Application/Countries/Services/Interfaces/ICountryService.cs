using System.Collections.Generic;
using System.Threading.Tasks;
using GGroove.CSFE.Application;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Countries.ViewModels;
using Groove.CSFE.Core.Entities;

namespace Groove.CSFE.Application.Countries.Services.Interfaces
{
    public interface ICountryService : IServiceBase<CountryModel, CountryViewModel>
    {
        Task<CountryViewModel> GetAsync(long id);
        Task<CountryViewModel> GetByCodeAsync(string code);
        Task<List<DropDownModel>> GetAllLocations();
        Task<List<DropDownModel<string>>> GetAllLocationSelections();

        Task<List<CountryLocationsViewModel>> GetAllCountryLocations();
        
    }
}
