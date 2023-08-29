using Groove.SP.Application.BulkFulfillment.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using System.Threading.Tasks;

namespace Groove.SP.Application.BulkFulfillment.Services.Interfaces
{
    public interface IBulkFulfillmentService : IServiceBase<POFulfillmentModel, BulkFulfillmentViewModel>
    {
        /// <summary>
        /// Searching on kendo grid
        /// </summary>
        /// <param name="request"></param>
        /// <param name="isInternal"></param>
        /// <param name="affiliates"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Task<DataSourceResult> SearchAsync(DataSourceRequest request, bool isInternal, string affiliates = "", long? organizationId = 0);

        Task<BulkFulfillmentViewModel> GetAsync(long id, bool isInternal, string affiliates);
        /// <summary>
        /// To get bulk booking including linked shipment and itineraries 
        /// </summary>
        /// <param name="bulkBookingId"></param>
        /// <returns></returns>
        Task<BulkFulfillmentViewModel> GetPlannedScheduleAsync(long bulkBookingId);
        Task<BulkFulfillmentViewModel> CreateAsync(InputBulkFulfillmentViewModel viewModel, IdentityInfo currentUser);
        Task<BulkFulfillmentViewModel> UpdateAsync(InputBulkFulfillmentViewModel viewModel, IdentityInfo currentUser);
        Task ConfirmBookingAsync(EdiSonConfirmPOFFViewModel importVM);
        Task<BulkFulfillmentViewModel> SubmitBookingAsync(long id, IdentityInfo currentUser);

        /// <summary>
        /// To amend bulk booking.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task AmendAsync(long Id, string userName);

        /// <summary>
        /// To cancel bulk booking.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userName"></param>
        /// <param name="cancelModel"></param>
        /// <returns></returns>
        Task<BulkFulfillmentViewModel> CancelAsync(long id, string userName, CancelBulkFulfillmentViewModel cancelModel);

        /// <summary>
        /// Plan to ship for bulk booking.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task PlanToShipAsync(long id, string userName);

        /// <summary>
        /// To re-load for bulk booking (mean: plan to ship again).
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        Task ReloadAsync(long id, InputBulkFulfillmentViewModel model, IdentityInfo currentUser);
    }
}