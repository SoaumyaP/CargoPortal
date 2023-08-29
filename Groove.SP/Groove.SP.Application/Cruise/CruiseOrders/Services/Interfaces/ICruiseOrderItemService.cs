using Groove.SP.Application.Common;
using Groove.SP.Application.CruiseOrders.ViewModels;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Core.Entities.Cruise;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.CruiseOrders.Services.Interfaces
{
    public interface ICruiseOrderItemService : IServiceBase<CruiseOrderItemModel, CruiseOrderItemViewModel>
    {
        /// <summary>
        /// Called via GUI, it is copy function
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<CruiseOrderItemViewModel> CreateCruiseOrderItemAsync(CruiseOrderItemViewModel viewModel, string userName);

        /// <summary>
        /// Called via GUI, it is edit function
        /// </summary>
        /// <param name="cruiseOderItemId"></param>
        /// <param name="model"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<IEnumerable<CruiseOrderItemViewModel>> ReviseCruiseOrderItemAsync(long cruiseOderItemId, ReviseCruiseOrderItemViewModel model, string userName);

        /// <summary>
        /// To hard-delete cruise order item, called via GUI
        /// <br></br>It will check if internal user or external same organization with who copied
        /// </summary>
        /// <param name="cruiseOderItemId">Id of cruise order id that is going to remove </param>
        /// <param name="currentUser">Identity information of current user</param>
        /// <returns></returns>
        Task<bool> DeleteCruiseOrderItemAsync(long cruiseOderItemId, IdentityInfo currentUser);
    }
}
