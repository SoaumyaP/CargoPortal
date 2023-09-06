import { ConsignmentModel } from 'src/app/features/consignment/models/consignment.model';
import { ConsolidationModel } from 'src/app/features/consolidation/models/consolidation.model';
import { ContractMasterModel } from 'src/app/features/shipment/master-bill-of-lading/models/contract-master-model';
import { ActivityModel } from '../activity/activity.model';
import { BaseModel } from '../base.model';
import { OrderType, POFulfillmentStageType } from '../enums/enums';
import { ShipmentBillOfLadingModel } from './shipment-bill-of-lading.model';
import { ShipmentContactModel } from './shipment-contact.model';
import { ShipmentItemModel } from './shipment-item.model';
import { ShipmentLoadDetailModel } from './shipment-load-detail.model';

export interface ShipmentModel extends BaseModel {
    id: number;
    shipmentNo: string;
    buyerCode: string;
    customerReferenceNo: string;
    modeOfTransport: string;
    cargoReadyDate: string;
    bookingDate: string;
    shipmentType: string;
    shipFrom: string;
    shipFromETDDate: string;
    shipTo: string;
    shipToETADate: string | null;
    movement: string;
    totalPackage: number | null;
    totalPackageUOM: string;
    totalUnit: number | null;
    totalUnitUOM: string;
    totalGrossWeight: number | null;
    totalGrossWeightUOM: string;
    totalNetWeight: number | null;
    totalNetWeightUOM: string;
    totalVolume: number | null;
    totalVolumeUOM: string;
    serviceType: string;
    incoterm: string;
    status: string;
    isFCL: boolean;
    fulfillmentId: number | null;
    fulfillmentNumber: string;
    bookingReferenceNo: string;
    fulfillmentStage: POFulfillmentStageType;
    isItineraryConfirmed: boolean;
    cyEmptyPickupTerminalCode: string | null;
    cyEmptyPickupTerminalDescription: string | null;
    cfsWarehouseCode: string | null;
    cfsWarehouseDescription: string | null;
    cyClosingDate: string | null;
    cfsClosingDate: string | null;
    billOfLadingNos: { item1: number; item2: string; }[];
    masterBillNos: { item1: number; item2: string; }[];
    bookingReferenceNumber: string;
    orderType: string;
    executionAgentName: string;
    shipperName: string;
    consigneeName: string;
    lastestActivity: string;
    isConfirmContainer: number;
    isConfirmConsolidation: number;
    latestMilestone: string;
    shipper: string;
    originAgent: string;
    destinationAgent: string;
    consignee: string;
    nominee: string;
    commercialInvoiceNo: string | null;
    invoiceDate: string | null;
    /**From buyer compliance. */
    enforceCommercialInvoiceFormat: boolean | null;
    contacts: ShipmentContactModel[];
    activities: ActivityModel[];
    shipmentBillOfLadings: ShipmentBillOfLadingModel[];
    shipmentItems: ShipmentItemModel[];
    consignments: ConsignmentModel[];
    shipmentLoadDetails: ShipmentLoadDetailModel[];
    consolidations: ConsolidationModel[];

    carrierContractNo?: string;
    /**
     * Value is from contractMaster.
     */
    carrierContractType?: string;

    /**
     * To contain information on contract.contractType
     */
    contractMaster?: ContractMasterModel;

    isException: boolean;

}

export interface ShipmentMilestoneModel {
    activityCode: string;
    title: string;
}