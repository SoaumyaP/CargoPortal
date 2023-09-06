import { EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';

export class NotePopupComponent {

    /** Define whether note popup should open */
    @Input()
    popupOpened: boolean = false;

    /** Define mode of popup */
    @Input()
    set formMode(mode: string) {
        this.isViewMode = mode === 'view';
        this.isEditMode = mode === 'edit';
        this.isAddMode = mode === 'add';
    }

    /** Execute as the popup closed */
    @Output()
    close: EventEmitter<any> = new EventEmitter<any>();

    /** Execute as adding new note */
    @Output()
    add: EventEmitter<any> = new EventEmitter<any>();

    /** Execute as adding edit note */
    @Output()
    edit: EventEmitter<any> = new EventEmitter<any>();

    @ViewChild('mainForm', { static: false }) currentForm: NgForm;

    /** Options for category selection */
    categoryOptions: { text: string, value: string }[] = [{ text: 'General', value: 'General' }];

    // Some properties to detect current mode
    isViewMode: boolean;
    isEditMode: boolean;
    isAddMode: boolean;

    constructor( protected _translateService: TranslateService) {
    }

    /** Based handlers as the popup closed */
    onFormClosed() {
        this.popupOpened = false;
        this.close.emit();
    }

    /** To get title of the popup */
    get title() {
        return this.isAddMode ? 'label.addNewMessage' : 'label.editMessage';
    }

}
