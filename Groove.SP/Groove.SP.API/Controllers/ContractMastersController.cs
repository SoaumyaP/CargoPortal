using Groove.SP.API.Filters;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.ContractMaster.ViewModels;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.MasterBillOfLading.Services.Interfaces;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractMastersController : ControllerBase
    {
        private readonly IContractMasterService _contractMasterService;
        public ContractMastersController(
            IContractMasterService contractMasterService)
        {
            _contractMasterService = contractMasterService;
        }

        [HttpGet("search")]
        [AppAuthorize(AppPermissions.Contract_List)]
        public async Task<IActionResult> SearchAsync([DataSourceRequest] DataSourceRequest request)
        {
            var viewModels = await _contractMasterService.ListAsync(request);
            return new JsonResult(viewModels);
        }

        /// <summary>
        /// To fetch data for Carrier contract no combo box as creating new master bill of lading, via GUI
        /// </summary>
        /// <param name="searchTerm">Text to search</param>
        /// <param name="carrierCode">Carrier code/SCAC to search</param>
        /// <param name="currentDate">Current date without time</param>
        /// <returns></returns>
        [HttpGet]
        [Route("masterBOLContractMasterOptions")]
        public async Task<IActionResult> GetMasterBOLContractMasterOptionsAsync(string searchTerm, string carrierCode, DateTime currentDate)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                throw new System.ApplicationException("Filter set is not correct.");
            }
            var viewModels = await _contractMasterService.GetMasterBOLContractMasterOptionsAsync(searchTerm, carrierCode, currentDate, CurrentUser);
            return new JsonResult(viewModels);
        }

        /// <summary>
        /// To fetch data for Carrier contract no combo box as editing shipment, via GUI
        /// </summary>
        /// <param name="searchTerm">Text to search</param>
        /// <param name="modeOfTransport">Mode of transport to search</param>
        /// <param name="currentDate">Current date without time</param>
        /// <returns></returns>
        [HttpGet]
        [Route("shipmentContractMasterOptions")]
        public async Task<IActionResult> GetShipmentContractMasterOptionsAsync(string searchTerm, long shipmentId, DateTime currentDate)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                throw new System.ApplicationException("Filter set is not correct.");
            }
            var viewModels = await _contractMasterService.GetShipmentContractMasterOptionsAsync(searchTerm, shipmentId, currentDate, CurrentUser);
            return new JsonResult(viewModels);
        }

        [HttpGet("{id}")]
        [AppAuthorize(AppPermissions.Contract_Detail)]
        public async Task<IActionResult> GetAsync(long id)
        {
            var viewModels = await _contractMasterService.GetByKeyAsync(id);
            return Ok(viewModels);
        }

        [HttpGet("{contractNo}/already-exists")]
        [AppAuthorize(AppPermissions.Contract_Detail)]
        public async Task<IActionResult> CheckAlreadyExistsAsync(string contractNo)
        {
            var isAlreadyExits = await _contractMasterService.CheckContractAlreadyExistsAsync(contractNo);
            return Ok(isAlreadyExits);

        }

        [HttpPost]
        [AppAuthorize()]
        public async Task<IActionResult> PostAsync(CreateContractMasterViewModel viewModel)
        {
            viewModel.AuditForAPI(CurrentUser.Username, false);
            await _contractMasterService.RemapContractHolderAsync(viewModel);
            var result = await _contractMasterService.CreateAsync(viewModel);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [AppAuthorize()]
        public async Task<IActionResult> UpdateAsync(long id, UpdateContractMasterViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                throw new AppEntityNotFoundException($"Object not found!");
            }

            viewModel.AuditForAPI(CurrentUser.Username, true);
            await _contractMasterService.RemapContractHolderAsync(viewModel);
            var result = await _contractMasterService.UpdateAsync(viewModel, id);
            return Ok(result);
        }

        [HttpPost("internal")]
        [AppAuthorize(AppPermissions.Contract_Detail_Add)]
        public async Task<IActionResult> PostInternalAsync(ContractMasterQueryModel viewModel)
        {
            var result = await _contractMasterService.CreateAsync(viewModel, CurrentUser.Username);
            return Ok(result);
        }

        [HttpPut("internal/{id}")]
        [AppAuthorize(AppPermissions.Contract_Detail_Edit)]
        public async Task<IActionResult> UpdateInternalAsync(long id, ContractMasterQueryModel viewModel)
        {
            var result = await _contractMasterService.UpdateAsync(id, viewModel, CurrentUser.Username);
            return Ok(result);
        }

        [HttpPut("internal/{id}/updateStatus")]
        [AppAuthorize(AppPermissions.Contract_Detail_Edit)]
        public async Task<IActionResult> UpdateStatusAsync(long id, ContractMasterQueryModel viewModel)
        {
            await _contractMasterService.UpdateStatusAsync(id, viewModel.Status, CurrentUser.Username);
            return Ok();
        }

        [HttpDelete("{id}")]
        [AppAuthorize()]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _contractMasterService.DeleteByKeysAsync(id);
            return Ok(result);
        }
    }
}
