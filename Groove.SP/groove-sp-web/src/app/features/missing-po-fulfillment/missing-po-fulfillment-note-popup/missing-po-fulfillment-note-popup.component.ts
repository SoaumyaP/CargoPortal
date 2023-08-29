import { Component, Input, OnChanges, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { StringHelper } from 'src/app/core';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { POLineItemModel } from 'src/app/core/models/po-lineitem.model';
import { NotePopupComponent } from 'src/app/core/notes/note-popup.component';
import { POFulfillmentNoteModel } from '../models/po-fulfillment-note.model';
import { POFulfillmentPOItemOptionModel } from '../models/po-fulfillment-poitemoption.model';

@Component({
  selector: 'app-missing-po-fulfillment-note-popup',
  templateUrl: './missing-po-fulfillment-note-popup.component.html',
  styleUrls: ['./missing-po-fulfillment-note-popup.component.scss']
})
export class MissingPOFulfillmentNotePopupComponent extends NotePopupComponent implements OnInit, OnChanges {

    /** Available options for PO Item auto-complete */
    @Input()
    poItemOptions: Array<POLineItemModel>;

    /** Data-source for PO Item auto-complete */
    poItemOptionsSource: Array<POFulfillmentPOItemOptionModel>;

    /** Data-source for PO Item auto-complete after filtered */
    filteredPOItemOptionsSource: Array<POFulfillmentPOItemOptionModel>;

    /** Data for note popup */
    @Input()
    model: POFulfillmentNoteModel;

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
                    const newOption = new POFulfillmentPOItemOptionModel();
                    newOption.mapFrom(x);
                    return newOption;
                });

            let purchaseOrders = this.poItemOptions.map(x => {
                const obj: {[k: string]: any} = {id : x.purchaseOrderId, name: x.purchaseOrderNumber };
                return obj;
            });

            function onlyUnique(value, index, self) {
                return self.map(x => x.id).indexOf(value.id) === index;
            }
            // Distinct the values
            purchaseOrders = purchaseOrders.filter(onlyUnique);

            purchaseOrders.map(x => {
                const newOption = new POFulfillmentPOItemOptionModel();
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
                const newOption = new POFulfillmentPOItemOptionModel();
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
        FormHelper.ValidateAllFields(this.currentForm);
        if (!this.currentForm.valid) {
            return;    
        }

        const poItemsOptionsSelected = this._getSelectedPOLineItemNoteModels();
        this.model.extendedData = JSON.stringify(poItemsOptionsSelected);
        this.add.emit(this.model);
    }

    /** As clicking on button Save in Edit mode */
    onEditClick() {
        FormHelper.ValidateAllFields(this.currentForm);
        if (!this.currentForm.valid) {
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
    poItemsValueChange (values: Array<POFulfillmentPOItemOptionModel>) {

        // NOTES: update for both two array variables.
        this.poItemOptionsSource.forEach(x => {
            x.isDisabled = false;
        });

        this.filteredPOItemOptionsSource.forEach(x => {
            x.isDisabled = false;
        });

        // All children of selected parent PO will be disabled
        const parentOptionsSelected = values.filter(x => x.isParentPO);
        if (!StringHelper.isNullOrEmpty(parentOptionsSelected)) {
            parentOptionsSelected.forEach(x => {
                this.poItemOptionsSource.forEach(y => {
                    if (y.purchaseOrderId === x.purchaseOrderId && !y.isParentPO) {
                        y.isDisabled = true;
                    }
                });
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
                this.poItemOptionsSource.forEach(y => {
                    if (y.purchaseOrderId === x.purchaseOrderId && y.isParentPO) {
                        y.isDisabled = true;
                    }
                });

                this.filteredPOItemOptionsSource.forEach(y => {
                    if (y.purchaseOrderId === x.purchaseOrderId && y.isParentPO) {
                        y.isDisabled = true;
                    }
                });
            });
        }
    }

    /** Check whether current PO Item option should be disabled */
    public isPOItemDisabled(itemArgs: { dataItem: POFulfillmentPOItemOptionModel, index: number }): boolean {
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
