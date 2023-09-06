import { Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges, ViewChild } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { faArrowCircleDown } from '@fortawesome/free-solid-svg-icons';
import { GridDataResult } from '@progress/kendo-angular-grid';
import { orderBy, SortDescriptor } from '@progress/kendo-data-query';
import { Subscription } from 'rxjs';
import { DateHelper, DATE_FORMAT, DropDowns } from 'src/app/core';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { WarehouseFulfillmentConfirmFormService } from '../warehouse-fulfillment-confirm-form/warehouse-fulfillment-confirm-form.service';

@Component({
  selector: 'app-warehouse-fulfillment-confirm-list',
  templateUrl: './warehouse-fulfillment-confirm-list.component.html',
  styleUrls: ['./warehouse-fulfillment-confirm-list.component.scss']
})
export class WarehouseFulfillmentConfirmListComponent implements OnChanges, OnInit, OnDestroy {
  @ViewChild('confirmForm', { static: false }) confirmForm: NgForm;
  @Input() warehouseBookings: any = [];

  applyForm: FormGroup;
  DATE_FORMAT = DATE_FORMAT;
  gridView: GridDataResult;
  gridSort: SortDescriptor[] = [
    {
      field: "",
      dir: "asc",
    },
  ];
  faArrowCircleDown = faArrowCircleDown;
  timeDropDown = DropDowns.TimeType;
  timeApplyDropDown = [
    { label: 'label.time', value: null },
    { label: 'label.am', value: 'AM' },
    { label: 'label.pm', value: 'PM' },
  ]
  isSavingWarehouseBookingsConfirm: boolean;
  subscriptions: Subscription = new Subscription();

  constructor(
    private warehouseService: WarehouseFulfillmentConfirmFormService,
    private notification: NotificationPopup,
    private router: Router,
    private _gaService: GoogleAnalyticsService
  ) { }


  ngOnChanges(changes: SimpleChanges): void {
    if (changes.warehouseBookings?.firstChange === false) {
      this.confirmForm.reset();
      this.loadWarehouseBookings();
    }
  }

  ngOnInit() {
    this.initForm();
    this.loadWarehouseBookings();
  }

  initForm() {
    this.applyForm = new FormGroup({
      confirmedHubArrivalDate: new FormControl(),
      time: new FormControl(),
      loadingBay: new FormControl(),
    })
  }

  loadWarehouseBookings() {
    this.gridView = {
      data: orderBy(this.warehouseBookings, this.gridSort),
      total: this.warehouseBookings.length
    }
  }

  gridSortChange(sort: SortDescriptor[]): void {
    this.gridSort = sort;
    this.loadWarehouseBookings();
  }

  applyWarehouseBookingConfirm() {
    for (let k in this.applyForm.value) {
      if (this.applyForm.value[k]) {
        for (let i of this.warehouseBookings) {
          i[k] = this.applyForm.value[k];
        }
      }
    }
    this.applyForm.reset();
  }

  isInvalidControl(controlName) {
    const control = this.getFormControl(controlName);
    if (control?.status === 'INVALID' && (control?.touched || control?.dirty)) {
      return true;
    }
    return false;
  }

  onSaveWarehouseBookingConfirm() {
    FormHelper.ValidateAllFields(this.confirmForm);
    if (this.warehouseBookings.length === 0 || this.isSavingWarehouseBookingsConfirm || this.confirmForm.invalid) {
      return;      
    }

    this.isSavingWarehouseBookingsConfirm = true;
    const data = this.warehouseBookings.map(c => DateHelper.formatDate(c));
    this.warehouseService.confirmWarehouseBookings(data).subscribe(
      res => {
        this.isSavingWarehouseBookingsConfirm = false;
        this.notification.showSuccessPopup('save.sucessNotification', 'label.poFulfillment');
        this._gaService.emitAction('Confirm', GAEventCategory.WarehouseBooking);
        this.router.navigate(['/po-fulfillments']);
      },
      err => {
        this.isSavingWarehouseBookingsConfirm = false;
        this.notification.showErrorPopup('save.failureNotification', 'label.poFulfillment');
      }
    )
  }

  onCancelWarehouseBookingConfirm() {
    this.router.navigate(['/po-fulfillments']);
  }

  getFormControl(controlName: string): AbstractControl {
    return this.confirmForm?.form?.controls[controlName];
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
