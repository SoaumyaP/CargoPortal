using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.BuyerApproval.Services.Interfaces;
using Groove.SP.Application.BuyerApproval.ViewModels;
using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Core.Data;
using Groove.SP.API.Filters;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuyerApprovalsController : ControllerBase
    {
        private readonly IBuyerApprovalService _buyerApprovalService;
        private readonly IPOFulfillmentService _poFulfillmentService;


        public BuyerApprovalsController(
            IBuyerApprovalService buyerApprovalService,
            IPOFulfillmentService poFulfillmentService)
        {
            _buyerApprovalService = buyerApprovalService;
            _poFulfillmentService = poFulfillmentService;
        }

        [HttpGet]
        [Route("search")]
        [AppAuthorize(AppPermissions.Order_PendingApprovalList)]
        public async Task<IActionResult> List([DataSourceRequest] DataSourceRequest request, string affiliates, long? organizationId = 0, string userRole = "", string statisticKey = "")
        {
            var viewModels = await _buyerApprovalService.ListAsync(request, CurrentUser.IsInternal, CurrentUser.Username, affiliates, organizationId, userRole, statisticKey);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}")]
        [AppAuthorize(AppPermissions.Order_PendingApproval_Detail)]
        public async Task<IActionResult> Get(long id, string affiliates)
        {
            var viewModels = await _buyerApprovalService.GetAsync(id, CurrentUser.IsInternal, CurrentUser.Username, affiliates);
            return new JsonResult(viewModels);
        }

        [HttpPut("{id}/Approve")]
        [AppAuthorize(AppPermissions.Order_PendingApproval_Detail)]
        public async Task<IActionResult> Approve(long id, [FromBody] BuyerApprovalViewModel viewModel)
        {
            viewModel.Audit(viewModel.Id, CurrentUser.Username);
            var viewModels = await _buyerApprovalService.ApproveAsync(id, viewModel, CurrentUser.Username);
            return new JsonResult(viewModels);
        }

        [HttpPut("{id}/Reject")]
        [AppAuthorize(AppPermissions.Order_PendingApproval_Detail)]
        public async Task<IActionResult> Reject(long id, [FromBody] BuyerApprovalViewModel viewModel)
        {
            viewModel.Audit(viewModel.Id, CurrentUser.Username);
            var viewModels = await _buyerApprovalService.RejectAsync(id, viewModel, CurrentUser.Username);
            return new JsonResult(viewModels);
        }
    }
}
