using Groove.SP.Application.Common;
using Groove.SP.Application.Consignment.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.Consignment.Services.Interfaces
{
    public interface IConsignmentService : IServiceBase<ConsignmentModel, ConsignmentViewModel>
    {
        Task<IEnumerable<ConsignmentViewModel>> GetConsignmentsByShipmentAsync(long shipmentId);

        Task<ConsignmentViewModel> UpdateAsync(ConsignmentViewModel viewModel, long id);

        Task<ConsignmentViewModel> GetAsync(long id, bool isInternal, string affiliates = "");

        Task<IEnumerable<ConsignmentViewModel>> GetByConsolidationAsync(long consolidationId, bool isInternal, string affiliates = "");

        Task MoveToTrashAsync(long id, string userName);

        /// <summary>
        /// To get data source for drop-down as adding new Consignment into Consolidation via GUI
        /// <br>
        /// <b>Only get Sea/Air mode of transport</b>
        /// </br>
        /// </summary>
        /// <param name="shipmentId">Id of shipment</param>
        /// <returns></returns>
        Task<IEnumerable<ConsignmentDropdownItemViewModel>> GetDropdownByShipmentAsync(long shipmentId);

        Task<DropDownListItem<string>> GetDropdownOriginCFSAsync(long id);
    }
}