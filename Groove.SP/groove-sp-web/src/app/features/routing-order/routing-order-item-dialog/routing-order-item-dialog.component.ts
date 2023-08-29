import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { PackageUOMType, StringHelper, UnitUOMType } from 'src/app/core';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
  selector: 'app-routing-order-item-dialog',
  templateUrl: './routing-order-item-dialog.component.html',
  styleUrls: ['./routing-order-item-dialog.component.scss']
})
export class RoutingOrderItemDialogComponent implements OnInit, OnChanges {
  @Input() public itemDetailDialogOpened: boolean = false;
  @Output() close: EventEmitter<any> = new EventEmitter();
  @Input() public model: any;

  defaultValue = DefaultValue2Hyphens;

  constructor(private commonService: CommonService) { }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.model?.currentValue) {
      if (!StringHelper.isNullOrEmpty(this.model.countryCodeOfOrigin)) {
        this.commonService.getCountryDropdownByCode(this.model.countryCodeOfOrigin).subscribe(
          (data) => {
            if (data) {
              this.model.countryNameOfOrigin = data.label;
            }
          }
        );
      }
    }
  }

  ngOnInit() { }

  get unitUOM() {
    if (this.model.unitUOM) {
      return UnitUOMType[this.model.unitUOM].toString();
    }
    return this.defaultValue;
  }

  get packageUOM() {
    if (this.model.packageUOM) {
      return PackageUOMType[this.model.packageUOM].toString();
    }
    return this.defaultValue;
  }

  onFormClosed() {
    this.itemDetailDialogOpened = false;
    this.close.emit();
  }
}
