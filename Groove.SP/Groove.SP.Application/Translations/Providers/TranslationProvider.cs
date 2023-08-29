using Groove.SP.Application.Caching;
using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Application.Translations.ViewModels;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Translations.Providers.Interfaces;
using Groove.SP.Application.Translations.Helpers;
using System.Linq;
using System;
using System.Threading;

namespace Groove.SP.Application.Translations.Providers
{
    public class TranslationProvider: ServiceBase<TranslationModel, TranslationViewModel>, ITranslationProvider
    {
        private readonly ICacheService cacheService;

        public TranslationProvider(IUnitOfWorkProvider unitOfWorkProvider, ICacheService cacheService)
            : base(unitOfWorkProvider)
        {
            this.cacheService = cacheService;
        }

        public async Task<Dictionary<string, Dictionary<string, string>>> GetAllTranslationsAsync()
        {
            var allTranslationsCacheKey = TranslationUtility.GetCacheKeyOfAllTranslations();

            var translations = this.cacheService.Get<Dictionary<string, Dictionary<string, string>>>(allTranslationsCacheKey);
            
            if (translations == null)
            {
                translations = new Dictionary<string, Dictionary<string, string>>();
                translations.Add(TranslationUtility.ENGLISH_KEY, new Dictionary<string, string>());
                translations.Add(TranslationUtility.CHINESE_TRADITIONAL_KEY, new Dictionary<string, string>());
                translations.Add(TranslationUtility.CHINESE_SIMPLIFIED_KEY, new Dictionary<string, string>());
                
                var transFromDB = await this.GetAllAsync();

                foreach (var translation in transFromDB)
                {
                    var key = translation.Key;

                    translations[TranslationUtility.ENGLISH_KEY][key] = translation.English;

                    // set fallback value as english
                    translations[TranslationUtility.CHINESE_TRADITIONAL_KEY][key] = translation.TraditionalChinese ?? translation.English;

                    // set fallback value as english
                    translations[TranslationUtility.CHINESE_SIMPLIFIED_KEY][key] = translation.SimplifiedChinese ?? translation.English;
                }

                this.cacheService.Set(allTranslationsCacheKey, translations);
            }

            return translations;
        }
        
        public async Task<string> GetCurrentTranslationsVersionAsync()
        {
            var versionCacheKey = TranslationUtility.GetCacheKeyOfVersion();
            var version = this.cacheService.Get<string>(versionCacheKey);

            if (string.IsNullOrEmpty(version))
            {
                var versionConfigKey = TranslationUtility.GetVersionConfigurationKey();
                var latestMessage = await this.Repository.GetAsync(null, x => x.OrderByDescending(m => m.UpdatedDate));
                if(latestMessage != null && latestMessage.UpdatedDate != null)
                {
                    version = latestMessage.UpdatedDate.Value.Ticks.ToString();
                }
                else
                {
                    version = DateTime.UtcNow.Ticks.ToString();
                }
                this.cacheService.Set(versionCacheKey, version);
            }

            return version;
        }
        
        public async Task<Dictionary<string, string>> GetTranslationsByLanguageAsync(string language)
        {
            var allTranslations = await this.GetAllTranslationsAsync();
            allTranslations.TryGetValue(language, out Dictionary<string, string> translationsInLanguage);

            return translationsInLanguage;
        }

        public async Task<string> GetTranslationByKeyAsync(string key, string language = null)
        {
            if (string.IsNullOrEmpty(language))
            {
                language = Thread.CurrentThread.CurrentCulture.Name.ToLower();
            }

            // fallback all not supported language to english
            switch (language)
            {
                case TranslationUtility.ENGLISH_KEY:
                case TranslationUtility.CHINESE_TRADITIONAL_KEY:
                case TranslationUtility.CHINESE_SIMPLIFIED_KEY:
                    break;
                default: 
                    language = TranslationUtility.ENGLISH_KEY;
                    break;
            }

            var translations = (await this.GetTranslationsByLanguageAsync(language));

            translations.TryGetValue(key, out string msg);

            return msg;
        }

        public async Task ResetCacheAsync()
        {
            // Remove cache for translation version
            var key = TranslationUtility.GetCacheKeyOfVersion();
            await cacheService.RemoveAsync(key);

            // Remove cache for translation values
            key = TranslationUtility.GetCacheKeyOfAllTranslations();
            await cacheService.RemoveAsync(key);
        }
    }
}
