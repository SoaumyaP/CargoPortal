import { BillOfLadingModel } from 'src/app/features/shipment/bill-of-lading/models/bill-of-lading.model';
import { ModelBase } from '../model-base.model';

export interface ShipmentBillOfLadingModel extends ModelBase {
    shipmentId: number;
    billOfLadingId: number;
    billOfLading: BillOfLadingModel;
}
