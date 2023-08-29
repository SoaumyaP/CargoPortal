using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Provider.Report;
using Groove.SP.Application.Scheduling.Services.Interfaces;
using Groove.SP.Application.Scheduling.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.Scheduling.Services
{
    public class SchedulingListService : ServiceBase<SchedulingModel, SchedulingListViewModel>, ISchedulingListService
    {
        private readonly IDataQuery _dataQuery;
        private readonly ITelerikReportProvider _telerikReportProvider;

        public SchedulingListService(
            IUnitOfWorkProvider unitOfWorkProvider,
            IDataQuery dataQuery,
            ITelerikReportProvider telerikReportProvider)
            : base(unitOfWorkProvider)
        {
            _dataQuery = dataQuery;
            _telerikReportProvider = telerikReportProvider;
        }

        public async Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, long organizationId)
        {
            string sql;
            IQueryable<SchedulingQueryModel> query;

            if (isInternal)
            {
                sql =
                    @"
                        SELECT SCH.Id, SCH.[Name], RPT.ReportGroup, RPT.ReportName, SCH.[Status], null AS [StatusName], SCH.CreatedBy,  SCH.UpdatedDate, null AS [NextOccurrence], SCH.TelerikSchedulingId
                        FROM Schedulings SCH WITH (NOLOCK)
                        INNER JOIN Reports RPT WITH (NOLOCK) ON RPT.Id = SCH.CSPortalReportId";
                query = _dataQuery.GetQueryable<SchedulingQueryModel>(sql);
            }
            else
            {
                sql =
                    @"
                        SELECT SCH.Id, SCH.[Name], RPT.ReportGroup, RPT.ReportName, SCH.[Status], null AS [StatusName], SCH.CreatedBy, SCH.UpdatedDate, null AS [NextOccurrence], SCH.TelerikSchedulingId
                        FROM Schedulings SCH WITH (NOLOCK)
                        INNER JOIN Reports RPT WITH (NOLOCK) ON RPT.Id = SCH.CSPortalReportId
                        WHERE SCH.CreatedOrganizationId = {0}
                    ";

                query = _dataQuery.GetQueryable<SchedulingQueryModel>(sql, organizationId);

            }
            var viewModels = await query.ToDataSourceResultAsync(request);
            var data = viewModels.Data as IEnumerable<SchedulingQueryModel>;
            if (data != null && data.Any())
            {
                var telerikTasks = await _telerikReportProvider.GetSchedulingListAsync();
                foreach (var item in data)
                {
                    item.NextOccurrence = telerikTasks.FirstOrDefault(x => x.Id == item.TelerikSchedulingId)?.NextOccurence;
                }
            }
            return viewModels;
        }
    }
}
