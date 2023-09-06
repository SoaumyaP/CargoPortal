import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { DropDowns, StringHelper, ViewSettingModuleIdType } from 'src/app/core';
import { EnumHelper } from 'src/app/core/helpers/enum.helper';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';
import { ViewSettingModel } from 'src/app/core/models/viewSetting.model';

@Component({
    selector: 'app-product-detail-popup',
    templateUrl: './product-detail-popup.component.html',
    styleUrls: ['./product-detail-popup.component.scss']
})
export class ProductDetailPopupComponent implements OnInit {
    @Input() public productPopupOpened: boolean = false;
    @Input() public model: any;
    @Input() public visibleColumns: ViewSettingModel[];
    @Output() close: EventEmitter<any> = new EventEmitter();
    commodityOptions = DropDowns.CommodityString;
    readonly defaultValue = DefaultValue2Hyphens;
    ViewSettingModuleIdType = ViewSettingModuleIdType;
    
    constructor() {
    }

    ngOnInit() {
    }

    onFormClosed() {
        this.productPopupOpened = false;
        this.close.emit();
    }

    labelFromEnum(arr, value) {
        return EnumHelper.convertToLabel(arr, value);
    }

    returnValue(field) {
        return this.model[field] ? this.model[field] : this.defaultValue;
    }

    isHiddenColumn(moduleId: ViewSettingModuleIdType, fieldId: string): boolean {
        if (this.visibleColumns === null) {
            return false;
        }

        return !this.visibleColumns.some(c => StringHelper.caseIgnoredCompare(c.field, fieldId));
    }
}
