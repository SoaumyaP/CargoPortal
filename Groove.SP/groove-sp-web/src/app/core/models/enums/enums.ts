export enum Roles {
    System_Admin = 1,
    CSR = 2,
    Sale = 3,
    Agent = 4,
    Registered_User = 5,
    Guest = 6,
    Pending = 7,
    Principal = 8,
    Shipper = 9,
    CruiseAgent = 10,
    CruisePrincipal = 11,
    Warehouse = 12,
    Factory = 13
}

export enum UserStatus {
    Rejected = 0,
    Pending = 1,
    Active = 2,
    Inactive = 3,
    WaitForConfirm = 4
}

export enum RoleStatus {
    Active = 1,
    Inactive = 0
}

export enum ShipmentStatus {
    Active = "Active",
    Inactive = "Inactive"
}

export enum ConsignmentStatus {
    Active = "Active",
    Inactive = "Inactive"
}

export enum ArticleMasterStatus {
    Active = "Active",
    Inactive = "Inactive"
}

export enum OrganizationStatus {
    Inactive = 0,
    Active = 1,
    Pending = 2
}

export enum ConnectionType {
    Active = 1,
    Pending = 2,
    Inactive = 3
}

export enum BuyerComplianceStatus {
    Inactive = 0,
    Active = 1
}

export enum BuyerComplianceStage {
    Cancelled = 0,
    Activated = 1,
    Draft = 2
}

export enum BuyerComplianceServiceType
{
    Freight = 10,
    WareHouse = 20,
    FreightWareHouse = 30,
    WareHouseFreight = 40,
}

export enum OrganizationType {
    General = 1,
    Agent = 2,
    Principal = 4
}

export enum SurveySendToUserType
{
    User = 1,
    UserInRelationship = 2
}

export enum SurveyQuestionType
{
    SingleAnswer = 10,
    MultiAnswers = 20,
    OpenEnded = 30,
    OpenEndedWithMultiLines = 40,
    RatingScale = 50
}

export enum AgentType {
    None = 1,
    Import = 2,
    Export = 3,
    Both = 4
}

export enum SOFormGenerationFileType {
    Pdf = 10,
    Excel = 20
}

export enum OrganizationRole {
    Shipper = 1,
    Consignee = 2,
    NotifyParty = 3,
    AlsoNotify = 4,
    ImportBroker = 5,
    ExportBroker = 6,
    OriginAgent = 7,
    DestinationAgent = 8,
    Principal = 9,
    Supplier = 10
}

export enum OrganizationNameRole {
    Shipper = 'Shipper',
    Consignee = 'Consignee',
    NotifyParty = 'Notify Party',
    AlsoNotify = 'Also Notify',
    ImportBroker = 'Import Broker',
    ExportBroker = 'Export Broker',
    OriginAgent = 'Origin Agent',
    DestinationAgent = 'Destination Agent',
    Principal = 'Principal',
    Supplier = 'Supplier',
    Delegation = 'Delegation',
    BillingParty = 'Billing Party',
    Pickup = 'Pickup'
}

export enum RoleSequence
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
    DestinationAgent = 110
}

export enum StatusStyle {
    Active = 'active',
    Inactive = 'inactive',
    Rejected = 'inactive',
    Pending = 'pending',
    Cancel = 'inactive',
    WaitForConfirm = 'waitForConfirm',
    New = 'new',
    Confirmed = 'confirmed',
    // stage
    Draft = 'draft',
    Activated = 'activated',
    Cancelled = 'cancelled',
    Approved = 'active',
    Published = 'published',
    Closed = 'closed'
}

export enum MilestoneType {
    ShipmentFreight = 1,
    Container = 2,
    ShipmentCruiseSeaOrOcean = 3,
    ShipmentCruiseAirOrCourier = 4,
    ShipmentCruiseRoad = 5,
    AirFreightShipment = 6
}

export enum ActivityType {
    FreightShipment = 'SM',
    Container = 'CM',
    CruiseShipment = 'CSM',
    VesselActivity = 'VA'
}

export enum IntegrationLogStatus {
    Succeed = 1,
    Failed = 2
}

export enum PurchaseOrderStatus {
    Active = 1,
    Cancel = 0
}

export enum RoutingOrderStatus {
    Active = 1,
    Cancel = 0
}

export enum RoutingOrderStageType {
    Released = 20,
    RateAccepted = 30,
    RateConfirmed = 40,
    ForwarderBookingRequest = 50,
    ForwarderBookingConfirmed = 60,
    ShipmentDispatch = 70,
    Closed = 80
}

