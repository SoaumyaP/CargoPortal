using Groove.SP.Application.Common;
using Groove.SP.Application.Reports.ViewModels;
using Groove.SP.Application.Scheduling.ViewModels;
using Groove.SP.Core.Entities;
using System.Threading.Tasks;

namespace Groove.SP.Application.Reports.Services.Interfaces
{
    public interface IReportService : IServiceBase<ReportModel, ReportViewModel>
    {
        Task<bool> IsAuthorized(long reportId, long selectedCustomerId, IdentityInfo currentUser);
        Task<byte[]> ExportDataAsync(long reportId, string jsonFilter);
        int GrantReportPermission(ReportGrantPermissionViewModel data);
        Task<ReportGrantPermissionViewModel> GetReportPermissionAsync(long reportId);

        /// <summary>
        /// Register Telerik user linked to organization.
        /// It is being used to adapt multi-tenant scope that provided user can only see specific data
        /// </summary>
        /// <param name="principalOrganizationId"></param>
        /// <returns></returns>
        public Task RegisterTelerikUserAsync(long principalOrganizationId);


        #region To integrate to Telerik

        /// <summary>
        /// To get token from Telerik reporting server
        /// </summary>
        /// <param name="currentUser">Leave null to get powerful token</param>
        /// <returns></returns>
        Task<TelerikAccessTokenModel> GetTelerikAccessToken(IdentityInfo currentUser = null);

        #endregion To integrate to Telerik
    }
}
 