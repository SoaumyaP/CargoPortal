import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { DropDowns, ViewSettingModuleIdType } from 'src/app/core';
import { EnumHelper } from 'src/app/core/helpers/enum.helper';
import { faInfoCircle } from '@fortawesome/free-solid-svg-icons';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { ViewSettingModel } from 'src/app/core/models/viewSetting.model';

@Component({
    selector: 'app-po-fulfillment-customer-detail-popup',
    templateUrl: './po-fulfillment-customer-detail-popup.component.html',
    styleUrls: ['./po-fulfillment-customer-detail-popup.component.scss']
})
export class POFulfillmentCustomerDetailPopupComponent implements OnInit {
    @Input() public customerDetailPopupOpened: boolean = false;
    @Output() close: EventEmitter<any> = new EventEmitter();
    @Input() public model: any;
    @Input() public viewSettings: ViewSettingModel[];
    commodityOptions = DropDowns.CommodityString;
    faInfoCircle = faInfoCircle;
    readonly defaultValue = DefaultValue2Hyphens;
    formHelper = FormHelper;
    viewSettingModuleIdType = ViewSettingModuleIdType;
    
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
