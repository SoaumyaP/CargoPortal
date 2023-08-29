import { EquipmentType } from '../enums/enums';

export const CYEquipmentTypes = [
    EquipmentType.TwentyGP,
    EquipmentType.TwentyRF,
    EquipmentType.FourtyRF,
    EquipmentType.TwentyNOR,
    EquipmentType.FourtyNOR,
    EquipmentType.TwentyHC,
    EquipmentType.FourtyHC,
    EquipmentType.FourtyFiveHC,
    EquipmentType.FourtyGP
];
export const CFSEquipmentTypes = [
    EquipmentType.LCLShipment
];

export enum MovementTypes {
    CYSlashCY = 'CY/CY',
    CFSSlashCY = 'CFS/CY',
    CYUnderscoreCY = 'CY_CY',
    CFSUnderscoreCY = 'CFS_CY',
}

export const ValidationResultType = {
    valid: 'Valid',
    invalid: 'Invalid'
};

export const POAdhocChangeLevels = {
    level1: 'Level1',
    level2: 'Level2',
    level3: 'Level3'
};

// #region App error message constants
export const POAdhocChangeErrorMsgs = {
    level1: 'msg.poBeingRevisedLevel1',
    level2: 'msg.poBeingRevisedLevel2',
    level3: 'msg.poBeingRevisedLevel3'
};

export const DocumentLevel = {
    Shipment: "Shipment",
    BillOfLading: "House BL",
    MasterBill: "Master BL",
    Container: "Container",
    POFulfillment: "Booking"
};

export enum MasterBillOfLadingTypes {
    OceanBillOfLading = 'Ocean bill of lading'
}

export const EmailValidationPattern = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
export const MultipleEmailValidationPattern = /^(|([a-zA-Z0-9_\-\.\+]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}){1,25})+((\s*(,\s*)|\s*$)(([a-zA-Z0-9_\-\.\+]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}){1,25})+)*$/;
export const MultipleEmailDomainValidationPattern = /^(|([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}){1,25})+((\s*(,+\s{0,})|\s*$)(([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}){1,25})+)*$/;

export const CubicMeter = 'CBM';
export const Kilograms = 'KGS';
export const Carton = 'Carton';

export const DefaultValue2Hyphens = '--';
export const DefaultDebounceTimeInput = 1000;

export const ReportName = {
    MasterSummaryReportNew: 'Master Summary Report New'
};

/**VA - Vessel Departure */
export const EVENT_7001 = '7001';
/**VA - Vessel Arrival */
export const EVENT_7002 = '7002';
/**SM - Shipment handover to consignee */
export const EVENT_2054 = '2054';

export enum bool {
    TrueString = 'True',
    FalseString = 'False'
}

export enum Separator {
    Semicolon = ';',
    TILDE = '~',
    HYPHEN = '-',
    COMMA = ','
}

export enum GAEventCategory {
    PurchaseOrder = 'Purchase Order',
    POBooking = 'PO Booking',
    WarehouseBooking = 'Warehouse Booking',
    BulkBooking = 'Bulk Booking',
    BookingApproval = 'Booking Approval',
    Shipment = 'Shipment',
    CruiseOrder = 'Cruise Order',
    Dashboard = 'Dashboard',
    UserProfile = 'User Profile',
    QuickTrack = 'Quick Track',
    QuickSearch = 'Quick Search',
    Container = 'Container',
    Consignment = 'Consignment',
    Consolidation = 'Consolidation',
    FreightSchedule = 'Freight Schedule',
    HouseBill = 'House Bill',
    MasterBill = 'Master Bill',
    RoutingOrder = 'Routing Order'
}

export const DefaultEnUsLang = 'en-us';

export const MilestoneEventCode = [
    'PM', //POMilestone
    'FM', //BookingMilestone
    'SM', //ShipmentMilestone
    'CM', //ContainerMilestone
    'VM' //VesselMilestone
];

export const tableRow = (node) => {
    if (node.tagName) {
        return node.tagName.toLowerCase() === "tr";
    }
};

export const closest = (node, predicate) => {
    while (node && !predicate(node)) {
        node = node.parentNode;
    }
    return node;
};

export enum DynamicView {
    moduleKey = 'viewSettingModuleId'
}