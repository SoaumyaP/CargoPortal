using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.SP.Application.Common;
using Groove.SP.Application.ShipmentLoadDetails.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;


namespace Groove.SP.Application.ShipmentLoadDetails.Services.Interfaces
{
    public interface IShipmentLoadDetailService : IServiceBase<ShipmentLoadDetailModel, ShipmentLoadDetailViewModel>
    {
        Task<ShipmentLoadDetailViewModel> GetAsync(long id);

        Task<IEnumerable<ShipmentLoadDetailListViewModel>> GetShipmentLoadDetailByContainer(long containerId, bool isInternal, string affiliates = "");

        Task<IEnumerable<ShipmentLoadDetailModel>> UpdateRangeByConsolidationAsync(long consolidationId, IEnumerable<UpdateShipmentLoadDetailViewModel> viewModel, string username);

        /// <summary>
        /// Return list of Shipment load detail in kendoGrid by consolidation
        /// </summary>
        /// <param name="request"></param>
        /// <param name="consolidationId"></param>
        /// <param name="isInternal"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Task<DataSourceResult> GetListByConsolidationAsync(DataSourceRequest request, long consolidationId, bool isInternal, long organizationId);
    }
}
