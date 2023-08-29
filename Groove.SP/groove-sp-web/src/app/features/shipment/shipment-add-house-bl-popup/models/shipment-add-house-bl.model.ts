export class ShipmentAddHouseBLModel {
    id: number;
    billOfLadingNo: string;
    executionAgentId: number;
    billOfLadingType: string;
    mainCarrier: string;
    mainVessel: string;
    totalGrossWeight: number;
    totalGrossWeightUOM: string;
    totalNetWeight: number;
    totalNetWeightUOM: string;
    totalPackage: number;
    totalPackageUOM: string;
    totalVolume: number;
    totalVolumeUOM: string;
    jobNumber: string;
    issueDate: string;
    modeOfTransport: string;
    shipFrom: string;
    shipFromETDDate: string;
    shipTo: string;
    shipToETADate: string;
    movement: string;
    incoterm: string;

    originAgent:string;
    destinationAgent:string;
    customer:string;
}