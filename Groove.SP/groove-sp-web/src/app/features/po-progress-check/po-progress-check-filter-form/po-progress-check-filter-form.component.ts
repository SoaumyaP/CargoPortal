import { Component, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { NgForm, Validators } from '@angular/forms';
import { Subscription } from 'rxjs';
import { DATE_FORMAT, DropDownListItemModel, Roles, StringHelper } from 'src/app/core';
import { CommonService } from 'src/app/core/services/common.service';
import { POProgressCheckFilterFormService } from './po-progress-check-filter-form.service';
import { POProgressCheckFilterModel } from '../po-progress-check.model';
import { faRedo, faSearch } from '@fortawesome/free-solid-svg-icons';
import { ActivatedRoute } from '@angular/router';
import moment from 'moment';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-po-progress-check-filter-form',
  templateUrl: './po-progress-check-filter-form.component.html',
  styleUrls: ['./po-progress-check-filter-form.component.scss']
})
export class POProgressCheckFilterFormComponent implements OnInit, OnDestroy {
  @Input('searching') isInSearchingProgress: boolean;
  @Output() search: EventEmitter<POProgressCheckFilterModel> = new EventEmitter();
  @Output() searchFromEmail: EventEmitter<any> = new EventEmitter();

  @ViewChild('mainForm', { static: false }) mainForm: NgForm;

  faRedo = faRedo;
  faSearch = faSearch;
  DATE_FORMAT = DATE_FORMAT;
  model: POProgressCheckFilterModel = new POProgressCheckFilterModel();
  customerOptions: DropDownListItemModel<number>[];
  supplierOptions: DropDownListItemModel<number>[];
  filteredSupplierOptions: DropDownListItemModel<number>[];
  enableSupplierControl: boolean;
  currentUser: any;
  roles = Roles;

  private _subscriptions: Array<Subscription> = [];
  public defaultDropDownItem: DropDownListItemModel<number> = new DropDownListItemModel<number>
    (
      this._translateService.instant('label.select'),
      null
    );

  constructor(
    private _activatedroute: ActivatedRoute,
    private _translateService: TranslateService,
    private _service: POProgressCheckFilterFormService,
    private _commonService: CommonService) { }

  ngOnInit() {
    const sub = this._service.getCustomerDataSource(this._service.currentUser.role.id, this._service.currentUser.affiliates).subscribe(
      data => {
        this.customerOptions = data;
        this.setDefaultValue();
        this.snapshotPOQueryParams();
      }
    )
    this._subscriptions.push(sub);
  }

  snapshotPOQueryParams() {
    const sub = this._activatedroute.queryParams.subscribe(
      params => {
        // Only populate data to filtering controls if navigate from Po detail page
        if (Object.keys(params).length > 0) {
          if (params['navigateMode'] === 'email') {
            const poIds = params['poIds'];
            const complianceId = params['buyerComplianceId'];
            this.model.selectedSupplierId = +params['supplierId'];
            const selectedCustomer = this.customerOptions.find(c => c.value == +params['customerId']);
            if (selectedCustomer) {
              this.model.selectedCustomerId = selectedCustomer.value;
              this.customerValueChange(selectedCustomer.value);
            }
            this.searchFromEmail.emit({ buyerComplianceId: complianceId, poIds: poIds });
          } else {
            this.model.poNoFrom = params['poNoFrom'];
            this.model.poNoTo = params['poNoTo'];
            this.model.cargoReadyDateFrom = moment(params['cargoReadyDateFrom']).toDate();
            this.model.cargoReadyDateTo = moment(params['cargoReadyDateTo']).toDate();
            this.model.selectedSupplierId = +params['supplierId'];
            const selectedCustomer = this.customerOptions.find(c => c.value == +params['customerId']);
            if (selectedCustomer) {
              this.model.selectedCustomerId = selectedCustomer.value;
              this.customerValueChange(selectedCustomer.value);
              this.onSearchClick(true);
            }
          }
        }
      }
    )
    this._subscriptions.push(sub);
  }

