import { HttpClient } from '@angular/common/http';
import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { faEllipsisV, faInfoCircle, faPlus } from '@fortawesome/free-solid-svg-icons';
import { TranslateService } from '@ngx-translate/core';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { DropDowns, FormComponent, StringHelper, UserContextService } from 'src/app/core';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { environment } from 'src/environments/environment';
import { OrganizationPreferenceService } from '../../organization-preference/Organization-preference.service';
import { BulkFulfillmentFormService } from '../bulk-fulfillment-form/bulk-fulfillment-form.service';

@Component({
  selector: 'app-bulk-fulfillment-item-form-dialog',
  templateUrl: './bulk-fulfillment-item-form-dialog.component.html',
  styleUrls: ['./bulk-fulfillment-item-form-dialog.component.scss']
})
export class BulkFulfillmentItemFormDialogComponent extends FormComponent implements OnInit, OnDestroy {
  @Input() public itemFormOpened: boolean = false;
  @Output() close: EventEmitter<any> = new EventEmitter();
  @Output() add: EventEmitter<any> = new EventEmitter<any>();
  @Output() edit: EventEmitter<any> = new EventEmitter<any>();

  faPlus = faPlus;
  faEllipsisV = faEllipsisV;
  faInfoCircle = faInfoCircle;

  @Input() public formMode: any;
  @Input() public model: any;
  @Input() public currentSelectedIndex = null;
  @Input() public originBalance: number;
  @Input() public originFulfillmentUnitQty: number;
  @Input() public isRequireBookedPackage: boolean;

  currentUser: any;

  CustomerPOFormModeType = {
    add: 0,
    edit: 1
  };

  validationRules = {
    'packageUOM': {
      'required': 'label.packageUOM'
    },
    'unitUOM': {
      'required': 'label.uom'
    },
    'descriptionOfGoods': {
      'required': 'label.descriptionOfGoods'
    },
    'fulfillmentUnitQty': {
      'required': 'label.bookedQty',
      'greaterThan': 'label.zero'
    },
    'bookedPackage': {
      'required': 'label.bookedPackage',
      'greaterThan': 'label.zero'
    },
    'netWeight': {
      'mustNotGreaterThan': 'label.grossWeight'
    },
    'grossWeight': {
      'required': 'label.grossWeightKGS',
      'greaterThan': 'label.zero'
    },
    'volumeCBM': {
      'required': 'label.volumeCBM',
      'greaterThan': 'label.zero'
    },
    'hsCode': {
      'numericMaxlength': 'validation.numericMaxlengthHsCode',
      'separatedSymbol': 'validation.separatedSymbolHsCode'
    }
  };

  commodityOptions = DropDowns.CommodityString;
  countryOptions: any;
  unitUOMTypeOptions = DropDowns.UnitUOMStringType;
  packageUOMTypeOptions = DropDowns.PackageUOMStringType;

  customerOrganization: any;
  supplierOrganization: any;
  // Store all subscriptions, then should un-subscribe at the end
  private _subscriptions: Array<Subscription> = [];

  productCodeOptions: any[];
  orgPreferences: any[];

  get ifEditMode(): boolean {
    return this.formMode === this.CustomerPOFormModeType.edit;
  }

  constructor(protected route: ActivatedRoute,
    public notification: NotificationPopup,
    public router: Router,
    public service: BulkFulfillmentFormService,
    public translateService: TranslateService,
    private _userContext: UserContextService,
    private httpClient: HttpClient,
    private orgPreferenceService: OrganizationPreferenceService) {
    super(route, service, notification, translateService, router);

    let sub = this._userContext.getCurrentUser().subscribe(user => {
      if (user) {
        this.currentUser = user;
        if (!user.isInternal) {
          this.service.affiliateCodes = user.affiliates;

          let getOrgPreferenceSub = orgPreferenceService.getListByOrganization(user.organizationId).subscribe(
            (data) => this.orgPreferences = data
          );

          this._subscriptions.push(getOrgPreferenceSub);
        }
      }
    });
    this._subscriptions.push(sub);
  }

  ngOnInit() {
    let sub = this.httpClient.get(`${environment.commonApiUrl}/countries/dropDownCode`).subscribe(countries => {
      this.countryOptions = countries;
    });
    this._subscriptions.push(sub);

    this.setDefaultValue();
  }

