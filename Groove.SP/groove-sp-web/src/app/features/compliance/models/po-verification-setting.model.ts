import { VerificationSetting } from 'src/app/core';

export class PurchaseOrderVerificationSettingModel {
    id: number;
    expectedShipDateVerification: VerificationSetting;
    expectedDeliveryDateVerification: VerificationSetting;
    consigneeVerification: VerificationSetting;
    shipperVerification: VerificationSetting;
    shipFromLocationVerification: VerificationSetting;
    shipToLocationVerification: VerificationSetting;
    paymentTermsVerification: VerificationSetting;
    paymentCurrencyVerification: VerificationSetting;
    modeOfTransportVerification: VerificationSetting;
    incotermVerification: VerificationSetting;
    logisticsServiceVerification: VerificationSetting;
    movementTypeVerification: VerificationSetting;
    preferredCarrierVerification: VerificationSetting;
}