import { FulfillmentType, ModeOfTransportType, PackageDimensionUnitType, PackageUOMType, POFulfillmentLoadStatus, POFulfillmentStageType, POFulfillmentStatus, UnitUOMType } from 'src/app/core/models/enums/enums';
import { LocalDateTime } from 'src/app/core/models/local-date-time.model';
import { DataType, Model, ModelBase } from 'src/app/core/models/model-base.model';
import { ViewSettingModel } from 'src/app/core/models/viewSetting.model';

@Model
export class BulkFulfillmentModel extends ModelBase {
    id: number = 0;
    number: string | null = null;
    stage: POFulfillmentStageType = POFulfillmentStageType.Draft;
    status: POFulfillmentStatus = POFulfillmentStatus.Active;
    statusName: string | null = null;
    // General
    owner: string = null;
    @DataType(LocalDateTime)
    cargoReadyDate: LocalDateTime = null;
    incoterm: string | null = null;
    isPartialShipment: boolean = null;
    bookedBy: string = null;
    modeOfTransport: ModeOfTransportType = ModeOfTransportType.Sea;
    preferredCarrier: number = null;
    logisticsService: string = null;
    movementType: string = null;
    shipFrom: number = null;
    shipTo: number = null;
    shipFromName: string = null;
    shipToName: string = null;
    receiptPort: string = null;
    receiptPortId: number | string = null;
    deliveryPort: string = null;
    deliveryPortId: number | string = null;
    @DataType(LocalDateTime)
    expectedShipDate: LocalDateTime = null;
    @DataType(LocalDateTime)
    expectedDeliveryDate: LocalDateTime = null;
    remarks: string = null;
    currentOrganization: any = null;
    isContainDangerousGoods: boolean = false;
    isCIQOrFumigation: boolean = false;
    isBatteryOrChemical: boolean = false;
    isExportLicence: boolean = false;
    isShipperPickup: boolean = false;
    isNotifyPartyAsConsignee: boolean = false;
    isGeneratePlanToShip: boolean = false;
    isAllowMixedCarton: boolean = false;
    vesselName: string = null;
    voyageNo: string = null;
    fulfillmentType: FulfillmentType = FulfillmentType.Bulk;
    cyClosingDate: string = null;
    cyEmptyPickupTerminalDescription: string = null;

    attachments: any[] = [];
    contacts: any[] = [];
    loads: BulkFulfillmentLoadModel[] = [];
    orders: BulkFulfillmentOrderModel[] = [];
    shipments: any[] = [];
    itineraries: any[] = [];
    viewSettings: ViewSettingModel[] = [];
}

export class BulkFulfillmentOrderModel {
    id: number;
    poFulfillmentId: number;
    customerPONumber: string;
    productCode: string;
    productName: string;
    fulfillmentUnitQty: number;
    packageUOM: PackageUOMType;
    unitUOM: UnitUOMType;
    commodity: string;
    hsCode: string;
    countryCodeOfOrigin: string;
    bookedPackage: number | null;
    volume: number | null;
    grossWeight: number | null;
    netWeight: number | null;
    chineseDescription: string;
    shippingMarks: string;
    loadedQty: number;
    openQty: number;
    status: number;

    // Using on UI
    /**mark as model is dragging. */
    public dragging: boolean;
    /**sequence number*/
    public sequence: number;
}

export class BulkFulfillmentLoadModel {
    public id: number;
    public equipmentType: string;
    public loadReferenceNumber: string;
    public containerNumber: string;
    public packageUOM: string;
    public subtotalPackageQuantity: number;
    public subtotalUnitQuantity: number;
    public subtotalVolume: number;
    public subtotalGrossWeight: number;
    public subtotalNetWeight: number;
    public plannedVolume: number;
    public plannedNetWeight: number;
    public plannedPackageQuantity: number;
    public plannedGrossWeight: number;
    public sealNumber: string;
    public sealNumber2: string;
    public totalGrossWeight: number;
    public totalNetWeight: number;
    public status: POFulfillmentLoadStatus;
    public loadingDate: string;
    public gateInDate: string;
    public details: Array<BulkFulfillmentLoadDetailModel>;
}

