import { Commodity, 
    FulfillmentAccuracy, 
    Incoterm, 
    LogisticsService, 
    ModeOfTransport,
    Movement } from 'src/app/core';

export class ComplianceSelectionModel {
    id: number;
    modeOfTransportIds: ModeOfTransport[];
    commodityIds: Commodity[];
    shipFromLocationIds: string[];
    shipToLocationIds: string[];
    movementTypeIds: Movement[];
    incotermTypeIds: Incoterm[];
    carrierIds: string[];
    fulfillmentAccuracies: FulfillmentAccuracy;
    carrierSelectionNotes: string;
    logisticsServiceSelectionIds: LogisticsService[];
}