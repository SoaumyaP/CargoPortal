import { BaseModel } from 'src/app/core/models/base.model';
import { ContractMasterModel } from './contract-master-model';

export interface MasterBillOfLadingModel extends BaseModel {
    id?: number;
    executionAgentId?: number;
    masterBillOfLadingNo: string;
    masterBillOfLadingType?: string;
    modeOfTransport?: string;
    movement?: string;
    carrierContractNo?: string;
    carrierName?: string;
    scac?: string;
    airlineCode?: string;
    vesselFlight?: string;
    vessel?: string;
    voyage?: string;
    flightNo?: string;
    placeOfReceipt?: string;
    portOfLoading?: string;
    portOfDischarge?: string;
    placeOfDelivery?: string;
    placeOfIssue?: string;
    issueDate: Date;
    onBoardDate: Date;

    /**
     * To define whether it is direct master or not
     *
     * + True, link directly to Shipment -> link to Shipment/Container via Consignments
     *
     * + False, link to House bill of lading -> link to Shipment/Container via BillOfLadingShipmentLoads
     */
    isDirectMaster: boolean;

    /**
     * For front-end only as editing Master bill of lading.
     * It links to Flight number for Air, else Vessel flight if Sea
     */
    vesselVoyage?: string;

    /**
     * To temporary set Itinerary Id linked to
     */
    itineraryId?: number;

    /**
     * To contain information on contract
     */
    contractMaster?: ContractMasterModel;

}


