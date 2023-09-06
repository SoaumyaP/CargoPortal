import { BaseModel } from 'src/app/core/models/base.model';

/**
 * House bill of lading model
 */
export interface BillOfLadingModel extends BaseModel {
    id: number;
    /**
     * House bill of lading number
     */
    billOfLadingNo: string;
    executionAgentId: number | null;
    billOfLadingType: string;
    mainCarrier: string;
    mainVessel: string;
    totalGrossWeight: number | null;
    totalGrossWeightUOM: string;
    totalNetWeight: number | null;
    totalNetWeightUOM: string;
    totalPackage: number | null;
    totalPackageUOM: string;
    totalVolume: number | null;
    totalVolumeUOM: string;
    jobNumber: string;
    issueDate: string;
    modeOfTransport: string;
    shipFrom: string;
    shipFromETDDate: Date;
    shipTo: string;
    shipToETADate: Date;
    movement: string;
    incoterm: string;
    shipper: string;
    originAgent: string;
    destinationAgent: string;
    consignee: string;
    notifyParty: string;
    nominationPrincipal: string;
    contacts: Array<BillOfLadingContactModel>;

    // master bill of lading
    masterBillOfLadingNo: string;
    masterBillOfLadingId?: number;
}

export interface BillOfLadingContactModel extends BaseModel {
    id: number;
    billOfLadingId: number;
    organizationId: number;
    organizationRole: string;
    companyName: string;
    address: string;
    contactName: string;
    contactNumber: string;
    contactEmail: string;
}
