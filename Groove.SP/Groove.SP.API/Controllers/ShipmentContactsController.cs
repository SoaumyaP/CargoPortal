﻿using System.Threading.Tasks;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.ShipmentContact.Services.Interfaces;
using Groove.SP.Application.ShipmentContact.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
    public class ShipmentContactsController : ControllerBase
    {
        private readonly IShipmentContactService _shipmentContactService;

        public ShipmentContactsController(IShipmentContactService shipmentContactService)
        {
            _shipmentContactService = shipmentContactService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(long id)
        {
            var result = await _shipmentContactService.GetAsync(id);
            return new JsonResult(result);
        }

        [HttpPost]
        
        public async Task<IActionResult> PostAsync(long id, [FromBody] ShipmentContactViewModel model)
        {
            var result = await _shipmentContactService.CreateAsync(model);
            return Ok(result);
        }

        [HttpPut("{id}")]
        
        public async Task<IActionResult> PutAsync(long id, [FromBody] ShipmentContactViewModel model)
        {
            var result = await _shipmentContactService.UpdateAsync(model, id);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var result = await _shipmentContactService.DeleteByKeysAsync(id);
            return Ok(result);
        }
    }
}