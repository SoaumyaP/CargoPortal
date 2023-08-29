using Groove.SP.Application.Common;
using Groove.SP.Application.BuyerApproval.ViewModels;
using Groove.SP.Application.WarehouseFulfillment.ViewModels;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Application.POFulfillmentCargoReceive.ViewModels;

namespace Groove.SP.Application.WarehouseFulfillment.Services.Interfaces
{
    public interface IWarehouseFulfillmentService : IServiceBase<POFulfillmentModel, WarehouseFulfillmentViewModel>
    {
        Task ConfirmWarehouseBookingsAsync(IdentityInfo currentUser, IEnumerable<InputConfirmWarehouseFulfillmentViewModel> viewModels);
        Task<IEnumerable<ConfirmWarehouseFulfillmentViewModel>> SearchWarehouseBookingToConfirmAsync(string jsonFilter, IdentityInfo currentUser, string affiliates = "");
        Task<BuyerApprovalViewModel> ApproveAsync(long id, BuyerApprovalViewModel viewModel, string userName);
        Task<BuyerApprovalViewModel> RejectAsync(long id, BuyerApprovalViewModel viewModel, string userName);
        Task<WarehouseFulfillmentViewModel> CancelAsync(long id, string userName, CancelWarehouseFulfillmentViewModel cancelViewModel);
        Task<WarehouseFulfillmentViewModel> GetAsync(long id, bool isInternal, string affiliates);
        Task<WarehouseFulfillmentViewModel> UpdateAsync(InputWarehouseFulfillmentViewModel model, IdentityInfo currentUser);
        Task CargoReceiveAsync(long id, List<WarehouseFulfillmentOrderViewModel> viewModels, IdentityInfo currentUser);

        /// <summary>
        /// To import warehouse booking to the system, a request fired from ediSON
        /// </summary>
        /// <remarks><em>It is silent method with try-catch inside.</em></remarks>
        /// <param name="model">View model to input data</param>
        /// <param name="profile"></param>
        /// <returns>Result of importing process</returns>
        Task<ImportingWarehouseBookingResultViewModel> ImportWarehouseBookingSilentAsync(ImportWarehouseBookingViewModel importData, string profile);

        Task ImportCargoReceiveAsync(ImportPOFulfillmentCargoReceiveViewModel importData, string userName);

        #region Mobile App APIs
        Task<WarehouseCargoReceiveMobileModel> GetWarehouseCargoReceiveAsync(string warehouseBookingNumber);
        Task<WarehouseCargoReceiveDateMobileModel> FullCargoReceiveAsync(string bookingNumber, string userName = "System");
        #endregion Mobile App APIs
    }
}