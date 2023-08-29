using Groove.SP.Application.Authorization;
using Groove.SP.Application.ShipmentLoadDetails.Services.Interfaces;
using Groove.SP.Application.ShipmentLoadDetails.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    /// <summary>
    /// To be used by CS Portal. Url is /api/shipmentLoadDetails/internal/
    /// </summary>
    [Route("api/shipmentLoadDetails/internal")]
    [ApiController]
    public class InternalShipmentLoadDetailsController : ControllerBase
    {
        private readonly IShipmentLoadDetailService _shipmentLoadDetailService;
        public InternalShipmentLoadDetailsController(
            IShipmentLoadDetailService shipmentLoadDetailService)
        {
            _shipmentLoadDetailService = shipmentLoadDetailService;
        }
    }
}
