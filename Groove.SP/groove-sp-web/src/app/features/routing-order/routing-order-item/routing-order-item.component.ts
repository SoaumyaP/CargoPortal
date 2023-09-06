import { Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges } from '@angular/core';
import { ControlContainer, NgForm } from '@angular/forms';
import { faPencilAlt, faPlus } from '@fortawesome/free-solid-svg-icons';
import { Subject, Subscription } from 'rxjs';
import { StringHelper, UnitUOMType } from 'src/app/core';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';
import { IntegrationData } from 'src/app/core/models/forms/integration-data';
import { ValidationData } from 'src/app/core/models/forms/validation-data';
import { ROLineItemModel } from 'src/app/core/models/routing-order.model';

@Component({
  selector: 'app-routing-order-item',
  templateUrl: './routing-order-item.component.html',
  styleUrls: ['./routing-order-item.component.scss'],
  viewProviders: [{ provide: ControlContainer, useExisting: NgForm }]
})
export class RoutingOrderItemComponent implements OnInit, OnDestroy, OnChanges {
  @Input()
  lineItems: ROLineItemModel[];
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

  faPlus = faPlus;
  faPencilAlt = faPencilAlt;
  defaultValue = DefaultValue2Hyphens;

  itemFormOpened: boolean = false;
  itemFormModeType = {
    add: 0,
    edit: 1
  };
  itemFormMode = this.itemFormModeType.add;
  editingItemIndex: number;
  lineItemModel: ROLineItemModel;

  itemDetailDialogOpened: boolean = false;

  // Store all subscriptions, then should un-subscribe at the end
  private _subscriptions: Array<Subscription> = [];

  constructor() { }

  ngOnChanges(changes: SimpleChanges): void {
  }

  ngOnInit() {
  }

  openItemDetailDialog(rowIndex: number): void {
    this.lineItemModel = { ...this.lineItems[rowIndex] };
    this.itemDetailDialogOpened = true;
  }

  itemDetailDialogClosedHandler() {
    this.itemDetailDialogOpened = false;
  }

  public itemRowCallback(args) {
    // Deleted row will be marked with removed property.
    return {};
  }

  public getUOMLabel(value: number, type: number): string {
    let typeName = UnitUOMType[type].toString();
    if (!StringHelper.isNullOrEmpty(value) && Math.abs(value) > 1) {
      return `label.${typeName.toLowerCase()}(plural)`;
    }
    return `label.${typeName.toLowerCase()}`;
  }

  ngOnDestroy() {
    this._subscriptions.map(x => x.unsubscribe);
  }
}
