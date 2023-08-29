using Groove.CSFE.Supplemental.Models;
using Groove.CSFE.Supplemental.Services;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Reflection;
using System.Security.Policy;
using System.Text;

namespace Groove.CSFE.Supplemental.Controllers
{
    [Route("api/balance-of-goods")]
    [ApiController]
    //[Authorize]
    [EnableCors]

    public class BalanceOfGoodsController : ControllerBase
    {
        private readonly IBalanceOfGoodsService service;

        public BalanceOfGoodsController(IBalanceOfGoodsService service) => this.service = service;
        [HttpOptions]
        [EnableCors]

        public async Task<IActionResult> Get([FromQuery] BalanceOfGoodSearchModel model)
        {
            Log.Logger.Information($"GET {nameof(this.Get)}, Query: {Request.QueryString.Value}");
            try
            {
                var principle = User.FindFirst(x => x.Type == "org_id")?.Value;
                if (string.IsNullOrEmpty(principle)) throw new BadHttpRequestException("Principle contains no value");
                var b = long.TryParse(principle, out long principleId);
                if (!b) throw new BadHttpRequestException("Invalid principle");
                if (principleId > 0 && (model.AccessiblePrinciples == null || model.AccessiblePrinciples.Count() == 0))
                    throw new BadHttpRequestException("Accessible Principle cannot be null");


                var result = await service.GetBalanceOfGoodAsync(model);
                Log.Logger.Information("Operation succeed");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Controller: {nameof(BalanceOfGoodsController)}, Action: {nameof(this.Get)}, Query: {Request.QueryString.Value}, Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }
        [HttpOptions]
        [HttpGet]
        [Route("search")]
        [EnableCors]

        public async Task<IActionResult> Get([FromQuery] string query)
        {
            Log.Logger.Information($"GET Controller: {nameof(BalanceOfGoodsController)}, Action: {nameof(this.Get)}, Query: {Request.QueryString.Value}");
            try
            {
                var d = System.Text.Json.JsonSerializer.Deserialize<FilterRoot>(query);
                if (d == null)
                    throw new BadHttpRequestException("query is not valid");
                var orgId = User.FindFirst(x => x.Type == "org_id")?.Value;
                if (string.IsNullOrEmpty(orgId))
                    throw new BadHttpRequestException("Organization contains no value");

                var b = long.TryParse(orgId, out long org);
                if (!b)
                    throw new BadHttpRequestException("Organization contains invalid value");
                if (org == 0) d.isInternal = true;

                if (!d.isInternal && (d.principles == null || d.principles.Count() == 0))
                    throw new BadHttpRequestException("Principle(s) is required");

                Log.Logger.Information($"Filter: {System.Text.Json.JsonSerializer.Serialize(d)}");
                var result = await service.GetBalanceOfGoodAsync(d);
                Log.Logger.Information("Operation succeed");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Controller: {nameof(BalanceOfGoodsController)}, Action: {nameof(this.Get)}, Query: {Request.QueryString.Value}, Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }
        [HttpOptions]
        [HttpGet]
        [Route("transaction")]
        [EnableCors]
        public async Task<IActionResult> Get([FromQuery] string mode, [FromQuery] int principleId, [FromQuery] int? articleId, [FromQuery] int? warehouseId, [FromQuery] string state)
        {
            try
            {
                if (string.IsNullOrEmpty(mode)) throw new BadHttpRequestException("Mode is required");
                if (principleId == 0) throw new BadHttpRequestException("Principle is required");
                if (mode.ToLower() == "article" && (!articleId.HasValue || articleId.Value <= 0)) throw new BadHttpRequestException("Article is required when querying article mode");
                if (mode.ToLower() == "warehouse" && (!warehouseId.HasValue || warehouseId.Value <= 0)) throw new BadHttpRequestException("Warehouse is required when querying warehouse mode");

                var d = System.Text.Json.JsonSerializer.Deserialize<FilterRoot>(state);
                if (d == null)
                    throw new BadHttpRequestException("query is not valid");

                Log.Logger.Information($"mode: {mode}, principle: {principleId}, article: {articleId}, warehouse: {warehouseId}, state: {System.Text.Json.JsonSerializer.Serialize(d)}");
                var result = await service.GetBalanceOfGoodDetailAsync(mode, principleId, articleId, warehouseId, d);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Controller: {nameof(BalanceOfGoodsController)}, Action: {nameof(this.Get)}, Query: {Request.QueryString.Value}, Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }
    }
}
