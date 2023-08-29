import {
  Component,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges
} from '@angular/core';
import { ControlContainer, NgForm } from '@angular/forms';
import { faInfoCircle } from '@fortawesome/free-solid-svg-icons';
import { DateHelper, POFulfillmentOrderStatus, StringHelper } from 'src/app/core';
import { EnumHelper } from 'src/app/core/helpers/enum.helper';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';

@Component({
  selector: 'app-warehouse-fulfillment-cargo-receive',
  templateUrl: './warehouse-fulfillment-cargo-receive.component.html',
  styleUrls: ['./warehouse-fulfillment-cargo-receive.component.scss'],
  viewProviders: [{ provide: ControlContainer, useExisting: NgForm }]
})
export class WarehouseFulfillmentCargoReceiveComponent implements OnChanges, OnInit {
  @Input() tabPrefix: string;
  @Input() validationRules: any;
  @Input() formErrors: {};
  @Input() orders: any[];

  defaultValue = DefaultValue2Hyphens;
  stringHelper = StringHelper;
  readonly POFulfillmentOrderStatus = POFulfillmentOrderStatus;
  maxDate: Date = new Date();
  
  faInfoCircle = faInfoCircle;

  receivedDate: Date = new Date();
  isReceivedAll: boolean = true;

  /** Model data for customer PO popup. It is details in the right */
  customerPOModel: any;
  /** Flag to open/close customer PO popup. */
  customerDetailPopupOpened = false;
  customerDetailModel: any;

  constructor() {

  }

  ngOnInit(): void {
    this.validationRules[`${this.tabPrefix}receivedDate`] = {
      'required': 'label.receivedDates'
    }
    
    this.onReceivedDateChanged(this.receivedDate);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.orders.currentValue) {
      // set default value
      this.orders.forEach((el, i) => {
        el.received = true;
        el.receivedQty = el.bookedPackage;
        this.validationRules[`${this.tabPrefix}receivedQty_${i}`] = {
          'required': 'label.receivedQty'
        }
      });
    }
  }

  onReceivedChanged(dataItem): void {
    if (!dataItem.received) {
      dataItem.receivedQty = null;
      dataItem.receivedDate = null;
    } else {
      dataItem.receivedDate = this.receivedDate.toUTCString();
      dataItem.receivedQty = dataItem.bookedPackage;
    }
    this.isReceivedAll = this.orders.findIndex(x => !x.received) === -1;
  }

  onOpenReceivedDate()  {
    this.maxDate = new Date();
  }

  onReceivedAllChanged(val: boolean) {
    this.orders.map(x => {
      x.received = val;
      if (!x.received) {
        x.receivedQty = null;
        x.receivedDate = null;
      } else {
        if (!x.receivedQty) {
          x.receivedQty = x.bookedPackage;
        }
        x.receivedDate = this.receivedDate.toUTCString();
      }
    });
  }

  onReceivedDateChanged(value) {
    // update received date for all received item.
    this.orders.forEach(el => {
      if (el.received) {
        el.receivedDate = value.toUTCString();
      } else {
        el.receivedDate = null;
      }
    })
  }

  openCustomerDetailPopup(dataItem) {
    this.customerDetailModel = dataItem;
    this.customerDetailPopupOpened = true;
  }

  customerDetailPopupClosedHandler() {
    this.customerDetailPopupOpened = false;
  }

  labelFromEnum(arr, value) {
    return EnumHelper.convertToLabel(arr, value);
  }

}
