using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.Note.Services.Interfaces;
using Groove.SP.Application.Cruise.CruiseOrderWarehouseInfos.Services.Interfaces;
using Groove.SP.Application.CruiseOrders.Services.Interfaces;
using Groove.SP.Application.PurchaseOrders.ViewModels; 

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CruiseOrderItemsController : ControllerBase
    {
        private readonly ICruiseOrderService _cruiseOrderService;
        private readonly ICruiseOrderItemService _cruiseOrderItemService;
        private readonly ICruiseOrderWarehouseInfoService _warehouseService;
        private readonly INoteService _noteService;

        public CruiseOrderItemsController(
            ICruiseOrderWarehouseInfoService warehouseService,
            INoteService noteService,
            ICruiseOrderService cruiseOrderService,
            ICruiseOrderItemService cruiseOrderItemService
            )
        {
            _noteService = noteService;
            _warehouseService = warehouseService;
            _cruiseOrderService = cruiseOrderService;
            _cruiseOrderItemService = cruiseOrderItemService;
        }

        #region Warehouses
        [HttpGet("{id}/warehouses")]
        [AppAuthorize(AppPermissions.CruiseOrder_Detail)]
        public async Task<IActionResult> GetWarehousesByCruiseOrderItem(long id)
        {
            var result = await _warehouseService.GetByCruiseOrderItemIdAsync(id);
            return Ok(result);
        }
        #endregion

        #region Dialogs
        [HttpGet("{id}/notes")]
        [AppAuthorize(AppPermissions.CruiseOrder_Detail)]
        public async Task<IActionResult> GetNotesByCruiseOrderItem(long id)
        {
            var result = await _noteService.GetCruiseOrderItemNotesByIdAsync(id);
            return new JsonResult(result);
        }
        #endregion

        #region Details
        [HttpPost]
        [AppAuthorize(AppPermissions.CruiseOrder_Detail)]
        public async Task<IActionResult> PostAsync([FromBody] CruiseOrderItemViewModel model)
        {
            var result = await _cruiseOrderItemService.CreateCruiseOrderItemAsync(model, CurrentUser.Username);
            return Ok(result);
        }

        /// <summary>
        /// To delete cruise order item which was copied from another via GUI (<em>contains value OriginalItemId</em>)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [AppAuthorize(AppPermissions.CruiseOrder_Detail)]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _cruiseOrderItemService.DeleteCruiseOrderItemAsync(id, CurrentUser);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [AppAuthorize(AppPermissions.CruiseOrder_Detail)]
        public async Task<IActionResult> ReviseAsync(long id, [FromBody] ReviseCruiseOrderItemViewModel model)
        {
            var result = await _cruiseOrderItemService.ReviseCruiseOrderItemAsync(id, model, CurrentUser.Username);
            return Ok(result);
        }
        #endregion
    }
}
