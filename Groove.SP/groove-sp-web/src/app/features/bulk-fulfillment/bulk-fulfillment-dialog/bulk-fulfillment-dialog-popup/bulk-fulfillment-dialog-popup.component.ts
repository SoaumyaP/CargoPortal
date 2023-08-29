import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { ArrayHelper } from 'src/app/core/helpers/array.helper';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { NotePopupComponent } from 'src/app/core/notes/note-popup.component';
import { BulkFulfillmentNoteItemModel } from '../../models/bulk-fulfillment-note-item.model';
import { BulkFulfillmentNoteModel } from '../../models/bulk-fulfillment-note.model';
import { BulkFulfillmentModel } from '../../models/bulk-fulfillment.model';

@Component({
    selector: 'app-bulk-fulfillment-dialog-popup',
    templateUrl: './bulk-fulfillment-dialog-popup.component.html',
    styleUrls: ['./bulk-fulfillment-dialog-popup.component.scss']
})
export class BulkFulfillmentDialogPopupComponent extends NotePopupComponent implements OnInit, OnChanges {
    @Input() bookingModel: BulkFulfillmentModel;
    @Input() bookingNoteModel: BulkFulfillmentNoteModel;
    @Input() itemDropdown: Array<BulkFulfillmentNoteItemModel> = [];

    sourceItemDropdown: Array<BulkFulfillmentNoteItemModel> = [];
    filteredItemDropdown: Array<BulkFulfillmentNoteItemModel> = [];

    constructor(protected _translateService: TranslateService) {
        super(_translateService);
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes.bookingModel?.currentValue) {
            this.sourceItemDropdown = this.itemDropdown;
            this.filteredItemDropdown = this.itemDropdown;
        }

        if (changes.bookingNoteModel?.currentValue) {
            let extendedData = this.bookingNoteModel.extendedData ? JSON.parse(this.bookingNoteModel.extendedData) : [];
            const items = ArrayHelper.uniqueBy([...extendedData, ...this.itemDropdown], "poFulfillmentOrderId") as Array<BulkFulfillmentNoteItemModel>;
            this.sourceItemDropdown = items;
            this.filteredItemDropdown = items;
        }
    }

    ngOnInit() {

    }

    /** As clicking on button Save in Add mode */
    onAddClick() {
        FormHelper.ValidateAllFields(this.currentForm);
        if (this.currentForm.invalid) {
            return;
        }
        
        if (this.bookingNoteModel.itemsSelected.length >= 0) {
            this.bookingNoteModel.extendedData = JSON.stringify(this.bookingNoteModel.itemsSelected);
        }
        this.add.emit(this.bookingNoteModel);
    }

    /** As clicking on button Save in Edit mode */
    onEditClick() {
        FormHelper.ValidateAllFields(this.currentForm);
        if (this.currentForm.invalid) {
            return;
        }

        this.bookingNoteModel.extendedData = JSON.stringify(this.bookingNoteModel.itemsSelected);
        this.remapSelectedItems();
        this.edit.emit(this.bookingNoteModel);
    }

    isItemSelected(selectedItem: any) {
        return this.bookingNoteModel.itemsSelected.some(c => c.poFulfillmentOrderId === selectedItem.poFulfillmentOrderId);
    }

    onItemFiltered(value: string) {
        if (value.length >= 1) {
            this.filteredItemDropdown = this.sourceItemDropdown.filter((s) => s.value.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        } else {
            this.filteredItemDropdown = this.sourceItemDropdown;
        }
    }

    /**
     * Remap value of selected items when user updating value of item in CarogoDetail section
     */
    remapSelectedItems() {
        for (let selectedItem of this.bookingNoteModel.itemsSelected) {
            const item = this.itemDropdown.find(c => c.poFulfillmentOrderId === selectedItem.poFulfillmentOrderId);
            if (item) {
                selectedItem.productCode = item.productCode;
                selectedItem.productName = item.productName;
                selectedItem.value = item.value;
            }
        }
    }
}
