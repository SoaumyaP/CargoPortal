
import { DefaultEnUsLang } from "../constants/app-constants";
import { Category, EquipmentStringType, EquipmentType, MasterDialogFilterCriteria, MessageDisplayOn, SOFormGenerationFileType } from "../enums/enums";

export class DropDowns {
    public static OrganizationTypeList: any = [
        {
            text: 'label.internalUser',
            value: 0,
            isOfficial: false
        },
        {
            text: 'label.general',
            value: 1,
            isOfficial: true
        },
        {
            text: 'label.agent',
            value: 2,
            isOfficial: true
        },
        {
            text: 'label.principal',
            value: 4,
            isOfficial: true
        }
    ];

    public static AgentOrganizationType: any = [
        {
            text: 'label.none',
            value: 1
        },
        {
            text: 'label.import',
            value: 2
        },
        {
            text: 'label.export',
            value: 3
        },
        {
            text: 'label.both',
            value: 4
        },
    ];

    public static SOFormGenerationFileType: any = [
        {
            text: 'label.pdf',
            value: SOFormGenerationFileType.Pdf
        },
        {
            text: 'label.excel',
            value: SOFormGenerationFileType.Excel
        }
    ];

    public static AllLanguages: any = [
        { text: 'EN', value: DefaultEnUsLang, default: true, browserCultureLang: 'en-US', browserLang: 'en' },
        { text: '简体', value: 'zh-hans', default: false, browserCultureLang: 'zh-CN' },
        { text: '繁体', value: 'zh-hant', default: false, browserCultureLang: 'zh-TW' }
    ];

    public static ApprovalAlertFrequency: any = [
        { label: 'label.daily', value: 24 },
        { label: 'label.every12Hours', value: 12 },
        { label: 'label.every6Hours', value: 6 },
        { label: 'label.every3Hours', value: 3 },
        { label: 'label.hourly', value: 1 }
    ];

    public static ApprovalDuration: any = [
        { label: 'label.every24Hours', value: 24 },
        { label: 'label.every36Hours', value: 36 },
        { label: 'label.every48Hours', value: 48 },
        { label: 'label.every72Hours', value: 72 },
        { label: 'label.noExpiration', value: 0 },
    ];

    public static PurchaseOrderTransmissionMethod: any =  [
        { label: 'label.edi', value: 10 },
        { label: 'label.excelUpload', value: 20 }
    ];

    public static BuyerComplianceServiceType: any =  [
        { label: 'label.freight', value: 10 },
        { label: 'label.warehouse', value: 20 },
        { label: 'label.freightWarehouse', value: 30 },
        { label: 'label.warehouseFreight', value: 40 }

    ];

    public static PurchaseOrderFrequency: any =  [
        { label: 'label.daily', value: 10 },
        { label: 'label.weekly', value: 20 },
        { label: 'label.other', value: 30 },
    ];

    public static VerificationSetting: any =  [
        { label: 'label.asPerPONotAllowOverride', value: 10, default: true },
        { label: 'label.asPerPOAllowOverride', value: 20, default: true },
        { label: 'label.manualInput', value: 30 , default: true},
    ];

    public static ExpectedVerificationSetting: any =  [
        { label: 'label.asPerPODefault', value: 40, default: true },
        { label: 'label.asPerPONotAllowOverride', value: 10, default: true },
        { label: 'label.asPerPOAllowOverride', value: 20, default: true },
        { label: 'label.manualInput', value: 30 , default: true}
    ];

    public static AllowMixedPack: any =  [
        { label: 'label.noMixedPack', value: 10 },
        { label: 'label.withMixedPurchaseOrder', value: 20 },
        { label: 'label.withMixedProduct', value: 30 },
        { label: 'label.withMixedPurchaseOrderAndProduct', value: 40 },
    ];

    public static ModeOfTransport: any =  [
        { label: 'label.sea', value: 1 << 0 },
        { label: 'label.air', value: 1 << 1 },
        { label: 'label.road', value: 1 << 2 },
        { label: 'label.railway', value: 1 << 3 },
        { label: 'label.courier', value: 1 << 5 },
        { label: 'label.multiModal', value: 1 << 4 }
    ];

