import { StringHelper } from '../helpers';

/** Data model for Cruise Order Item auto-complete */
export class CruiseOrderLineItemOptionModel {

    /**
     * Value as a key for searching. Template is {Purchase order number}[ - {Product code}]
     */
    public optionText: string;

    constructor(
        public id: number,
        public poLine: number,
        public itemId?: string,
        public itemName?: string
    ) {
        this.optionText = `${this.poLine}${StringHelper.isNullOrEmpty(this.itemId) ? '' :  ' - ' + this.itemId}${StringHelper.isNullOrEmpty(this.itemName) ? '' :  ' - ' + this.itemName}`;
    }
}