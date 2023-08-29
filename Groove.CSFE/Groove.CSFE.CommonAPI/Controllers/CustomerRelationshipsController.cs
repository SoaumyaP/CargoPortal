using System;
using System.Linq;
using System.Threading.Tasks;
using Groove.CSFE.Application.Authorization;
using Groove.CSFE.Application.Interfaces.Repositories;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Groove.CSFE.CommonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    public class CustomerRelationshipsController : ControllerBase
    {
        private readonly IRepository<CustomerRelationshipModel> _customerRelationshipReository;

        public CustomerRelationshipsController(
            IRepository<CustomerRelationshipModel> customerRelationshipReository
            )
        {
            _customerRelationshipReository = customerRelationshipReository;
        }

        [HttpGet]
        [Route("getByOrgType/{orgType}")]
        public async Task<IActionResult> GetOrgReferenceDataSourceAsync(string orgType, string idList)
        {
            var ids = idList.Split(";").Select(s => Int64.TryParse(s, out long n) ? n : (long?)null).ToList();
            if (orgType.Equals("Principal", StringComparison.OrdinalIgnoreCase))
            {
                var viewModels = await _customerRelationshipReository.QueryAsNoTracking(c => ids.Contains(c.CustomerId)).ToListAsync();
                return new JsonResult(viewModels);
            }

            if (orgType.Equals("General", StringComparison.OrdinalIgnoreCase))
            {
                var viewModels = await _customerRelationshipReository.QueryAsNoTracking(c => ids.Contains(c.SupplierId)).ToListAsync();
                return new JsonResult(viewModels);
            }

            return Ok();
        }
    }
}
