using Groove.SP.API.Filters;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.POFulfillmentShortshipOrder.Services.Interfaces;
using Groove.SP.Application.POFulfillmentShortshipOrder.ViewModels;
using Groove.SP.Core.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShortshipsController : ControllerBase
    {
        private readonly IPOFulfillmentShortshipOrderListService _poFulfillmentShortshipOrderListService;
        private readonly IPOFulfillmentShortshipOrderService _poFulfillmentShortshipOrderService;

        public ShortshipsController(
            IPOFulfillmentShortshipOrderListService poFulfillmentShortshipOrderListService,
            IPOFulfillmentShortshipOrderService poFulfillmentShortshipOrderService
            )
        {
            _poFulfillmentShortshipOrderListService = poFulfillmentShortshipOrderListService;
            _poFulfillmentShortshipOrderService = poFulfillmentShortshipOrderService;
        }

        [HttpGet]
        [Route("statistics/unread")]
        public async Task<IActionResult> CountUnreadAsync(string affiliates, long? orgId)
        {
            var numberOfUnread = await _poFulfillmentShortshipOrderService.CountUnreadAsync(CurrentUser.IsInternal, affiliates, orgId);
            return Ok(numberOfUnread);
        }

        [HttpPut]
        [Route("{id}/read-or-unread")]
        public async Task<IActionResult> ReadOrUnReadAsync(long id, POFulfillmentShortshipOrderViewModel viewModel)
        {
            await _poFulfillmentShortshipOrderService.ReadOrUnreadAsync(id, viewModel);
            return Ok();
        }

        [HttpGet]
        [Route("search")]
        [AppAuthorize(AppPermissions.Dashboard_Shortships_List)]
        public async Task<IActionResult> SearchListAsync([DataSourceRequest] DataSourceRequest request, string affiliates, long? organizationId = 0, string userRole = "", string statisticKey = "")
        {
            var viewModels = await _poFulfillmentShortshipOrderListService.SearchListAsync(request, CurrentUser.IsInternal, affiliates, organizationId, userRole);
            return new JsonResult(viewModels);
        }
    }
}