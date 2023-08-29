using System.Collections.Generic;

namespace Groove.SP.Core.Models
{
    public static class AppConstant
    {
        public const string SYSTEM_USERNAME = "System";
        public const string EDISON_USERNAME = "ediSon";
        public const string DEFAULT_PERMISSION_CACHE = "user-permission";
        public const int ACCOUNT_NUMBER_LENGTH = 5;
        public const string HEADER_LOCALIZATION_CULTURE = "Culture";

        public const string ROLE_GUEST = "Guest";
        public const string ROLE_PENDING = "Pending";

        public const string DEFAULT_PROFILE_IMPORT = "Unknown";
        public const string INTEGRATION_LOG_REMARK_DONE = "Done";
        public const string KILOGGRAMS = "KGS";
        public const string CUBIC_METER = "CBM";
        public const string PIECES = "PCS";
        public const string TWENTYFOOT_EQUIVALENT_UNIT = "TEU";

        public const string DefaultValue2Hyphens = "--";
        public const string ContractKey = "CK";
        public const string PrefixNVOBooking = "CS";

        public const char TILDE = '~';

        public const long DEFAULT_VIEW_SETTING_CACHE_SECONDS = 1800;
    }

    public static class Seperator
    {
        public const char HYPHEN = '-';
        public const char COMMA = ',';
        public const char SEMICOLON = ';';
    }

    public static class EntityType
    {
        public const string Shipment = "SHI";
        public const string BillOfLading = "BOL";
        public const string MasterBill = "MBL";
        public const string Container = "CTN";
        public const string Consignment = "CSM";
        public const string CustomerPO = "CPO";
        public const string POFullfillment = "POF";
        public const string CruiseOrder = "CRO";
        public const string CruiseOrderItem = "COI";
        public const string FreightScheduler = "FSC";
        public const string RoutingOrder = "ROD";
    }

    public static class DocumentLevel
    {
        public const string Shipment = "Shipment";
        public const string BillOfLading = "House BL";
        public const string MasterBill = "Master BL";
        public const string Container = "Container";
        public const string POFulfillment = "Booking";
    }

    public static class DialogCategory
    {
        public const string General = "General";
    }

    public static class ExceptionResources
    {
        public const string AppConfig_InvalidKey = "Invalid app setting key: {0}";
        public const string AzureStorage_Error = "Azure storage error: {0}";
        public const string BlobStorage_InvalidCategory = "Invalid category: {0}";
        public const string BlobStorage_InvalidKey = "Invalid blob key: {0} - {1}";
        public const string BlobStorage_NotFound = "Not found: {0}";
    }

    public static class Milestone
    {
        public static List<QuickTrackMilestone> ContainerMileStones =>
            new List<QuickTrackMilestone>
            {
                new QuickTrackMilestone { ActivityCode = "3001", ActivityDescription = "Gate In", ActivityDate = null },
                new QuickTrackMilestone { ActivityCode = "3002", ActivityDescription = "Vessel Load", ActivityDate = null },
                new QuickTrackMilestone { ActivityCode = "3003", ActivityDescription = "Port Departure", ActivityDate = null },
                new QuickTrackMilestone { ActivityCode = "3004", ActivityDescription = "Port Arrival", ActivityDate = null },
                new QuickTrackMilestone { ActivityCode = "3005", ActivityDescription = "Vessel Unload", ActivityDate = null },
                new QuickTrackMilestone { ActivityCode = "3006", ActivityDescription = "Gate Out", ActivityDate = null }
            };

        public static List<QuickTrackMilestone> ShipmentMileStones =>
            new List<QuickTrackMilestone>
            {
                new QuickTrackMilestone { ActivityCode = "2005", ActivityDescription = "Shipment Booked", ActivityDate = null },
                new QuickTrackMilestone { ActivityCode = "2014", ActivityDescription = "Handover from Shipper", ActivityDate = null },
                new QuickTrackMilestone { ActivityCode = "2029", ActivityDescription = "Departure from Port", ActivityDate = null },
                new QuickTrackMilestone { ActivityCode = "2029", ActivityDescription = "In Transit", ActivityDate = null },
                new QuickTrackMilestone { ActivityCode = "2039", ActivityDescription = "Arrival at Port", ActivityDate = null },
                new QuickTrackMilestone { ActivityCode = "2054", ActivityDescription = "Handover to Consignee", ActivityDate = null }
            };
    }

    public static class ActivityType
    {
        public const string ShipmentMilestone = "SM";
        public const string ContainerMilestone = "CM";
    }

    public static class OrganizationRole
    {
        public const string Shipper = "Shipper";
        public const string Consignee = "Consignee";
        public const string NotifyParty = "Notify Party";
        public const string OriginAgent = "Origin Agent";
        public const string DestinationAgent = "Destination Agent";
        public const string Principal = "Principal";
        public const string Supplier = "Supplier";
        public const string Delegation = "Delegation";
        public const string Pickup = "Pickup";
        public const string BillingParty = "Billing Party";
        public const string AlsoNotify = "Also Notify";
        public const string ImportBroker = "Import Broker";
        public const string ExportBroker = "Export Broker";
        public const string WarehouseProvider = "Warehouse Provider";
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

