import {
  Component,
  EventEmitter,
  Input,
  OnDestroy,
  OnInit,
  Output
} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  faCheck,
  faEllipsisV,
  faInfoCircle,
  faPencilAlt,
  faPlus,
  faTrashAlt
} from '@fortawesome/free-solid-svg-icons';
import { TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import {
  DATE_HOUR_FORMAT_12,
  DropDowns,
  POFulfillmentOrderStatus,
  POFulfillmentStageType,
  Roles, StringHelper, UserContextService
} from 'src/app/core';
import { EnumHelper } from 'src/app/core/helpers/enum.helper';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { WarehouseFulfillmentFormService } from '../warehouse-fulfillment-form/warehouse-fulfillment-form.service';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';

@Component({
  selector: 'app-warehouse-fulfillment-customer-po',
  templateUrl: './warehouse-fulfillment-customer-po.component.html',
  styleUrls: ['./warehouse-fulfillment-customer-po.component.scss']
})
export class WarehouseFulfillmentCustomerPoComponent implements OnInit, OnDestroy {
  @Input() poffModel: any;
  @Input() isViewMode: boolean;

  @Output() close: EventEmitter<any> = new EventEmitter();
  // Store all subscriptions, then should un-subscribe at the end
  private _subscriptions: Array<Subscription> = [];
  POFulfillmentOrderStatus = POFulfillmentOrderStatus;
  POFulfillmentStageType = POFulfillmentStageType;
  POFulfillmentOrderStatusOptions = DropDowns.POFulfillmentOrderStatus;
  currentUser: any;

  defaultValue = DefaultValue2Hyphens;
  faPlus = faPlus;
  faEllipsisV = faEllipsisV;
  faPencilAlt = faPencilAlt;
  faTrashAlt = faTrashAlt;
  faInfoCircle = faInfoCircle;
  faCheck = faCheck;
  stringHelper = StringHelper;
  customerFormOpened = false;

  /** Model data for customer PO popup. It is details in the right */
  customerPOModel: any;
  /** Flag to open/close customer PO popup. */
  customerDetailPopupOpened = false;
  customerDetailModel: any;
  gridMissingFields = [];
  DATE_HOUR_FORMAT_12 = DATE_HOUR_FORMAT_12;

  constructor(
    public service: WarehouseFulfillmentFormService,
    activatedRoute: ActivatedRoute,
    public translateService: TranslateService,
    private notification: NotificationPopup,
    private userContext: UserContextService) {
  }

  ngOnInit(): void {
    // Apply business rules for each specific system user roles
    const currentUser = this.userContext.currentUser;
    this.currentUser = currentUser;
    if (currentUser && !currentUser.isInternal && (currentUser.role.id === Roles.Agent ||
      currentUser.role.id === Roles.Principal)) {
      this.poffModel.orders.forEach(o => {
        o.isShowBookedPackageWarning = this.isShowBookedPackageWarning(o);
      });
    }
  }

  isShowBookedPackageWarning(lineItem: any): boolean {
    return lineItem.bookedPackage && lineItem.outerQuantity && (lineItem.fulfillmentUnitQty % lineItem.outerQuantity > 0);
  }

  isItemExistsInPoffOrder(poId: number, lineId: number) {
    const bookingOrders = !this.poffModel.orders ? [] : this.poffModel.orders;
    for (let index = 0; index < bookingOrders.length; index++) {
      if (bookingOrders[index].purchaseOrderId === poId && bookingOrders[index].poLineItemId === lineId) {
        return true;
      }
    }
    return false;
  }

  public get isRequirePackageUOM() {
    return true;
  }

  customerFormClosedHandler() {
    this.customerFormOpened = false;
    this.close.emit();
  }

  customerDetailPopupClosedHandler() {
    this.customerDetailPopupOpened = false;
  }

  openCustomerDetailPopup(dataItem) {
    this.customerDetailModel = dataItem;
    this.customerDetailPopupOpened = true;
  }

  /** To enforce immutability for orders.
   * Call it every after changed data of this.poffModel.orders.
   * To work with pipe poFulfillmentCustomerOrder*/
  private _enforceImmutableOrders() {
    this.poffModel.orders = Object.assign([], this.poffModel.orders);
  }

  labelFromEnum(arr, value) {
    return EnumHelper.convertToLabel(arr, value);
  }

  get receivedDate() {
    const firstReceivedOrder = this.poffModel.orders?.find(x => x.poFulfillmentCargoReceiveItem?.inDate);
    let inDate = firstReceivedOrder?.poFulfillmentCargoReceiveItem?.inDate;
    if (!inDate) {
      return null;
    }
    var date = this.convertUTCDateToLocalDate(new Date(inDate));
    return date?.toString();
  }

  convertUTCDateToLocalDate(date) {
    if (!date) {
      return null;
    }

    var offset = date.getTimezoneOffset() / 60;
    var hours = date.getHours();

    date.setHours(hours - offset);
    return date;
  }

  _validateBookedPackage() {
    for (let i = 0; i < this.poffModel.orders.length; i++) {
      const order = this.poffModel.orders[i];
      if (!order.bookedPackage || order.bookedPackage <= 0) {
        if (!this.gridMissingFields.includes('Booked Package')) {
          this.gridMissingFields.push('Booked Package');
        }
        order.isValidRow = false;
      }
    }
  }

  /**Reset to mark all rows is a valid row */
  resetRowErrors() {
    this.poffModel.orders.forEach(o => {
      o.isValidRow = true;
    });
  }

  public rowCallback(args) {
    return {
      'error-row': args.dataItem.isValidRow === false,
    };
  }

  ngOnDestroy(): void {
    this._subscriptions.map(x => x.unsubscribe());
  }
}

