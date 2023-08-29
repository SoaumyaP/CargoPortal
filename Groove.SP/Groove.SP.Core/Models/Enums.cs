using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Groove.SP.Core.Models
{
    public enum UserStatus
    {
        [Display(Name = "label.deleted", Description = "OrganizationDeletedDescription")]
        Deleted = -1,
        [Display(Name = "label.rejected", Description = "OrganizationRejectedDescription")]
        Rejected = 0,
        [Display(Name = "label.pending", Description = "OrganizationPendingDescription")]
        Pending = 1,
        [Display(Name = "label.active", Description = "OrganizationActiveDescription")]
        Active = 2,
        [Display(Name = "label.inactive", Description = "OrganizationActiveDescription")]
        Inactive = 3,
        [Display(Name = "label.waitForConfirm", Description = "OrganizationActiveDescription")]
        WaitForConfirm = 4
    }

    public enum RoleStatus
    {
        [Display(Name = "label.active", Description = "RoleActiveDescription")]
        Active = 1,
        [Display(Name = "label.inactive", Description = "RoleInActiveDescription")]
        Inactive = 0
    }

    public enum Role
    {
        [Display(Description = "System Admin")]
        SystemAdmin = 1,
        [Display(Description = "CSR")]
        CSR = 2,
        [Display(Description = "Sale")]
        Sale = 3,
        [Display(Description = "Agent")]
        Agent = 4,
        [Display(Description = "Registered User")]
        RegisteredUser = 5,
        [Display(Description = "Guest")]
        Guest = 6,
        [Display(Description = "Pending")]
        Pending = 7,
        [Display(Description = "Principal")]
        Principal = 8,
        [Display(Description = "Shipper")]
        Shipper = 9,
        [Display(Description = "Cruise Agent")]
        CruiseAgent = 10,
        [Display(Description = "Cruise Principal")]
        CruisePrincipal = 11,
        [Display(Description = "Warehouse")]
        Warehouse = 12,
        [Display(Description = "Factory")]
        Factory = 13
    }

    public enum RoleSequence
    {
        Principal = 10,
        Shipper = 20,
        Consignee = 30,
        NotifyParty = 40,
        AlsoNotifyParty = 50,
        Supplier = 60,
        Delegation = 70,
        PickupAddress = 80,
        BillingAddress = 90,
        OriginAgent = 100,
        DestinationAgent = 110,
        WarehouseProvider = 120
    }

    public enum IntegrationStatus
    {
        [Display(Name = "label.succeed")]
        Succeed = 1,
        [Display(Name = "label.failed")]
        Failed = 2
    }

    [Flags]
    public enum OrganizationType
    {
        [Display(Name = "label.general")]
        General = 1,
        [Display(Name = "label.agent")]
        Agent = 2,
        [Display(Name = "label.principal")]
        Principal = 4
    }

    public enum PurchaseOrderStatus
    {
        [Display(Name = "label.active")]
        Active = 1,
        [Display(Name = "label.cancel")]
        Cancel = 0
    }

    public enum RoutingOrderStatus
    {
        [Display(Name = "label.active")]
        Active = 1,
        [Display(Name = "label.cancel")]
        Cancel = 0
    }

    public enum BuyerComplianceStatus
    {
        [Display(Name = "label.active")]
        Active = 1,
        [Display(Name = "label.inactive")]
        Inactive = 0
    }

    public enum BuyerComplianceStage
    {
        [Display(Name = "label.draft")]
        Draft = 2,
        [Display(Name = "label.activated")]
        Activated = 1,
        [Display(Name = "label.cancelled")]
        Canceled = 0,
    }

    public enum RoutingOrderStageType
    {
        [Display(Name = "label.released")]
        Released = 20,
        [Display(Name = "label.rateAccepted")]
        RateAccepted = 30,
        [Display(Name = "label.rateConfirmed")]
        RateConfirmed = 40,
        [Display(Name = "label.booked")]
        ForwarderBookingRequest = 50,
        [Display(Name = "label.bookingConfirmed")]
        ForwarderBookingConfirmed = 60,
        [Display(Name = "label.shipmentDispatch")]
        ShipmentDispatch = 70,
        [Display(Name = "label.closed")]
        Closed = 80
    }

    public enum POStageType
    {
        [Display(Name = "label.draft")]
        Draft = 10,
        [Display(Name = "label.released")]
        Released = 20,
        [Display(Name = "label.booked")]
        ForwarderBookingRequest = 30,
        [Display(Name = "label.bookingConfirmed")]
        ForwarderBookingConfirmed = 40,
        [Display(Name = "label.cargoReceived")]
        CargoReceived = 45,
        [Display(Name = "label.shipmentDispatch")]
        ShipmentDispatch = 50,
        [Display(Name = "label.closed")]
        Closed = 60,
        Completed = 70
    }

    public enum POType
    {
        Bulk = 10,
        Blanket = 20,
        Allocated = 30
    }

    public enum OrderFulfillmentPolicy
    {
        AllowMissingPO = 10,
        NotAllowMissingPO = 20
    }

    public enum FulfillmentType
    {
        PO = 1,
        Bulk = 2,
        Warehouse = 3
    }

    public enum ImportDataProgressStatus
    {
        Started = 1,
        Success,
        Failed,
        Aborted,
        Warning
    }

    public enum PurchaseOrderTransmissionMethodType
    {
        EDI = 10,
        ExcelUpload = 20
    }

    public enum BuyerComplianceServiceType
    {
        Freight = 10,
        WareHouse = 20,
        FreightWareHouse = 30,
        WareHouseFreight = 40,

    }

    public enum AgentAssignmentMethodType
    {
        BySystem = 1,
        ByPO = 2
    }

    public enum PurchaseOrderTransmissionFrequencyType
    {
        Daily = 10,
        Weekly = 20,
        Other = 30
    }

    public enum ApprovalAlertFrequencyType
    {
        Daily = 24,
        Every12Hours = 12,
        Every6Hours = 6,
        Every3Hours = 3,
        Hourly = 1,
    }

    public enum ApprovalDurationType
    {
        Every24Hours = 24,
        Every36Hours = 36,
        Every48Hours = 48,
        Every72Hours = 72,
        NoExpiration = 0
    }

    public enum VerificationSettingType
    {
        AsPerPONotAllowOverride = 10,
        AsPerPOAllowOverride = 20,
        ManualInput = 30,
        AsPerPODefault = 40
    }

    public enum EmailSettingType
    {
        [Display(Name = "emailSettingType.bookingImportviaAPI")]
        BookingImportviaAPI = 10,
        [Display(Name = "emailSettingType.bookingImportedFailure")]
        BookingImportedFailure = 20,
        [Display(Name = "emailSettingType.bookingImportedSuccessfully")]
        BookingImportedSuccessfully = 30,
        [Display(Name = "emailSettingType.bookingApproval")]
        BookingApproval = 40,
        [Display(Name = "emailSettingType.bookingRejected")]
        BookingRejected = 50,
        [Display(Name = "emailSettingType.bookingApproved")]
        BookingApproved = 60,
        [Display(Name = "emailSettingType.bookingConfirmed")]
        BookingConfirmed = 70,
        [Display(Name = "emailSettingType.bookingCargoReceived")]
        BookingCargoReceived = 80
    }

    public enum AllowMixedPackType
    {
        NoMixedPack = 10,
        WithMixedPurchaseOrder = 20,
        WithMixedProduct = 30,
        WithMixedPurchaseOrderAndProduct = 40
    }

    [Flags]
    public enum ModeOfTransportType
    {
        [Display(Name = "label.sea", Description = "Sea")]
        Sea = 1 << 0,
        [Display(Name = "label.air", Description = "Air")]
        Air = 1 << 1,
        [Display(Name = "label.road", Description = "Road")]
        Road = 1 << 2,
        [Display(Name = "label.railway", Description = "Railway")]
        Railway = 1 << 3,
        [Display(Name = "label.multiModal", Description = "MultiModal")]
        MultiModal = 1 << 4,
        [Display(Name = "label.courier", Description = "Courier")]
        Courier = 1 << 5
    }

    [Flags]
    public enum CommodityType
    {
        [Display(Name = "label.generalGoods")]
        GeneralGoods = 1 << 0,
        [Display(Name = "label.garments")]
        Garments = 1 << 1,
        [Display(Name = "label.accessories")]
        Accessories = 1 << 2,
        [Display(Name = "label.toys")]
        Toys = 1 << 3,
        [Display(Name = "label.plasticGoods")]
        PlasticGoods = 1 << 4,
        [Display(Name = "label.household")]
        Household = 1 << 5,
        [Display(Name = "label.textiles")]
        Textiles = 1 << 6,
        [Display(Name = "label.hardware")]
        Hardware = 1 << 7,
        [Display(Name = "label.stationery")]
        Stationery = 1 << 8,
        [Display(Name = "label.houseware")]
        Houseware = 1 << 9,
        [Display(Name = "label.kitchenware")]
        Kitchenware = 1 << 10,
        [Display(Name = "label.footwear")]
        Footwear = 1 << 11,
        [Display(Name = "label.furniture")]
        Furniture = 1 << 12,
        [Display(Name = "label.electionics")]
        Electionics = 1 << 13,
        [Display(Name = "label.electricalGoods")]
        ElectricalGoods = 1 << 14,
        [Display(Name = "label.nonPerishableGroceries")]
        NonPerishableGroceries = 1 << 15
    }

    [Flags]
    public enum IncotermType
    {
        [Display(Description = "EXW")]
        EXW = 1 << 0,
        [Display(Description = "FCA")]
        FCA = 1 << 1,
        [Display(Description = "CPT")]
        CPT = 1 << 2,
        [Display(Description = "CIP")]
        CIP = 1 << 3,
        [Display(Description = "DAT")]
        DAT = 1 << 4,
        [Display(Description = "DAP")]
        DAP = 1 << 5,
        [Display(Description = "DDP")]
        DDP = 1 << 6,
        [Display(Description = "FAS")]
        FAS = 1 << 7,
        [Display(Description = "FOB")]
        FOB = 1 << 8,
        [Display(Description = "CFR")]
        CFR = 1 << 9,
        [Display(Description = "CIF")]
        CIF = 1 << 10,
        [Display(Description = "DPU")]
        DPU = 1 << 11
    }

    [Flags]
    public enum MovementType
    {
        [Display(Name = "CY/CY", Description = "CY/CY")]
        CY_CY = 1 << 0,

        [Display(Name = "CFS/CY", Description = "CFS/CY")]
        CFS_CY = 1 << 1,

        [Display(Name = "CY/CFS", Description = "CY/CFS")]
        CY_CFS = 1 << 2,

        [Display(Name = "CFS/CFS", Description = "CFS/CFS")]
        CFS_CFS = 1 << 3
    }

    [Flags]
    public enum LogisticsServiceType
    {
        [Display(Name = "label.internationalPortToPort", Description = "Port-to-Port")]
        InternationalPortToPort = 1 << 0,
        [Display(Name = "label.internationalPortToDoor", Description = "Port-to-Door")]
        InternationalPortToDoor = 1 << 1,
        [Display(Name = "label.internationalDoorToPort", Description = "Door-to-Port")]
        InternationalDoorToPort = 1 << 2,
        [Display(Name = "label.internationalDoorToDoor", Description = "Door-to-Door")]
        InternationalDoorToDoor = 1 << 3,
        [Display(Name = "label.internationalAirportToDoor", Description = "Airport-to-Door")]
        InternationalAirportToDoor = 1 << 4,
        [Display(Name = "label.internationalAirportToAirport", Description = "Airport-to-Airport")]
        InternationalAirportToAirport = 1 << 5,
        [Display(Name = "label.internationalDoorToAirport", Description = "Door-to-Airport")]
        InternationalDoorToAirport = 1 << 6,
    }

    public enum EquipmentType
    {
        [Display(Name = "label.twentyDG", ShortName = "20DG", Description = "20' Dangerous Container")]
        [EnumMember(Value = "20DG")]
        TwentyDG = 3,

        [Display(Name = "label.twentyFR", ShortName = "20FR", Description = "20' Flat Rack")]
        [EnumMember(Value = "20FR")]
        TwentyFR = 5,

        [Display(Name = "label.twentyGH", ShortName = "20GH", Description = "20' GOH Container")]
        [EnumMember(Value = "20GH")]
        TwentyGH = 7,

        [Display(Name = "label.twentyGP", ShortName = "20GP", Description = "20' Container")]
        [EnumMember(Value = "20GP")]
        TwentyGP = 10,

        [Display(Name = "label.twentyHC", ShortName = "20HC", Description = "20' High Cube")]
        [EnumMember(Value = "20HC")]
        TwentyHC = 11,

        [Display(Name = "label.twentyHT", ShortName = "20HT", Description = "20' HT Container")]
        [EnumMember(Value = "20HT")]
        TwentyHT = 12,

        [Display(Name = "label.twentyHW", ShortName = "20HW", Description = "20' High Wide")]
        [EnumMember(Value = "20HW")]
        TwentyHW = 13,

        [Display(Name = "label.twentyNOR", ShortName = "20NOR", Description = "20' Reefer Dry")]
        [EnumMember(Value = "20NOR")]
        TwentyNOR = 14,

        [Display(Name = "label.twentyOS", ShortName = "20OS", Description = "20' Both Full Side Door Opening Container")]
        [EnumMember(Value = "20OS")]
        TwentyOS = 15,

        [Display(Name = "label.twentyOT", ShortName = "20OT", Description = "20' Open Top Container")]
        [EnumMember(Value = "20OT")]
        TwentyOT = 16,

        [Display(Name = "label.fourtyGP", ShortName = "40GP", Description = "40' Container")]
        [EnumMember(Value = "40GP")]
        FourtyGP = 20,

        [Display(Name = "label.fourtyHC", ShortName = "40HC", Description = "40' High Cube")]
        [EnumMember(Value = "40HC")]
        FourtyHC = 21,

        [Display(Name = "label.fourtyHG", ShortName = "40HG", Description = "40' HC GOH Container")]
        [EnumMember(Value = "40HG")]
        FourtyHG = 22,

        [Display(Name = "label.fourtyHNOR", ShortName = "40HNOR", Description = "40' HC Reefer Dry Container")]
        [EnumMember(Value = "40HNOR")]
        FourtyHNOR = 23,

        [Display(Name = "label.fourtyHO", ShortName = "40HO", Description = "40' HC Open Top Container")]
        [EnumMember(Value = "40HO")]
        FourtyHO = 24,

        [Display(Name = "label.fourtyHQDG", ShortName = "40HQDG", Description = "40' HQ DG Container")]
        [EnumMember(Value = "40HQDG")]
        FourtyHQDG = 25,

        [Display(Name = "label.fourtyHR", ShortName = "40HR", Description = "40' HC Reefer Container")]
        [EnumMember(Value = "40HR")]
        FourtyHR = 26,

        [Display(Name = "label.fourtyHW", ShortName = "40HW", Description = "40' High Cube Pallet Wide")]
        [EnumMember(Value = "40HW")]
        FourtyHW = 27,

        [Display(Name = "label.fourtyNOR", ShortName = "40NOR", Description = "40' Reefer Dry")]
        [EnumMember(Value = "40NOR")]
        FourtyNOR = 28,

        [Display(Name = "label.fourtyOT", ShortName = "40OT", Description = "40' Open Top Container")]
        [EnumMember(Value = "40OT")]
        FourtyOT = 29,

        [Display(Name = "label.twentyRF", ShortName = "20RF", Description = "20' Reefer")]
        [EnumMember(Value = "20RF")]
        TwentyRF = 30,

        [Display(Name = "label.twentyTK", ShortName = "20TK", Description = "20' Tank Container")]
        [EnumMember(Value = "20TK")]
        TwentyTK = 31,

        [Display(Name = "label.twentyVH", ShortName = "20VH", Description = "20' Ventilated Container")]
        [EnumMember(Value = "20VH")]
        TwentyVH = 32,

        [Display(Name = "label.fourtyDG", ShortName = "40DG", Description = "40' Dangerous Conatiner")]
        [EnumMember(Value = "40DG")]
        FourtyDG = 33,

        [Display(Name = "label.fourtyFQ", ShortName = "40FQ", Description = "40' High Cube Flat Rack")]
        [EnumMember(Value = "40FQ")]
        FourtyFQ = 34,

        [Display(Name = "label.fourtyFR", ShortName = "40FR", Description = "40' Flat Rack")]
        [EnumMember(Value = "40FR")]
        FourtyFR = 35,

        [Display(Name = "label.fourtyGH", ShortName = "40GH", Description = "40' GOH Container")]
        [EnumMember(Value = "40GH")]
        FourtyGH = 36,

        [Display(Name = "label.fourtyPS", ShortName = "40PS", Description = "40' Plus")]
        [EnumMember(Value = "40PS")]
        FourtyPS = 37,

        [Display(Name = "label.fourtyRF", ShortName = "40RF", Description = "40' Reefer")]
        [EnumMember(Value = "40RF")]
        FourtyRF = 40,

        [Display(Name = "label.fourtyTK", ShortName = "40TK", Description = "40' Tank")]
        [EnumMember(Value = "40TK")]
        FourtyTK = 41,

        [Display(Name = "label.fourtyFiveGO", ShortName = "45GO", Description = "45' GOH")]
        [EnumMember(Value = "45GO")]
        FourtyFiveGO = 51,

        [Display(Name = "label.fourtyFiveHC", ShortName = "45HC", Description = "45' High Cube")]
        [EnumMember(Value = "45HC")]
        FourtyFiveHC = 52,

        [Display(Name = "label.fourtyFiveHG", ShortName = "45HG", Description = "45' HC GOH Container")]
        [EnumMember(Value = "45HG")]
        FourtyFiveHG = 54,

        [Display(Name = "label.fourtyFiveHT", ShortName = "45HT", Description = "45' Hard Top Container")]
        [EnumMember(Value = "45HT")]
        FourtyFiveHT = 55,

        [Display(Name = "label.fourtyFiveHW", ShortName = "45HW", Description = "45' HC Pallet Wide")]
        [EnumMember(Value = "45HW")]
        FourtyFiveHW = 56,

        [Display(Name = "label.fourtyFiveRF", ShortName = "45RF", Description = "45' Reefer Container")]
        [EnumMember(Value = "45RF")]
        FourtyFiveRF = 57,

        [Display(Name = "label.fourtyEightHC", ShortName = "48HC", Description = "48' HC Container")]
        [EnumMember(Value = "48HC")]
        FourtyEightHC = 58,

        [Display(Name = "label.airShipment", ShortName = "Air", Description = "Air")]
        Air = 50,

        [Display(Name = "label.lclShipment", ShortName = "LCL", Description = "LCL")]
        LCL = 60,

        [Display(Name = "label.truck", ShortName = "Truck", Description = "Truck")]
        Truck = 70
    }


    public enum POFulfillmentStatus
    {
        Active = 10,
        Inactive = 20
    }

    public enum POFulfillmentStage
    {
        [Display(Name = "label.draft")]
        Draft = 10,
        [Display(Name = "label.booked")]
        ForwarderBookingRequest = 20,
        [Display(Name = "label.bookingConfirmed")]
        ForwarderBookingConfirmed = 30,
        [Display(Name = "label.cargoReceived")]
        CargoReceived = 35,
        [Display(Name = "label.shipmentDispatch")]
        ShipmentDispatch = 40,
        [Display(Name = "label.closed")]
        Closed = 50
    }

    public enum POFulfillmentOrderStatus
    {
        Received = 2,
        Active = 1,
        Inactive = 0
    }

    public enum PurchaseOrderAdhocChangePriority
    {
        NotChanged = 0,
        Level1 = 1,
        Level2 = 2,
        Level3 = 3,
    }

    public enum POFulfillmentLoadStatus
    {
        Active = 1,
        Inactive = 0
    }

    public enum POFulfillmentItinerayStatus
    {
        Active = 10,
        Inactive = 20
    }

    public enum ValidationResultType
    {
        [Display(Name = "label.pendingForApproval")]
        PendingForApproval = 10,
        [Display(Name = "label.bookingAccepted")]
        BookingAccepted = 20,
        [Display(Name = "label.bookingRejected")]
        BookingRejected = 30,
        [Display(Name = "label.warehouseApproval")]
        WarehouseApproval = 40
    }

    [Flags]
    public enum FulfillmentAccuracyType
    {
        ShortShipment = 1,
        NormalShipment = 2,
        OverShipment = 4
    }

    [Flags]
    public enum CargoLoadabilityType
    {
        LightLoaded = 1,
        NormalLoaded = 2,
        OverLoaded = 4
    }

    [Flags]
    public enum BookingTimelessType
    {
        [Display(Description = "Early Booking")]
        EarlyBooking = 1,
        [Display(Description = "Ontime Booking")]
        OntimeBooking = 2,
        [Display(Description = "Late Booking")]
        LateBooking = 4
    }

    public enum ApproverSettingType
    {
        AnyoneInOrganization = 10,
        SpecifiedApprover = 20
    }

    public enum ItineraryIsEmptyType
    {
        Yes = 1,
        No = 0
    }

    public enum FieldDeserializationStatus
    {
        WasNotPresent,
        HasValue
    }

    public enum UnitUOMType
    {
        Each = 10,
        Pair = 20,
        Set = 30,
        Piece = 40
    }

    public enum PackageUOMType
    {
        Carton = 10,
        Pallet = 20,
        Bag = 30,
        Box = 40,
        Piece = 50,
        Roll = 60,
        Tube = 70,
        Package = 80,
        Bundle = 90,
        Set = 100,
        Can = 110,
        Case = 120,
        Crate = 130,
        Cylinder = 140,
        Drum = 150,
        Pipe = 160
    }

    public enum ExceptionType
    {
        [Display(Name = "label.poFulfillmentException")]
        POFulfillmentException = 10,
        [Display(Name = "label.shipmentException")]
        ShipmentException = 20,
        [Display(Name = "label.consignmentException")]
        ConsignmentException = 30,
        [Display(Name = "label.containerException")]
        ContainerException = 40,
        [Display(Name = "label.consolidationException")]
        ConsolidationException = 50,
    }

    public enum DimensionUnitType
    {
        CM = 10,
        INCH = 20
    }

    public enum BuyerApprovalStatus
    {
        Active = 10,
        Inactive = 20
    }

    public enum BuyerApprovalStage
    {
        [Display(Name = "label.pending")]
        Pending = 10,
        [Display(Name = "label.approved")]
        Approved = 20,
        [Display(Name = "label.rejected")]
        Rejected = 30,
        [Display(Name = "label.cancelled")]
        Cancel = 40,
        [Display(Name = "label.overdue")]
        Overdue = 50
    }

    public enum POFulfillmentBookingRequestStatus
    {
        Active = 10,
        Inactive = 20
    }

    public enum AgentType
    {
        Origin = 1,
        Destination = 2
    }

    public enum OrderType
    {
        Freight = 1,
        Cruise = 2
    }

    #region Cruise Business

    public enum CruiseOrderStatus
    {
        [Display(Name = "label.active")]
        Active = 1,
        [Display(Name = "label.cancel")]
        Cancel = 0
    }

    public enum CruiseUOM
    {
        /// <summary>
        /// Each
        /// </summary>
        Each = 10,

        /// <summary>
        /// Carton
        /// </summary>
        Cartons = 20,

        /// <summary>
        /// Package
        /// </summary>
        Packages = 30,

        /// <summary>
        /// Piece
        /// </summary>
        Pieces = 40,

        /// <summary>
        /// Unit
        /// </summary>
        Unit = 50
    }

    #endregion

    public enum ConsolidationStage
    {
        New = 10,
        Confirmed = 20
    }

    public enum ContractMasterStatus
    {
        [Display(Name = "label.active")]
        Active = 1,
        [Display(Name = "label.inactive")]
        Inactive = 0
    }

    public enum SchedulingStatus
    {
        [Display(Name = "label.active")]
        Active = 1,
        [Display(Name = "label.inactive")]
        Inactive = 0
    }

    public enum SurveyStatus
    {
        [Display(Name = "label.draft")]
        Draft = 10,
        [Display(Name = "label.published")]
        Published = 20,
        [Display(Name = "label.closed")]
        Closed = 30
    }

    public enum SurveyParticipantType
    {
        UserRole = 10,
        Organization = 20,
        SpecifiedUser = 30
    }

    public enum SurveySendToUserType
    {
        User = 1,
        UserInRelationship = 2
    }

    public enum SurveyQuestionType
    {
        [Display(Name = "label.singleAnswer")]
        SingleAnswer = 10,
        [Display(Name = "label.multipleAnswers")]
        MultiAnswers = 20,
        [Display(Name = "label.openEnded")]
        OpenEnded = 30,
        [Display(Name = "label.openEndedWithMultipleLines")]
        OpenEndedWithMultiLines = 40,
        [Display(Name = "label.ratingScale")]
        RatingScale = 50
    }

    public enum FileProtocolType
    {
        SFTP = 10,
        SCP = 20,
        FTP = 30,
        WebDAV = 40,
        AmazonS3 = 50
    }

    public enum BooleanOption
    {
        /// <summary>
        /// Yes
        /// </summary>
        Yes = 1,

        /// <summary>
        /// No
        /// </summary>
        No = 0
    }

    public enum Timezone
    {
        UTC8 = 8,
    }

    public enum CategorizedPOType
    {
        Supplier = 10,
        Consignee = 20,
        Destination = 30,
        Stage = 40,
        Status = 50
    };

    public enum SOFormGenerationFileType
    {
        Pdf = 10,
        Excel = 20
    }

    public enum POSyncMode
    {
        Add = 10,
        Update = 20,
        Delete = 30,
    }

    public enum PaymentStatusType
    {
        Paid = 1,
        Partial = 2,
    }

    public enum ViewSettingType
    {
        Field = 10,
        Column = 20
    }

    public enum DateForComparison
    {
        [Display(Name = "label.cargoReadyDates")]
        CargoReadyDate = 10,
        [Display(Name = "label.expectedShipDates")]
        ExpectedShipDate = 20,
    }
}