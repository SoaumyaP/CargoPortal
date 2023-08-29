import { Model, ModelBase, DataType } from 'src/app/core/models/model-base.model';
import { CruiseOrderItemModel } from './cruise-order-item.model';

@Model
export class CruiseOrderModel extends ModelBase {
    id: number = 0;
    actualDeliveryDate: string | null = undefined;
    actualShipDate: string | null = undefined;
    approvalStatus: string = undefined;
    approvedDate: string | null = undefined;
    approver: string = undefined;
    budgetAccount: string = undefined;
    budgetId: string = undefined;
    budgetPeriod: number | null = undefined;
    budgetYear: number | null = undefined;
    certificateId: string = undefined;
    certificateNumber: string = undefined;
    creationUser: string = undefined;
    delivered: string = undefined;
    deliveryMeans: string = undefined;
    department: string = undefined;
    firstReceivingPoint: string = undefined;
    invoiced: number | null = undefined;
    maintenanceObject: string = undefined;
    maker: string = undefined;
    poCause: string = undefined;
    poId: string = undefined;
    poType: string = undefined;
    poPriority: string = undefined;
    requestApprovedDate: string | null = undefined;
    requestDate: string | null = undefined;
    requestPriority: string = undefined;
    requestType: string = undefined;
    requestType2: string = undefined;
    requestType3: string = undefined;
    requestor: string = undefined;
    ship: string = undefined;
    withWO: string = undefined;
    estimatedDeliveryDate: string | null = undefined;
    poNumber: string = undefined;
    poStatus: string | null = undefined;
    poSubject: string = undefined;
    poDate: string | null = undefined;
    statusName: string = undefined;
    customer: string = undefined;
    supplier: string = undefined;
    consignee: string = undefined;
    contacts: Array<any> = undefined;
    @DataType(CruiseOrderItemModel)
    items: Array<CruiseOrderItemModel> = [];
}
