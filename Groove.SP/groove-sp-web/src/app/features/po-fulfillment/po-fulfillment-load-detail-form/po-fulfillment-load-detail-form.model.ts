import { PackageDimensionUnitType } from 'src/app/core';

export class POFulfillmentCustomerPOModel {
    public id: number;
    public customerPONumber: string;
    public productCode: string;
    public bookedPackage: number;
    public fulfillmentUnitQty: number;
    public openQty: number;
    public loadedQty: number;
    public packageUOM: string;
    public innerQuantity: number;
    public outerQuantity: number;
    public volume: number;
    public grossWeight: number;
    public netWeight?: number;
    public shippingMarks: string;
    public descriptionOfGoods: string;

    // Using on UI
    /**mark as model is dragging. */
    public dragging: boolean;
    /**sequence number*/
    public sequence: number;
}

export class POFulfillmentLoadModel {
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
    public status: number;
    public loadingDate: string;
    public details: Array<any>;
}

export class POFulfillmentLoadDetailModel {
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
    public packageUOM: string;
    public height?: number;
    public width?: number;
    public length?: number;
    public volume?: number;
    public netWeight?: number;
    public grossWeight: number;
    public shippingMarks: string;
    public packageDescription: string;

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

export class POFulfillmentLoadDetailFormModel extends POFulfillmentLoadDetailModel {

    private readonly DEFAULT_PACKAGE_UOM: string = 'Carton';

    public dimensionUnit: PackageDimensionUnitType;
    public customerPO: POFulfillmentCustomerPOModel;
    public load: POFulfillmentLoadModel;
    public originalIndex: number;

    public constructor(customerPO: POFulfillmentCustomerPOModel, selectedLoad?: POFulfillmentLoadModel, loads?: Array<POFulfillmentLoadModel>, dimensionUnit?: PackageDimensionUnitType) {
        super();
        this.originalIndex = selectedLoad.details?.length;
        this.load = selectedLoad;
        this.customerPO = customerPO;
        this.dimensionUnit = dimensionUnit || PackageDimensionUnitType.CM;
        this.customerPONumber = customerPO && customerPO.customerPONumber || null;
        this.productCode = customerPO && customerPO.productCode || null;
        this.packageUOM = this.load.packageUOM || this.DEFAULT_PACKAGE_UOM;

        // Set default values for model
        const loadDetails = [].concat.apply([], loads.map(x => x.details));

        const usedUnitQuantity = loadDetails.filter(x => x.poFulfillmentOrderId === customerPO.id).reduce((a, b) => a + (b['unitQuantity'] || 0), 0);
        const usedGrossWeight = loadDetails.filter(x => x.poFulfillmentOrderId === customerPO.id).reduce((a, b) => a + (b['grossWeight'] || 0), 0);
        const usedNetWeight = loadDetails.filter(x => x.poFulfillmentOrderId === customerPO.id).reduce((a, b) => a + (b['netWeight'] || 0), 0);
        const usedVolume = loadDetails.filter(x => x.poFulfillmentOrderId === customerPO.id).reduce((a, b) => a + (b['volume'] || 0), 0);

        // No need to re-calculate for PackageQuantity as it re-calculated from parent form app-po-fulfillment-load-detail
        this.packageQuantity = customerPO && customerPO.openQty;

        this.sequence = selectedLoad && selectedLoad.details && selectedLoad.details.length + 1 || 1;
        this.unitQuantity = (customerPO.fulfillmentUnitQty - usedUnitQuantity < 0 ) ? 0 : (customerPO.fulfillmentUnitQty - usedUnitQuantity);
        this.grossWeight = !customerPO.grossWeight ? 0 : ((customerPO.grossWeight - usedGrossWeight < 0 ) ? 0 : (customerPO.grossWeight - usedGrossWeight));
        this.netWeight = !customerPO.netWeight ? null : ((customerPO.netWeight - usedNetWeight < 0 ) ? 0 : (customerPO.netWeight - usedNetWeight));
        this.volume = !customerPO.volume ? 0 : ((customerPO.volume - usedVolume < 0 ) ? 0 : (customerPO.volume - usedVolume));

        this.shippingMarks = customerPO.shippingMarks;
        this.packageDescription = customerPO.descriptionOfGoods;
    }
}


