import { Component, Input, OnChanges, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { StringHelper } from 'src/app/core';
import { RoutingOrderNoteModel } from 'src/app/core/models/routing-order.model';
import { NotePopupComponent } from 'src/app/core/notes/note-popup.component';

@Component({
    selector: 'app-add-routing-order-note',
    templateUrl: './add-routing-order-note.component.html',
    styleUrls: ['./add-routing-order-note.component.scss']
})
export class AddRoutingOrderNoteComponent extends NotePopupComponent  implements OnInit, OnChanges {

    /** Available options for Routing Order Item auto-complete */
    @Input()
    lineItemOptions: Array<string>;

    /** Data-source for Routing Order Item auto-complete */
    lineItemOptionsSource: Array<string>;

    /** Data-source for Routing Order Item auto-complete after filtered */
    filteredLineItemOptionsSource: Array<string>;

    /** Data for note popup */
    @Input()
    model: RoutingOrderNoteModel;

    @ViewChild('mainForm', { static: false }) mainForm: NgForm;
    
    constructor(protected _translateService: TranslateService) {
        super(_translateService);
    }

    ngOnInit() {
    }

    ngOnChanges() {

        // Initialize data-source for Routing Order Item auto-complete

        // clone object
        this.lineItemOptionsSource = Object.assign([], this.lineItemOptions);

        // Add selected options if not in current source
        if (!StringHelper.isNullOrEmpty(this.model) && !StringHelper.isNullOrEmpty(this.model.lineItems)) {
            this.model.lineItems
            .filter(x => !this.lineItemOptions.some(y => y === x))
            .forEach(x => {
                this.lineItemOptionsSource.push(x);
            });
        }
        this.filteredLineItemOptionsSource = this.lineItemOptionsSource;
    }

    /** As clicking on button Save in Add mode */
    onAddClick() {
        this.mainForm.form.markAllAsTouched();
        if (!this.mainForm.valid) {
            return;    
        }

        this.model.extendedData = JSON.stringify(this.model.lineItems);
        this.add.emit(this.model);
    }

    /** As clicking on button Save in Edit mode */
    onEditClick() {
        this.mainForm.form.markAllAsTouched();
        if (!this.mainForm.valid) {
            return;    
        }

        this.model.extendedData = JSON.stringify(this.model.lineItems);
        this.edit.emit(this.model);
    }

    /** Check whether current Routing Order Item option is selected */
    isLineItemSelected(value) {
        return this.model.lineItems.some(item => item === value);
    }

    /** Handler for Routing Order Item auto-complete filter changed */
    lineItemsFilterChange(value) {
        if (value.length >= 1) {
            this.filteredLineItemOptionsSource = this.lineItemOptionsSource.filter((s) =>
                    s.toLowerCase().indexOf(value.toLowerCase()) !== -1
                    || this.model.lineItems.indexOf(s.toLowerCase()) !== -1
            );
        } else {
            this.filteredLineItemOptionsSource = this.lineItemOptionsSource;
        }
    }
}
