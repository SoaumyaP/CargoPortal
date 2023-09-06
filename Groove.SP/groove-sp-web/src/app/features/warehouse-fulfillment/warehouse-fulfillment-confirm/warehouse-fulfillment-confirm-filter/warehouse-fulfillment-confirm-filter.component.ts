import { AfterViewInit, Component, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, NgForm, Validators } from '@angular/forms';
import { Subscription } from 'rxjs';
import { DateHelper, DATE_FORMAT, DropDownListItemModel, StringHelper } from 'src/app/core';
import { CommonService } from 'src/app/core/services/common.service';
import { faRedo, fas, faSearch } from '@fortawesome/free-solid-svg-icons';
import { ActivatedRoute } from '@angular/router';
import moment from 'moment';
import { TranslateService } from '@ngx-translate/core';
import { ComplianceFormService } from 'src/app/features/compliance/compliance-form/compliance-form.service';
import { WarehouseFulfillmentConfirmFormService } from '../warehouse-fulfillment-confirm-form/warehouse-fulfillment-confirm-form.service';
import { WarehouseFulfillmentConfirmQueryModel } from '../../models/warehouse-fulfillment-confirm.model';
import { FormHelper } from 'src/app/core/helpers/form.helper';

@Component({
  selector: 'app-warehouse-fulfillment-confirm-filter',
  templateUrl: './warehouse-fulfillment-confirm-filter.component.html',
  styleUrls: ['./warehouse-fulfillment-confirm-filter.component.scss']
})
export class WarehouseFulfillmentConfirmFilterComponent implements OnInit, AfterViewInit {
  @Input('searching') isSearchingWarehouseBookingConfirm: boolean;
  @Output() search: EventEmitter<any> = new EventEmitter();
  @Output() reset: EventEmitter<any> = new EventEmitter();

  faRedo = faRedo;
  faSearch = faSearch;
  DATE_FORMAT = DATE_FORMAT;
  customerOptions: DropDownListItemModel<number>[];
  enableSupplierControl: boolean;
  defaultDropDownItem: DropDownListItemModel<number> = new DropDownListItemModel<number>(this._translateService.instant('label.select'), null);
  private subscriptions: Subscription = new Subscription();
  filterForm: FormGroup;
  isNavigateFromBookingPage: boolean;

  constructor(
    private _activatedroute: ActivatedRoute,
    private _translateService: TranslateService,
    private complianceFormService: ComplianceFormService,
    private warehouseFulfillmentConfirmFormService: WarehouseFulfillmentConfirmFormService,
    private _commonService: CommonService) {
  }

  ngOnInit() {
    this.initForm();

    const sub = this.complianceFormService.getWarehouseCustomerDropdown().subscribe(
      data => {
        this.customerOptions = data;
        this.snapshotWarehouseBookingQueryParams();
      }
    )
    this.subscriptions.add(sub);
  }

  ngAfterViewInit(): void {
    this.subcribleFromControlsChanged();
  }

  initForm() {
    this.filterForm = new FormGroup({
      selectedCustomerId: new FormControl(null, Validators.required),
      selectedSupplier: new FormControl({ value: null, disabled: true }),
      bookingNoFrom: new FormControl(),
      bookingNoTo: new FormControl(),
      expectedHubArrivalDateFrom: new FormControl(),
      expectedHubArrivalDateTo: new FormControl(),
    })
  }

  subcribleFromControlsChanged() {
    this.filterForm.controls['selectedCustomerId'].valueChanges.subscribe(
      value => {
        if (!value || value === 0) {
          this.filterForm.controls['selectedSupplier'].disable();
          this.filterForm.controls['selectedSupplier'].setValue(null);
        } else {
          this.filterForm.controls['selectedSupplier'].enable();
          this.populateSupplier();
        }
      }
    )

    this.filterForm.controls['bookingNoFrom'].valueChanges.subscribe(
      value => {
        this.validateBookingNoRange();
      }
    )

    this.filterForm.controls['bookingNoTo'].valueChanges.subscribe(
      value => {
        this.validateBookingNoRange();
      }
    )

    this.filterForm.controls['expectedHubArrivalDateFrom'].valueChanges.subscribe(
      value => {
        this.validateExpectedHubArrivalDateToRange();
      }
    )

    this.filterForm.controls['expectedHubArrivalDateTo'].valueChanges.subscribe(
      value => {
        this.validateExpectedHubArrivalDateToRange();
      }
    )
  }

