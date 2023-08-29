namespace Groove.SP.Application.Translations.Helpers
{
    public static class TranslationUtility
    {
        public const string ENGLISH_KEY = "en-us";
        public const string CHINESE_TRADITIONAL_KEY = "zh-hant";
        public const string CHINESE_SIMPLIFIED_KEY = "zh-hans";

        private const string CACHE_PREFIX = "GROOVE.SP.TRANSLATION_";

        private const string CACHE_KEY_TRANSLATION_VERSION = "VERSION";

        private const string CACHE_KEY_ALL_TRANSLATIONS = "ALL";

        public static string GetCacheKeyOfVersion()
        {
            return $"{CACHE_PREFIX}{CACHE_KEY_TRANSLATION_VERSION}";
        }

        public static string GetVersionConfigurationKey()
        {
            return GetCacheKeyOfVersion();
        }

        public static string GetCacheKeyOfAllTranslations()
        {
            return $"{CACHE_PREFIX}{CACHE_KEY_ALL_TRANSLATIONS}";
        }
    }
}
