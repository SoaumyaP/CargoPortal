using Groove.SP.Application.CargoDetail.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Consignment.ViewModels;
using Groove.SP.Application.Consolidation.ViewModels;
using Groove.SP.Application.ShipmentLoadDetails.ViewModels;
using Groove.SP.Application.ShipmentLoads.ViewModels;
using Groove.SP.Core.Entities;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.Consolidation.Services.Interfaces
{
    public interface IConsolidationService : IServiceBase<ConsolidationModel, ConsolidationViewModel>
    {
        Task<IEnumerable<ConsolidationViewModel>> GetConsolidationsByShipmentAsync(long shipmentId);

        Task<IEnumerable<ConsolidationViewModel>> GetConsolidationsByConsignmentAsync(long consignmentId);

        // Services are called by Internal Consolidation Controller

        /// <summary>
        /// Update consolidation via UI.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="consolidationId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<ConsolidationViewModel> UpdateConsolidationAsync(UpdateConsolidationViewModel model, long consolidationId, string userName);

        /// <summary>
        /// Create new consolidation via UI.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ConsolidationViewModel> CreateConsolidationAsync(InputConsolidationViewModel model);

        /// <summary>
        /// Get consolidation for UI.
        /// </summary>
        /// <param name="consolidationId"></param>
        /// <param name="isInternal"></param>
        /// <param name="affiliates"></param>
        /// <returns></returns>
        Task<ConsolidationInternalViewModel> GetInternalConsolidationAsync(long consolidationId, bool isInternal, string affiliates ="");

        /// <summary>
        /// To change the stage of new consolidation to confirmed and also create the container and link it to the consolidation.
        /// </summary>
        /// <param name="consolidationId"></param>
        /// <param name="isInternal"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<ConsolidationInternalViewModel> ConfirmConsolidationAsync(long consolidationId, bool isInternal, string userName);

        /// <summary>
        /// Pls call to validate before confirming the consolidation. Throw an exception if invalid.
        /// </summary>
        /// <param name="consolidationId"></param>
        /// <returns>Throw exception</returns>
        Task ValidateConfirmConsolidationAsync(long consolidationId);

        /// <summary>
        /// To change the stage of confirmed Consolidation to new.
        /// </summary>
        /// <param name="consolidationId"></param>
        /// <param name="isInternal"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<ConsolidationInternalViewModel> UnconfirmConsolidationAsync(long consolidationId, bool isInternal, string userName);

        /// <summary>
        /// To link consolidation with consignment in ShipmentLoads.
        /// </summary>
        /// <param name="consolidationId"></param>
        /// <param name="consignmentId"></param>
        /// <param name="viewModel"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        Task<ShipmentLoadViewModel> CreateLinkingConsignmentAsync(long consolidationId, long consignmentId, ConsignmentViewModel viewModel, string currentUser);

        /// <summary>
        /// To remove the linked between consolidation and consignment.
        /// Also remove the ShipmentLoadDetails of Consolidation and Consignment.
        /// </summary>
        /// <param name="consolidationId"></param>
        /// <param name="consignmentId"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        Task<bool> DeleteLinkingConsignmentAsync(long consolidationId, long consignmentId, string currentUser);

        /// <summary>
        /// Call to load the list of Cargo details on the consolidation.
        /// </summary>
        /// <param name="consolidationId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<IEnumerable<ShipmentLoadDetailViewModel>> LoadCargoDetail(long consolidationId, List<CargoDetailLoadViewModel> model);

        Task UpdateConsolidationTotalAmountAsync(long consolidationId);
    }
}