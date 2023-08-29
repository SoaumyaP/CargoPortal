using Groove.SP.Application.Common;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Models;
using System.Threading.Tasks;


namespace Groove.SP.Application.POFulfillment.Services.Interfaces
{
    public interface IPOFulfillmentListService
    {
        Task<DataSourceResult> GetListPOFulfillmentAsync(DataSourceRequest request, bool isInternal, string affiliates = "", long? organizationId = 0, string userRole = "", string statisticKey = "");
        Task<int> GetBookingAwaitingForSubmissionAsync(long? organizationId = 0, string userRole = "");
        Task<int> GetBookingPendingForApprovalAsync(string affiliates = "", IdentityInfo currentUser = null);
    }
}
