using Groove.CSFE.Application.WarehouseLocations.Services.Interfaces;
using Groove.CSFE.Core.Data;
using Groove.CSFE.Core.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.WarehouseLocations.Services
{
    public class WarehouseLocationListService : IWarehouseLocationListService
    {
        private readonly IDataQuery _dataQuery;
        public WarehouseLocationListService(IDataQuery dataQuery)
        {
            _dataQuery = dataQuery;
        }

        public async Task<DataSourceResult> GetListWarehouseLocationAsync(DataSourceRequest request)
        {
           IQueryable<WarehouseLocationQueryModel> query;
            string sql;
            sql =
                @"
                     SELECT
	                    WL.Id
	                    ,WL.Code
	                    ,WL.[Name]
	                    ,CONCAT(L.LocationDescription, ' - ', C.[Name]) AS [Location]
	                    ,IIF(WL.ContactPerson IS NULL, '', WL.ContactPerson) AS [ContactName]
	                    ,O.[Name] AS [Provider]
	                    ,WL.OrganizationId
                    FROM WarehouseLocations WL (NOLOCK)
	                INNER JOIN Locations L (NOLOCK) ON WL.LocationId = L.Id
                    INNER JOIN Countries C (NOLOCK) ON L.CountryId = C.Id
	                INNER JOIN Organizations O (NOLOCK) ON WL.OrganizationId = O.Id
                ";
            query = _dataQuery.GetQueryable<WarehouseLocationQueryModel>(sql);
            return await query.ToDataSourceResultAsync(request);
        }
    }
}
