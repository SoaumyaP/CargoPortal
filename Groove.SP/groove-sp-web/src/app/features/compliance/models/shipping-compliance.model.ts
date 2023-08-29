import { AllowMixedPack } from 'src/app/core';

export class ShippingComplianceModel {
    id: number;
    allowPartialShipment: boolean;
    allowMixedCarton: boolean;
    allowMixedPack: AllowMixedPack;
    allowMultiplePOPerFulfillment: boolean;
}