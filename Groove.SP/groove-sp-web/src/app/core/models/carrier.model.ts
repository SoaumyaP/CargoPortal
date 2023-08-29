import { CarrierStatus } from "./enums/enums";

export class CarrierModel {
    id: number;
    carrierCode: string;
    carrierNumber: number;
    modeOfTransport: string;
    name: string;
    status: CarrierStatus;
}

export class CarrierSelectionModel extends CarrierModel {
    displayName: string;
    /**
     *
     */
    constructor(carrier: CarrierModel) {
        super();
        this.modeOfTransport = carrier.modeOfTransport
        this.name = carrier.name;
        this.carrierCode = carrier.carrierCode;
        this.displayName = `${carrier.carrierCode} - ${carrier.name}`;
    }
}