export class BulkFulfillmentLoadDetailModel {
    public id: number;
    public poFulfillmentOrderId: number;
    public poFulfillmentLoadId: number;
    public customerPONumber: string;
    public productCode: string;
    public sequence: number;
    /**Loaded quantity */
    public unitQuantity: number;
    /**Loaded package */
    public packageQuantity: number;
    public packageUOM: PackageUOMType;
    public height?: number;
    public width?: number;
    public length?: number;
    public volume?: number;
    public netWeight?: number;
    public grossWeight: number;
    public shippingMarks: string;
    public packageDescription: string;

    // Using on UI
    /**mark as model is dragging. */
    public dragging: boolean;

    public constructor() {
        this.id = null;
        this.poFulfillmentOrderId = null;
        this.customerPONumber = null;
        this.productCode = null;
        this.sequence = null;
        this.unitQuantity = null;
        this.packageQuantity = null;
        this.packageUOM = null;
        this.height = null;
        this.width = null;
        this.length = null;
        this.volume = null;
        this.netWeight = null;
        this.grossWeight = null;
        this.shippingMarks = null;
        this.packageDescription = null;
    }
}

export class BulkFulfillmentLoadDetailFormModel extends BulkFulfillmentLoadDetailModel {

    private readonly DEFAULT_PACKAGE_UOM: PackageUOMType.Carton;

    public dimensionUnit: PackageDimensionUnitType;
    public order: BulkFulfillmentOrderModel;
    public load: BulkFulfillmentLoadModel;

    public constructor(orderModel: BulkFulfillmentOrderModel, selectedLoad?: BulkFulfillmentLoadModel, loads?: Array<BulkFulfillmentLoadModel>, dimensionUnit?: PackageDimensionUnitType) {
        super();
        this.load = selectedLoad;
        this.order = orderModel;
        this.dimensionUnit = dimensionUnit || PackageDimensionUnitType.CM;
        this.productCode = orderModel && orderModel.productCode || null;
        this.packageUOM = orderModel && orderModel.packageUOM || this.DEFAULT_PACKAGE_UOM;

        // Set default values for model
        const loadDetails = [].concat.apply([], loads.map(x => x.details));

        const usedUnitQuantity = loadDetails.filter(x => x.poFulfillmentOrderId === orderModel.id).reduce((a, b) => a + (b['unitQuantity'] || 0), 0);
        const usedGrossWeight = loadDetails.filter(x => x.poFulfillmentOrderId === orderModel.id).reduce((a, b) => a + (b['grossWeight'] || 0), 0);
        const usedNetWeight = loadDetails.filter(x => x.poFulfillmentOrderId === orderModel.id).reduce((a, b) => a + (b['netWeight'] || 0), 0);
        const usedVolume = loadDetails.filter(x => x.poFulfillmentOrderId === orderModel.id).reduce((a, b) => a + (b['volume'] || 0), 0);

        // No need to re-calculate for PackageQuantity as it re-calculated from parent form app-po-fulfillment-load-detail
        this.packageQuantity = orderModel && orderModel.openQty;

        this.sequence = selectedLoad && selectedLoad.details && selectedLoad.details.length + 1 || 1;
        this.unitQuantity = (orderModel.fulfillmentUnitQty - usedUnitQuantity < 0) ? 0 : (orderModel.fulfillmentUnitQty - usedUnitQuantity);
        this.grossWeight = !orderModel.grossWeight ? 0 : ((orderModel.grossWeight - usedGrossWeight < 0) ? 0 : (orderModel.grossWeight - usedGrossWeight));
        this.netWeight = !orderModel.netWeight ? null : ((orderModel.netWeight - usedNetWeight < 0) ? 0 : (orderModel.netWeight - usedNetWeight));
        this.volume = !orderModel.volume ? 0 : ((orderModel.volume - usedVolume < 0) ? 0 : (orderModel.volume - usedVolume));

        this.shippingMarks = orderModel.shippingMarks;
        this.packageDescription = orderModel.productName;
    }
}