import { ModelBase } from '../model-base.model';

export interface ShipmentContactModel extends ModelBase {
    id: number;
    shipmentId: number;
    organizationId: number;
    organizationRole: string;
    companyName: string;
    address: string;
    contactName: string;
    contactNumber: string;
    contactEmail: string;
}