  snapshotWarehouseBookingQueryParams() {
    const sub = this._activatedroute.paramMap.subscribe(
      params => {
        //Only populate data to filtering controls if navigate from wahouse booking detail page
        if (Object.keys(params['params']).map(params => params).length > 0) {
          this.isNavigateFromBookingPage = true;
          this.getFormControl('bookingNoFrom').setValue(params.get('bookingNoFrom'));
          this.getFormControl('bookingNoTo').setValue(params.get('bookingNoTo'));
          this.getFormControl('expectedHubArrivalDateFrom').setValue(moment(params.get('expectedHubArrivalDateFrom')).toDate());
          this.getFormControl('expectedHubArrivalDateTo').setValue(moment(params.get('expectedHubArrivalDateTo')).toDate());
          this.getFormControl('expectedHubArrivalDateTo').markAsTouched();
          this.getFormControl('selectedCustomerId').setValue(+params.get('customerId'));
          this.getFormControl('selectedSupplier').setValue(params.get('supplier'));
        }
      }
    )
    this.subscriptions.add(sub);
  }

  populateSupplier() {
    if (this.isNavigateFromBookingPage) {
      this.onSearchWarehouseBookings();
      this.isNavigateFromBookingPage = false;
    }
  }

  validateBookingNoRange() {
    let bookingNoFrom: any = this.filterForm.controls['bookingNoFrom'].value;
    let bookingNoTo: any = this.filterForm.controls['bookingNoTo'].value;

    if (StringHelper.isNullOrEmpty(bookingNoTo)) {
      return;
    }

    // let to compare number
    if (!isNaN(bookingNoFrom) && !isNaN(bookingNoTo)) {
      bookingNoFrom = Number.parseInt(bookingNoFrom);
      bookingNoTo = Number.parseInt(bookingNoTo);
    }

    if (StringHelper.isNullOrEmpty(bookingNoFrom) || bookingNoFrom > bookingNoTo) {
      this.filterForm.controls['bookingNoTo'].markAsTouched();
      this.filterForm.controls['bookingNoTo'].setErrors({ 'invalidRangeInput': true });
    } else {
      this.filterForm.controls['bookingNoTo'].setErrors(null);
    }
  }

  validateExpectedHubArrivalDateToRange() {
    let expectedHubArrivalDateFrom = this.filterForm.controls['expectedHubArrivalDateFrom'].value;
    let expectedHubArrivalDateTo = this.filterForm.controls['expectedHubArrivalDateTo'].value;

    if (StringHelper.isNullOrEmpty(expectedHubArrivalDateTo)) {
      return;
    }

    if (StringHelper.isNullOrEmpty(expectedHubArrivalDateFrom)
      || new Date(expectedHubArrivalDateFrom).setHours(0, 0, 0, 0) > new Date(expectedHubArrivalDateTo).setHours(0, 0, 0, 0)) {
      this.filterForm.controls['expectedHubArrivalDateTo'].setErrors({ 'invalidRangeInput': true });
    } else {
      this.filterForm.controls['expectedHubArrivalDateTo'].setErrors(null);
    }
  }

  onSearchWarehouseBookings() {
    FormHelper.ValidateAllFields(this.filterForm);
    if (this.isSearchingWarehouseBookingConfirm || this.filterForm.invalid) {
      return;
    }

    this.isSearchingWarehouseBookingConfirm = true;
    const fModel = { ...this.filterForm.value };
    let model = new WarehouseFulfillmentConfirmQueryModel();
    model.selectedCustomerId = fModel.selectedCustomerId;
    model.bookingNoFrom = fModel.bookingNoFrom;
    model.bookingNoTo = fModel.bookingNoTo;

    if (fModel.selectedSupplier && fModel.selectedSupplier.trim() !== '') {
      model.selectedSupplier = fModel.selectedSupplier
    }

    if (fModel.expectedHubArrivalDateFrom) {
      model.expectedHubArrivalDateFrom = moment(fModel.expectedHubArrivalDateFrom).format('YYYY-MM-DD');
    }

    if (fModel.expectedHubArrivalDateTo) {
      model.expectedHubArrivalDateTo = moment(fModel.expectedHubArrivalDateTo).format('YYYY-MM-DD');
    }

    this.warehouseFulfillmentConfirmFormService.searchWarehouseBookingConfirm(model.buildToQueryParams).subscribe(
      data => {
        this.isSearchingWarehouseBookingConfirm = false;
        this.search.emit(data);
      },
      err => {
        this.isSearchingWarehouseBookingConfirm = false;
      }
    )
  }

  resetForm() {
    this.filterForm.reset();
    this.reset.emit();
  }

  getFormControl(controlName: string): AbstractControl {
    return this.filterForm?.controls[controlName];
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
