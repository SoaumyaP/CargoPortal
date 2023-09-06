import { PackageUOMType, UnitUOMType } from '../enums/enums';
import { ModelBase } from '../model-base.model';

export interface ShipmentItemModel extends ModelBase {
    id: number;
    poLineItemId: number | null;
    purchaseOrderId: number | null;
    customerPONumber: string;
    productCode: string;
    productName: string;
    orderedUnitQty: number;
    fulfillmentUnitQty: number;
    balanceUnitQty: number;
    loadedQty: number;
    openQty: number;
    unitUOM: UnitUOMType;
    packageUOM: PackageUOMType;
    commodity: string;
    hsCode: string;
    countryCodeOfOrigin: string;
    shipmentId: number;
    bookedPackage: number | null;
    volume: number | null;
    grossWeight: number | null;
    netWeight: number | null;
    innerQuantity: number | null;
    outerQuantity: number | null;
}
