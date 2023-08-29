using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GGroove.CSFE.Application;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Countries.Services.Interfaces;
using Groove.CSFE.Application.Countries.ViewModels;
using Groove.CSFE.Application.Interfaces.UnitOfWork;
using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Groove.CSFE.Application.Countries.Services
{
    public class CountryService : ServiceBase<CountryModel, CountryViewModel>, ICountryService
    {
        public CountryService(IUnitOfWorkProvider unitOfWorkProvider)
            : base(unitOfWorkProvider)
        { }

        public async Task<CountryViewModel> GetAsync(long id)
        {
            var model = await Repository.GetAsNoTrackingAsync(s => s.Id == id);
            return Mapper.Map<CountryModel, CountryViewModel>(model);
        }

        public async Task<CountryViewModel> GetByCodeAsync(string code)
        {
            var model = await Repository.GetAsNoTrackingAsync(s => s.Code == code);

            return Mapper.Map<CountryModel, CountryViewModel>(model);
        }

        public async Task<List<DropDownModel>> GetAllLocations()
        {
            var data = await Repository.QueryAsNoTracking(null, x => x.OrderBy(y => y.Name), x => x.Include(y => y.Locations)).ToListAsync();
            var locations = data?.SelectMany(x => x.Locations.Select(y => new DropDownModel { Description = x.Id + "-" + y.Id, Label = y.LocationDescription })).ToList();
            locations.AddRange(data?.Select(x => new DropDownModel { Description = x.Id.ToString(), Label = "All " + x.Name + " Location" }));
            return locations;
        }
        public async Task<List<DropDownModel<string>>> GetAllLocationSelections()
        {
            var data = await Repository.QueryAsNoTracking(null, x => x.OrderBy(y => y.Name), x => x.Include(y => y.Locations)).ToListAsync();
            var locations = data?.SelectMany(x => x.Locations.Select(y => new DropDownModel<string>
            {
                Label = y.LocationDescription,
                Value = y.Id.ToString()
            })).ToList();
            locations.AddRange(data?.Select(x => new DropDownModel<string>
            {
                Label = "All " + x.Name + " Location",
                Value = x.Id + "-" + string.Join(",", data.SelectMany(c => c.Locations.Where(l => l.CountryId == x.Id).Select(l => l.Id)).ToList()),
            }));
            return locations;
        }
        public async Task<List<CountryLocationsViewModel>> GetAllCountryLocations()
        {
            var data = await Repository.QueryAsNoTracking(null, x => x.OrderBy(y => y.Name), x => x.Include(y => y.Locations)).ToListAsync();
            var result = data.Select(x =>
            new CountryLocationsViewModel()
            {
                CountryId = x.Id,
                LocationIds = x.Locations.Select(y => y.Id).ToList()
            }).ToList();
            return result;
        }
    }
}
