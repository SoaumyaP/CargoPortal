using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.Shipments.Services.Interfaces;
using Groove.SP.Application.BillOfLading.Services.Interfaces;
using Groove.SP.Application.Container.Services.Interfaces;
using Groove.SP.Application.MasterBillOfLading.Services.Interfaces;
using Groove.SP.Application.MasterBillOfLadingContact.Services.Interfaces;
using Groove.SP.Application.Attachment.Services.Interfaces;
using Groove.SP.Application.MasterBillOfLading.ViewModels;
using Groove.SP.Application.MasterBillOfLadingContact.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Groove.SP.Core.Models;
using Groove.SP.Application.Itinerary.Services.Interfaces;
using System;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Data;
using Groove.SP.API.Filters;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterBillOfLadingsController : ControllerBase
    {
        private readonly IMasterBillOfLadingService _masterBOLService;
        private readonly IBillOfLadingService _bolService;
        private readonly IContainerService _containerService;
        private readonly IShipmentService _shipmentService;
        private readonly IMasterBillOfLadingContactService _masterBOLContactService;
        private readonly IAttachmentService _attachmentService;
        private readonly IItineraryService _itineraryService;

        public MasterBillOfLadingsController(IMasterBillOfLadingService masterBOLService,
            IContainerService containerService,
            IShipmentService shipmentService,
            IMasterBillOfLadingContactService masterBOLContactService,
            IAttachmentService attachmentService,
            IBillOfLadingService bolService,
            IItineraryService itineraryService)
        {
            _masterBOLService = masterBOLService;
            _bolService = bolService;
            _containerService = containerService;
            _shipmentService = shipmentService;
            _masterBOLContactService = masterBOLContactService;
            _attachmentService = attachmentService;
            _itineraryService = itineraryService;
        }

        #region Master Bills

        [HttpGet]
        [Route("search")]
        [AppAuthorize(AppPermissions.BillOfLading_ListOfMasterBL)]
        public async Task<IActionResult> Search([DataSourceRequest] DataSourceRequest request, string affiliates)
        {
            var viewModels = await _masterBOLService.ListAsync(request, CurrentUser.IsInternal, CurrentUser.UserRoleId, CurrentUser.OrganizationId, affiliates);

            return new JsonResult(viewModels);
        }

        /// <summary>
        /// To fetch data for auto-complete called from GUI as assign master bl to house bl
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="isDirectMaster"></param>
        /// <param name="affiliates"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("searchByNumber")]
        [AppAuthorize(AppPermissions.BillOfLading_MasterBLDetail_Add)]
        public async Task<IActionResult> GetMasterBillOfLadingListBySearchingNumberAsync(string searchTerm, bool isDirectMaster, string affiliates)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                throw new System.ApplicationException("Filter set is not correct.");
            }
            var viewModels = await _masterBOLService.GetMasterBillOfLadingListBySearchingNumberAsync(searchTerm, isDirectMaster, CurrentUser.IsInternal, affiliates);
            return new JsonResult(viewModels);
        }


        [HttpGet]
        [Route("{id}")]
        [AppAuthorize(AppPermissions.BillOfLading_MasterBLDetail)]
        public async Task<IActionResult> Get(long id, string affiliates)
        {
            var result = await _masterBOLService.GetAsync(id, CurrentUser.IsInternal, affiliates);
            var checkingRoles = new[] { (long)Role.Shipper, (long)Role.Principal, (long)Role.CruisePrincipal, (long)Role.Agent };

            // Checking Shipper/Principal/Cruise Principal/Agent can ONLY see the value of Contract No if their OrganizationId is matched with ContractHolder 
            if (checkingRoles.Contains(CurrentUser.UserRoleId) && result.ContractMaster != null)
            {
                if (result.ContractMaster.ContractHolder != CurrentUser.OrganizationId.ToString())
                {
                    result.ContractMaster.RealContractNo = null;
                }
            }
            return new JsonResult(result);
        }

        [HttpPost]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        public async Task<IActionResult> PostAsync([FromBody] CreateMasterBillOfLadingViewModel model)
        {
            // Not need to audit API as ediSON fulfilled audit information
            var result = await _masterBOLService.CreateAsync(model, CurrentUser.Username);
            return new JsonResult(result);
        }              

        [HttpPut("{id}")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        public async Task<IActionResult> PutAsync(long id, [FromBody] UpdateMasterBillOfLadingViewModel model)
        {
            // Not need to audit API as ediSON fulfilled audit information
            var result = await _masterBOLService.UpdateAsync(model, CurrentUser.Username, id);
            return new JsonResult(result);
        }

        /// <summary>
        /// To update master bill of lading via GUI
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [AppAuthorize(AppPermissions.BillOfLading_MasterBLDetail_Edit)]
        [Route("internal/{id}")]
        public async Task<IActionResult> PutAsync(long id, [FromBody] MasterBillOfLadingViewModel model)
        {
            // Manually audit, ignore CreateBy/CreatedDate
            var createdBy = model.CreatedBy;
            var createdDate = model.CreatedDate;
            model.Audit(CurrentUser.Username);
            model.CreatedBy = createdBy;
            model.CreatedDate = createdDate;

            var result = await _masterBOLService.UpdateAsync(model, CurrentUser.Username, id);
            return new JsonResult(result);
        }

        [HttpDelete("{id}")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]

        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _masterBOLService.DeleteByKeysAsync(id);
            return new JsonResult(result);
        }

        #endregion

        #region Contacts

        [HttpGet("{id}/contacts")]
        [AppAuthorize(AppPermissions.BillOfLading_MasterBLDetail)]
        public async Task<IActionResult> GetContacts(long id)
        {
            var viewModel = await _masterBOLContactService.GetContactsByMasterBOLIdAsync(id);

            if (viewModel != null)
            {
                // "Delegation party" displayed only for Admin/Internal/Shipper/Supplier/Delegation/OriginAgent
                var allowedOrganizationRoles = new List<string> { OrganizationRole.Shipper, OrganizationRole.Supplier, OrganizationRole.Delegation, OrganizationRole.OriginAgent };
                var allowedOrganizationIds = viewModel.ToList().Where(x => allowedOrganizationRoles.Contains(x.OrganizationRole)).Select(x => x.OrganizationId);

                if (!CurrentUser.IsInternal
                    && allowedOrganizationIds != null
                    && allowedOrganizationIds.Any()
                    && !allowedOrganizationIds.Contains(CurrentUser.OrganizationId)
                    )
                {
                    viewModel = viewModel.Where(x => !x.OrganizationRole.Equals(OrganizationRole.Delegation));
                }
            }

            return new JsonResult(viewModel);
        }

        [HttpPost("{id}/contacts")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]

        public async Task<IActionResult> PostContactAsync(long id, [FromBody] MasterBillOfLadingContactViewModel model)
        {
            model.MasterBillOfLadingId = id;
            var result = await _masterBOLContactService.CreateAsync(model);
            return new JsonResult(result);
        }

        [HttpPut("{id}/contacts/{contactId}")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]

        public async Task<IActionResult> PutContactAsync(long id, long contactId, [FromBody] MasterBillOfLadingContactViewModel model)
        {
            model.MasterBillOfLadingId = id;
            var result = await _masterBOLContactService.UpdateAsync(model, contactId);
            return new JsonResult(result);
        }

        [HttpDelete("{id}/contacts/{contactId}")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]

        public async Task<IActionResult> DeleteContactAsync(long contactId)
        {
            var result = await _masterBOLContactService.DeleteByKeysAsync(contactId);
            return new JsonResult(result);
        }

        #endregion

        [HttpGet]
        [Route("{id}/billOfLadings")]
        [AppAuthorize(AppPermissions.BillOfLading_MasterBLDetail)]
        public async Task<IActionResult> GetBillOfLadings(long id, string affiliates)
        {
            var result = await _bolService.GetBOLsByMasterBOLAsync(id, CurrentUser.IsInternal, affiliates);
            return new JsonResult(result);
        }

        /// <summary>
        /// To load data grid of containers on master bill of lading details page
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isDirectMaster"></param>
        /// <param name="affiliates"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/containers")]
        [AppAuthorize(AppPermissions.BillOfLading_MasterBLDetail)]
        public async Task<IActionResult> GetContainers(long id, bool isDirectMaster)
        {
            var result = await _containerService.GetContainersByMasterBOLAsync(id, isDirectMaster);
            return new JsonResult(result);
        }

        /// <summary>
        /// To load data grid of shipments on master bill of lading details page
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isDirectMaster"></param>
        /// <param name="affiliates"></param>
        /// <returns></returns>
        [HttpGet()]
        [Route("{id}/shipments")]
        [AppAuthorize(AppPermissions.BillOfLading_MasterBLDetail)]
        public async Task<IActionResult> GetShipments(long id, bool isDirectMaster, string affiliates)
        {
            var viewModel = await _shipmentService.GetShipmentsByMasterBOLAsync(id, isDirectMaster, CurrentUser.IsInternal, affiliates);
            return new JsonResult(viewModel);
        }

        [HttpGet()]
        [Route("{id}/attachments")]
        [AppAuthorize(AppPermissions.BillOfLading_MasterBLDetail)]
        public async Task<IActionResult> GetAttachments(long id)
        {
            var viewModel = await _attachmentService.GetAttachmentsCrossModuleAsync(EntityType.MasterBill, id, CurrentUser.UserRoleId, CurrentUser.OrganizationId);
            return new JsonResult(viewModel);
        }

        [HttpGet()]
        [Route("{id}/itineraries")]
        [AppAuthorize(AppPermissions.BillOfLading_MasterBLDetail)]
        public async Task<IActionResult> GetItineraries(long id)
        {
            var viewModel = await _itineraryService.GetItinerariesByMasterBOL(id);
            return new JsonResult(viewModel);
        }
        

        /// <summary>
        /// To assign house bill of lading to master bill of lading, via GUI
        /// </summary>
        /// <param name="houseBOLId"></param>
        /// <param name="masterBOLId"></param>
        /// <returns></returns>
        [HttpPut("{masterBOLId}/assignHouseBOL")]
        [AppAuthorize(AppPermissions.BillOfLading_MasterBLDetail_Add)]
        public async Task<IActionResult> AssignHouseBillOfLadingAsync(long masterBOLId, long houseBOLId)
        {
            await _masterBOLService.AssignHouseBLToMasterBLAsync(masterBOLId, houseBOLId, CurrentUser.Username);
            return new JsonResult(true);
        }

        /// <summary>
        /// To remove house bill of lading from master bill of lading, via GUI
        /// </summary>
        /// <param name="houseBOLId"></param>
        /// <param name="masterBOLId"></param>
        /// <returns></returns>
        [HttpPut("{masterBOLId}/removeHouseBOL")]
        [AppAuthorize(AppPermissions.BillOfLading_MasterBLDetail_Add)]
        public async Task<IActionResult> RemoveHouseBillOfLadingAsync(long masterBOLId, long houseBOLId)
        {
            await _masterBOLService.RemoveHouseBLFromMasterBLAsync(masterBOLId, houseBOLId, CurrentUser.Username);
            return new JsonResult(true);
        }
    }
}
