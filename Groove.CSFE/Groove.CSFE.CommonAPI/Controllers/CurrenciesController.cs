using Groove.CSFE.Application.Authorization;
using Groove.CSFE.Application.Currencies.Services;
using Microsoft.AspNetCore.Mvc;

namespace Groove.CSFE.CommonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AppAuthorize]
    public class CurrenciesController : ControllerBase
    {
        private readonly ICurrencyService _currencyServicee;

        public CurrenciesController(ICurrencyService currencyServicee)
        {
            _currencyServicee = currencyServicee;
        }

        [HttpGet]
        //TODO:
        //[AppAuthorize(AppPermissions.PO_Detail_Edit, AppPermissions.PO_Fulfillment_Detail_Edit)]
        public IActionResult GetAllCurrencies(string code)
        {
            var viewModels = _currencyServicee.GetAllCurrencies();
            return new JsonResult(viewModels);
        }
    }
}
