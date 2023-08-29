import { BaseModel } from "../base.model";
import { OrderType } from "../enums/enums";
import { ShipmentModel } from "../shipments/shipment.model";

export interface CargoDetailModel extends BaseModel{
    id: number;
    shipmentId: number;
    sequence: number;
    shippingMarks: string;
    description: string;
    unit: number;
    unitUOM: string;
    package: number;
    packageUOM: string;
    volume: number;
    volumeUOM: string;
    grossWeight: number;
    grossWeightUOM: string;
    netWeight: number | null;
    netWeightUOM: string;
    commodity: string;
    hsCode: string;
    productNumber: string;
    countryOfOrigin: string;
    orderType: OrderType;
    // Depend on OrderType, it will link to PurchaseOrders or CruiseOrders
    orderId: number | null;
    itemId: number | null;
    shipment: ShipmentModel;
  }