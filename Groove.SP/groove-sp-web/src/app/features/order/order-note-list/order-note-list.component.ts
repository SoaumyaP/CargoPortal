import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { DateHelper } from 'src/app/core/helpers/date.helper';
import { GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { NoteModel } from 'src/app/core/models/note.model';
import { NoteListComponent } from 'src/app/core/notes/note-list.component';
import { NoteService } from 'src/app/core/notes/note.service';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { OrderNoteModel } from '../models/order-note.model';

@Component({
    selector: 'app-order-note-list',
    templateUrl: './order-note-list.component.html',
    styleUrls: ['./order-note-list.component.scss']
})

export class OrderNoteListComponent extends NoteListComponent implements OnInit, OnDestroy {

    /** Data for the grid of notes */
    @Input()
    noteList: NoteModel[];

    /** Available options for PO Item auto-complete */
    @Input()
    poItems: any[];

    /** Purchase order id which notes linking */
    @Input()
    poId: number;

    /** Data for note popup */
    noteDetails: OrderNoteModel;

    /** Available options for PO Item auto-complete */
    poItemOptions: Array<string>;

    /** Register all subscriptions then un-subscript at the end */
    private _subscriptions: Array<Subscription> = [];

    constructor(
        public notification: NotificationPopup,
        protected _noteService: NoteService,
        private _gaService: GoogleAnalyticsService) {
        super(notification, _noteService);
    }

    ngOnInit() {
        // Initialize available options
        this.poItemOptions = this.poItems.map(i => i.productCode);
    }

    ngOnDestroy() {
        this._subscriptions.forEach(i => {
            i.unsubscribe();
        });
    }

    /** Call back to check whether current note can be editable */
    canEditNote = (createdBy: string): boolean => {
        return this.currentUser.isInternal || (this.currentUser.username === createdBy);
    }

    /** Handler as user clicking on Add button */
    onAddNoteClick() {
        this.noteDetails = new OrderNoteModel(this.currentUser.name);
        this.onAddNoteBase();
    }

    /** Handler as user clicking on Edit button */
    onEditNoteClick(note: OrderNoteModel) {
        this.noteDetails = Object.assign({}, note);
        this.onEditNoteBase();
    }

    /* Handler as user clicking on Delete button */
    onDeleteNoteClick(noteId: number) {

        const callback = (): void => {
            const sub = this._noteService.deleteNote(noteId).subscribe(
                data => {
                    this.noteList = this.noteList.filter((el) => {
                        return el.id !== noteId;
                    });
                    this.notification.showSuccessPopup(
                        'msg.deleteNoteSuccessfully',
                        'label.message'
                    );
                    this._gaService.emitEvent('delete_dialog', GAEventCategory.PurchaseOrder, 'Delete Dialog');

                },
                error => {
                    this.notification.showErrorPopup(
                        'save.failureNotification',
                        'label.message'
                    );
                }
            );
            this._subscriptions.push(sub);
        };

        this.onDeleteNoteBase(callback);
    }

    /** Handler as user adding new note */
    onNoteAdded(note: OrderNoteModel) {

        this.notePopupOpened = false;
        note.purchaseOrderId = this.poId;

        const sub = this._noteService
            .createNote(DateHelper.formatDate(note))
            .subscribe(
                (newNoteModel) => {
                    this.notification.showSuccessPopup(
                        'save.sucessNotification',
                        'label.message'
                    );
                    const newOrderNoteModel = new OrderNoteModel();
                    newOrderNoteModel.MapFrom(newNoteModel);
                    this.noteList.push(newOrderNoteModel);
                    this.noteList = Object.assign([], this.noteList);
                    this._gaService.emitEvent('add_dialog', GAEventCategory.PurchaseOrder, 'Add Dialog');
                },
                (error) => {
                    this.notification.showErrorPopup(
                        'save.failureNotification',
                        'label.message'
                    );
                }
            );
        this._subscriptions.push(sub);
    }

    /** Handler as user editing a note */
    onNoteEdited(note: NoteModel) {
        this.notePopupOpened = false;

        const sub = this._noteService
            .updateNote(note.id, note)
            .subscribe(
                (data) => {
                    this.notification.showSuccessPopup(
                        'save.sucessNotification',
                        'label.message'
                    );

                    this.noteList.forEach((el, i) => {
                        if (el.id === note.id) {
                            this.noteList[i] = note;
                        }
                    });

                    this._gaService.emitEvent('edit_dialog', GAEventCategory.PurchaseOrder, 'Edit Dialog');

                }
            );
        this._subscriptions.push(sub);
    }

    /** Handler as note popup opened */
    openNotePopup(note: OrderNoteModel) {
        this.onEditNoteClick(note);
        this.onViewNoteBase();
    }

    /** Handler as note popup closed */
    onNotePopupClosed() {
        this.notePopupOpened = false;
    }

}