    public static Commodity: any =  [
        { label: 'label.generalGoods', value: 1 << 0 },
        { label: 'label.garments', value: 1 << 1 },
        { label: 'label.accessories', value: 1 << 2 },
        { label: 'label.toys', value: 1 << 3 },
        { label: 'label.plasticGoods', value: 1 << 4 },
        { label: 'label.household', value: 1 << 5 },
        { label: 'label.textiles', value: 1 << 6 },
        { label: 'label.hardware', value: 1 << 7 },
        { label: 'label.stationery', value: 1 << 8 },
        { label: 'label.houseware', value: 1 << 9 },
        { label: 'label.kitchenware', value: 1 << 10 },
        { label: 'label.footwear', value: 1 << 11 },
        { label: 'label.furniture', value: 1 << 12 },
        { label: 'label.electionics', value: 1 << 13 },
        { label: 'label.electricalGoods', value: 1 << 14 },
        { label: 'label.nonPerishableGroceries', value: 1 << 15 }
    ];

    public static CommodityString: any =  [
        { label: 'label.generalGoods', value: 'General Goods' },
        { label: 'label.garments', value: 'Garments' },
        { label: 'label.accessories', value: 'Accessories' },
        { label: 'label.toys', value: 'Toys' },
        { label: 'label.plasticGoods', value: 'Plastic Goods' },
        { label: 'label.household', value: 'Household' },
        { label: 'label.textiles', value: 'Textiles' },
        { label: 'label.hardware', value: 'Hardware' },
        { label: 'label.stationery', value: 'Stationery' },
        { label: 'label.houseware', value: 'Houseware' },
        { label: 'label.kitchenware', value: 'Kitchenware' },
        { label: 'label.footwear', value: 'Footwear' },
        { label: 'label.furniture', value: 'Furniture' },
        { label: 'label.electionics', value: 'Electionics' },
        { label: 'label.electricalGoods', value: 'Electrical Goods' },
        { label: 'label.nonPerishableGroceries', value: 'Non-perishable Groceries' }
    ];

    public static MovementType: any =  [
        { label: 'CY/CY', value: 1 << 0 },
        { label: 'CFS/CY', value: 1 << 1 },
        { label: 'CY/CFS', value: 1 << 2 },
        { label: 'CFS/CFS', value: 1 << 3 }
    ];

    public static MasterReportMovementType: any =  [
        { label: 'Any', value: ''},
        { label: 'CY', value: 'CY'},
        { label: 'CFS', value: 'CFS' },
    ];

    public static MasterReportType: any =  [
        { label: 'PO level', value: 'PO level'},
        { label: 'Item level', value: 'Item level'}
    ];

    /** Please sort incotermType by label (alphabetically) before insert value*/
    public static IncotermType: any =  [
        { label: 'CFR', value: 1 << 9 },
        { label: 'CIF', value: 1 << 10 },
        { label: 'CIP', value: 1 << 3 },
        { label: 'CPT', value: 1 << 2 },
        { label: 'DAP', value: 1 << 5 },
        { label: 'DAT', value: 1 << 4 },
        { label: 'DDP', value: 1 << 6 },
        { label: 'DPU', value: 1 << 11 },
        { label: 'EXW', value: 1 << 0 },
        { label: 'FAS', value: 1 << 7 },
        { label: 'FCA', value: 1 << 1 },
        { label: 'FOB', value: 1 << 8 },
    ];

    public static LogisticsService: any =  [
        { label: 'label.internationalPortToPort', value: 1 << 0 },
        { label: 'label.internationalPortToDoor', value: 1 << 1 },
        { label: 'label.internationalDoorToPort', value: 1 << 2 },
        { label: 'label.internationalDoorToDoor', value: 1 << 3 }
    ];

    public static ValidationResult: any =  [
        { label: 'label.pendingForApproval', value: 10 },
        { label: 'label.bookingAccepted', value: 20 },
        { label: 'label.bookingRejected', value: 30 },
        { label: 'label.warehouseApproval', value: 40 },
    ];

    public static FulfillmentAccuracy: any =  [
        { label: 'label.shortShipment', value: 1 << 0 },
        { label: 'label.normalShipment', value: 1 << 1 },
        { label: 'label.overShipment', value: 1 << 2 },
    ];

    public static CargoLoadability: any =  [
        { label: 'label.lightLoaded', value: 1 << 0 },
        { label: 'label.normalLoaded', value: 1 << 1 },
        { label: 'label.overLoaded', value: 1 << 2 },
    ];

    public static BookingTimeless: any =  [
        { label: 'label.earlyBooking', value: 1 << 0 },
        { label: 'label.ontimeBooking', value: 1 << 1 },
        { label: 'label.lateBooking', value: 1 << 2 },
    ];

    public static ApproverSetting: any =  [
        { label: 'label.anyoneInOrganization', value: 10 },
        { label: 'label.specifiedApprover', value: 20 },
    ];