  bindingSupplier() {
    const supplier = this.supplierOptions.find(c => c.value == this.model.selectedSupplierId);
    this.model.selectedSupplierId = supplier ? supplier.value : 0;
    this.model.selectedSupplier = supplier?.text;
  }


  setDefaultValue() {
    this.model.cargoReadyDateFrom = new Date();
    this.enableSupplierControl = false;
    this.currentUser = this._service.currentUser;
    if (this.currentUser.role.id === this.roles.Shipper) {
      this.model.selectedSupplierId = this.currentUser.organizationId;
      this.customerValueChange(this.customerOptions?.length > 0 ? this.customerOptions[0].value : 0);
    }
    this.frmControlByName('customer')?.setValidators(Validators.required);
  }

  customerValueChange(value: number) {
    this.enableSupplierControl = false;
    this.model.selectedSupplier = null;

    if (value !== null && value > 0) {
      this.enableSupplierControl = true;
      this._commonService.getSupplierByCustomerId(value).subscribe(
        data => {
          this.supplierOptions = data;
          this.bindingSupplier();
        }
      );
    } else {
      this.frmControlByName('customer')?.setErrors({ 'required': true });
    }
  }

  supplierFilterChange(value: string) {
    this.filteredSupplierOptions = [];
    if (value?.length >= 3) {
      this.filteredSupplierOptions = this.supplierOptions?.filter((opt) =>
        opt.text.toLowerCase().indexOf(value.toLowerCase()) !== -1) || [];
    }
  }

  supplierValueChange(value: string) {
    if (value?.length > 0) {
      let selectedSupplier = this.supplierOptions.find(opt => StringHelper.caseIgnoredCompare(opt.text, value));
      this.model.selectedSupplierId = selectedSupplier ? selectedSupplier.value : 0;
    }
  }

  validatePORangeInput() {
    let poNoFrom: any = this.model.poNoFrom;
    let poNoTo: any = this.model.poNoTo;

    if (StringHelper.isNullOrEmpty(poNoTo)) {
      return;
    }

    // let to compare number
    if (!isNaN(poNoFrom) && !isNaN(poNoTo)) {
      poNoFrom = Number.parseInt(poNoFrom);
      poNoTo = Number.parseInt(poNoTo);
    }

    if (StringHelper.isNullOrEmpty(poNoFrom) || poNoFrom > poNoTo) {
      this.frmControlByName('poNoTo').setErrors({ 'invalidRangeInput': true });
    }
  }

  validateCargoReadyDateRangeInput() {
    let crdFrom = this.model.cargoReadyDateFrom;
    let crdTo = this.model.cargoReadyDateTo;

    if (StringHelper.isNullOrEmpty(crdTo)) {
      return;
    }

    if (StringHelper.isNullOrEmpty(crdFrom)
      || new Date(crdFrom).setHours(0, 0, 0, 0) > new Date(crdTo).setHours(0, 0, 0, 0)) {
      this.frmControlByName('cargoReadyDateTo').setErrors({ 'invalidRangeInput': true });
    }
  }

  onSearchClick(isNavigateFromDetailPage?: boolean) {
    this.validateFormControls();

    if (this.isDisabledSearchBtn && !isNavigateFromDetailPage) {
      return;
    }

    this.search.emit(this.model);
  }

  validateFormControls(){
    this.frmControlByName('customer').markAsTouched();
    this.frmControlByName('customer').updateValueAndValidity();
  }

  resetForm() {
    this.model = new POProgressCheckFilterModel();
    this.mainForm.reset();
    this.setDefaultValue();
    this._service.resetFilterEvent$.next();
  }

  get isDisabledSearchBtn() {
    if (!this.model.selectedCustomerId || this.model.selectedCustomerId <= 0) {
      return true;
    }
    if (this.isInSearchingProgress) {
      return true;
    }
    return this.mainForm.invalid;
  }

  // convenience getter for easy access to form fields
  frmControlByName(
    name: string
  ) {
    if (this.mainForm?.controls) {
      return this.mainForm.controls[name];
    }
    return null;
  }

  ngOnDestroy(): void {
    this._subscriptions.map(x => x.unsubscribe());
  }
}