export enum CruiseOrderStatus {
    Active = 1,
    Cancel = 0
}

export enum POStageType {
    Draft = 10,
    Released = 20,
    ForwarderBookingRequest = 30,
    ForwarderBookingConfirmed = 40,
    CargoReceived = 45,
    ShipmentDispatch = 50,
    Closed = 60,
}

export enum POFulfillmentStageType {
    Draft = 10,
    ForwarderBookingRequest = 20,
    ForwarderBookingConfirmed = 30,
    CargoReceived = 35,
    ShipmentDispatch = 40,
    Closed = 50,
}

export enum FulfillmentType
{
    PO = 1,
    Bulk = 2,
    Warehouse = 3
}

export enum OrderFulfillmentPolicy {
    AllowMissingPO = 10,
    NotAllowMissingPO = 20
}

export enum POType {
    Bulk = 10,
    Blanket = 20,
    Allocated = 30
}

export enum POTypeText {
    Bulk = 'Bulk',
    Blanket = 'Blanket',
    Allocated = 'Allocated'
}

export enum ImportDataProgressStatus {
    Started = 1,
    Success = 2,
    Failed = 3,
    Aborted = 4,
    Warning = 5
}

export enum VerificationSetting {
    AsPerPO = 10,
    AsPerPOAllowOverride = 20,
    ManualInput = 30,
    AsPerPODefault = 40
}

export enum ApprovalAlertFrequency {
    Daily = 24,
    Every12Hours = 12,
    Every6Hours = 6,
    Every3Hours = 3,
    Hourly = 1,
}

export enum ApprovalDuration {
    Every24Hours = 24,
    Every36Hours = 36,
    Every48Hours = 48,
    Every72Hours = 72,
    NoExpiration = 0
}

export enum PurchaseOrderTransmissionMethod {
    EDI = 10,
    ExcelUpload = 20
}

export enum PurchaseOrderTransmissionFrequency {
    Daily = 10,
    Weekly = 20,
    Other = 30
}

export enum EquipmentType {
    TwentyGP = '20GP',
    TwentyNOR = '20NOR',
    TwentyRF = '20RF',
    TwentyHC = '20HC',
    FourtyGP = '40GP',
    FourtyNOR = '40NOR',
    FourtyRF = '40RF',
    FourtyHC = '40HC',
    FourtyFiveHC = '45HC',
    AirShipment = 'Air',
    LCLShipment = 'LCL',
    Truck = 'Truck'
}

export enum EquipmentStringType {
    TwentyGP = "20' Container",
    TwentyNOR = "20' Reefer Dry",
    TwentyRF = "20' Reefer",
    TwentyHC = "20' High Cube",
    FourtyGP = "40' Container",
    FourtyNOR = "40' Reefer Dry",
    FourtyRF = "40' Reefer",
    FourtyHC = "40' High Cube",
    FourtyFiveHC = "45' High Cube",
    AirShipment = "Air",
    LCLShipment = "LCL",
    Truck = "Truck"
}

export enum AllowMixedPack {
    NoMixedPack = 10,
    WithMixedPurchaseOrder = 20,
    WithMixedProduct = 30,
    WithMixedPurchaseOrderAndProduct = 40
}

export enum ModeOfTransportType {
    Sea = 'Sea',
    Ocean = 'Ocean',
    Air = 'Air',
    Courier = 'Courier',
    Road = 'Road',
    Railway = 'Railway',
    MultiModal = 'MultiModal'
}

export enum ModeOfTransport {
    Sea = 1 << 0,
    Air = 1 << 1,
    Road = 1 << 2,
    Railway = 1 << 3,
    MultiModal = 1 << 4,
    Courier = 1 << 5
}

export enum Movement {
    CY_CY = 1 << 0,
    CFS_CY = 1 << 1,
    CY_CFS = 1 << 2,
    CFS_CFS = 1 << 3
}

export enum Incoterm {
    'EXW' = 1 << 0,
    'FCA' = 1 << 1,
    'CPT' = 1 << 2,
    'CIP' = 1 << 3,
    'DAT' = 1 << 4,
    'DAP' = 1 << 5,
    'DDP' = 1 << 6,
    'FAS' = 1 << 7,
    'FOB' = 1 << 8,
    'CFR' = 1 << 9,
    'CIF' = 1 << 10
}