    public static BuyerComplianceStatus: any = [
        { label: 'label.inactive', value: 0 },
        { label: 'label.active', value: 1 },
    ];

    public static ItineraryIsEmptyType: any = [
        { label: 'label.yes', value: 1 },
        { label: 'label.no', value: 0 },
    ];

    public static ItineraryType: any = [
        { label: 'label.require', value: 1 },
        { label: 'label.notRequire', value: 0 },
    ];

    public static POFulfillmentOrderStatus: any = [
        { label: 'label.active', value: 1 },
        { label: 'label.received', value: 2 },
        { label: 'label.inactive', value: 0 }
    ];

    public static POFulfillmentLoadStatus: any = [
        { label: 'label.active', value: 1 },
        { label: 'label.inactive', value: 0 }
    ];

    public static UnitUOMStringType: any = [
        { label: 'label.each', value: 'Each' },
        { label: 'label.pair', value: 'Pair' },
        { label: 'label.set', value: 'Set' },
        { label: 'label.piece', value: 'Piece' }
    ];

    public static EquipmentStringType: any =  [
        { label: 'label.lclShipment', value: 'LCL' },
        { label: 'label.twentyGP', value: '20GP' },
        { label: 'label.twentyNOR', value: '20NOR' },
        { label: 'label.twentyRF', value: '20RF' },
        { label: 'label.twentyHC', value: '20HC' },
        { label: 'label.fourtyGP', value: '40GP' },
        { label: 'label.fourtyNOR', value: '40NOR' },
        { label: 'label.fourtyRF', value: '40RF' },
        { label: 'label.fourtyHC', value: '40HC' },
        { label: 'label.fourtyFiveHC', value: '45HC' },
        { label: 'label.airShipment', value: 'Air' },
        { label: 'label.truck', value: 'Truck' }
    ];

    public static ContainerType: any =  [
        { label: 'label.twentyGP', value: EquipmentType.TwentyGP },
        { label: 'label.twentyNOR', value: EquipmentType.TwentyNOR },
        { label: 'label.twentyRF', value: EquipmentType.TwentyRF },
        { label: 'label.twentyHC', value: EquipmentType.TwentyHC },
        { label: 'label.fourtyGP', value: EquipmentType.FourtyGP },
        { label: 'label.fourtyNOR', value: EquipmentType.FourtyNOR },
        { label: 'label.fourtyRF', value: EquipmentType.FourtyRF },
        { label: 'label.fourtyHC', value: EquipmentType.FourtyHC },
        { label: 'label.fourtyFiveHC', value: EquipmentType .FourtyFiveHC }
    ]

    public static ContainerStringType: any =  [
        { label: 'label.twentyGP', value: EquipmentStringType.TwentyGP },
        { label: 'label.twentyNOR', value: EquipmentStringType.TwentyNOR },
        { label: 'label.twentyRF', value: EquipmentStringType.TwentyRF },
        { label: 'label.twentyHC', value: EquipmentStringType.TwentyHC },
        { label: 'label.fourtyGP', value: EquipmentStringType.FourtyGP },
        { label: 'label.fourtyNOR', value: EquipmentStringType.FourtyNOR },
        { label: 'label.fourtyRF', value: EquipmentStringType.FourtyRF },
        { label: 'label.fourtyHC', value: EquipmentStringType.FourtyHC },
        { label: 'label.fourtyFiveHC', value: EquipmentStringType.FourtyFiveHC }
    ]

    public static PackageUOMStringType: any = [
        { label: 'label.carton', value: 'Carton' },
        { label: 'label.pallet', value: 'Pallet' },
        { label: 'label.bag', value: 'Bag' },
        { label: 'label.box', value: 'Box' },
        { label: 'label.piece', value: 'Piece' },
        { label: 'label.roll', value: 'Roll' },
        { label: 'label.tube', value: 'Tube' },
        { label: 'label.package', value: 'Package' },
        { label: 'label.bundle', value: 'Bundle' },
        { label: 'label.set', value: 'Set' },
        { label: 'label.can', value: 'Can' },
        { label: 'label.case', value: 'Case' },
        { label: 'label.crate', value: 'Crate' },
        { label: 'label.cylinder', value: 'Cylinder' },
        { label: 'label.drum', value: 'Drum' },
        { label: 'label.pipe', value: 'Pipe' }
    ];

    public static ModeOfTransportStringType: any =  [
        { label: 'label.sea', value: 'Sea' },
        { label: 'label.air', value: 'Air' },
        { label: 'label.road', value: 'Road' },
        { label: 'label.railway', value: 'Railway' },
        { label: 'label.courier', value: 'Courier' },
        { label: 'label.multiModal', value: 'MultiModal' }
    ];

