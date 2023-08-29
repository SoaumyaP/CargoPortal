using System.Threading.Tasks;
using Groove.CSFE.Application.Authorization;
using Groove.CSFE.Application.Vessels.Services.Interfaces;
using Groove.CSFE.Application.Vessels.ViewModels;
using Groove.CSFE.CommonAPI.Filters;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Data;
using Microsoft.AspNetCore.Mvc;

namespace Groove.CSFE.CommonAPI.Controllers
{
    [Route("api/vessels/internal")]
    [ApiController]
    [AppAuthorize]
    public class InternalVesselsController : ControllerBase
    {
        private readonly IVesselService _vesselService;

        public InternalVesselsController(IVesselService vesselService)
        {
            _vesselService = vesselService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchAsync([DataSourceRequest] DataSourceRequest request, string affiliates)
        {
            var viewModels = await _vesselService.ListAsync(request, CurrentUser.IsInternal, affiliates);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(string filterType)
        {
            if (string.IsNullOrEmpty(filterType))
            {
                var vesselViewModels = await _vesselService.GetAllAsync();
                return Ok(vesselViewModels);
            }

            switch (filterType.ToLower())
            {
                case "realactive":
                    return Ok(await _vesselService.GetRealActiveListAsync());
                case "active":
                    return Ok(await _vesselService.GetActiveListAsync());
            }
            return Ok(await _vesselService.GetAllAsync());

        }

        [HttpGet]
        [Route("searchRealActive")]
        public async Task<IActionResult> SearchRealActiveAsync(string name)
        {
            var result = await _vesselService.SearchRealActiveByNameAsync(name);
            return Ok(result);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetByIdAsync(long id)
        {
            var vesselViewModel = await _vesselService.GetByKeysAsync(id);
            return Ok(vesselViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(VesselViewModel viewModel)
        {
            viewModel.Audit(CurrentUser.Username);
            viewModel.MarkAuditFieldStatus();
            var vesselCreated = await _vesselService.CreateAsync(viewModel);
            return Ok(vesselCreated);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(long id, VesselViewModel viewModel)
        {
            viewModel.Audit(CurrentUser.Username);
            viewModel.MarkAuditFieldStatus(true);
            var vesselUpdated = await _vesselService.UpdateAsync(viewModel, id);
            return Ok(vesselUpdated);
        }

        [HttpPut("{id}/updateStatus")]
        [AppAuthorize(AppPermissions.Organization_Vessel_Detail_Edit)]
        public async Task<IActionResult> UpdateStatusAsync(long id, VesselViewModel viewModel)
        {
            var vesselUpdated = await _vesselService.UpdateStatusAsync(id, viewModel.Status ?? VesselStatus.Inactive, CurrentUser.Username);
            return Ok(vesselUpdated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _vesselService.DeleteAsync(id);
            return Ok(result);
        }
    }
}