  private setDefaultValue() {
    if (this.model?.fulfillmentUnitQty === 0) {
      this.model.fulfillmentUnitQty = null;
    }
  }

  onFulfillmentUnitQtyChanged() {
    const bookedQty = StringHelper.isNullOrEmpty(this.model.fulfillmentUnitQty) ? 0 : this.model.fulfillmentUnitQty;
    this.model.balanceUnitQty = this.originBalance - bookedQty + this.originFulfillmentUnitQty;
  }

  onBookedPackageChanged() {
    if (StringHelper.isNullOrEmpty(this.model.bookedPackage)) {
      this.model.bookedPackage = 0;
    }
  }

  onHsCodeValueChanged() {
    this.validateHsCode(this.model.hsCode, true);
  }

  onProductCodeFilterChange(val): void {
    this.productCodeOptions = [];
    if (val.length < 3) {
      return;
    }

    if (!this.currentUser?.isInternal) {
      this.orgPreferences.filter(x => x.productCode.toLowerCase().indexOf(val.toLowerCase()) !== -1)?.map(
        (x) => this.productCodeOptions.push({
          name: x.productCode
        })
      );
    }
  }

  onProductCodeValueChange(val): void {
    if (val.length <= 0) {
      return;
    }

    if (!this.currentUser?.isInternal) {
      const orgPreference = this.orgPreferences.find(x => StringHelper.caseIgnoredCompare(x.productCode, val));

      if (orgPreference) {
        // populate HS Code
        if (!StringHelper.isNullOrEmpty(orgPreference.hsCode)) {
          this.model.hsCode = orgPreference.hsCode;
        }

        // populate Chinese Description
        if (!StringHelper.isNullOrEmpty(orgPreference.chineseDescription)) {
          this.model.chineseDescription = orgPreference.chineseDescription;
        }
      }
    }
  }

  validateHsCode(hsCodeInputValue: string, isSetErrorOnForm?: boolean) {
    if (!StringHelper.isNullOrEmpty(hsCodeInputValue)) {
      let isValidHsCode = true;
      let valueToTest = hsCodeInputValue.replace(/[.\s]/g, '');
      const hsCodeControl = isSetErrorOnForm ? this.mainForm.form.get('hsCode') : null;

      // check for comma first: Please separate the HS Code by comma.
      if (/^\b([a-zA-Z0-9,])+$/g.test(valueToTest)) {
      } else {
        if (isSetErrorOnForm) {
          this.setInvalidControl('hsCode', 'separatedSymbol');
          hsCodeControl.markAsDirty();
        }
        isValidHsCode = false;
      }
      // fast return
      if (!isValidHsCode) {
        return isValidHsCode;
      }


      // Check for valid format: Its length must be in 4, 6, 8, 10, and 13 digits only.
      valueToTest = valueToTest.replace(/[^0-9]/g, ',');
      const hsCodeArray = valueToTest.split(',');

      hsCodeArray.map((item: string) => {
        if (/^(?:\d{4},?|\d{6},?|\d{8},?|\d{10},?|\d{13},?)$/g.test(item)) {
          // is valid
        } else {
          if (isSetErrorOnForm) {
            this.setInvalidControl('hsCode', 'numericMaxlength');
            hsCodeControl.markAsDirty();
          }
          isValidHsCode = false;
        }
      });
      return isValidHsCode;
    }
  }

  onValidateNetWeightKGS() {
    if (StringHelper.isNullOrEmpty(this.model.netWeight) || StringHelper.isNullOrEmpty(this.model.grossWeight)) {
      this.setValidControl('netWeight');
      return;
    }
    if (this.model.netWeight > this.model.grossWeight) {
      this.setInvalidControl('netWeight', 'mustNotGreaterThan');
    } else {
      this.setValidControl('netWeight');
    }
  }

  onFormClosed() {
    this.itemFormOpened = false;
    this.close.emit();
  }

  onSave() {
    this.validateAllFields(false);
    if (this.mainForm.valid) {
      switch (this.formMode) {
        case this.CustomerPOFormModeType.add:
          this.add.emit(this.model);
          break;
        case this.CustomerPOFormModeType.edit:
          this.edit.emit(this.model);
          break;
      }
    }
  }

  ngOnDestroy() {
    this._subscriptions.map(x => x.unsubscribe());
  }
}
