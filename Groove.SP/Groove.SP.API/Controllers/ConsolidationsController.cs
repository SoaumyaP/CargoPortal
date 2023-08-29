using System.Threading.Tasks;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.Consolidation.Services.Interfaces;
using Groove.SP.Application.Consolidation.ViewModels;
using Groove.SP.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Groove.SP.API.Controllers
{
    /// <summary>
    /// To be used by Edison ETL. Url is /api/consolidations/
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
    public class ConsolidationsController : ControllerBase
    {
        private readonly IConsolidationService _consolidationService;
        public ConsolidationsController(
            IConsolidationService consolidationService)
        {
            _consolidationService = consolidationService;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _consolidationService.GetAsync(id);
            return new JsonResult(result);
        }

        [HttpPost]
        
        public async Task<IActionResult> PostAsync([FromBody] ConsolidationViewModel model)
        {
            // Default stage from the API should be "Confirmed" if no stage value is inputted by the user.
            if (!model.IsPropertyDirty(nameof(model.Stage)))
            {
                model.Stage = ConsolidationStage.Confirmed;
                model.FieldStatus[nameof(model.Stage)] = FieldDeserializationStatus.HasValue;

            }
            var result = await _consolidationService.CreateAsync(model);
            return new JsonResult(result);
        }

        [HttpPut]
        
        [Route("{id}")]
        public async Task<IActionResult> PutAsync(long id, [FromBody] ConsolidationViewModel model)
        {
            var result = await _consolidationService.UpdateAsync(model, id);
            return new JsonResult(result);
        }

        [HttpDelete]
        
        [Route("{id}")]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _consolidationService.DeleteByKeysAsync(id);
            return new JsonResult(result);
        }
    }
}
