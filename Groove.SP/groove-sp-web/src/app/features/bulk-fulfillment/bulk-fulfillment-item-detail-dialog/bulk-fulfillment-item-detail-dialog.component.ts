import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { StringHelper, ViewSettingModuleIdType } from 'src/app/core';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';
import { ViewSettingModel } from 'src/app/core/models/viewSetting.model';
import { CommonService } from 'src/app/core/services/common.service';

@Component({
  selector: 'app-bulk-fulfillment-item-detail-dialog',
  templateUrl: './bulk-fulfillment-item-detail-dialog.component.html',
  styleUrls: ['./bulk-fulfillment-item-detail-dialog.component.scss']
})
export class BulkFulfillmentItemDetailDialogComponent implements OnInit, OnChanges {
  @Input() public itemDetailDialogOpened: boolean = false;
  @Output() close: EventEmitter<any> = new EventEmitter();
  @Input() public model: any;
  @Input() isViewMode: boolean;
  @Input() viewSettings: ViewSettingModel[];
  viewSettingModuleIdType = ViewSettingModuleIdType;
  formHelper = FormHelper;

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

  onFormClosed() {
    this.itemDetailDialogOpened = false;
    this.close.emit();
  }
}
