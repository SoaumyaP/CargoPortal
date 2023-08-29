using Groove.SP.API.Filters;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.FreightScheduler.Services.Interfaces;
using Groove.SP.Core.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VesselArrivalsController : ControllerBase
    {
        private readonly IFreightSchedulerService _freightSchedulerService;

        public VesselArrivalsController(IFreightSchedulerService freightSchedulerService)
        {
            _freightSchedulerService = freightSchedulerService;
        }

        [HttpGet]
        [Route("search")]
        [AppAuthorize(AppPermissions.Dashboard_VesselArrival_List)]
        public async Task<IActionResult> SearchAsync([DataSourceRequest] DataSourceRequest request, string affiliates, long? organizationId, string customerRelationships,
            string statisticKey = "", string statisticFilter = "", bool isExport = false)
        {
            var result = await _freightSchedulerService.GetListVesselArrivalAsync(request, CurrentUser.IsInternal, affiliates, organizationId, customerRelationships, statisticKey, statisticFilter, isExport);
            return Ok(result);
        }
    }
}