    /** Using for Itinerary. */
    public static ModeOfTransportStringTypeForItinerary: any =  [
        { label: 'label.sea', value: 'Sea' },
        { label: 'label.air', value: 'Air' },
        { label: 'label.road', value: 'Road' },
        { label: 'label.railway', value: 'Railway' },
        { label: 'label.courier', value: 'Courier' },
    ];

    public static MovementStringType: any =  [
        { label: 'CY/CY', value: 'CY_CY' },
        { label: 'CFS/CY', value: 'CFS_CY' },
        { label: 'CY/CFS', value: 'CY_CFS' },
        { label: 'CFS/CFS', value: 'CFS_CFS' }
    ];

    public static CYMovementStringType: any = [
        { label: 'CY/CY', value: 'CY/CY' },
        { label: 'CY/CFS', value: 'CY/CFS' }
    ];
    public static CFSMovementStringType: any = [
        { label: 'CFS/CY', value: 'CFS/CY' },
        { label: 'CFS/CFS', value: 'CFS/CFS' }
    ];

    public static CustomMovementStringType: any =  [
        { label: 'CY', value: 'CY_CY' },
        { label: 'CFS', value: 'CFS_CY' }
    ];

    public static CustomMovementType: any =  [
        { label: 'CY', value: 1 << 0 },
        { label: 'CFS', value: 1 << 1 }
    ];

    /** Please sort incotermType by label (alphabetically) before insert value*/
    public static IncotermStringType: any =  [
        { label: 'CFR', value: 'CFR' },
        { label: 'CIF', value: 'CIF' },
        { label: 'CIP', value: 'CIP' },
        { label: 'CPT', value: 'CPT' },
        { label: 'DAP', value: 'DAP' },
        { label: 'DAT', value: 'DAT' },
        { label: 'DDP', value: 'DDP' },
        { label: 'DPU', value: 'DPU' },
        { label: 'EXW', value: 'EXW' },
        { label: 'FAS', value: 'FAS' },
        { label: 'FCA', value: 'FCA' },
        { label: 'FOB', value: 'FOB' },
    ];

    public static LogisticsServiceStringType: any =  [
        { label: 'label.internationalPortToPort', value: 'InternationalPortToPort'},
        { label: 'label.internationalPortToDoor', value: 'InternationalPortToDoor'},
        { label: 'label.internationalDoorToPort', value: 'InternationalDoorToPort'},
        { label: 'label.internationalDoorToDoor', value: 'InternationalDoorToDoor'}
    ];

    public static LogisticsServiceType: any =  [
        { label: 'label.internationalPortToPort', value: 1 << 0},
        { label: 'label.internationalPortToDoor', value: 1 << 1},
        { label: 'label.internationalDoorToPort', value: 1 << 2},
        { label: 'label.internationalDoorToDoor', value: 1 << 3}
    ];

    public static AirLogisticServiceStringType = [
        { label: 'label.internationalAirportToAirport', value: 'InternationalAirportToAirport'},
        { label: 'label.internationalAirportToDoor', value: 'InternationalAirportToDoor'},
        { label: 'label.internationalDoorToAirport', value: 'InternationalDoorToAirport'},
        { label: 'label.internationalDoorToDoor', value: 'InternationalDoorToDoor'}
    ];

    public static AirLogisticServiceType = [
        { label: 'label.internationalAirportToDoor', value: 1 << 4},
        { label: 'label.internationalAirportToAirport', value: 1 << 5},
        { label: 'label.internationalDoorToAirport', value: 1 << 6},
        { label: 'label.internationalDoorToDoor', value: 1 << 3}
    ];

    public static PackageDimensionUnitStringType: any = [
        { label: 'CM', value: 10 },
        { label: 'INCH', value: 20 }
    ];

    public static BuyerApprovalStage: any = [
        { text: 'label.pending', value: 10 },
        { text: 'label.approved', value: 20 },
        { text: 'label.rejected', value: 30 },
        { text: 'label.cancelled', value: 40 },
        { text: 'label.overdue', value: 50 }
    ];

    public static POStageType: any = [
        { label: 'Any', value: 0},
        { label: 'label.released', value: 20 },
        { label: 'label.forwarderBookingRequest', value: 30 },
        { label: 'label.forwarderBookingConfirmed', value: 40 },
        { label: 'label.cargoReceived', value: 45 },
        { label: 'label.shipmentDispatch', value: 50 },
        { label: 'label.closed', value: 60}
    ];

