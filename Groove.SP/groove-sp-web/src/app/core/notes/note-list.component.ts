import { EventEmitter, Input, OnInit, Output } from '@angular/core';
import { faEllipsisV, faPencilAlt, faPlus, faTrashAlt } from '@fortawesome/free-solid-svg-icons';
import { DATE_FORMAT } from 'src/app/core/helpers/date.helper';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { NoteService } from './note.service';

export class NoteListComponent {

    /**Define whether current note can be editable */
    @Input()
    canEditNote: (value: string) => boolean;

    /**Define whether can add new note */
    @Input()
    canAddNote: boolean;

    /**Define whether all notes will be readonly */
    @Input()
    canEditAllNotes: boolean;

    /**Current user information */
    @Input()
    currentUser: any;

    DATE_FORMAT = DATE_FORMAT;
    faPlus = faPlus;
    faEllipsisV = faEllipsisV;
    faPencilAlt = faPencilAlt;
    faTrashAlt = faTrashAlt;

    // Properties of the form component
    noteFormMode: string;
    notePopupOpened: boolean;

    constructor(public notification: NotificationPopup,
        protected _noteService: NoteService) { }

    /** To call based handler as clicking on Add button */
    onAddNoteBase() {
        this.noteFormMode = 'add';
        this.notePopupOpened = true;
    }

    /** To call based handler as clicking on Edit button */
    onEditNoteBase() {
        this.noteFormMode = 'edit';
        this.notePopupOpened = true;
    }

    /** To call based handler as clicking on Delete button */
    onDeleteNoteBase(callback: any) {
        const confirmDlg = this.notification.showConfirmationDialog('msg.deleteNoteConfirm', 'label.message');

        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                   callback();
                }
            });
    }

    /** To call based handler as note popup closed */
    onNotePopupClosedBase() {
        this.notePopupOpened = false;
    }

    /** To call based handler as note popup opened */
    onViewNoteBase() {
        this.noteFormMode = 'view';
    }

}
