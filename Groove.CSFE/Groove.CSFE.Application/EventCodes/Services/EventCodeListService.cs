using Groove.CSFE.Application.EventCodes.Services.Interfaces;
using Groove.CSFE.Application.Interfaces.Repositories;
using Groove.CSFE.Core.Data;
using Groove.CSFE.Core.Entities;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.EventCodes.Services
{
    public class EventCodeListService : IEventCodeListService
    {
        private readonly IRepository<EventCodeModel> _eventCodeRepository;
        private readonly IDataQuery _dataQuery;

        public EventCodeListService(IRepository<EventCodeModel> eventCodeRepository, IDataQuery dataQuery)
        {
            _eventCodeRepository = eventCodeRepository;
            _dataQuery = dataQuery;
        }

        public async Task<DataSourceResult> SearchAsync(DataSourceRequest request)
        {
            var sql = @"
                        SELECT
	                        EC.ActivityCode,
                            EC.ActivityTypeCode,
	                        EC.ActivityDescription,
	                        ET.[Description] as [ActivityTypeDescription],
	                        CASE WHEN EC.LocationRequired = 1 THEN 'Yes'
		                        ELSE ''
	                        END as [LocationRequired],
	                        CASE WHEN EC.RemarkRequired = 1 THEN 'Yes'
		                        ELSE ''
	                        END as [RemarkRequired],
	                        EC.[Status],
	                        CASE
		                        WHEN EC.[Status] = 1 THEN 'Active'
		                        ELSE 'Inactive'
	                        END as [StatusName],
	                        EC.SortSequence,
                            EC.CreatedDate
                        FROM EventCodes EC INNER JOIN EventTypes ET ON EC.ActivityTypeCode = ET.Code
                     ";

            return await _dataQuery.GetQueryable<EventCodeQueryModel>(sql).ToDataSourceResultAsync(request);
        }
    }
}
