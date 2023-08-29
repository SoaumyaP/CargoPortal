using Groove.SP.API.Filters;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.MasterDialog.Services.Interfaces;
using Groove.SP.Application.MasterDialog.ViewModels;
using Groove.SP.Core.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterDialogsController : ControllerBase
    {
        public readonly IMasterDialogService _masterDialogService;

        public MasterDialogsController(IMasterDialogService masterDialogService)
        {
            _masterDialogService = masterDialogService;
        }

        [HttpGet("{id}")]
        [AppAuthorize(AppPermissions.Shipment_MasterDialog_List)]
        public async Task<IActionResult> GetByKey(long id)
        {
            var viewModels = await _masterDialogService.GetAsync(id, CurrentUser.IsInternal, CurrentUser.OrganizationId);
            return new JsonResult(viewModels);
        }

        [HttpPost]
        [AppAuthorize(AppPermissions.Shipment_MasterDialog_List_Add)]
        public async Task<IActionResult> Create(MasterDialogViewModel viewModel)
        {
            var viewModels = await _masterDialogService.CreateAsync(viewModel, CurrentUser.Username);
            return new JsonResult(viewModels);
        }

        [HttpPut("{id}")]
        [AppAuthorize(AppPermissions.Shipment_MasterDialog_List_Edit)]
        public async Task<IActionResult> Update(long id, MasterDialogViewModel viewModel)
        {
            var viewModels = await _masterDialogService.UpdateAsync(id, viewModel, CurrentUser.Username, CurrentUser.IsInternal, CurrentUser.OrganizationId);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("search")]
        [AppAuthorize(AppPermissions.Shipment_MasterDialog_List)]
        public async Task<IActionResult> Search([DataSourceRequest] DataSourceRequest request, string affiliates)
        {
            var viewModels = await _masterDialogService.ListAsync(request, CurrentUser.IsInternal, affiliates, CurrentUser.OrganizationId);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("searchNumberByFilterCriteria")]
        [AppAuthorize(AppPermissions.Shipment_MasterDialog_List_Edit, AppPermissions.Shipment_MasterDialog_List_Add)]
        public async Task<IActionResult> SearchNumberByFilterCriteria(string filterValue, string filterCriteria)
        {
            var viewModels = await _masterDialogService.SearchNumberByFilterCriteriaAsync(filterCriteria, filterValue, CurrentUser.IsInternal, CurrentUser.OrganizationId);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("searchListOfPurchaseOrders")]
        [AppAuthorize(AppPermissions.Shipment_MasterDialog_List_Edit, AppPermissions.Shipment_MasterDialog_List_Add)]
        public async Task<IActionResult> SearchListOfPurchaseOrders(string messageShownOn, string filterCriteria, string filterValue, string searchTerm, int skip, int take)
        {
            var viewModels = await _masterDialogService.SearchListOfPurchaseOrdersAsync(messageShownOn, filterCriteria, filterValue, searchTerm, skip, take);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}/searchListOfPurchaseOrdersById")]
        [AppAuthorize(AppPermissions.Shipment_MasterDialog_List_Edit, AppPermissions.Shipment_MasterDialog_List_Add)]
        public async Task<IActionResult> SearchListOfPurchaseOrdersByMasterDialogId(long id, string searchTerm, int skip, int take)
        {
            var viewModels = await _masterDialogService.SearchListOfPurchaseOrdersByMasterDialogIdAsync(id, searchTerm, skip, take);
            return new JsonResult(viewModels);
        }

        [HttpDelete("{id}")]
        [AppAuthorize(AppPermissions.Shipment_MasterDialog_List_Edit)]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _masterDialogService.DeleteByKeysAsync(id, CurrentUser.IsInternal, CurrentUser.OrganizationId);
            return Ok(result);
        }
    }
}