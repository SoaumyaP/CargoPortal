using Groove.SP.Application.CargoDetail.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.CargoDetail.Services.Interfaces
{
    public interface ICargoDetailService : IServiceBase<CargoDetailModel, CargoDetailViewModel>
    {
        /// <summary>
        /// Get list of cargo details by shipment
        /// </summary>
        /// <param name="shipmentId"></param>
        /// <returns></returns>
        Task<IEnumerable<CargoDetailListViewModel>> GetCargoDetailsByShipmentAsync(long shipmentId);

        /// <summary>
        /// Get list of cargo details by container
        /// </summary>
        /// <param name="containerId"></param>
        /// <returns></returns>
        Task<IEnumerable<CargoDetailListViewModel>> GetCargoDetailsByContainerAsync(long containerId);

        /// <summary>
        /// Return list of unloaded cargo detail by consolidation
        /// </summary>
        /// <param name="consolidationId"></param>
        /// <returns></returns>
        Task<IEnumerable<CargoDetailLoadViewModel>> GetUnloadCargoDetailsByConsolidationAsync(long consolidationId);
    }
}
