using Groove.SP.Application.Common;
using Groove.SP.Application.Translations.ViewModels;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.Translations.Providers.Interfaces
{
    public interface ITranslationProvider: IServiceBase<TranslationModel, TranslationViewModel>
    {
        Task<string> GetCurrentTranslationsVersionAsync();
        Task<Dictionary<string, string>> GetTranslationsByLanguageAsync(string language);
        Task<Dictionary<string, Dictionary<string, string>>> GetAllTranslationsAsync();

        /// <summary>
        /// To get translation by current language.
        /// </summary>
        /// <param name="key">Translation key</param>
        /// <param name="language">Language/ Culture name. Leave null to use current thread's culture name</param>
        /// <returns></returns>
        Task<string> GetTranslationByKeyAsync(string key, string language = null);

        /// <summary>
        /// To reset/clean up all caches on translations.
        /// </summary>
        /// <returns></returns>
        Task ResetCacheAsync();
    }
}
