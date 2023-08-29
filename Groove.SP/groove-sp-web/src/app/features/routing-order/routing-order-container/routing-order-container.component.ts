import { Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges } from '@angular/core';
import { ControlContainer, NgForm } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { DropDowns, EquipmentStringType, StringHelper, ValidationDataType } from 'src/app/core';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';
import { IntegrationData } from 'src/app/core/models/forms/integration-data';
import { RoutingOrderContainerModel } from 'src/app/core/models/routing-order.model';

@Component({
  selector: 'app-routing-order-container',
  templateUrl: './routing-order-container.component.html',
  styleUrls: ['./routing-order-container.component.scss'],
  viewProviders: [{ provide: ControlContainer, useExisting: NgForm }]
})
export class RoutingOrderContainerComponent implements OnInit, OnDestroy, OnChanges {
  @Input()
  containers: RoutingOrderContainerModel[];
  @Input()
  isAddMode: boolean;
  @Input()
  isEditMode: boolean;
  @Input()
  parentIntegration$: Subject<IntegrationData>;
  @Input()
  isViewMode: boolean;
  @Input()
  tabPrefix: string;
  @Input()
  formErrors: any;
  @Input()
  validationRules: any;

  defaultValue = DefaultValue2Hyphens;
  errorMessages = {
    atLeastOneContainer: null
  }

  equipmentStringTypeOptions = DropDowns.EquipmentStringType.filter(
    e => e.value !== EquipmentStringType.LCLShipment && e.value !== EquipmentStringType.AirShipment && e.value !== EquipmentStringType.Truck);

  // Store all subscriptions, then should un-subscribe at the end
  private _subscriptions: Array<Subscription> = [];

  constructor() { }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes?.containers?.currentValue) {
      this.resetEquipmentTypeOptions();
      this.addLoadGridValidation();
    }

    if (changes?.isViewMode?.currentValue) {
      this.resetErrorMessages();
    }
  }

  ngOnInit() {
  }

  equipmentTypeChanged(rowIndex: number): void {
    this.resetEquipmentTypeOptions();
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
    for (let i = 0; i < this.containers.length; i++) {
      if (StringHelper.isNullOrEmpty(this.containers[i].removed) || !this.containers[i].removed) {
        this.addLoadValidationAt_(i);
      }
    }
  }

  private getEquipmentTypeOptions(rowIndex: number) {
    let existingEquipmentTypes = this._filterContainerList.map(x => x.containerType)
      .filter(x => x != null && x !== this.containers[rowIndex].containerType);

    let result = this.equipmentStringTypeOptions?.filter(x => existingEquipmentTypes.indexOf(x.value) === -1);
    return result || [];
  }

  resetEquipmentTypeOptions() {
    for (let index = 0; index < this.containers.length; index++) {
      if (StringHelper.isNullOrEmpty(this.containers[index].removed) || !this.containers[index].removed) {
        this.containers[index].equipmentTypeOptions = this.getEquipmentTypeOptions(index);
      }
    }
  }

  private get _filterContainerList() {
    // exclude call row removed
    return this.containers.filter(x => StringHelper.isNullOrEmpty(x.removed) || !x.removed);
  }

  public loadRowCallback(args) {
    // Deleted row will be marked with removed property.
    return {
      'hide-row': args.dataItem.removed,
      'error-row': args.dataItem.isValidRow === false,
    };
  }

  private resetErrorMessages(): void {
    this.errorMessages.atLeastOneContainer = null;
  }

  ngOnDestroy() {
    this._subscriptions.map(x => x.unsubscribe);
  }
}
