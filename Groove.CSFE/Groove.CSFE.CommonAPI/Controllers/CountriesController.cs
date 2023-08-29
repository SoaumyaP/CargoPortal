using System.Linq;
using System.Threading.Tasks;
using GGroove.CSFE.Application;
using Groove.CSFE.Application.Authorization;
using Groove.CSFE.Application.Countries.Services.Interfaces;
using Groove.CSFE.Application.Countries.ViewModels;
using Groove.CSFE.Application.Locations.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Groove.CSFE.CommonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly ICountryService _countryService;
        private readonly ILocationService _locationService;

        public CountriesController(ICountryService countryService, ILocationService locationService)
        {
            _countryService = countryService;
            _locationService = locationService;
        }

        [HttpGet]
        [AppAuthorize]
        public async Task<IActionResult> GetAllCountries()
        {
            var viewModels = await _countryService.GetAllAsync();
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("DropDown")]
        public async Task<IActionResult> GetDropDown()
        {
            var viewModels = await _countryService.GetAllAsync();
            var result = viewModels?.Select(x => new DropDownModel { Value = x.Id, Label = x.Name });
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("code:{code}")]
        [AppAuthorize]
        public async Task<IActionResult> GetByCodeAsync(string code)
        {
            var viewModel = await _countryService.GetByCodeAsync(code);
            return Ok(viewModel);
        }

        [HttpGet]
        [Route("DropDownCode/{code}")]
        [AppAuthorize]
        public async Task<IActionResult> GetDropdownByCodeAsync(string code)
        {
            var viewModels = await _countryService.GetAllAsync();
            var result = viewModels?.Select(x => new DropDownModel { Description = x.Code, Label = x.Name })
                ?.Where(x => x.Description == code)
                ?.FirstOrDefault();
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("DropDownCode")]
        [AppAuthorize]
        public async Task<IActionResult> GetDropDownCode()
        {
            var viewModels = await _countryService.GetAllAsync();
            var result = viewModels?.Select(x => new DropDownModel { Description = x.Code, Label = x.Name });
            return new JsonResult(result);
        }

        [HttpGet]
        [Route("AllLocations")]
        [AppAuthorize]
        public async Task<IActionResult> GetAllLocations()
        {
            var viewModels = await _countryService.GetAllLocations();
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("AllLocationSelections")]
        [AppAuthorize]
        public async Task<IActionResult> GetAllLocationSelections()
        {
            var viewModels = await _countryService.GetAllLocationSelections();
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("AllCountryLocations")]
        [AppAuthorize]
        public async Task<IActionResult> GetAllCountryLocations()
        {
            var viewModels = await _countryService.GetAllCountryLocations();
            return new JsonResult(viewModels);
        }

        [HttpGet]
        [Route("{id}/Locations/DropDown")]
        [AppAuthorize]
        public async Task<IActionResult> GetLocationsDropdown(long id)
        {
            var result = await _locationService.GetDropDownByCountryIdAsync(id);
            return new JsonResult(result);
        }

        [HttpPost]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        
        public async Task<IActionResult> PostAsync([FromBody]CountryViewModel model)
        {
            var result = await _countryService.CreateAsync(model);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        
        public async Task<IActionResult> UpdateAsync(long id, [FromBody] CountryViewModel model)
        {
            var result = await _countryService.UpdateAsync(model, id);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
        
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _countryService.DeleteAsync(id);
            return Ok(result);
        }
    }
}
