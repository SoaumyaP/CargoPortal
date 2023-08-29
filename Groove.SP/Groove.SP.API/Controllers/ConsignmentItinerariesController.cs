using System.Threading.Tasks;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.Consignment.Services.Interfaces;
using Groove.SP.Application.Consignment.ViewModels;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Itinerary.Services.Interfaces;
using Groove.SP.Application.PurchaseOrders.Services.Interfaces;
using Groove.SP.Application.Shipments.Services.Interfaces;
using Groove.SP.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
    public class ConsignmentItinerariesController : ControllerBase
    {
        public readonly IConsignmentItineraryService _consignmentItineraryService;
        private readonly IItineraryService _itineraryService;
        private readonly IRepository<ConsignmentItineraryModel> _consignmentItineraryRepository;
        private readonly IPurchaseOrderService _purchaseOrderService;

        public ConsignmentItinerariesController(
            IConsignmentItineraryService consignmentItineraryService,
            IItineraryService itineraryService,
            IPurchaseOrderService purchaseOrderService,
            IRepository<ConsignmentItineraryModel> consignmentItineraryRepository
            )
        {
            _consignmentItineraryService = consignmentItineraryService;
            _itineraryService = itineraryService;
            _consignmentItineraryRepository = consignmentItineraryRepository;
            _purchaseOrderService = purchaseOrderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery]long consignmentId, [FromQuery]long itineraryId)
        {
            var result = await _consignmentItineraryService.GetAsync(consignmentId, itineraryId);
            return new JsonResult(result);
        }

        [HttpPost]
        
        public async Task<IActionResult> PostAsync([FromBody]ConsignmentItineraryViewModel model)
        {
            var result = await _consignmentItineraryService.ConfirmItineraryToShipmentAsync(model);
            return new JsonResult(result);
        }


        [HttpPut]

        public async Task<IActionResult> PutAsync([FromQuery] long consignmentId, [FromQuery] long itineraryId, [FromBody] ConsignmentItineraryViewModel model)
        {
            var consignmentItinerary = await _consignmentItineraryRepository.GetAsNoTrackingAsync(c =>
             c.ConsignmentId == consignmentId && c.ItineraryId == itineraryId
            , null
            , c => c.Include(s => s.Shipment).ThenInclude(s => s.POFulfillment));

            var oldShipmentId = consignmentItinerary?.ShipmentId;
            var oldBooking = consignmentItinerary?.Shipment?.POFulfillment;

            ConsignmentItineraryViewModel result = null;
            if (consignmentItinerary != null)
            {
                result = await _consignmentItineraryService.UpdateAsync(model, consignmentId, itineraryId);
            }
            await _itineraryService.ChangeStageOfBookingAndPOAsync(model.ShipmentId);
            if (oldShipmentId.HasValue)
            {
                await _itineraryService.ChangeStageOfBookingAndPOAsync(oldShipmentId, oldBooking);
            }
            return new JsonResult(result);
        }

        [HttpDelete]
        
        public async Task<IActionResult> DeleteAsync([FromQuery]long consignmentId, [FromQuery]long itineraryId)
        {
            var consignmentItinerary= await _consignmentItineraryRepository.GetAsNoTrackingAsync(c => 
            c.ConsignmentId == consignmentId && c.ItineraryId == itineraryId
            ,null
            ,c=>c.Include(s=>s.Shipment).ThenInclude(s=>s.POFulfillment));

            var booking = consignmentItinerary?.Shipment?.POFulfillment;

            var result = await _consignmentItineraryService.DeleteByKeysAsync(consignmentId, itineraryId);
            if (consignmentItinerary?.ShipmentId != null)
            {
                await _itineraryService.ChangeStageOfBookingAndPOAsync(consignmentItinerary.ShipmentId,booking);
            }
            return Ok(result);
        }
    }
}
