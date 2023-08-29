using System.Threading.Tasks;
using Groove.CSFE.Application.Authorization;
using Groove.CSFE.Application.Carriers.Services;
using Groove.CSFE.Application.Ports.Services;
using Microsoft.AspNetCore.Mvc;

namespace Groove.CSFE.CommonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    public class PortsController : ControllerBase
    {
        private readonly IPortService _portService;

        public PortsController(IPortService portService)
        {
            _portService = portService;
        }

        [HttpGet]
        //TODO:
        //[AppAuthorize(AppPermissions.PO_Detail_Edit, AppPermissions.PO_Fulfillment_Detail_Edit)]
        public IActionResult GetAllPorts(string code)
        {
            var viewModels = _portService.GetAllPorts(code);
            return new JsonResult(viewModels);
        }
    }
}
