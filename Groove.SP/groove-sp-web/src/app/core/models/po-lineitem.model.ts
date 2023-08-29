import { StringHelper } from '../helpers';

/**Base model data for Purchase Order Line Item */
export class POLineItemModel {

    /**
     * Value as a key for searching. Template is {Purchase order number}[ - {Product code}]
     */
    public value: string;

    constructor(
        public purchaseOrderId: number,
        public purchaseOrderNumber: string,
        public poLineItemId?: number,
        public productCode?: string
    ) {
        this.value = `${this.purchaseOrderNumber}${StringHelper.isNullOrEmpty(this.poLineItemId) ? '' :  ' - ' + this.productCode}`;
    }
}
