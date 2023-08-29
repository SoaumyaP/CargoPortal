using Groove.SP.Application.Common;
using Groove.SP.Application.Consignment.Services.Interfaces;
using Groove.SP.Application.Consignment.ViewModels;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Itinerary.Services.Interfaces;
using Groove.SP.Application.PurchaseOrders.Services.Interfaces;
using Groove.SP.Core.Entities;
using System.Threading.Tasks;

namespace Groove.SP.Application.Consignment.Services
{
    public class ConsignmentItineraryService : ServiceBase<ConsignmentItineraryModel, ConsignmentItineraryViewModel>, IConsignmentItineraryService
    {
        private readonly IItineraryService _itineraryService;
        private readonly IPurchaseOrderService _purchaseOrderService;

        public ConsignmentItineraryService(IItineraryService itineraryService,
            IPurchaseOrderService purchaseOrderService,
            IUnitOfWorkProvider unitOfWorkProvider)
            : base(unitOfWorkProvider)
        {
            _itineraryService = itineraryService;
            _purchaseOrderService = purchaseOrderService;
        }

        public async Task<ConsignmentItineraryViewModel> ConfirmItineraryToShipmentAsync(ConsignmentItineraryViewModel model)
        {
            var result = await CreateAsync(model);
            await _itineraryService.ChangeStageOfBookingAndPOAsync(model.ShipmentId);
            await _purchaseOrderService.ChangeStagePOWithoutBookingAsync(model.ConsignmentId);
            await _itineraryService.UpdateCYClosingDateToPOFulfillmentAsync(model.ConsignmentId);
            return result;

        }
    }
}
