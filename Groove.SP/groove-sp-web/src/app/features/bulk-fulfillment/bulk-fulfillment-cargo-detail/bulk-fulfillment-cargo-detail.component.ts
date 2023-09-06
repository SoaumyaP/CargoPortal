import { Component, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges } from '@angular/core';
import { ControlContainer, NgForm } from '@angular/forms';
import { faPencilAlt, faPlus } from '@fortawesome/free-solid-svg-icons';
import { Subject, Subscription } from 'rxjs';
import { DropDowns, EquipmentStringType, POFulfillmentOrderStatus, StringHelper, ValidationDataType, ViewSettingModuleIdType } from 'src/app/core';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';
import { IntegrationData } from 'src/app/core/models/forms/integration-data';
import { ValidationData } from 'src/app/core/models/forms/validation-data';
import { ViewSettingModel } from 'src/app/core/models/viewSetting.model';
import { BulkFulfillmentOrderModel } from '../models/bulk-fulfillment.model';

@Component({
  selector: 'app-bulk-fulfillment-cargo-detail',
  templateUrl: './bulk-fulfillment-cargo-detail.component.html',
  styleUrls: ['./bulk-fulfillment-cargo-detail.component.scss'],
  viewProviders: [{ provide: ControlContainer, useExisting: NgForm }]
})
export class BulkFulfillmentCargoDetailComponent implements OnInit, OnDestroy, OnChanges {
  @Input()
  orders: BulkFulfillmentOrderModel[];
  @Input()
  loads: any[];
  @Input()
  isHiddenLoads: boolean;
  @Input()
  isAddMode: boolean;
  @Input()
  isEditMode: boolean;
  @Input()
  saveAsDraft: boolean;
  @Input()
  parentIntegration$: Subject<IntegrationData>;
  @Input()
  isLoadTabEditable: boolean;
  @Input()
  tabPrefix: string;
  @Input()
  formErrors: any;
  @Input()
  validationRules: any;
  @Input() viewSettings: ViewSettingModel[];
  @Input() isViewMode: boolean;
  viewSettingModuleIdType = ViewSettingModuleIdType;
  formHelper = FormHelper;
  
  @Input()
  allowMixedCarton: boolean;

  @Output()
  mixedCartonChange: EventEmitter<boolean> = new EventEmitter<boolean>();

  faPlus = faPlus;
  faPencilAlt = faPencilAlt;
  defaultValue = DefaultValue2Hyphens;
  errorMessages = {
    atLeastOneContainer: null,
    atLeastOneCargoItem: null,
    invalidBookedPackage: null
  }

  itemFormOpened: boolean = false;
  itemFormModeType = {
    add: 0,
    edit: 1
  };
  itemFormMode = this.itemFormModeType.add;
  editingItemIndex: number;
  orderModel: BulkFulfillmentOrderModel;

  itemDetailDialogOpened: boolean = false;

  equipmentStringTypeOptions = DropDowns.EquipmentStringType.filter(
    e => e.value !== EquipmentStringType.LCLShipment && e.value !== EquipmentStringType.AirShipment && e.value !== EquipmentStringType.Truck);
  groupedLoads: any[] = [];

  // Store all subscriptions, then should un-subscribe at the end
  private _subscriptions: Array<Subscription> = [];

