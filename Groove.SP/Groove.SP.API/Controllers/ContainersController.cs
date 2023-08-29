using System.Threading.Tasks;
using Groove.SP.API.Filters;
using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.Attachment.Services.Interfaces;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.Container.Services.Interfaces;
using Groove.SP.Application.Container.ViewModels;
using Groove.SP.Application.Itinerary.Services.Interfaces;
using Groove.SP.Application.Provider.Report;
using Groove.SP.Application.ShipmentLoadDetails.Services.Interfaces;
using Groove.SP.Application.Translations.Providers.Interfaces;
using Groove.SP.Core.Data;
using Groove.SP.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContainersController : ControllerBase
    {
        private readonly IContainerService _containerService;
        private readonly IActivityService _activityService;
        private readonly IItineraryService _itineraryService;
        private readonly IShipmentLoadDetailService _shipmentLoadDetailService;
        private readonly IAttachmentService _attachmentService;
        private readonly ITranslationProvider _translation;
        public ContainersController(
            IContainerService containerService,
            IActivityService activityService,
            IItineraryService itineraryService,
            IShipmentLoadDetailService shipmentLoadDetailService,
            IAttachmentService attachmentService,
            ITranslationProvider translation)
        {
            _containerService = containerService;
            _activityService = activityService;
            _itineraryService = itineraryService;
            _shipmentLoadDetailService = shipmentLoadDetailService;
            _attachmentService = attachmentService;
            _translation = translation;
        }

        [HttpGet]
        [Route("{containerNoOrId}")]
        [AppAuthorize(AppPermissions.Shipment_ContainerDetail)]
        public async Task<IActionResult> GetContainerDetail(string containerNoOrId, string affiliates)
        {
            var result = await _containerService.GetContainerAsync(containerNoOrId, CurrentUser.IsInternal, affiliates);
            return new JsonResult(result);
        }

        [HttpGet()]
        [Route("quicktrack/{containerNo}")]
        public async Task<IActionResult> GetQuickTrack(string containerNo)
        {
            var viewModel = await _containerService.GetQuickTrackAsync(containerNo);

            if (viewModel != null) return Ok(viewModel);

            return NotFound(new
            {
                message = await _translation.GetTranslationByKeyAsync("label.quicktrackAPIMessage")
            });
        }

        [HttpGet]
        [Route("{id}/activities")]
        [AppAuthorize(AppPermissions.Shipment_ContainerDetail)]
        public async Task<IActionResult> GetContainerActivities(long id)
        {
            var result = await _activityService.GetActivityCrossModuleAsync(EntityType.Container, id);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/itineraries")]
        [AppAuthorize(AppPermissions.Shipment_ContainerDetail)]
        public async Task<IActionResult> GetContainerItineraries(long id)
        {
            var result = await _itineraryService.GetItinerariesByContainer(id);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/cargodetails")]
        [AppAuthorize(AppPermissions.Shipment_ContainerDetail)]
        public async Task<IActionResult> GetContainerCargoDetails(long id, string affiliates)
        {
            var result = await _shipmentLoadDetailService.GetShipmentLoadDetailByContainer(id, CurrentUser.IsInternal, affiliates);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/attachments")]
        [AppAuthorize(AppPermissions.Shipment_ContainerDetail)]
        public async Task<IActionResult> GetContainerAttachments(long id)
        {
            var result = await _attachmentService.GetAttachmentsCrossModuleAsync(EntityType.Container, id, CurrentUser.UserRoleId, CurrentUser.OrganizationId);
            return new JsonResult(result);
        }

        [HttpPost]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]

        public async Task<IActionResult> PostAsync([FromBody] ContainerViewModel model)
        {
            var result = await _containerService.CreateAsync(model);
            return new JsonResult(result);
        }

        [HttpPut]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]

        [Route("{id}")]
        public async Task<IActionResult> PutAsync(long id, [FromBody] ContainerViewModel model)
        {
            var result = await _containerService.UpdateAsync(model, id);
            return new JsonResult(result);
        }

        [HttpDelete]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]

        [Route("{id}")]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _containerService.DeleteByKeysAsync(id);
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("{id}/TestReport")]
        public async Task<IActionResult> TestReportAsync(long id)
        {
            var rq = new ContainerReportRequest() { ContainerId = id, ReportFormat = ReportFormat.Pdf };
            var content = await _containerService.TestReportAsync(rq);
            return File(content, "application/octet-stream");
        }

        [HttpGet]
        [Route("search")]
        [AppAuthorize(AppPermissions.Shipment_Container_List)]
        public async Task<IActionResult> Search([DataSourceRequest] DataSourceRequest request, string affiliates)
        {
            var viewModels = await _containerService.GetListAsync(request, CurrentUser.IsInternal, affiliates);

            return new JsonResult(viewModels);
        }
    }
}
