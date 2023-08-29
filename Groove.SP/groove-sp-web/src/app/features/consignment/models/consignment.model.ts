import { BaseModel } from 'src/app/core/models/base.model';
import { ShipmentModel } from 'src/app/core/models/shipments/shipment.model';

export interface ConsignmentModel extends BaseModel {
    id: number;
    shipmentId: number;
    consignmentType: string;
    consignmentDate: string | null;
    confirmedDate: string | null;
    executionAgentId: number;
    executionAgentName: string;
    agentReferenceNumber: string;
    consignmentMasterBL: string;
    consignmentHouseBL: string;
    shipFrom: string;
    shipFromETDDate: string;
    shipTo: string;
    shipToETADate: string;
    status: string;
    modeOfTransport: string;
    movement: string;
    unit: number | null;
    unitUOM: string;
    package: number | null;
    packageUOM: string;
    volume: number | null;
    volumeUOM: string;
    grossWeight: number | null;
    grossWeightUOM: string;
    netWeight: number | null;
    netWeightUOM: string;
    houseBillId: number | null;
    masterBillId: number | null;
    triangleTradeFlag: boolean;
    memoBOLFlag: boolean;
    sequence: number | null;
    shipment: ShipmentModel;
    serviceType: string;
}
