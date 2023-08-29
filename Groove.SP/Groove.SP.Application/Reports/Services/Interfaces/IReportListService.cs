using Groove.SP.Application.Common;
using Groove.SP.Application.Reports.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.Reports.Services.Interfaces
{
    public interface IReportListService : IServiceBase<ReportModel, ReportListViewModel>
    {
        /// <summary>
        /// To fetch data for report list
        /// </summary>
        /// <param name="request">Meta data of requesting from grid</param>
        /// <param name="isInternal">Whether it is internal user</param>
        /// <param name="roleId">Role of current user</param>
        /// <param name="selectedOrganizationId">Id of selected principal</param>
        /// <param name="affiliates">Affiliates string of external user. Ex: [2,127,128]</param>
        /// <returns></returns>
        Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, long roleId, long selectedOrganizationId, string affiliates);

        /// <summary>
        /// To get options data source which is used by report drop-down list on Scheduling form
        /// </summary>
        /// <param name="isInternal">If current is internal user</param>
        /// <param name="roleId">Role id of current user</param>
        /// <param name="affiliates">Affiliates of current user if external</param>
        /// <returns></returns>
        Task<IEnumerable<ReportQueryModel>> SchedulingReportOptionsAsync(bool isInternal, long roleId, string affiliates);
    }
}
 