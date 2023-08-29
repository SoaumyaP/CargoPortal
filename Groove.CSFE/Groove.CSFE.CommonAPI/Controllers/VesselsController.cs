using Groove.CSFE.Application.Authorization;
using Groove.CSFE.Application.Vessels.Services.Interfaces;
using Groove.CSFE.Application.Vessels.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.CSFE.CommonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    public class VesselsController : ControllerBase
    {
        private readonly IVesselService _vesselService;

        public VesselsController(IVesselService vesselService)
        {
            _vesselService = vesselService;
        }

        /// <summary>
        /// Called from third-party via public API
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        public async Task<IActionResult> CreateAsync(CreateVesselViewModel viewModel)
        {
            viewModel.AuditForAPI(CurrentUser.Username,false);
            var vesselCreated = await _vesselService.CreateAsync(viewModel);
            return Ok(vesselCreated);
        }

        /// <summary>
        /// Called from third-party via public API
        /// </summary>
        /// <param name="id"></param>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        public async Task<IActionResult> UpdateAsync(long id, UpdateVesselViewModel viewModel)
        {
            viewModel.AuditForAPI(CurrentUser.Username,true);
            var vesselUpdated = await _vesselService.UpdateAsync(viewModel, id);
            return Ok(vesselUpdated);
        }

        /// <summary>
        /// Called from third-party via public API
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _vesselService.DeleteAsync(id);
            return Ok(result);
        }
    }
}
