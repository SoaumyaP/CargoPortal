using Microsoft.AspNetCore.Mvc;
using Groove.SP.Application.CruiseOrders.Services.Interfaces;
using Groove.SP.Application.Translations.Providers.Interfaces;
using Groove.SP.Core.Models;
using Groove.SP.Application.Providers.BlobStorage;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Groove.SP.Application.Authorization;
using System.Collections.Generic;
using Groove.SP.Application.CruiseOrders.ViewModels;
using FluentValidation;
using FluentValidation.Results;
using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.Activity.ViewModels;
using System.Linq;
using Groove.SP.Core.Data;
using Groove.SP.API.Filters;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CruiseOrdersController : ControllerBase
    {
        private readonly ICruiseOrderService _cruiseOrderService;
        private readonly ICruiseOrderListService _cruiseOrderListService;
        private readonly IActivityService _activityService;
        private readonly ITranslationProvider _translation;
        private readonly IBlobStorage _blobStorage;
        private readonly AppConfig _appConfig;
        private readonly IValidator<IEnumerable<CreateCruiseOrderViewModel>> _createBulkValidator;

        public CruiseOrdersController(
            ICruiseOrderService cruiseOrderService, 
            ITranslationProvider translation, 
            IBlobStorage blobStorage, 
            IOptions<AppConfig> appConfig,
            ICruiseOrderListService cruiseOrderListService,
            IActivityService activityService,
            IValidator<IEnumerable<CreateCruiseOrderViewModel>> createBulkValidator
            )
        {
            _cruiseOrderService = cruiseOrderService;
            _cruiseOrderListService = cruiseOrderListService;
            _activityService = activityService;
            _appConfig = appConfig.Value;
            _translation = translation;
            _blobStorage = blobStorage;
            _createBulkValidator = createBulkValidator;
        }


        [HttpPost("lists")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]

        public async Task<IActionResult> PostAsync([FromBody] IEnumerable<CreateCruiseOrderViewModel> model)
        {           
            // Manually call data validation
            var validationResult = _createBulkValidator.Validate(model);
            if(!validationResult.IsValid)
            {
                var returnedErrors = new List<ValidationFailure>();
                foreach (var item in validationResult.Errors)
                {
                    // No need to return all information
                    var error = new ValidationFailure(item.PropertyName, item.ErrorMessage);
                    returnedErrors.Add(error);
                }
                return BadRequest(returnedErrors);
            }            

            var result = await _cruiseOrderService.CreateBulkAsync(model, CurrentUser.Username);
            return Ok(result);
        }

        [HttpPost]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]

        public async Task<IActionResult> PostAsync([FromBody] CreateCruiseOrderViewModel model)
        {
            model.AuditForAPI(CurrentUser.Username, false);
            var result = await _cruiseOrderService.CreateAsync(model, CurrentUser.Username);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [AppAuthorize(AppPermissions.CruiseOrder_Detail)]

        public async Task<IActionResult> PutAsync(long id, [FromBody] UpdateCruiseOrderViewModel model)
        {
            model.AuditForAPI(CurrentUser.Username, true);
            model.MarkAuditFieldStatus();
            var result = await _cruiseOrderService.UpdateAsync(id, model, CurrentUser.Username);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            await _cruiseOrderService.DeleteAsync(id);
            return Ok();
        }

        [HttpGet]
        [Route("{id}")]
        [AppAuthorize(AppPermissions.CruiseOrder_Detail)]
        public async Task<IActionResult> Get(long id)
        {
            var viewModels = await _cruiseOrderService.GetAsync(id);
            var isAccesible = true;
            if (!CurrentUser.IsInternal)
            {
                isAccesible = viewModels?.Contacts?.Any(c => c.OrganizationId == CurrentUser.OrganizationId) ?? false;
            }
            return new JsonResult(isAccesible ? viewModels : null);
        }

        [HttpGet]
        [Route("search")]
        [AppAuthorize(AppPermissions.CruiseOrder_List)]
        public async Task<IActionResult> Get([DataSourceRequest] DataSourceRequest request)
        {
            var viewModels = await _cruiseOrderListService.ListAsync(request, CurrentUser.IsInternal, CurrentUser.OrganizationId);

            return new JsonResult(viewModels);
        }

        #region Activities
        [HttpGet("{id}/activities")]
        [AppAuthorize(AppPermissions.CruiseOrder_Detail)]
        public async Task<IActionResult> GetActivities(long id)
        {
            var result = await _activityService.GetActivities(EntityType.CruiseOrder, id);
            return new JsonResult(result);
        }

        [HttpPost("{id}/activities")]
        [AppAuthorize(AppPermissions.CruiseOrder_Detail)]
        public async Task<IActionResult> PostActivityAsync(long id, [FromBody] ActivityViewModel model)
        {
            model.Audit(CurrentUser.Username);
            var result = await _activityService.CreateAsync(model);
            return Ok(result);
        }

        [HttpPut("{id}/activities/{activityId}")]
        [AppAuthorize(AppPermissions.CruiseOrder_Detail)]
        public async Task<IActionResult> PutActivityAsync(long id, long activityId, [FromBody] ActivityViewModel model)
        {
            model.Audit(CurrentUser.Username);
            var result = await _activityService.UpdateAsync(model, activityId);
            return Ok();
        }

        [HttpDelete("{id}/activities/{activityId}")]
        [AppAuthorize(AppPermissions.CruiseOrder_Detail)]
        public async Task<IActionResult> DeleteActivityAsync(long activityId)
        {
            await _activityService.DeleteAsync(activityId);
            return Ok();
        }
        #endregion
    }
}
