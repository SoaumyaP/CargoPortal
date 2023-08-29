using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GGroove.CSFE.Application;
using Groove.CSFE.Application.Authorization;
using Groove.CSFE.Application.Locations.Services.Interfaces;
using Groove.CSFE.Application.Locations.ViewModels;
using Groove.CSFE.CommonAPI.Filters;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Data;
using Microsoft.AspNetCore.Mvc;

namespace Groove.CSFE.CommonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationsController(ILocationService locationService)
        {
            _locationService = locationService;
        }
        
        [HttpGet("search")]
        [AppAuthorize(AppPermissions.Organization_Location_List)]
        public async Task<IActionResult> SearchAsync([DataSourceRequest] DataSourceRequest request)
        {
            var viewModels = await _locationService.ListAsync(request);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}")]
        [AppAuthorize()]
        public async Task<IActionResult> GetById(long id)
        {
            var viewModels = await _locationService.GetAsync(id);
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("code:{code}")]
        [AppAuthorize]
        public async Task<IActionResult> GetByCode(string code)
        {
            var viewModel = await _locationService.GetByCodeAsync(code);
            return Ok(viewModel);
        }

        [HttpGet]
        [Route("description:{description}")]
        [AppAuthorize]
        public async Task<IActionResult> GetByDescriptionAsync(string description)
        {
            var viewModel = await _locationService.GetByDescriptionAsync(description);
            return Ok(viewModel);
        }

        [HttpPost]
        [Route("Code")]
        public async Task<IActionResult> GetByCodes([FromBody] List<string> codes)
        {
            var viewModels = await (codes == null || !codes.Any()
                ? _locationService.GetAllAsync()
                : _locationService.GetByCodesAsync(codes));
            return Ok(viewModels);
        }

        [HttpGet]
        //TODO
        //[AppAuthorize(AppPermissions.PO_Fulfillment_Detail_Edit)]
        [AppAuthorize]
        public async Task<IActionResult> GetAllLocations([FromQuery]string countryIds)
        {
            var countryIdList = countryIds?.Split(',').Where(x => long.TryParse(x, out _))
                .Select(long.Parse);

            if(countryIdList != null && countryIdList.Any())
            {
                return Ok(await _locationService.GetAsync(countryIdList));
            }

            var viewModels = await _locationService.GetAllAsync();
            return new JsonResult(viewModels);
        }

        /// <summary>
        /// To get location suggestions datasource that belongs to the selected country.
        /// </summary>
        /// <param name="group">CountryId</param>
        /// <param name="locationName">LocationDescription</param>
        /// <returns></returns>
        [HttpGet("suggestionByCountry")]
        public async Task<IActionResult> GetSuggestionByCountryAsync([FromQuery] long group, string locationName)
        {
            if (group == 0 || string.IsNullOrWhiteSpace(locationName))
            {
                return new JsonResult(new DropDownModel<long> { });
            }

            var location = await _locationService.GetAsync(new List<long> { group });

            var res = location.Select(x => x.LocationDescription).Where(x => x.Contains(locationName, StringComparison.OrdinalIgnoreCase));

            return Ok(res);
        }

        /// <summary>
        /// To verify whether the location is valid in CS Portal.
        /// </summary>
        /// <param name="countryId"></param>
        /// <param name="locationName"></param>
        /// <returns></returns>
        [HttpGet("verifyLocationName")]
        public async Task<IActionResult> VerifyLocationNameAsync([FromQuery] long countryId, string locationName)
        {
            if (string.IsNullOrWhiteSpace(locationName))
            {
                return Ok(false);
            }

            var location = new List<LocationViewModel>();
            if (countryId == 0)
            {
                location = (List<LocationViewModel>)await _locationService.GetAllAsync();
            }
            else
            {
                location = (List<LocationViewModel>)await _locationService.GetAsync(new List<long> { countryId });
            }

            return Ok(location.Any(x => x.LocationDescription.Equals(locationName)));
        }

        [HttpGet("byLocationName")]
        public async Task<IActionResult> GetByLocationNameAsync([FromQuery] long countryId, string locationName)
        {
            if (countryId != 0)
            {
                return Ok(await _locationService.GetByDescriptionAsync(locationName, countryId));
            }
            return Ok(await _locationService.GetByDescriptionAsync(locationName));
        }

        [HttpPost]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        
        public async Task<IActionResult> PostAsync([FromBody]LocationViewModel model)
        {
            var result = await _locationService.CreateAsync(model);
            return Ok(result);
        }

        /// <summary>
        /// To create new Location via GUI
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("internal")]
        [AppAuthorize(AppPermissions.Organization_Location_Detail_Add)]
        public async Task<IActionResult> PostInternalAsync([FromBody] LocationViewModel viewModel)
        {
            viewModel.Audit(CurrentUser.Username);
            viewModel.FieldStatus[nameof(LocationViewModel.CreatedBy)] = FieldDeserializationStatus.HasValue;
            viewModel.FieldStatus[nameof(LocationViewModel.CreatedDate)] = FieldDeserializationStatus.HasValue;
            viewModel.FieldStatus[nameof(LocationViewModel.UpdatedBy)] = FieldDeserializationStatus.HasValue;
            viewModel.FieldStatus[nameof(LocationViewModel.UpdatedDate)] = FieldDeserializationStatus.HasValue;
            var result = await _locationService.CreateAsync(viewModel);
            return Ok(result);
        }

        /// <summary>
        /// To update Location via GUI
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("internal/{id}")]
        [AppAuthorize(AppPermissions.Organization_Location_Detail_Edit)]
        public async Task<IActionResult> UpdateInternalAsync(long id, [FromBody] LocationViewModel viewModel)
        {
            viewModel.Audit(CurrentUser.Username);
            viewModel.FieldStatus[nameof(LocationViewModel.CreatedBy)] = FieldDeserializationStatus.WasNotPresent;
            viewModel.FieldStatus[nameof(LocationViewModel.CreatedDate)] = FieldDeserializationStatus.WasNotPresent;
            viewModel.FieldStatus[nameof(LocationViewModel.UpdatedBy)] = FieldDeserializationStatus.HasValue;
            viewModel.FieldStatus[nameof(LocationViewModel.UpdatedDate)] = FieldDeserializationStatus.HasValue;
            var result = await _locationService.UpdateAsync(viewModel, id);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        
        public async Task<IActionResult> UpdateAsync(long id, [FromBody]LocationViewModel model)
        {
            var result = await _locationService.UpdateAsync(model, id);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _locationService.DeleteAsync(id);
            return Ok(result);
        }

        [HttpGet("{id}/SameCountry/DropDown")]
        public async Task<IActionResult> GetLocationSameCountryDropDownAsync(long id)
        {
            var result = await _locationService.GetLocationSameCountryDropDownAsync(id);
            return new JsonResult(result);
        }
    }
}