export enum Commodity {
    GeneralGoods = 10,
    Garments = 20,
    Accessories = 30,
    Toys = 40,
    PlasticGoods = 50,
    Household = 60,
    Textiles = 70,
    Hardware = 80,
    Stationery = 90,
    Houseware = 100,
    Kitchenware = 110,
    Footwear = 120,
    Furniture = 130,
    Electionics = 140,
    ElectricalGoods = 150,
    NonPerishableGroceries = 160
}

export enum LogisticsService {
    PortToPort = 'InternationalPortToPort',
    PortToDoor = 'InternationalPortToDoor',
    DoorToPort = 'InternationalDoorToPort',
    DoorToDoor = 'InternationalDoorToDoor'
}


export enum ValidationResultPolicy {
    PendingForApproval = 10,
    BookingAccepted = 20,
    BookingRejected = 30,
    WarehouseApproval = 40
}

export enum FulfillmentAccuracy {
    ShortShipment = 1,
    NormalShipment = 2,
    OverShipment = 4
}

export enum CargoLoadability {
    LightLoaded = 10,
    NormalLoaded = 20,
    OverLoaded = 30
}

export enum BookingTimeless {
    EarlyBooking = 1,
    OntimeBooking = 2,
    LateBooking = 4
}

export enum ApproverSetting {
    AnyoneInOrganization = 10,
    SpecifiedApprover = 20
}

export enum ItineraryIsEmptyType {
    Yes = 1,
    No = 0
}

export enum POFulfillmentStatus {
    Active = 10,
    Inactive = 20
}

export enum VesselStatus {
    Active = 1,
    Inactive = 0
}

export enum CarrierStatus {
    Active = 1,
    Inactive = 0
}

export enum EventCodeStatus {
    Active = 1,
    Inactive = 0
}

export enum POFulfillmentOrderStatus {
    Active = 1,
    Received = 2,
    Inactive = 0
}

export enum POFulfillmentLoadStatus {
    Active = 1,
    Inactive = 0
}

export enum PackageUOMType {
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

export enum UnitUOMType {
    Each = 10,
    Pair = 20,
    Set = 30,
    Piece = 40
}

export enum EntityType {
    Shipment = 'SHI',
    BillOfLading = 'BOL',
    MasterBill = 'MBL',
    Container = 'CTN',
    Consignment = 'CSM',
    CustomerPO = 'CPO',
    POFulfillment = 'POF',
    CruiseOrder = 'CRO',
    CruiseOrderItem = 'COI',
    RoutingOrder = 'ROD'
}

export enum AttachmentType {
    ShippingOrderForm = 'Shipping Order Form',
    PackingList = 'Packing List',
    CommercialInvoice = 'Commercial Invoice',
    MasterBL = 'Master BL',
    HouseBL = 'House BL',
    Manifest = 'Manifest',
    Others = 'Others',
    ExportLicense = 'Export License',
    CertificateOfOrigin = 'Certificate of Origin',
    FormA = 'Form A',
    FumigationCertificate = 'Fumigation Certificate',
    PackingDeclaration = 'Packing Declaration',
    MSDS = 'MSDS',
    LetterOfCredit = 'Letter of Credit',
    InsuranceCertificate = 'Insurance Certificate',
    BookingForm = 'Booking Form',
    Miscellaneous = 'Miscellaneous'
}

export enum BuyerApprovalStage {
    Pending = 10,
    Approved = 20,
    Rejected = 30,
    Cancelled = 40,
    Overdue = 50
}

export enum PackageDimensionUnitType {
    CM = 10,
    INCH = 20
}

export enum AgentType {
    Origin = 1,
    Destination = 2
}

export enum YesNoType {
    Yes = 1,
    No = 0
}

export enum ValidationDataType {
    Input = 'input',
    Business = 'business'
}

export enum PurchaseOrderAdhocChangePriority {
    NotChanged = 0,
    Level1 = 1,
    Level2 = 2,
    Level3 = 3
}

export enum ConsolidationStage {
    New = 10,
    Confirmed = 20
}

export enum ConsolidationStageName {
    New = 'New',
    Confirmed = 'Confirmed'
}

export enum BookingPortType {
    ShipFrom = 1,
    ShipTo = 2
}

export enum OrderType {
    Freight = 1,
    Cruise = 2
}

export enum FormMode {
    Add = 'add',
    Update = 'update',
    Edit = 'edit',
    View = 'view'
}

export enum MasterReportType {
    POLevel = 'PO level',
    ItemLevel = 'Item level'
}

export enum ShipmentTab {
    General,
    Ativity,
    Itinerary,
    CargoDetails,
    CustomerPo,
    Contact,
    Attachment,
    Dialog
}

export enum MessageDisplayOn {
    PurchaseOrders = 'Purchase Orders',
    Bookings = 'Bookings',
    Shipments = 'Shipments',
}

export enum Category {
    General = 'General',
}

export enum MasterDialogFilterCriteria {
    MasterBLNo = 'Master BL No.',
    HouseBLNo = 'House BL No.',
    ContainerNo = 'Container No.',
    PurchaseOrderNo = 'Purchase Order No.',
    BookingNo = 'Booking No.',
    ShipmentNo = 'Shipment No.',
}

export enum InvoiceType {
    Invoice = 'N',
    Statement = 'F',
    Manual = 'I'
}

export enum AgentAssignmentMode {
    Default = 'Default',
    Change = 'Change'
}

export enum VerificationSettingType {
    AsPerPONotAllowOverride = 10,
    AsPerPOAllowOverride = 20,
    ManualInput = 30
}

export enum EventLevelMapping {
    PurchaseOrder = 1,
    POFulfillment = 2,
    Shipment = 4,
    Container = 5
}

export enum StatisticKey {
    // End-to-end shipment status
    POBooked = 'booked',
    POUnbooked = 'unbooked',
    POInOriginDC = 'inOriginDC',
    VesselArrival = 'vesselArrival',
    POInTransit = 'inTransit',
    POCustomsCleared = 'customsCleared',
    POPendingDCDelivery = 'pendingDCDelivery',
    PODCDeliveryConfirmed = 'dcDeliveryConfirmed',
    POManagedToDate = 'managedToDate',

