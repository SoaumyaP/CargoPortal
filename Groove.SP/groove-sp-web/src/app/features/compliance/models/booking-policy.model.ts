import { ApproverSetting, 
    BookingTimeless, 
    CargoLoadability, 
    FulfillmentAccuracy, 
    Incoterm, 
    ItineraryIsEmptyType, 
    LogisticsService, 
    ModeOfTransportType, 
    Movement, 
    ValidationResultPolicy } from 'src/app/core';

export class BookingPolicyModel {
    name: string;
    modeOfTransportIds: ModeOfTransportType[];
    incotermTypeIds: Incoterm[];
    shipFromIds: string[];
    shipToIds: string[];
    fulfillmentAccuracyIds: FulfillmentAccuracy[];
    bookingTimelessIds: BookingTimeless[];
    logisticsServiceSelectionIds: LogisticsService[];
    movementTypeIds: Movement[];
    carrierIds: string[];
    cargoLoadabilityIds: CargoLoadability[];
    approverSetting: ApproverSetting;
    approverUser: string;
    action: ValidationResultPolicy;
    itineraryIsEmpty: ItineraryIsEmptyType | null;
    order: number;
}