    public static class LogisticServiceType
    {
        public const string InternationalPortToPort = "Port-to-Port";
        public const string InternationalPortToDoor = "Port-to-Door";
        public const string InternationalDoorToPort = "Door-to-Port";
        public const string InternationalDoorToDoor = "Door-to-Door";
        public const string InternationalAirportToAirport = "Airport-to-Airport";
        public const string InternationalAirportToDoor = "Airport-to-Door";
        public const string InternationalDoorToAirport = "Door-to-Airport";
    }

    public static class Movement
    {
        public const string CYSlashCY = "CY/CY";
        public const string CYSlashCFS = "CY/CFS";
        public const string CFSSlashCY = "CFS/CY";
        public const string CFSSlashCFS = "CFS/CFS";
    }

    public static class BillOfLadingType
    {
        public const string FCR = "FCR";
        public const string HBL = "HBL";
        public const string SeawayBill = "Seaway Bill";
        public const string TelexRelease = "Telex Release";
    }

    public static class Commodity
    {
        public const string GeneralGoods = "General Goods";
        public const string Garments = "Garments";
        public const string Accessories = "Accessories";
        public const string Toys = "Toys";
        public const string PlasticGoods = "Plastic Goods";
        public const string Household = "Household";
        public const string Textiles = "Textiles";
        public const string Hardware = "Hardware";
        public const string Stationery = "Stationery";
        public const string Houseware = "Houseware";
        public const string Kitchenware = "Kitchenware";
        public const string Footwear = "Footwear";
        public const string Furniture = "Furniture";
        public const string Electionics = "Electionics";
        public const string ElectricalGoods = "Electrical Goods";
        public const string NonperishableGroceries = "Non-perishable Groceries";
    }

    public static class Event
    {
        public const string EVENT_1007 = "1007";

        /// <summary>
        /// Booking Confirmed
        /// </summary>
        public const string EVENT_1008 = "1008";

        /// <summary>
        /// PM - Shipment Dispatch
        /// </summary>
        public const string EVENT_1009 = "1009";

        /// <summary>
        /// PM - PO Closed
        /// </summary>
        public const string EVENT_1010 = "1010";

        /// <summary>
        /// PA - Progress Check
        /// </summary>
        public const string EVENT_1013 = "1013";

        public const string EVENT_1051 = "1051";
        public const string EVENT_1052 = "1052";
        public const string EVENT_1053 = "1053";
        public const string EVENT_1054 = "1054";
        public const string EVENT_1055 = "1055";
        public const string EVENT_1056 = "1056";
        public const string EVENT_1057 = "1057";
        public const string EVENT_1058 = "1058";
        public const string EVENT_1059 = "1059";
        public const string EVENT_1060 = "1060";
        public const string EVENT_1061 = "1061";

        public const string EVENT_1062 = "1062";

        /// <summary>
        /// FM - Cargo Received
        /// </summary>
        public const string EVENT_1063 = "1063";

        public const string EVENT_1064 = "1064";
        public const string EVENT_1067 = "1067";

        /// <summary>
        /// FM - Goods Dispatch
        /// </summary>
        public const string EVENT_1068 = "1068";

        /// <summary>
        /// FM - Booking Closed
        /// </summary>
        public const string EVENT_1071 = "1071";
        public const string EVENT_2005 = "2005";

        /// <summary>
        /// SM - Cargo handover at origin
        /// </summary>
        public const string EVENT_2014 = "2014";

        /// <summary>
        /// SM - Shipment actual departure from origin port
        /// </summary>
        public const string EVENT_2029 = "2029";

        /// <summary>
        /// SM - Shipment actual arrival at discharge port
        /// </summary>
        public const string EVENT_2039 = "2039";

        /// <summary>
        /// SM - Shipment handover to consignee
        /// </summary>
        public const string EVENT_2054 = "2054";

        /// <summary>
        /// CM - Container - Actual Port Departure
        /// </summary>
        public const string EVENT_3003 = "3003";

        /// <summary>
        /// CM - Container - Actual Port Arrival
        /// </summary>
        public const string EVENT_3004 = "3004";

        /// <summary>
        /// VA - Vessel Departure
        /// </summary>
        public const string EVENT_7001 = "7001";

        /// <summary>
        /// VA - Vessel Arrival
        /// </summary>
        public const string EVENT_7002 = "7002";

        /// <summary>
        /// VA - Flight Departure
        /// </summary>
        public const string EVENT_7003 = "7003";

        /// <summary>
        /// VA - Flight Arrival
        /// </summary>
        public const string EVENT_7004 = "7004";

        /// <summary>
        /// PE - PO Cancelled
        /// </summary>
        public const string EVENT_1005 = "1005";

        /// <summary>
        /// FE - Shipment Cancellation
        /// </summary>
        public const string EVENT_1070 = "1070";
    }

