using Groove.SP.API.Models;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Application.WarehouseFulfillment.Services.Interfaces;
using Groove.SP.Application.WarehouseFulfillment.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    /// <summary>
    /// WarehouseBookings will be called by ediSON
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseBookingsController : ControllerBase
    {
        private readonly IWarehouseFulfillmentService _warehouseFulfillmentService;
        public WarehouseBookingsController(
            IWarehouseFulfillmentService warehouseFulfillmentService)
        {
            _warehouseFulfillmentService = warehouseFulfillmentService;
        }       

        [HttpPost]
        [Route("import")]
        [AppAuthorize]
        public async Task<IActionResult> ImportFromExcel([FromForm] ImportWarehouseBookingViewModel model, [FromQuery] string profile)
        {
            var importBookingResult = await _warehouseFulfillmentService.ImportWarehouseBookingSilentAsync(model, profile);

            if (!importBookingResult.Success)
            {
                // If there is exception
                if (importBookingResult.Results.ContainsKey("errors")) {
                    // Exception is sensitive, not expose outside
                    importBookingResult.Exception = null;
                    return StatusCode(500, importBookingResult);
                }
                // Else, validation failed
                return BadRequest(importBookingResult);
            }

            // Created
            return Ok(importBookingResult);
        }

        [HttpPost]
        [Route("cargoreceives")]
        [AppAuthorize]
        public async Task<IActionResult> ImportCargoReceive([FromBody] ImportPOFulfillmentCargoReceiveViewModel model)
        {
            await _warehouseFulfillmentService.ImportCargoReceiveAsync(model, CurrentUser.Username);
            return Ok();
        }

        #region Mobile app APIs
               
        [HttpGet("{bookingNumber:required}/cargoReceive")]
        [Authorize(Policy = AppConstants.SECURITY_MOBILE_APP_POLICY)]
        public async Task<IActionResult> GetCargoReceiveAsync(string bookingNumber)
        {
            var viewModel = await _warehouseFulfillmentService.GetWarehouseCargoReceiveAsync(bookingNumber);
            return new JsonResult(viewModel);
        }

        [HttpPost("{bookingNumber:required}/fullCargoReceive")]
        [Authorize(Policy = AppConstants.SECURITY_MOBILE_APP_POLICY)]
        public async Task<IActionResult> FullCargoReceiveAsync(string bookingNumber)
        {
            try
            {
                var result = await _warehouseFulfillmentService.FullCargoReceiveAsync(bookingNumber);

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new
                {
                    ex.Message
                });
            }
        }

        #endregion Mobile app APIs
    }
}
