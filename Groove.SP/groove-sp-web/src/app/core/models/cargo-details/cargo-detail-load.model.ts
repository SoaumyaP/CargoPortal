export interface CargoDetailLoadModel {
    id: number;
    shipmentId: number;
    shipmentNo: string;
    orderId: number;
    poNumber: string;
    itemId: number;
    productCode: string;
    unit: number | null;
    unitUOM: string;
    package: number | null;
    packageUOM: string;
    volume: number | null;
    volumeUOM: string;
    grossWeight: number;
    grossWeightUOM: string;
    netWeight: number | null;
    netWeightUOM: string;
    // Load info
    consignmentId: number | null;
    containerId: number | null;
    shipmentLoadId: number;
    sequence: number;
    // On UI: Drag drop status
    dragging: boolean;
}