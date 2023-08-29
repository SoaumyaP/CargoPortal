using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Groove.CSFE.Application.AlternativeLocations.Services.Interfaces;
using Groove.CSFE.Application.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Groove.CSFE.CommonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    public class AlternativeLocationsController : ControllerBase
    {
        private readonly IAlternativeLocationService _alternativeLocationService;

        public AlternativeLocationsController(IAlternativeLocationService alternativeLocationService)
        {
            _alternativeLocationService = alternativeLocationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _alternativeLocationService.GetAllAsync();

            return Ok(result);
        }
    }
}