import { ModelBase } from '../model-base.model';

export interface ShipmentLoadDetailModel extends ModelBase {
    id: number;
    shipmentId: number | null;
    consignmentId: number | null;
    cargoDetailId: number | null;
    shipmentLoadId: number | null;
    containerId: number | null;
    consolidationId: number | null;
    package: number | null;
    packageUOM: string;
    unit: number | null;
    unitUOM: string;
    volume: number | null;
    volumeUOM: string;
    grossWeight: number | null;
    grossWeightUOM: string;
    netWeight: number | null;
    netWeightUOM: string;
    sequence: number | null;
    billOfLadingNos: { item1: number; item2: string; }[];
}