    public static class AttachmentType
    {
        public const string SHIPPING_ORDER_FORM = "Shipping Order Form";
        public const string PACKING_LIST = "Packing List";
        public const string COMMERCIAL_INVOICE = "Commercial Invoice";
        public const string FACTORY_COMMERCIAL_INVOICE = "Factory Commercial Invoice";
        // Changed Ocean Bill Of Lading to Master BL
        public const string MASTER_BL = "Master BL";
        // Changed Bill Of Lading to House BL 
        public const string HOUSE_BL = "House BL";
        public const string MANIFEST = "Manifest";
        public const string OTHERS = "Others";
        public const string EXPORT_LICENSE = "Export License";
        public const string CERTIFICATE_OF_ORIGIN = "Certificate of Origin";
        public const string FORM_A = "Form A";
        public const string FUMIGATION_CERTIFICATE = "Fumigation Certificate";
        public const string PACKING_DECLARATION = "Packing Declaration";
        public const string MSDS = "MSDS";
        public const string LETTER_OF_CREDIT = "Letter of Credit";
        public const string INSURANCE_CERTIFICATE = "Insurance Certificate";
        public const string BOOKING_FORM = "Booking Form";
        public const string MISCELLANEOUS = "Miscellaneous";

    }

    public static class StatusType
    {
        public const string ACTIVE = "Active";
        public const string INACTIVE = "Inactive";
    }

    public static class MimeTypes
    {
        /// <summary>
        /// xlsx
        /// </summary>
        public const string TextXlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    }

    public static class Schedulers
    {
        public const string UpdatedViaItineraryAPI = "updatedviaItineraryAPI#";
        public const string UpdatedViaFreightSchedulerAPI = "updatedviaFreightSchedulerAPI#";
        public const string UpdatedViaApplicationUI = "updatedviaApplicationUI#";
    }

    public static class POListReturnType
    {
        public const string CHILD_LEVEL = "Child level";
        public const string PARENT_LEVEL = "Parent level";
    }

    public static class AgentAssignmentMode
    {
        public const string DEFAULT = "Default";
        public const string CHANGE = "Change";
    }

    public static class FileExtensions
    {
        public const string EXCEL_97_2003 = ".xls";
        public const string EXCEL_WORKSHEET = ".xlsx";
        public const string CSV = ".csv";
    }

    public static class FormModeType
    {
        public const string VIEW = "view";
        public const string EDIT = "edit";
        public const string ADD = "add";
        public const string COPY = "copy";
    }

    public static class ViewSettingModuleId
    {
        public const string BOOKING_LIST = "Booking.List";

        public const string BULKBOOKING_COPY_LIST = "BulkBooking.CopyList";
        public const string BULKBOOKING_DETAIL = "BulkBooking.Detail";
        public const string BULKBOOKING_DETAIL_PLANNED_SCHEDULE = "BulkBooking.Detail.PlannedSchedule";
        public const string BULKBOOKING_DETAIL_CONTACTS = "BulkBooking.Detail.Contacts";
        public const string BULKBOOKING_DETAIL_CARGO_DETAILS = "BulkBooking.Detail.CargoDetails";
        public const string BULKBOOKING_DETAIL_CARGO_DETAILS_ITEM = "BulkBooking.Detail.CargoDetails.Item";
        public const string BULKBOOKING_DETAIL_LOADS = "BulkBooking.Detail.Loads";
        public const string BULKBOOKING_DETAIL_LOAD_DETAILS = "BulkBooking.Detail.LoadDetails";

        public const string FREIGHTBOOKING_DETAIL = "FreightBooking.Detail";
        public const string FREIGHTBOOKING_DETAIL_CONTACTS = "FreightBooking.Detail.Contacts";
        public const string FREIGHTBOOKING_DETAIL_CUSTOMER_POS = "FreightBooking.Detail.CustomerPOs";
        public const string FREIGHTBOOKING_DETAIL_LOADS = "FreightBooking.Detail.Loads";
        public const string FREIGHTBOOKING_DETAIL_SHIPMENT_PLANNED_SCHEDULE = "FreightBooking.Detail.Shipment.PlannedSchedule";
        public const string FREIGHTBOOKING_DETAIL_SHIPMENT = "FreightBooking.Detail.Shipment";
        public const string FREIGHTBOOKING_DETAIL_LOAD_DETAILS = "FreightBooking.Detail.LoadDetails";
        public const string FREIGHTBOOKING_DETAIL_SHIPMENT_SHIPMENTITINERARIES = "FreightBooking.Detail.Shipment.ShipmentItineraries";

        public const string PO_LIST = "PO.List";
        public const string PO_ITEM_QUICK_SEARCH_LIST = "PO.ItemQuickSearch.List";
        public const string PO_DETAIL = "PO.Detail";
        public const string PO_DETAIL_CONTACTS = "PO.Detail.Contacts";
        public const string PO_DETAIL_ITEMS = "PO.Detail.LineItems";
        public const string PO_DETAIL_BOOKINGS = "PO.Detail.Bookings";
        public const string PO_DETAIL_ALLOCATEDPOS = "PO.Detail.AllocatedPOs";
    }
}