import { ModelBase } from '../model-base.model';

export interface ActivityModel extends ModelBase {
    id: number;
    activityCode: string;
    shipmentId: number | null;
    containerId: number | null;
    consignmentId: number | null;
    purchaseOrderId: number | null;
    poFulfillmentId: number | null;
    cruiseOrderId: number | null;
    activityType: string;
    activityDescription: string;
    activityDate: string;
    location: string;
    remark: string;
    resolved: boolean | null;
    resolution: string;
    resolutionDate: string | null;
}
