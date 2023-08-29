import { Component, Input, OnChanges, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { StringHelper } from 'src/app/core';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { POLineItemModel } from 'src/app/core/models/po-lineitem.model';
import { NotePopupComponent } from 'src/app/core/notes/note-popup.component';
import { ShipmentItemOptionModel } from '../models/shipment-item-option.model';
import { ShipmentNoteModel } from '../models/shipment-note.model';

@Component({
  selector: 'app-shipment-note-popup',
  templateUrl: './shipment-note-popup.component.html',
  styleUrls: ['./shipment-note-popup.component.scss']
})
export class ShipmentNotePopupComponent extends NotePopupComponent implements OnInit, OnChanges {
  @ViewChild('mainForm', { static: false }) mainForm: NgForm;

  /** Available options for PO Item auto-complete */
  @Input()
  poItemOptions: Array<POLineItemModel>;

  /** Data-source for PO Item auto-complete */
  poItemOptionsSource: Array<ShipmentItemOptionModel>;

  /** Data-source for PO Item auto-complete after filtered */
  filteredPOItemOptionsSource: Array<ShipmentItemOptionModel>;

  /** Data for note popup */
  @Input()
  model: ShipmentNoteModel;

  constructor(protected _translateService: TranslateService) {
    super(_translateService);
  }

  ngOnInit() {
  }

  ngOnChanges() {

    // Initialize data-source for PO Item auto-complete
    this.poItemOptionsSource = [];
    if (!StringHelper.isNullOrEmpty(this.poItemOptions)) {
      this.poItemOptionsSource = this.poItemOptions
        .map(x => {
          const newOption = new ShipmentItemOptionModel();
          newOption.mapFrom(x);
          return newOption;
        });

      let purchaseOrders = this.poItemOptions.map(x => {
        const obj: { [k: string]: any } = { id: x.purchaseOrderId, name: x.purchaseOrderNumber };
        return obj;
      });

      function onlyUnique(value, index, self) {
        return self.map(x => x.id).indexOf(value.id) === index;
      }
      // Distinct the values
      purchaseOrders = purchaseOrders.filter(onlyUnique);

      purchaseOrders.map(x => {
        const newOption = new ShipmentItemOptionModel();
        newOption.purchaseOrderId = x.id;
        newOption.purchaseOrderNumber = x.name;
        newOption.value = x.name;

        this.poItemOptionsSource.push(newOption);
      });
    }

    // Add selected options if not in current source
    if (!StringHelper.isNullOrEmpty(this.model) && !StringHelper.isNullOrEmpty(this.model.poItems)) {
      this.model.poItems
        .filter(x => !this.poItemOptionsSource.some(y => y.value === x.value))
        .map(x => {
          const newOption = new ShipmentItemOptionModel();
          newOption.mapFrom(x);
          this.poItemOptionsSource.push(newOption);
        });
    }
    // Call method to update properties for display UI
    this._updateStatePOLineItemsOptions();
    this.filteredPOItemOptionsSource = this.poItemOptionsSource;
  }

  /** As clicking on button Save in Add mode */
  onAddClick() {
    FormHelper.ValidateAllFields(this.mainForm);
    if (this.mainForm.invalid) {
      return;
    }

    const poItemsOptionsSelected = this._getSelectedPOLineItemNoteModels();
    this.model.extendedData = JSON.stringify(poItemsOptionsSelected);
    this.add.emit(this.model);
  }

  /** As clicking on button Save in Edit mode */
  onEditClick() {
    FormHelper.ValidateAllFields(this.mainForm);
    if (this.mainForm.invalid) {
      return;
    }
    
    const poItemsOptionsSelected = this._getSelectedPOLineItemNoteModels();
    this.model.extendedData = JSON.stringify(poItemsOptionsSelected);
    this.edit.emit(this.model);
  }

  /** Check whether current PO Item option is selected */
  isPOItemSelected(option) {
    return this.model.poItems.some(item => item.value === option.value);
  }

  /** Handler for PO Item auto-complete filter changed */
  poItemsFilterChange(value: string) {
    if (value.length >= 1) {
      this.filteredPOItemOptionsSource = this.poItemOptionsSource.filter((s) =>
        s.optionText.toLowerCase().indexOf(value.toLowerCase()) !== -1
        || this.model.poItems.map(x => x.value).indexOf(s.value) !== -1);
    } else {
      this.filteredPOItemOptionsSource = this.poItemOptionsSource;
    }
  }

  /** Handler for PO Item auto-complete value changed.
   * To disable/ enable related options
  */
  poItemsValueChange(values: Array<ShipmentItemOptionModel>) {

    this.filteredPOItemOptionsSource.forEach(x => {
      x.isDisabled = false;
    });

    // All children of selected parent PO will be disabled
    const parentOptionsSelected = values.filter(x => x.isParentPO);
    if (!StringHelper.isNullOrEmpty(parentOptionsSelected)) {
      parentOptionsSelected.forEach(x => {
        this.filteredPOItemOptionsSource.forEach(y => {
          if (y.purchaseOrderId === x.purchaseOrderId && !y.isParentPO) {
            y.isDisabled = true;
          }
        });
      });
    }

    // Parent PO of any child option will be disabled
    const childOptionsSelected = values.filter(x => !x.isParentPO);
    if (!StringHelper.isNullOrEmpty(childOptionsSelected)) {
      childOptionsSelected.forEach(x => {
        this.filteredPOItemOptionsSource.forEach(y => {
          if (y.purchaseOrderId === x.purchaseOrderId && y.isParentPO) {
            y.isDisabled = true;
          }
        });
      });
    }
  }

  /** Check whether current PO Item option should be disabled */
  public isPOItemDisabled(itemArgs: { dataItem: ShipmentItemOptionModel, index: number }): boolean {
    return itemArgs.dataItem.isDisabled;
  }

  /** Get current selected data of PO Item auto-complete*/
  private _getSelectedPOLineItemNoteModels(): Array<POLineItemModel> {
    let poItemsOptionsSelected = [];
    if (!StringHelper.isNullOrEmpty(this.model.poItems)) {
      poItemsOptionsSelected = this.model.poItems.map(x => {
        return new POLineItemModel(x.purchaseOrderId, x.purchaseOrderNumber, x.poLineItemId, x.productCode);
      });
    }
    return poItemsOptionsSelected;
  }

  /** To update some properties on UI display: isDisabled, isOrphan,... */
  private _updateStatePOLineItemsOptions(): void {

    if (StringHelper.isNullOrEmpty(this.model) || StringHelper.isNullOrEmpty(this.model.poItems)) {
      return;
    }

    // All children of selected parent PO will be disabled
    const parentOptionsSelected = this.model.poItems.filter(x => StringHelper.isNullOrEmpty(x.poLineItemId));
    if (!StringHelper.isNullOrEmpty(parentOptionsSelected)) {
      parentOptionsSelected.forEach(x => {
        this.poItemOptionsSource.forEach(y => {
          if (y.purchaseOrderId === x.purchaseOrderId && !y.isParentPO) {
            y.isDisabled = true;
          }
        });
      });
    }

    // Parent PO of any child option will be disabled
    const childOptionsSelected = this.model.poItems.filter(x => !StringHelper.isNullOrEmpty(x.poLineItemId));
    if (!StringHelper.isNullOrEmpty(childOptionsSelected)) {
      childOptionsSelected.forEach(x => {
        this.poItemOptionsSource.forEach(y => {
          if (y.purchaseOrderId === x.purchaseOrderId && y.isParentPO) {
            y.isDisabled = true;
          }
        });
      });
    }

    // Some selected data which removed, not existed in the source
    this.poItemOptionsSource.filter(x => !x.isParentPO).forEach(y => {
      if (!this.poItemOptionsSource.some(z => z.isParentPO && z.purchaseOrderId === y.purchaseOrderId)) {
        y.isOrphan = true;
      }
    });
  }

}
