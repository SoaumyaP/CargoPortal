using Microsoft.AspNetCore.Mvc;
using Groove.SP.Application.Translations.Providers.Interfaces;
using Groove.SP.Core.Models;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.Cruise.CruiseOrderWarehouseInfos.Services.Interfaces;
using Groove.SP.Application.Cruise.CruiseOrderWarehouseInfos.ViewModels;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
    public class CruiseOrderWarehouseInfosController : ControllerBase
    {
        private readonly ICruiseOrderWarehouseInfoService _cruiseOrderWarehouseInfoService;
        private readonly ITranslationProvider _translation;
        private readonly AppConfig _appConfig;

        public CruiseOrderWarehouseInfosController(
            ICruiseOrderWarehouseInfoService cruiseOrderWarehouseInfoService, 
            ITranslationProvider translation, 
            IOptions<AppConfig> appConfig,
            IActivityService activityService
            )
        {
            _cruiseOrderWarehouseInfoService = cruiseOrderWarehouseInfoService;
            _appConfig = appConfig.Value;
            _translation = translation;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(long id)
        {
            var result = await _cruiseOrderWarehouseInfoService.GetAsync(id);
            return new JsonResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] CruiseOrderWarehouseInfoViewModel model)
        {
            model.ValidateAndThrow();
            model.AuditForAPI(CurrentUser.Username, false);
            model.MarkAuditFieldStatus();
            var result = await _cruiseOrderWarehouseInfoService.CreateAsync(model);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(long Id, [FromBody] CruiseOrderWarehouseInfoViewModel model)
        {
            model.ValidateAndThrow(true);
            model.AuditForAPI(CurrentUser.Username, true);
            model.MarkAuditFieldStatus(true);
            var result = await _cruiseOrderWarehouseInfoService.UpdateAsync(model, Id);
            return new JsonResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(long Id)
        {
            var result = await _cruiseOrderWarehouseInfoService.DeleteByKeysAsync(Id);
            return new JsonResult(result);
        }
    }
}