using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.SP.Application.Translations.Providers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Groove.SP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranslationsController : ControllerBase
    {
        private readonly ITranslationProvider _translationProvider;

        public TranslationsController(ITranslationProvider translationProvider)
        {
            _translationProvider = translationProvider;
        }
        
        //[ResponseCache(Duration = 604800)] // browser cache for same url for 1 week
        //No need to cache as
        //1. There is only one request to server to get translations -> no impact on performance
        //2. To able to refresh translation without restarting server
        [HttpGet("{language}.json")]
        public async Task<Dictionary<string, string>> GetTranslations(string language)
        {
            return await _translationProvider.GetTranslationsByLanguageAsync(language);
        }

        [HttpGet("Version")]
        public async Task<string> GetCurrentTranslationVersion()
        {
            return await _translationProvider.GetCurrentTranslationsVersionAsync();
        }

        [Authorize()]
        [HttpPut("Reset")]
        public async Task<IActionResult> ResetCacheAsync()
        {
            if (CurrentUser.IsInternal)
            {
                await _translationProvider.ResetCacheAsync();
                return Ok();
            }
            return Unauthorized();
        }
    }
}