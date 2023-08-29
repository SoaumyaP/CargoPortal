using Groove.SP.Application.ContractType.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractTypesController : ControllerBase
    {
        private readonly IContractTypeService _contractTypeService;

        public ContractTypesController(
            IContractTypeService contractTypeService)
        {
            _contractTypeService = contractTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var viewModels = await _contractTypeService.GetAllAsync();
            return new JsonResult(viewModels);
        }
    }
}
