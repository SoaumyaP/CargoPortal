import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { DATE_FORMAT, DropDowns, ViewSettingModuleIdType } from 'src/app/core';
import { EnumHelper } from 'src/app/core/helpers/enum.helper';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { ViewSettingModel } from 'src/app/core/models/viewSetting.model';
import { POFulfillmentFormService } from '../../po-fulfillment/po-fulfillment-form/po-fulfillment-form.service';
import { BulkFulfillmentFormService } from '../bulk-fulfillment-form/bulk-fulfillment-form.service';

@Component({
  selector: 'app-bulk-fulfillment-planned-schedule',
  templateUrl: './bulk-fulfillment-planned-schedule.component.html',
  styleUrls: ['./bulk-fulfillment-planned-schedule.component.scss']
})
export class BulkFulfillmentPlannedScheduleComponent implements OnChanges, OnInit {
  @Input() bulkBookingId: number;
  @Input() bulkBooking: any;
  @Input() isViewMode: boolean;
  @Input() viewSettings: ViewSettingModel[];

  shipment: any = {};
  itineraries: any[];
  modeOfTransportOptions = DropDowns.ModeOfTransportStringType;
  DATE_FORMAT = DATE_FORMAT;
  formHelper = FormHelper;
  viewSettingModuleIdType = ViewSettingModuleIdType;

  constructor(
    private bulkFulfillmentFormService: BulkFulfillmentFormService
  ) { }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.bulkBookingId?.firstChange === false) {
      this.bulkFulfillmentFormService.getPlannedSchedule(this.bulkBookingId).subscribe(
        data => {
          this.shipment = data.shipments[0];
          this.itineraries = data.itineraries;
        }
      )
    }
  }

  ngOnInit() {
  }

  labelFromEnum(arr, value) {
    return EnumHelper.convertToLabel(arr, value);
  }
}
