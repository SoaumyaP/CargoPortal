using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GGroove.CSFE.Application;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Interfaces.Repositories;
using Groove.CSFE.Application.Interfaces.UnitOfWork;
using Groove.CSFE.Application.Locations.Services.Interfaces;
using Groove.CSFE.Application.Locations.ViewModels;
using Groove.CSFE.Core.Data;
using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Groove.CSFE.Application.Locations.Services
{
    public class LocationService : ServiceBase<LocationModel, LocationViewModel>, ILocationService
    {
        private readonly IDataQuery _dataQuery;
        private readonly ICountryRepository _countryRepository;

        public LocationService(IUnitOfWorkProvider unitOfWorkProvider, 
            IDataQuery dataQuery,
            ICountryRepository countryRepository
            )
            : base(unitOfWorkProvider)
        {
            _dataQuery = dataQuery;
            _countryRepository = countryRepository;
        }

        public override async Task<IEnumerable<LocationViewModel>> GetAllAsync()
        {
            var models = await this.Repository.QueryAsNoTracking(null, null, x => x.Include(m => m.Country)).ToListAsync();
            return Mapper.Map<IEnumerable<LocationViewModel>>(models);
        }

        public async Task<IEnumerable<LocationViewModel>> GetByCodesAsync(IEnumerable<string> codes)
        {
            var models = await this.Repository.QueryAsNoTracking(x => codes.Contains(x.Name.Trim())).ToListAsync();
            return Mapper.Map<IEnumerable<LocationViewModel>>(models);
        }

        public override async Task<DataSourceResult> ListAsync(DataSourceRequest request)
        {
            IQueryable<LocationQueryModel> query;
            string sql;
            sql =
                @"
                   SELECT
                        L.Id,
                        C.Id CountryId,
	                    C.Name CountryName,
	                    L.Name,
	                    L.LocationDescription,
	                    L.EdiSonPortCode
                    FROM Locations L (NOLOCK)
                    INNER JOIN Countries C (NOLOCK) 
	                    ON L.CountryId = C.Id
                ";
            query = _dataQuery.GetQueryable<LocationQueryModel>(sql);
            return await query.ToDataSourceResultAsync(request);
        }

        public async Task<IEnumerable<LocationViewModel>> GetAsync(IEnumerable<long> countryIds)
        {
            var query = this.Repository.GetListQueryable().AsNoTracking();

            if(countryIds != null || countryIds.Any())
            {
                query = query.Where(l => countryIds.Contains(l.CountryId));
            }

            var result = await query.ToListAsync();
            return Mapper.Map<IEnumerable<LocationViewModel>>(result);
        }

        public async Task<LocationViewModel> GetAsync(long id)
        {
            var model = await Repository.GetAsNoTrackingAsync(s => s.Id == id);
            return Mapper.Map<LocationModel, LocationViewModel>(model);
        }

        public async Task<LocationViewModel> GetByCodeAsync(string code)
        {
            var model = await Repository.GetAsNoTrackingAsync(s => s.Name == code,
               null, x => x.Include(m => m.Country));

            return Mapper.Map<LocationModel, LocationViewModel>(model);
        }

        public async Task<LocationViewModel> GetByDescriptionAsync(string description)
        {
            if (!string.IsNullOrWhiteSpace(description))
            {
                description = description.Trim();
            }
            var model = await Repository.GetAsNoTrackingAsync(s => s.LocationDescription == description,
               null, x => x.Include(m => m.Country));

            return Mapper.Map<LocationModel, LocationViewModel>(model);
        }

        public async Task<LocationViewModel> GetByDescriptionAsync(string description, long countryId)
        {
            if (!string.IsNullOrWhiteSpace(description))
            {
                description = description.Trim();
            }
            var model = await Repository.GetAsNoTrackingAsync(s => s.LocationDescription == description && s.CountryId == countryId,
               null, x => x.Include(m => m.Country));

            return Mapper.Map<LocationModel, LocationViewModel>(model);
        }

        public async Task<IEnumerable<dynamic>> GetDropDownByCountryIdAsync(long countryId)
        {
            var models = await Repository.QueryAsNoTracking(s => s.CountryId == countryId, o => o.OrderBy(x => x.LocationDescription)).ToListAsync();
            var result = models?.Select(x => new { Value = x.Id.ToString(), Label = x.LocationDescription });
            return result;
        }

        public async Task<IEnumerable<DropDownModel<long>>> GetLocationSameCountryDropDownAsync(long locationId)
        {
            var locationQuery = from location in Repository.QueryAsNoTracking(x => x.Id == locationId) select location.CountryId;

            var query = from location in Repository.QueryAsNoTracking()
                        join country in _countryRepository.QueryAsNoTracking() on location.CountryId equals country.Id
                        where locationQuery.Contains(country.Id)
                        select new DropDownModel<long>
                        {
                            Value = location.Id,
                            Label = location.LocationDescription,
                            Description = location.CountryId.ToString()
                        };
            return await query.ToListAsync();

        }
    }
}
