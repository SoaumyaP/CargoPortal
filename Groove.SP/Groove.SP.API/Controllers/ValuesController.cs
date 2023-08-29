using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Groove.SP.API.Filters.Authorization;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        public ValuesController()
        {
            
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "Default Run" };
        }

        [Route("ExTranslationException")]
        [HttpGet]
        public ActionResult<IEnumerable<string>> ExTranslationException()
        {
            throw new AppException("msg.appAccountPending");
        }

        [Route("ExAppAuthorize")]
        [HttpGet]
        [AppAuthorize(AppPermissions.Dashboard)]
        public ActionResult<IEnumerable<string>> ExAppAuthorize()
        {
            return new string[] { "Example" };
        }
    }
}
