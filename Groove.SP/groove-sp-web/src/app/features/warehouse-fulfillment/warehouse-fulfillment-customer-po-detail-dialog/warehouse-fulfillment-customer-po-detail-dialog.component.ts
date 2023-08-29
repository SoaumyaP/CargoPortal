import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges
} from '@angular/core';
import { faInfoCircle } from '@fortawesome/free-solid-svg-icons';
import { DropDowns, StringHelper } from 'src/app/core';
import { EnumHelper } from 'src/app/core/helpers/enum.helper';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
  selector: 'app-warehouse-fulfillment-customer-po-detail-dialog',
  templateUrl: './warehouse-fulfillment-customer-po-detail-dialog.component.html',
  styleUrls: ['./warehouse-fulfillment-customer-po-detail-dialog.component.scss']
})
export class WarehouseFulfillmentCustomerPoDetailDialogComponent implements OnChanges {
  @Input() public customerDetailPopupOpened: boolean = false;
  @Output() close: EventEmitter<any> = new EventEmitter();
  @Input() public model: any;

  faInfoCircle = faInfoCircle;
  defaultValue = DefaultValue2Hyphens;

  constructor(public _commonService: CommonService) { }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.model?.currentValue) {
      if (!StringHelper.isNullOrEmpty(this.model.countryCodeOfOrigin)) {
        this._commonService.getCountryDropdownByCode(this.model.countryCodeOfOrigin).subscribe(
          (data) => {
            if (data) {
              this.model.countryNameOfOrigin = data.label;
            }
          }
        );
      }
    }
  }

  onFormClosed() {
    this.customerDetailPopupOpened = false;
    this.close.emit();
  }

  get dimension(): string {
    if (!this.model) {
      return this.defaultValue;
    }
    if (StringHelper.isNullOrEmpty(this.model.length)
      && StringHelper.isNullOrEmpty(this.model.width)
      && StringHelper.isNullOrEmpty(this.model.height)) {
      return this.defaultValue;
    }
    return `${this.model.length || 0} x ${this.model.width || 0} x ${this.model.height || 0}`;
  }

  labelFromEnum(arr, value) {
    return EnumHelper.convertToLabel(arr, value);
  }

  returnValue(field) {
    return this.model[field] ? this.model[field] : this.defaultValue;
  }
}

