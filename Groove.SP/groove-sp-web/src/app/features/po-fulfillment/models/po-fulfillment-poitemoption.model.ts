import { StringHelper } from 'src/app/core';
import { POLineItemModel } from 'src/app/core/models/po-lineitem.model';

/** Data model for PO Item auto-complete on Booking */
export class POFulfillmentPOItemOptionModel {
    public purchaseOrderId: number;
    public purchaseOrderNumber: string;
    public poLineItemId?: number;
    public productCode?: string;
    public value: string;
    /** Define it should be disable, not able to select */
    public isDisabled: boolean = false;
    /**In case it is removed, not existed in current source */
    public isOrphan: boolean = false;
    /** Define it is purchase order as parent level */
    public get isParentPO(): boolean {
        return StringHelper.isNullOrEmpty(this.poLineItemId);
    }

    /** Value which is shown in list of options */
    public get optionText(): string {
        if (this.isParentPO) {
            return this.purchaseOrderNumber;
        } else if (this.isOrphan) {
            return `${this.purchaseOrderNumber} - ${this.productCode}`;
        } else {
            return `${this.productCode}`;
        }
    }

    /**Convert from PO line item to booking */
    public mapFrom(data: POLineItemModel) {
        this.purchaseOrderId = data.purchaseOrderId;
        this.purchaseOrderNumber = data.purchaseOrderNumber;
        this.poLineItemId = data.poLineItemId;
        this.productCode = data.productCode;
        this.value = data.value;
    }
}
