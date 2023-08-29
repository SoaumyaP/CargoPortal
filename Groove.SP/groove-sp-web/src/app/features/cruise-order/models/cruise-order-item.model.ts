import { StringHelper } from 'src/app/core';
import { LocalDate } from 'src/app/core/models/local-date.model';
import { Model, ModelBase, DataType } from 'src/app/core/models/model-base.model';
import { CruiseOrderItemShipmentReferenceModel } from './cruise-order-item-shipment.model';

@Model
export class CruiseOrderItemModel extends ModelBase {
    @DataType(Number)
    id: number = 0;
    lineEstimatedDeliveryDate: string | null = undefined;
    firstReceivedDate: string | null = undefined;
    makerReferenceOfItemName2: string = undefined;
    requestLineShoreNotes: string = undefined;
    shipRequestLineNotes: string = undefined;
    quantityDelivered: number | null = undefined;
    requestLine: number = undefined;
    requestNumber: string = undefined;
    requestQuantity: number | null = undefined;
    uom: string | null = undefined;
    currencyCode: string | null = undefined;
    itemId: string = undefined;
    itemName: string = undefined;
    netUSUnitPrice: number | null = undefined;
    netUnitPrice: number | null = undefined;
    totalOrderPrice: number | null = undefined;
    orderQuantity: number | null = undefined;
    poLine: number | null = undefined;
    latestDialog: string | null = undefined;
    sub1: string = undefined;
    sub2: string = undefined;
    origin: string = undefined;
    approvedDate: string | null = undefined;
    quotedCostCurrency: string = undefined;
    readyDate: string | null = undefined;
    reqOnboardDate: string | null = undefined;
    commercialInvoice: boolean | null = undefined;
    contract: string = undefined;
    shipboardLoadingLocation: string = undefined;
    deliveryPort: string = undefined;
    destination: string = undefined;
    approvedBy: string = undefined;
    comments: string = undefined;
    priority: string = undefined;
    quotedCost: number | null = undefined;
    delayCause: string = undefined;
    deliveryTicket: string = undefined;
    destinationCountry: string = undefined;
    buyerName: string = undefined;
    itemUpdates: string | null = undefined;
    /** allow to delete only if value is available */
    originalItemId: number = undefined;
    /** allow to delete if internal user or current external user belongs to same organization */
    originalOrganizationId: number = undefined;
    orderId: number = undefined;
    shipmentId: number | null = undefined;
    shipment: CruiseOrderItemShipmentReferenceModel | null = undefined;
}

@Model
export class ReviseCruiseOrderItemModel extends ModelBase {
    id: number = 0;
    lineEstimatedDeliveryDate: string | null = undefined;
    firstReceivedDate: string | null = undefined;
    makerReferenceOfItemName2: string = undefined;
    requestLineShoreNotes: string = undefined;
    shipRequestLineNotes: string = undefined;
    quantityDelivered: number | null = undefined;
    requestLine: number = undefined;
    requestNumber: string = undefined;
    requestQuantity: number | null = undefined;
    uom: string = undefined;
    currencyCode: string = undefined;
    itemId: string = undefined;
    itemName: string = undefined;
    netUSUnitPrice: number | null = undefined;
    netUnitPrice: number | null = undefined;
    totalOrderPrice: number | null = undefined;
    orderQuantity: number | null = undefined;
    poLine: number | null = undefined;
    sub1: string = undefined;
    sub2: string = undefined;
    origin: string = undefined;
    @DataType(LocalDate)
    approvedDate: LocalDate | null = undefined;
    quotedCostCurrency: string = undefined;
    @DataType(LocalDate)
    readyDate: LocalDate | null = undefined;
    @DataType(LocalDate)
    reqOnboardDate: LocalDate | null = undefined;
    commercialInvoice: boolean | null = undefined;
    contract: string = undefined;
    shipboardLoadingLocation: string = undefined;
    deliveryPort: string = undefined;
    destination: string = undefined;
    approvedBy: string = undefined;
    comments: string = undefined;
    priority: string = undefined;
    quotedCost: number | null = undefined;
    delayCause: string = undefined;
    deliveryTicket: string = undefined;
    destinationCountry: string = undefined;
    buyerName: string = undefined;
    itemUpdates: string | null = undefined;
    orderId: number = undefined;

    shipmentId: number | null = undefined;
    shipmentNumber: string = undefined;

    selectedItems: Array<string> = [];
    selectedItemPOLines: Array<number> = [];

    public static mapToReviseModel(data: CruiseOrderItemModel): ReviseCruiseOrderItemModel {

        if (data) {
            const selectedItems = [];
            selectedItems.push(`${data.poLine}${StringHelper.isNullOrEmpty(data.itemId) ? '' :  ' - ' + data.itemId}${StringHelper.isNullOrEmpty(data.itemName) ? '' :  ' - ' + data.itemName}`);
            const result = new ReviseCruiseOrderItemModel();
            Object.keys(result).forEach(prop => {
                result[prop] = data[prop];
            });
            result.selectedItems = selectedItems;
            result.shipmentId = data.shipmentId;
            result.shipmentNumber = data.shipment?.shipmentNo;
            return result;
        }
        return new ReviseCruiseOrderItemModel();
    }
}
