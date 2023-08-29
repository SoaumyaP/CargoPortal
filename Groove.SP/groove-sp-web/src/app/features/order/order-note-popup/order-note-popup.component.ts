import { Component, Input, OnChanges, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { StringHelper } from 'src/app/core';
import { NotePopupComponent } from 'src/app/core/notes/note-popup.component';
import { OrderNoteModel } from '../models/order-note.model';

@Component({
    selector: 'app-order-note-popup',
    templateUrl: './order-note-popup.component.html',
    styleUrls: ['./order-note-popup.component.scss']
})
export class OrderNotePopupComponent extends NotePopupComponent  implements OnInit, OnChanges {

    /** Available options for PO Item auto-complete */
    @Input()
    poItemOptions: Array<string>;

    /** Data-source for PO Item auto-complete */
    poItemOptionsSource: Array<string>;

    /** Data-source for PO Item auto-complete after filtered */
    filteredPOItemOptionsSource: Array<string>;

    /** Data for note popup */
    @Input()
    model: OrderNoteModel;

    @ViewChild('mainForm', { static: false }) mainForm: NgForm;
    
    constructor(protected _translateService: TranslateService) {
        super(_translateService);
    }

    ngOnInit() {
    }

    ngOnChanges() {

        // Initialize data-source for PO Item auto-complete

        // clone object
        this.poItemOptionsSource = Object.assign([], this.poItemOptions);

        // Add selected options if not in current source
        if (!StringHelper.isNullOrEmpty(this.model) && !StringHelper.isNullOrEmpty(this.model.poItems)) {
            this.model.poItems
            .filter(x => !this.poItemOptions.some(y => y === x))
            .forEach(x => {
                this.poItemOptionsSource.push(x);
            });
        }
        this.filteredPOItemOptionsSource = this.poItemOptionsSource;
    }

    /** As clicking on button Save in Add mode */
    onAddClick() {
        this.mainForm.form.markAllAsTouched();
        if (!this.mainForm.valid) {
            return;    
        }

        this.model.extendedData = JSON.stringify(this.model.poItems);
        this.add.emit(this.model);
    }

    /** As clicking on button Save in Edit mode */
    onEditClick() {
        this.mainForm.form.markAllAsTouched();
        if (!this.mainForm.valid) {
            return;    
        }

        this.model.extendedData = JSON.stringify(this.model.poItems);
        this.edit.emit(this.model);
    }

    /** Check whether current PO Item option is selected */
    isPOItemSelected(value) {
        return this.model.poItems.some(item => item === value);
    }

    /** Handler for PO Item auto-complete filter changed */
    poItemsFilterChange(value) {
        if (value.length >= 1) {
            this.filteredPOItemOptionsSource = this.poItemOptionsSource.filter((s) =>
                    s.toLowerCase().indexOf(value.toLowerCase()) !== -1
                    || this.model.poItems.indexOf(s.toLowerCase()) !== -1
            );
        } else {
            this.filteredPOItemOptionsSource = this.poItemOptionsSource;
        }
    }
}
