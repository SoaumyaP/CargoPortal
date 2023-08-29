using Groove.SP.API.Filters;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.GlobalIdMasterDialog.Services.Interfaces;
using Groove.SP.Application.Note.Services.Interfaces;
using Groove.SP.Application.RoutingOrder.Services;
using Groove.SP.Application.RoutingOrder.Services.Interfaces;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Data;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.FormulaParsing.Utilities;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoutingOrdersController : ControllerBase
    {
        private readonly IRoutingOrderListService _routingOrderListService;
        private readonly IRoutingOrderService _routingOrderService;
        private readonly INoteService _noteService;
        private readonly IGlobalIdMasterDialogService _globalIdMasterDialogService;

        public RoutingOrdersController(IRoutingOrderListService routingOrderListService,
            IRoutingOrderService routingOrderService,
            INoteService noteService,
            IGlobalIdMasterDialogService globalIdMasterDialogService)
        {
            _routingOrderListService = routingOrderListService;
            _routingOrderService = routingOrderService;
            _noteService = noteService;
            _globalIdMasterDialogService = globalIdMasterDialogService;
        }

        [HttpGet]
        [Route("search")]
        [AppAuthorize(AppPermissions.RoutingOrder_List)]
        public async Task<IActionResult> SearchAsync([DataSourceRequest] DataSourceRequest request, string affiliates, string customerRelationships = "", bool isExport = false, long? organizationId = 0)
        {
            var result = await _routingOrderListService.ListAsync(request, CurrentUser.IsInternal, affiliates, customerRelationships, organizationId);

            return new JsonResult(result);
        }

        [HttpPost]
        [Route("import")]
        [AppAuthorize(Scope = "spapi.importroutingorder")]
        public async Task<IActionResult> ImportAsync([FromForm] IFormFile routingOrderForm)
        {
            var result = await _routingOrderService.ImportXMLAsync(
                routingOrderForm.GetAllBytes(),
                routingOrderForm.FileName,
                CurrentUser.Username);
            switch (result.Type)
            {
                case Application.RoutingOrder.ViewModels.ImportingRoutingOrderResult.ValidationFailed:
                    return BadRequest(result);
                case Application.RoutingOrder.ViewModels.ImportingRoutingOrderResult.ErrorDuringImport:
                    return StatusCode(500, result);
                case Application.RoutingOrder.ViewModels.ImportingRoutingOrderResult.Success:
                    return Ok(result);
                default:
                    break;
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("{routingOrderId}")]
        [AppAuthorize(AppPermissions.RoutingOrder_Detail)]
        public async Task<IActionResult> GetByIdAsync(long routingOrderId, string affiliates)
        {
            var result = await _routingOrderService.GetByIdAsync(routingOrderId, CurrentUser.IsInternal, affiliates);

            return new JsonResult(result);
        }

        [HttpGet("{id}/notes")]
        [AppAuthorize(AppPermissions.RoutingOrder_Detail)]
        public async Task<IActionResult> GetNotesAsync(long id)
        {
            var result = await _noteService.GetRoutingOrderNotesAsync(id);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/MasterDialogs")]
        [AppAuthorize(AppPermissions.RoutingOrder_Detail)]
        public async Task<IActionResult> GetMasterDialogsAsync(long id)
        {
            var result = await _globalIdMasterDialogService.GetByRoutingOrderIdAsync(id);
            return new JsonResult(result);
        }
    }
}