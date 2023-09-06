import { Component, Input, OnInit } from '@angular/core';
import { DATE_FORMAT, StringHelper, ViewSettingModuleIdType } from 'src/app/core';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { FormHelper } from 'src/app/core/helpers/form.helper';

@Component({
  selector: 'app-po-fulfillment-shipment',
  templateUrl: './po-fulfillment-shipment.component.html',
  styleUrls: ['./po-fulfillment-shipment.component.scss']
})
export class PoFulfillmentShipmentComponent implements OnInit {
  @Input() currentUser;
  @Input('data')
  shipmentList: any[];
  @Input()
  isViewMode: boolean;
  @Input()
  viewSettings: any[];

  viewSettingModuleIdType = ViewSettingModuleIdType;

  isCanClickShipmentNo: boolean;
  
  DATE_FORMAT = DATE_FORMAT;

  constructor() { }

  ngOnInit() {
    this.isCanClickShipmentNo = this.currentUser?.permissions?.some(c => c.name === AppPermissions.Shipment_Detail);
  }

  isVisibleField(moduleId: ViewSettingModuleIdType, fieldId: string): boolean {
      if (!this.isViewMode) { // apply for view mode only
          return true;
      }

      return !FormHelper.isHiddenColumn(this.viewSettings, moduleId, fieldId);
  }

}
