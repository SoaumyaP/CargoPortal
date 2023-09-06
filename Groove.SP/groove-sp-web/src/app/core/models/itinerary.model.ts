import { BaseModel } from './base.model';

export interface ItineraryModel extends BaseModel {
    id: number;
    sequence: number | null;
    modeOfTransport: string;
    carrierName: string;
    scac: string;
    airlineCode: string;
    vesselFlight: string;
    vesselName: string;
    voyage: string;
    flightNumber: string;
    loadingPort: string;
    etdDate: string;
    etaDate: string;
    dischargePort: string;
    roadFreightRef: string;
    status: string;
    isImportFromApi: boolean;
    isCalledFromApp: boolean;
    consignmentId: number | null;
    scheduleId: number | null;
}
