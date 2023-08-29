using Groove.CSFE.Application.EventTypes.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.CSFE.CommonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventTypesController : ControllerBase
    {
        private readonly IEventTypeService _eventTypeService;

        public EventTypesController(IEventTypeService eventTypeService)
        {
            _eventTypeService = eventTypeService;
        }

        [HttpGet("dropdown")]
        public async Task<IActionResult> GetEventTypesDropdownAsync()
        {
            var result = await _eventTypeService.GetEventTypesDropdownAsync();
            return Ok(result);
        }
    }
}
