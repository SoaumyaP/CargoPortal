namespace Groove.CSFE.Core
{
    public static class AppConstants
    {
        public const string DELIMITER_PARENT_ID = ".";
        public const string SYSTEM_USERNAME = "System";
        public const string HEADER_LOCALIZATION_CULTURE = "Culture";
        public const string ENGLISH_KEY = "en-us";
        public const string CHINESE_TRADITIONAL_KEY = "zh-hant";
        public const string CHINESE_SIMPLIFIED_KEY = "zh-hans";
        public const string DEFAULT_PROFILE_IMPORT = "Unknown";
        public const string INTEGRATION_LOG_REMARK_DONE = "Done";
        public const string SECURITY_USER_ROLE_SWITCH = "urole_switch";

    }

    public static class ModeOfTransport
    {
        public const string Sea = "Sea";
        public const string Air = "Air";
        public const string Road = "Road";
        public const string Railway = "Railway";
        public const string Courier = "Courier";
        /// <summary>
        /// It should not apply for Itinerary but used for Booking and Shipment
        /// </summary>
        public const string MultiModal = "MultiModal";
    }

    public static class EventTypeLevelDescription
    {
        public const string PurchaseOrder = "PO";
        public const string POFulfillment = "Booking";
        public const string ShippingOrder = "SO";
        public const string Shipment = "Shipment";
        public const string Container = "Container";
        public const string MasterBL = "Master";
        public const string Vessel = "Vessel";
        public const string End = "End";
        public const string CruiseOrder = "Cruise";
    }
}