    POIssuedInLastWeek = 'poIssuedInLastWeek',
    POIssuedInThisWeek = 'poIssuedInThisWeek',

    ShipmentVolumeByOrigin = 'shipmentVolumeByOrigin',
    ShipmentVolumeByDestination = 'shipmentVolumeByDestination',
    ShipmentInLastWeek = 'shipmentInLastWeek',
    ShipmentInThisWeek = 'shipmentInThisWeek',

    ShipmentTop10Carrier = 'top10Carrier',

    CYCYShipmentVolumeInThisWeek = 'cycyShipmentVolumeInThisWeek',
    CFSCYShipmentVolumeInThisWeek = 'cfscyShipmentVolumeInThisWeek',
    CFSCFSShipmentVolumeInThisWeek = 'cfscfsShipmentVolumeInThisWeek',

    MonthlyPortToPortShipmentVolume = 'monthlyPortToPortShipmentVolume',
    MonthlyPortToDoorShipmentVolume = 'monthlyPortToDoorShipmentVolume',
    MonthlyDoorToDoorShipmentVolume = 'monthlyDoorToDoorShipmentVolume',
    MonthlyDoorToPortShipmentVolume = 'monthlyDoorToPortShipmentVolume',

    MonthlyCYCYShipmentVolume = 'monthlyCYCYShipmentVolume',
    MonthlyCFSCYShipmentVolume = 'monthlyCFSCYShipmentVolume',
    MonthlyCFSCFSShipmentVolume = 'monthlyCFSCFSShipmentVolume',

    ManualInputMovementShipmentVolume = 'manualInputMovementShipmentVolume',
    ManualInputServiceTypeShipmentVolume = 'manualInputServiceTypeShipmentVolume',

    ShipmentByConsigneeThisWeek = 'shipmentByConsigneeThisWeek',
    ShipmentByShipperThisWeek = 'shipmentByShipperThisWeek',
    
    DraftBooking = "draftBooking",
    pendingBooking = "pendingBooking",

    CategorizedSupplier = "categorizedSupplier",
    CategorizedConsignee = "categorizedConsignee",
    CategorizedDestination = "categorizedDestination",
    CategorizedStage = "categorizedStage",
    CategorizedStatus = "categorizedStatus",
    Shortships = "shortships",
}

export enum CategorizedPOType {
    Supplier = 10,
    Consignee = 20,
    Destination = 30,
    Stage = 40,
    Status = 50
}

export enum MaxLengthValueInput {
    PhoneNumber = 30
}

export enum ContractMasterStatus {
    Inactive = 0,
    Active = 1
}

export enum SchedulingStatus {
    Inactive = 0,
    Active = 1
}

export enum AgentAssignmentMethodType {
    bySystem = 1,
    byPO = 2
}

export enum EmailSettingType {
    BookingImportviaAPI = 10,
    BookingImportedFailure = 20,
    BookingImportedSuccessfully = 30,
    BookingApproval = 40,
    BookingRejected = 50,
    BookingApproved = 60,
    BookingConfirmed = 70,
    BookingCargoReceived = 80
}

