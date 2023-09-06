export class ConsolidationModel {
    id: number;
    loadPlanNumber?: string;
    originCFS: string;
    originCFSCode?: string;
    modeOfTransport: string;
    cfsCutoffDate: Date;
    equipmentType: string;
    carrierSONumber?: string;
    totalGrossWeight?: number;
    totalGrossWeightUOM?: string;
    totalNetWeight?: number;
    totalNetWeightUOM?: string;
    totalPackage?: number;
    totalPackageUOM?: string;
    totalVolume: number;
    totalVolumeUOM?: string;
    stage: number;
    consignmentId?: number;
    containerId?: number;
    carrierSONo?: string;
    containerNo?: string;
    sealNo?: string;
    sealNo2?: string;
    loadingDate?: Date;
}