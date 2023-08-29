using Groove.SP.Application.Common;
using Groove.SP.Application.GlobalIdMasterDialog.Services.Interfaces;
using Groove.SP.Application.GlobalIdMasterDialog.ViewModels;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.GlobalIdMasterDialog.Services
{
    public class GlobalIdMasterDialogService : ServiceBase<GlobalIdMasterDialogModel, GlobalIdMasterDialogViewModel>, IGlobalIdMasterDialogService
    {
        public GlobalIdMasterDialogService(
            IUnitOfWorkProvider unitOfWorkProvider)
            :base(unitOfWorkProvider)
        {

        }

        public async Task<IEnumerable<GlobalIdMasterDialogViewModel>> GetByPurchaseOrderAsync(long poId)
        {
            var globalId = CommonHelper.GenerateGlobalId(poId, EntityType.CustomerPO);
            var result = await Repository.Query(m => m.GlobalId == globalId, null, x => x.Include(m => m.MasterDialog)).ToListAsync();

            return Mapper.Map<IEnumerable<GlobalIdMasterDialogViewModel>>(result);
        }

        public async Task<IEnumerable<GlobalIdMasterDialogViewModel>> GetByPOFulfillmentAsync(long poffId)
        {
            var globalId = CommonHelper.GenerateGlobalId(poffId, EntityType.POFullfillment);
            var result = await Repository.Query(m => m.GlobalId == globalId, null, x => x.Include(m => m.MasterDialog)).ToListAsync();

            return Mapper.Map<IEnumerable<GlobalIdMasterDialogViewModel>>(result);
        }

        public async Task<IEnumerable<GlobalIdMasterDialogViewModel>> GetByShipmentAsync(long shipmentId)
        {
            var globalId = CommonHelper.GenerateGlobalId(shipmentId, EntityType.Shipment);
            var result = await Repository.Query(m => m.GlobalId == globalId, null, x => x.Include(m => m.MasterDialog)).ToListAsync();

            return Mapper.Map<IEnumerable<GlobalIdMasterDialogViewModel>>(result);
        }

        public async Task<IEnumerable<GlobalIdMasterDialogViewModel>> GetByRoutingOrderIdAsync(long routingOrderId)
        {
            var globalId = CommonHelper.GenerateGlobalId(routingOrderId, EntityType.RoutingOrder);
            var result = await Repository.Query(m => m.GlobalId == globalId, null, x => x.Include(m => m.MasterDialog)).ToListAsync();

            return Mapper.Map<IEnumerable<GlobalIdMasterDialogViewModel>>(result);
        }
    }
}
