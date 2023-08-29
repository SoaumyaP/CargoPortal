using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Reports.Services.Interfaces;
using Groove.SP.Application.Reports.ViewModels;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.Reports.Services
{
    public class ReportListService : ServiceBase<ReportModel, ReportListViewModel>, IReportListService
    {
        private readonly IDataQuery _dataQuery;
        public ReportListService(
            IUnitOfWorkProvider unitOfWorkProvider,
            IDataQuery dataQuery) : base(unitOfWorkProvider)
        {
            _dataQuery = dataQuery;
        }

        public async Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, long roleId, long selectedOrganizationId, string affiliates)
        {
            if (selectedOrganizationId == 0)
            {

                return new DataSourceResult
                {
                    Data = Enumerable.Empty<object>(),
                    Total = 0
                };
            }

            string sql;
            IQueryable<ReportQueryModel> query;

            if (isInternal)
            {
                sql =
                    @"
                    SELECT Id, ReportName, ReportUrl, ReportDescription, ReportGroup, LastRunTime, null AS [TelerikCategoryId], null AS [TelerikCategoryName], null AS [TelerikReportId]
		            FROM Reports
                    WHERE ReportName NOT IN ('Master Summary Report (PO Level)', 'Master Summary Report (Item Level)')";
                query = _dataQuery.GetQueryable<ReportQueryModel>(sql);
            }
            else
            {
                var purifiedAffiliates = affiliates.PurifyAffiliates();
                sql =
                    @"
                    SELECT Id, ReportName, ReportUrl, ReportDescription, ReportGroup, LastRunTime, null AS [TelerikCategoryId], null AS [TelerikCategoryName], null AS [TelerikReportId]
                    FROM Reports R
                    WHERE Id IN (
	                    SELECT ReportId 
	                    FROM ReportPermissions RP 
	                    WHERE 
                            RP.ReportId = R.Id
                            -- check matched role assigned
                            AND RP.RoleId = {0}
                            AND (
                                -- if there is not specific organization assigned
                                RP.OrganizationIds IS NULL 
				                OR 
                                -- exists in specific organization assigned
                                    EXISTS (SELECT 1 
							                FROM [dbo].[fn_SplitStringToTable] (RP.OrganizationIds, ',') 
							                WHERE [Value] IN (SELECT [VALUE] FROM [dbo].[fn_SplitStringToTable] ({1}, ',')))
		                    )
                        AND R.ReportName NOT IN ('Master Summary Report (PO Level)', 'Master Summary Report (Item Level)')
                    )";

                query = _dataQuery.GetQueryable<ReportQueryModel>(sql, roleId, purifiedAffiliates);

            }

            return await query.ToDataSourceResultAsync(request);
        }

        public async Task<IEnumerable<ReportQueryModel>> SchedulingReportOptionsAsync(bool isInternal, long roleId, string affiliates)
        {
            string sql;
            IQueryable<ReportQueryModel> query;

            if (isInternal)
            {
                sql =
                    @"
                    SELECT Id, ReportName, ReportUrl, ReportDescription, ReportGroup, null AS [LastRunTime], TelerikCategoryId, TelerikCategoryName, TelerikReportId
		            FROM Reports WITH(NOLOCK)
                    WHERE SchedulingApply = 1";
                query = _dataQuery.GetQueryable<ReportQueryModel>(sql);
            }
            else
            {
                var purifiedAffiliates = affiliates.PurifyAffiliates();
                sql =
                    @"
                    SELECT Id, ReportName, ReportUrl, ReportDescription, ReportGroup, null AS [LastRunTime], TelerikCategoryId, TelerikCategoryName, TelerikReportId
                    FROM Reports R
                    WHERE Id IN (
	                    SELECT ReportId 
	                    FROM ReportPermissions RP 
	                    WHERE 
                            RP.ReportId = R.Id
                            -- check matched role assigned
                            AND RP.RoleId = {0}
                            AND (
                                -- if there is not specific organization assigned
                                RP.OrganizationIds IS NULL 
				                OR 
                                -- exists in specific organization assigned
                                    EXISTS (SELECT 1 
							                FROM [dbo].[fn_SplitStringToTable] (RP.OrganizationIds, ',') 
							                WHERE [Value] IN (SELECT [VALUE] FROM [dbo].[fn_SplitStringToTable] ({1}, ',')))
		                    )
                    ) AND R.SchedulingApply = 1 AND R.ReportName NOT IN ('Master Summary Report (PO Level)', 'Master Summary Report (Item Level)')

                   
                    UNION ALL

                    SELECT Id, ReportName, ReportUrl, ReportDescription, ReportGroup, null AS [LastRunTime], TelerikCategoryId, TelerikCategoryName, TelerikReportId
                    FROM Reports R
                    WHERE R.ReportName IN ('Master Summary Report (PO Level)', 'Master Summary Report (Item Level)')
                        AND EXISTS (
	                                SELECT 1
                                    FROM Reports R
                                    WHERE Id IN (
	                                    SELECT ReportId 
	                                    FROM ReportPermissions RP 
	                                    WHERE 
                                            RP.ReportId = R.Id
                                            -- check matched role assigned
                                            AND RP.RoleId = {0}
                                            AND (
                                                -- if there is not specific organization assigned
                                                RP.OrganizationIds IS NULL 
				                                OR 
                                                -- exists in specific organization assigned
                                                    EXISTS (SELECT 1 
							                                FROM [dbo].[fn_SplitStringToTable] (RP.OrganizationIds, ',') 
							                                WHERE [Value] IN (SELECT [VALUE] FROM [dbo].[fn_SplitStringToTable] ({1}, ',')))
		                                    )
                                        AND R.SchedulingApply = 0
                                        AND R.ReportName = 'Master Summary Report New'
                                    ) 
                            )
                    ";

                query = _dataQuery.GetQueryable<ReportQueryModel>(sql, roleId, purifiedAffiliates);
            }
            var result = query.ToList();
            return result;
        }
    }
}
