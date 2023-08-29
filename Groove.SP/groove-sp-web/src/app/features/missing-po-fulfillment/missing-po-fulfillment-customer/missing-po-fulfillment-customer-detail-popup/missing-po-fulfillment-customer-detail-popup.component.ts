import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { DropDowns } from 'src/app/core';
import { EnumHelper } from 'src/app/core/helpers/enum.helper';
import { faInfoCircle } from '@fortawesome/free-solid-svg-icons';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';

@Component({
    selector: 'app-missing-po-fulfillment-customer-detail-popup',
    templateUrl: './missing-po-fulfillment-customer-detail-popup.component.html',
    styleUrls: ['./missing-po-fulfillment-customer-detail-popup.component.scss']
})
export class MissingPOFulfillmentCustomerDetailPopupComponent implements OnInit {
    @Input() public customerDetailPopupOpened: boolean = false;
    @Output() close: EventEmitter<any> = new EventEmitter();
    @Input() public model: any;
    commodityOptions = DropDowns.CommodityString;
    faInfoCircle = faInfoCircle;
    readonly defaultValue = DefaultValue2Hyphens;

    constructor() {
    }

    ngOnInit() {
    }

    onFormClosed() {
        this.customerDetailPopupOpened = false;
        this.close.emit();
    }

    labelFromEnum(arr, value) {
        return EnumHelper.convertToLabel(arr, value);
    }

    returnValue(field) {
        return this.model[field] ? this.model[field] : this.defaultValue;
    }
}
