import { ApprovalAlertFrequency, 
    ApprovalDuration, 
    ApproverSetting, 
    BuyerComplianceStage, 
    BuyerComplianceStatus, 
    POType, PurchaseOrderTransmissionFrequency, 
    PurchaseOrderTransmissionMethod, 
    ValidationResultPolicy} from 'src/app/core';
import { AgentAssignmentModel } from './agent-assignment.model';
import { BookingPolicyModel } from './booking-policy.model';
import { BookingTimelessModel } from './booking-timeless.model';
import { CargoLoadabilityModel } from './cargo-loadability.model';
import { ComplianceSelectionModel } from './compliance-selection.model';
import { PurchaseOrderVerificationSettingModel } from './po-verification-setting.model';
import { ProductVerificationSettingModel } from './product-verification-setting.model';
import { ShippingComplianceModel } from './shipping-compliance.model';

export class BuyerComplianceModel {
    id: number;
    organizationId: number;
    organizationName: string;
    principleCode: string;
    name: string;
    effectiveDate: string;
    enforceCommercialInvoiceFormat: boolean;
    enforcePackingListFormat: boolean;
    isAssignedAgent: boolean;
    allowToBookIn: POType;
    shortShipTolerancePercentage: number | null;
    overshipTolerancePercentage: number | null;
    status: BuyerComplianceStatus;
    statusName: string;
    stage: BuyerComplianceStage;
    stageName: string;
    purchaseOrderTransmissionMethods: PurchaseOrderTransmissionMethod;
    purchaseOrderTransmissionFrequency: PurchaseOrderTransmissionFrequency;
    approvalAlertFrequency: ApprovalAlertFrequency;
    approvalDuration: ApprovalDuration;
    purchaseOrderTransmissionNotes: string;
    purchaseOrderVerificationSetting: PurchaseOrderVerificationSettingModel;
    productVerificationSetting: ProductVerificationSettingModel;
    bookingTimeless: BookingTimelessModel;
    cargoLoadabilities: CargoLoadabilityModel[];
    agentAssignments: AgentAssignmentModel[];
    shippingCompliance: ShippingComplianceModel;
    complianceSelection: ComplianceSelectionModel;
    bookingPolicies: BookingPolicyModel[];
    bookingPolicyAction: ValidationResultPolicy;
    bookingApproverSetting: ApproverSetting;
    bookingApproverUser: string;
    hsCodeShipFromCountryIds: [];
    hsCodeShipFromIds: number[];
    hsCodeShipToCountryIds: number[];
    hsCodeShipToIds: number[];
}