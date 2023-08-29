import { VerificationSetting } from 'src/app/core';

export class ProductVerificationSettingModel {
    id: number;
    preferredCarrierVerification: VerificationSetting;
    productCodeVerification: VerificationSetting;
    commodityVerification: VerificationSetting;
    hsCodeVerification: VerificationSetting;
    countryOfOriginVerification: VerificationSetting;
    isRequireGrossWeight: boolean;
    isRequireVolume: boolean;
}