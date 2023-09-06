import { Component, Input, OnChanges, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { StringHelper } from 'src/app/core';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { NotePopupComponent } from 'src/app/core/notes/note-popup.component';
import { CruiseOrderLineItemNoteModel } from '../../../models/cruise-order-item-note.model';

@Component({
    selector: 'app-cruise-order-item-note-popup',
    templateUrl: './cruise-order-item-note-popup.component.html',
    styleUrls: ['./cruise-order-item-note-popup.component.scss']
})
export class CruiseOrderItemNotePopupComponent extends NotePopupComponent implements OnInit, OnChanges {
    @ViewChild('mainForm', { static: false }) mainForm: NgForm;
    
    /** Data for note popup */
    @Input()
    model: CruiseOrderLineItemNoteModel;
    @Input()
    popupOpened: boolean;
    @Input()
    isAddMode: boolean;
    @Input()
    itemOptions: Array<string>;

    /** Options for category selection */
    categoryOptions: { text: string, value: string }[] = [
        { text: 'WMD- Will Miss Dry Dock', value: 'WMD- Will Miss Dry Dock' },
        { text: 'PDD- Post Dry Dock', value: 'PDD- Post Dry Dock' },
        { text: 'ATR- At Risk', value: 'ATR- At Risk' },
        { text: 'RA- Required Air Freight', value: 'RA- Required Air Freight' },
        { text: 'AA- Air Freight Approved', value: 'AA- Air Freight Approved' },
        { text: 'LTE- Cargo Ready Late', value: 'LTE- Cargo Ready Late' },
        { text: 'NRN- No Reply Received - Needs Assistance', value: 'NRN- No Reply Received - Needs Assistance' },
        { text: 'PND- Pending', value: 'PND- Pending' },
        { text: 'RP- Partial Order', value: 'RP- Partial Order' },
        { text: 'NRR- No Reply Received', value: 'NRR- No Reply Received' },
        { text: 'CNR- Cargo Not Ready', value: 'CNR- Cargo Not Ready' },
        { text: 'CNF- Cargo Confirmed', value: 'CNF- Cargo Confirmed' },
        { text: 'CON- Cargo Confirmed On Time', value: 'CON- Cargo Confirmed On Time' },
        { text: 'GR- Goods Received', value: 'GR- Goods Received' },
        { text: 'EDOB- Delivering Prior To DD', value: 'EDOB- Delivering Prior To DD' },
        { text: 'CAN- Cancelled', value: 'CAN- Cancelled' },
        { text: 'SVC- Service Only', value: 'SVC- Service Only' },
        { text: 'DIR- Direct Delivery', value: 'DIR- Direct Delivery' }
    ];

    /** Data-source for cruise order Item auto-complete */
    itemOptionsSource: Array<string>;

    /** Data-source for cruise order Item auto-complete after filtered */
    filteredItemOptionsSource: Array<string>;

    constructor(protected _translateService: TranslateService) {
        super(_translateService);
    }

    ngOnInit() {
       
    }

    ngOnChanges(): void {
        // Initialize data-source for cruise order Item auto-complete
        // clone object
        this.itemOptionsSource = Object.assign([], this.itemOptions);

        // Add selected options if not in current source
        if (!StringHelper.isNullOrEmpty(this.model) && !StringHelper.isNullOrEmpty(this.model.cruiseOrderLineItems)) {
            this.model.cruiseOrderLineItems
                .filter(x => !this.itemOptions.some(y => y === x))
                .forEach(x => {
                    this.itemOptionsSource.push(x);
                });
        }
        this.filteredItemOptionsSource = this.itemOptionsSource;
    }

    /** Check whether current cruise order Item option is selected */
    isItemSelected(value) {
        return this.model.cruiseOrderLineItems.some(item => item === value);
    }

    /** Handler for item options auto-complete filter changed */
    itemsFilterChange(value: string) {
        if (value.length >= 1) {
            this.filteredItemOptionsSource = this.itemOptionsSource.filter((s) =>
                s.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        } else {
            this.filteredItemOptionsSource = this.itemOptionsSource;
        }
    }

    /** As clicking on button Save in Add mode */
    onAddClick() {
        FormHelper.ValidateAllFields(this.mainForm);
        if (!this.mainForm.valid) {
            return;
        }

        this.model.extendedData = JSON.stringify(this.model.cruiseOrderLineItems);
        this.add.emit(this.model);
    }

    /** As clicking on button Save in Edit mode */
    onEditClick() {
        FormHelper.ValidateAllFields(this.mainForm);
        if (!this.mainForm.valid) {
            return;
        }
        
        this.model.extendedData = JSON.stringify(this.model.cruiseOrderLineItems);
        this.edit.emit(this.model);
    }
}