 export enum DashboardChartName{
    shipmentVolumeByMovement = 'Shipment Volume By Movement',
    shipmentVolumeByServiceType = 'Shipment Volume By Service Type',
}

export enum SurveyStatus {
    Draft = 10,
    Published = 20,
    Closed = 30
}

export enum SurveyParticipantType
{
    UserRole = 10,
    Organization = 20,
    SpecifiedUser = 30
}

export enum DialogActionType
{
    Submit,
    Cancel,
    Close
}

export enum EventOrderType {
    Before = 0,
    After = 1
}

export enum EmailSettingTypeName {
    'emailSettingType.bookingImportviaAPI' = EmailSettingType.BookingImportviaAPI,
    'emailSettingType.bookingImportedFailure' = EmailSettingType.BookingImportedFailure,
    'emailSettingType.bookingImportedSuccessfully' = EmailSettingType.BookingImportedSuccessfully,
    'emailSettingType.bookingApproval' = EmailSettingType.BookingApproval,
    'emailSettingType.bookingRejected' = EmailSettingType.BookingRejected,
    'emailSettingType.bookingApproved' = EmailSettingType.BookingApproved,
    'emailSettingType.bookingConfirmed' = EmailSettingType.BookingConfirmed,
    'emailSettingType.bookingCargoReceived' = EmailSettingType.BookingCargoReceived
}

export enum ViewSettingModuleIdType
{
    PO_LIST = "PO.List",
    PO_ITEM_QUICK_SEARCH_LIST = "PO.ItemQuickSearch.List",
    PO_DETAIL = "PO.Detail",
    PO_DETAIL_CONTACTS = "PO.Detail.Contacts",
    PO_DETAIL_ITEMS = "PO.Detail.LineItems",
    PO_DETAIL_BOOKINGS = "PO.Detail.Bookings",
    PO_DETAIL_ALLOCATEDPOS = "PO.Detail.AllocatedPOs",
    
    BOOKING_LIST = "Booking.List",

    BULKBOOKING_COPY_LIST = "BulkBooking.CopyList",
    BULKBOOKING_DETAIL = "BulkBooking.Detail",
    BULKBOOKING_DETAIL_PLANNED_SCHEDULE = "BulkBooking.Detail.PlannedSchedule",
    BULKBOOKING_DETAIL_CONTACTS = "BulkBooking.Detail.Contacts",
    BULKBOOKING_DETAIL_CARGO_DETAILS = "BulkBooking.Detail.CargoDetails",
    BULKBOOKING_DETAIL_CARGO_DETAILS_ITEM = "BulkBooking.Detail.CargoDetails.Item",
    BULKBOOKING_DETAIL_LOADS = "BulkBooking.Detail.Loads",
    BULKBOOKING_DETAIL_LOAD_DETAILS = "BulkBooking.Detail.LoadDetails",
    BULKBOOKING_DETAIL_LOAD_DETAILS_CONTAINERINFO = "BulkBooking.Detail.LoadDetails.ContainerInfo",
    
    FREIGHTBOOKING_DETAIL = "FreightBooking.Detail",
    FREIGHTBOOKING_DETAIL_CONTACTS = "FreightBooking.Detail.Contacts",
    FREIGHTBOOKING_DETAIL_CUSTOMER_POS = "FreightBooking.Detail.CustomerPOs",
    FREIGHTBOOKING_DETAIL_LOADS = "FreightBooking.Detail.Loads",
    FREIGHTBOOKING_DETAIL_SHIPMENT_PLANNED_SCHEDULE = "FreightBooking.Detail.Shipment.PlannedSchedule",
    FREIGHTBOOKING_DETAIL_SHIPMENT = "FreightBooking.Detail.Shipment",
    FREIGHTBOOKING_DETAIL_LOAD_DETAILS = "FreightBooking.Detail.LoadDetails",
    FREIGHTBOOKING_DETAIL_LOAD_DETAILS_CONTAINERINFO = "FreightBooking.Detail.LoadDetails.ContainerInfo",
    FREIGHTBOOKING_DETAIL_SHIPMENT_SHIPMENTITINERARIES = "FreightBooking.Detail.Shipment.ShipmentItineraries"
}

export enum FormModeType {
    View = 'view',
    Edit = 'edit',
    Add = 'add',
    Copy = 'copy',
}

export enum PaymentStatusType {
    Paid = 1,
    Partial = 2
}