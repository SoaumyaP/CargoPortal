using Groove.SP.Application.Common;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Reports.Services.Interfaces;
using Groove.SP.Application.Reports.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System.Threading.Tasks;

namespace Groove.SP.Application.Reports.Services
{
    
    /// <summary>
    /// To provide methods to integrate to Telerik Report Server
    /// </summary>
    public partial class ReportService : ServiceBase<ReportModel, ReportViewModel>, IReportService
    {
        public async Task<TelerikAccessTokenModel> GetTelerikAccessToken(IdentityInfo currentUser = null)
        {
            // If it is internal, use powerful account to view all data from Telerik
            var userName = _appConfig.Report.ReportUsername;
            var password = _appConfig.Report.ReportPassword;

            // Agent/Warehouse will get all data from Telerik
            // Customer/Principal will get by specific telerik username (only current org)
            if (currentUser != null && !currentUser.IsInternal && currentUser.UserRoleId != (int)Role.Agent && currentUser.UserRoleId != (int)Role.Warehouse)
            {
                // Try to get default report user name for current organization id
                var sql = $@"

                            SELECT TOP(1) @result = ReportUserName
		                    FROM [report].[ReportingUserProfiles] RUP WITH(NOLOCK)
		                    WHERE RUP.OrganizationId = {currentUser.OrganizationId} AND RUP.SystemUser = 1
                          ";
                userName = _dataQuery.GetValueFromVariable(sql, null);
                password = _appConfig.Report.SystemUserPassword;
            }
            if (string.IsNullOrEmpty(userName))
            {
                throw new AppException($"Can not find Telerik user for organization id {currentUser.OrganizationId}");
            }

            try
            {
                var accessToken = await _telerikReportProvider.GetAccessToken(userName, password);
                return accessToken;
            }
            catch 
            {
                throw new AppException($"Can not get Telerik access token for user {userName}");
            }
        }
    }
}