  constructor() { }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes?.loads?.currentValue) {
      this.groupLoads();
    }

    if (changes?.isLoadTabEditable?.currentValue) {
      this.resetErrorMessages();
    }
  }

  ngOnInit() {
    if (this.isAddMode) {
      this.addBlankLoad();
    }
  }

  groupLoads(): void {
    this.groupedLoads = [];
    if (!this.loads) {
      return;
    }
    this.loads.forEach(load => {
      let existingLoadIndex = this.groupedLoads.findIndex(l => l.equipmentType === load.equipmentType);
      if (existingLoadIndex !== -1) {
        this.groupedLoads[existingLoadIndex].quantity += 1;
      } else {
        this.groupedLoads.push({
          equipmentType: load.equipmentType,
          quantity: 1
        });
      }
    });
    this.resetEquipmentTypeOptions();
    this.addLoadGridValidation();
  }

  addOrder(): void {
    this.orderModel = new BulkFulfillmentOrderModel();
    this.orderModel.status = POFulfillmentOrderStatus.Active;
    this.itemFormMode = this.itemFormModeType.add;
    this.itemFormOpened = true;
  }

  onOrderAdded(data: BulkFulfillmentOrderModel): void {
    this.itemFormOpened = false;
    data.bookedPackage = data.bookedPackage | 0;
    this.orders.push(data);
    this.errorMessages.atLeastOneCargoItem = null;
  }

  editOrder(rowIndex: number): void {
    this.editingItemIndex = rowIndex;
    this.orderModel = { ...this.orders[rowIndex] };
    this.itemFormMode = this.itemFormModeType.edit;
    this.itemFormOpened = true;
  }

  onOrderEdited(data: BulkFulfillmentOrderModel) {
    this.itemFormOpened = false;
    this.orders[this.editingItemIndex].customerPONumber = data.customerPONumber;
    this.orders[this.editingItemIndex].productCode = data.productCode;
    this.orders[this.editingItemIndex].countryCodeOfOrigin = data.countryCodeOfOrigin;
    this.orders[this.editingItemIndex].productName = data.productName;
    this.orders[this.editingItemIndex].chineseDescription = data.chineseDescription;
    this.orders[this.editingItemIndex].fulfillmentUnitQty = data.fulfillmentUnitQty;
    this.orders[this.editingItemIndex].bookedPackage = data.bookedPackage;
    this.orders[this.editingItemIndex].grossWeight = data.grossWeight;
    this.orders[this.editingItemIndex].netWeight = data.netWeight;
    this.orders[this.editingItemIndex].volume = data.volume;
    this.orders[this.editingItemIndex].unitUOM = data.unitUOM;
    this.orders[this.editingItemIndex].packageUOM = data.packageUOM;
    this.orders[this.editingItemIndex].hsCode = data.hsCode;
    this.orders[this.editingItemIndex].commodity = data.commodity;
    this.orders[this.editingItemIndex].shippingMarks = data.shippingMarks;
  }

  itemFormClosedHandler(): void {
    this.itemFormOpened = false;
  }

  deleteOrder(rowIndex: number): void {
    this.orders.splice(rowIndex, 1);
  }

  openItemDetailDialog(rowIndex: number): void {
    this.orderModel = { ...this.orders[rowIndex] };
    this.itemDetailDialogOpened = true;
  }

  itemDetailDialogClosedHandler() {
    this.itemDetailDialogOpened = false;
  }

  equipmentTypeChanged(rowIndex: number): void {
    this.resetEquipmentTypeOptions();
  }

  addBlankLoad(equipmentType = null): void {
    this.groupedLoads.push({
      'equipmentType': equipmentType,
      'quantity': 1
    });
    this.errorMessages.atLeastOneContainer = null;
    this.resetEquipmentTypeOptions();
    this.addLoadValidationAt_((this.groupedLoads.length || 1) - 1);
  }

  addLoadValidationAt_(rowIndex: number): void {
    this.validationRules[`${this.tabPrefix}equipmentType_${rowIndex}`] = {
      required: 'label.equipmentType'
    };
    this.validationRules[`${this.tabPrefix}containerQty_${rowIndex}`] = {
      required: 'label.container'
    };
  }

  addLoadGridValidation(): void {
    for (let i = 0; i < this.groupedLoads.length; i++) {
      if (StringHelper.isNullOrEmpty(this.groupedLoads[i].removed) || !this.groupedLoads[i].removed) {
        this.addLoadValidationAt_(i);
      }
    }
  }

  deleteLoad(rowIndex: number): void {
    // Update properties for current contact row
    const rowData = this.groupedLoads[rowIndex];
    rowData.removed = true;

    // Reset formErrors and validationRules for this row
    const formErrorNames = [
      `equipmentType_${rowIndex}`,
      `containerQty_${rowIndex}`
    ];

    // Clear current formErrors for current row
    Object.keys(this.formErrors)
      .filter(x => formErrorNames.some(y => x.startsWith(this.tabPrefix + y)))
      .map(x => {
        delete this.formErrors[x];
      });

    // Clear also validationRules for current row
    Object.keys(this.validationRules)
      .filter(x => formErrorNames.some(y => x.startsWith(this.tabPrefix + y)))
      .map(x => {
        delete this.validationRules[x];
      });

    // Call other business

    // Pass value to avoid default control validation.
    this.groupedLoads[rowIndex].equipmentType = -1;
    this.groupedLoads[rowIndex].quantity = 1;

    this.resetEquipmentTypeOptions();
  }

  deleteAllLoads(): void {
    for (let i = 0; i < this.groupedLoads.length; i++) {
      if (!this.groupedLoads[i].removed) {
        this.deleteLoad(i);
      }
    }
  }

  private getEquipmentTypeOptions(rowIndex: number): void {
    let existingEquipmentTypes = this._filterLoadList.map(x => x.equipmentType)
      .filter(x => x != null && x !== this.groupedLoads[rowIndex].equipmentType);

    let result = this.equipmentStringTypeOptions?.filter(x => existingEquipmentTypes.indexOf(x.value) === -1);
    return result || [];
  }

  resetEquipmentTypeOptions() {
    let ordinalNumber = 1;
    for (let index = 0; index < this.groupedLoads.length; index++) {
      if (StringHelper.isNullOrEmpty(this.groupedLoads[index].removed) || !this.groupedLoads[index].removed) {
        this.groupedLoads[index].equipmentTypeOptions = this.getEquipmentTypeOptions(index);
        this.groupedLoads[index].ordinalNo = ordinalNumber;
        ordinalNumber++;
      }
    }
  }

  get totalBookedPackage(): number {
    let total = this.orders?.map(x => x.bookedPackage)
      .reduce(function (acc, val) { return acc + val; }, 0);

    return total || 0;
  }

  get totalGrossWeight(): number {
    let total = this.orders?.map(x => x.grossWeight)
      .reduce(function (acc, val) { return acc + val; }, 0);

    return total || 0;
  }

  get totalVolume(): number {
    let total = this.orders?.map(x => x.volume)
      .reduce(function (acc, val) { return acc + val; }, 0);

    return total || 0;
  }

  get canAddContainer(): boolean {
    if (!this._filterLoadList || this._filterLoadList.length <= 0) {
      return true;
    }
    return !this._filterLoadList.some(x => StringHelper.isNullOrEmpty(x.equipmentType) || StringHelper.isNullOrEmpty(x.quantity));
  }

  private get _filterLoadList() {
    // exclude call row removed
    return this.groupedLoads.filter(x => StringHelper.isNullOrEmpty(x.removed) || !x.removed);
  }

  public loadRowCallback(args) {
    // Deleted row will be marked with removed property.
    return {
      'hide-row': args.dataItem.removed,
      'error-row': args.dataItem.isValidRow === false,
    };
  }

  public itemRowCallback(args) {
    // Deleted row will be marked with removed property.
    return {};
  }

  private resetErrorMessages(): void {
    this.errorMessages.atLeastOneContainer = null;
    this.errorMessages.atLeastOneCargoItem = null;
    this.errorMessages.invalidBookedPackage = null;
  }

  private validate(): ValidationData[] {
    let result: ValidationData[] = [];

    if (!this.isHiddenLoads && (!this._filterLoadList || this._filterLoadList.length === 0)) {
      const errorMessage = 'validation.specifyContainerType';

      this.errorMessages.atLeastOneContainer = errorMessage;
      result.push(
        new ValidationData(ValidationDataType.Business, false, errorMessage)
      );
    }

    if (!this.orders || this.orders.length === 0) {
      const errorMessage = 'validation.atLeastOneCargoItem';

      this.errorMessages.atLeastOneCargoItem = errorMessage;
      result.push(
        new ValidationData(ValidationDataType.Business, false, errorMessage)
      );
    }
    else {
      if (this.allowMixedCarton) {
        if (this.totalBookedPackage === 0) {
          const errorMessage = 'validation.totalBookedPackageMustGreaterThanZero';
          this.errorMessages.invalidBookedPackage = errorMessage;

          result.push(
            new ValidationData(ValidationDataType.Business, false, errorMessage)
          );
        }
      }
      else {
        let invalid = this.orders.findIndex(x => x.bookedPackage <= 0) !== -1;
        if (invalid) {
          const errorMessage = 'validation.bookedPackageMustGreaterThanZero';
          this.errorMessages.invalidBookedPackage = errorMessage;

          result.push(
            new ValidationData(ValidationDataType.Business, false, errorMessage)
          );
        }
      }
    }

    return result;
  }

  public validateBeforeSaving(): ValidationData[] {
    let result: ValidationData[] = [];

    this.resetErrorMessages();
    if (!this.saveAsDraft) {
      result = this.validate();
    }
    // In case there is any error.
    const errors = Object.keys(this.formErrors)?.filter(x => x.startsWith(this.tabPrefix));
    for (let index = 0; index < errors.length; index++) {
      const err = Reflect.get(this.formErrors, errors[index]);
      if (err && !StringHelper.isNullOrEmpty(err)) {
        result.push(new ValidationData(ValidationDataType.Input, false));
      }
    }
    return result;
  }

  public validateBeforeSubmitting(): ValidationData[] {
    this.resetErrorMessages();
    let result: ValidationData[] = [];
    result = this.validate();
    return result;
  }

  public getUOMLabel(value: number, type: string): string {
      if (!StringHelper.isNullOrEmpty(value) && Math.abs(value) > 1 ) {
          return `label.${type.toLowerCase()}(plural)`;
        }
      return `label.${type.toLowerCase()}`;
    }

  ngOnDestroy() {
    this._subscriptions.map(x => x.unsubscribe);
  }
}
