using Groove.SP.Application.ShipmentLoads.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    /// <summary>
    /// To be used by CS Portal. Url is /api/shipmentLoads/internal/
    /// </summary>
    [Route("api/shipmentloads/internal")]
    [ApiController]
    public class InternalShipmentLoadsController : ControllerBase
    {
        private readonly IShipmentLoadService _shipmentLoadService;
        public InternalShipmentLoadsController(
            IShipmentLoadService shipmentLoadService)
        {
            _shipmentLoadService = shipmentLoadService;
        }
    }
}