    public static BookingStageType: any = [
        { label: 'Any', value: 0 },
        { label: 'label.draft', value: 10 },
        { label: 'label.forwarderBookingRequest', value: 20 },
        { label: 'label.forwarderBookingConfirmed', value: 30 },
        { label: 'label.cargoReceived', value: 35 },
        { label: 'label.shipmentDispatch', value: 40 },
        { label: 'label.closed', value: 50 }
    ];

    /** Please sort incotermType by label (alphabetically) before insert value*/
    public static IncotermCustomStringType: any =  [
        { label: 'Any', value: '0' },
        { label: 'CFR', value: 'CFR' },
        { label: 'CIF', value: 'CIF' },
        { label: 'CIP', value: 'CIP' },
        { label: 'CPT', value: 'CPT' },
        { label: 'DAP', value: 'DAP' },
        { label: 'DAT', value: 'DAT' },
        { label: 'DDP', value: 'DDP' },
        { label: 'DPU', value: 'DPU' },
        { label: 'EXW', value: 'EXW' },
        { label: 'FAS', value: 'FAS' },
        { label: 'FCA', value: 'FCA' },
        { label: 'FOB', value: 'FOB' },
    ];

    public static YesNoType: any = [
        { label: 'label.yes', value: 1 },
        { label: 'label.no', value: 0 },
    ];

    public static AgentType: any = [
        { label: 'label.origin', value: 1 },
        { label: 'label.destination', value: 2 },
    ];

    public static BookingPortType: any = [
        { label: 'label.shipFrom', value: 1 },
        { label: 'label.shipTo', value: 2 },
    ];

    public static MessageDisplayOn: any = [
        { label: 'Please select', value: null },
        { label: 'label.purchaseOrders', value: MessageDisplayOn.PurchaseOrders },
        { label: 'label.bookings', value: MessageDisplayOn.Bookings },
        { label: 'label.shipments', value: MessageDisplayOn.Shipments },
    ];

    public static MasterDialogFilterCriteria: any = [
        { label: 'Please select', value: null },
        { label: 'label.masterBLNo', value: MasterDialogFilterCriteria.MasterBLNo },
        { label: 'label.houseBillOfLadingNumber', value: MasterDialogFilterCriteria.HouseBLNo },
        { label: 'label.containerNo', value: MasterDialogFilterCriteria.ContainerNo },
        { label: 'label.purchaseOrderNo', value: MasterDialogFilterCriteria.PurchaseOrderNo },
        { label: 'label.fulfillmentNumber', value: MasterDialogFilterCriteria.BookingNo },
        { label: 'label.shipmentNo', value: MasterDialogFilterCriteria.ShipmentNo },
    ];

    public static Category: any = [
        { label: 'label.general', value: Category.General },
    ];

    public static ProductionStartedType: any = [
        { label: 'label.productionStarted', value: null},
        { label: 'label.yes', value: true},
        { label: 'label.no', value: false},
    ];
    public static QCRequiredType: any = [
        { label: 'label.qcRequired', value: null},
        { label: 'label.yes', value: true},
        { label: 'label.no', value: false},
    ];
    public static ShortShipType: any = [
        { label: 'label.shortShip', value: null},
        { label: 'label.yes', value: true},
        { label: 'label.no', value: false},
    ];
    public static SplitShipmentType: any = [
        { label: 'label.splitShipment', value: null},
        { label: 'label.yes', value: true},
        { label: 'label.no', value: false},
    ];

    public static BillOfLadingType: any = [
        { label: 'label.fCR', value: 'FCR'},
        { label: 'label.hBL', value: 'HBL'},
        { label: 'label.seawayBill', value: 'Seaway Bill'},
        { label: 'label.telexRelease', value: 'Telex Release'},
    ];

    public static TimeType: any = [
        { label: 'label.select', value: null},
        { label: 'label.am', value: 'AM'},
        { label: 'label.pm', value: 'PM'},
    ];

      /** Using for Scheduling */
    public static SchedulingDocumentFormats: any =  [
        { label: 'Excel 97-2003', value: 'XLS' },
        { label: 'Excel Worksheet', value: 'XLSX' },
    ];

    public static BuyerComplianceDateForComparisons: any = [
        /**
         * Cargo Ready Date or Ex-work Date
         */
        { label: 'label.cargoReadyDates', value: 10},
        /**
         * Expected Ship Date is default value
         */
        { label: 'label.expectedShipDates', value: 20},
    ];
}
