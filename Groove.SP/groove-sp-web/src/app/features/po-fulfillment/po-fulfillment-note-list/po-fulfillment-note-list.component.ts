import { Component, Input, OnChanges, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { DateHelper, StringHelper } from 'src/app/core';
import { GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { NoteModel } from 'src/app/core/models/note.model';
import { POLineItemModel } from 'src/app/core/models/po-lineitem.model';
import { NoteListComponent } from 'src/app/core/notes/note-list.component';
import { NoteService } from 'src/app/core/notes/note.service';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { POFulfillmentNoteModel } from '../models/po-fulfillment-note.model';
import { POFulfillmentFormService } from '../po-fulfillment-form/po-fulfillment-form.service';

@Component({
  selector: 'app-po-fulfillment-note-list',
  templateUrl: './po-fulfillment-note-list.component.html',
  styleUrls: ['./po-fulfillment-note-list.component.scss']
})
export class PoFulfillmentNoteListComponent extends NoteListComponent implements OnInit, OnChanges, OnDestroy {

    /** PO fulfillment id which notes linking */
    @Input()
    poFulfillmentId: number;

    /** List of available orders of booking */
    @Input()
    poFulfillmentOrders: any[];

    /** Data for grid of notes */
    @Input()
    noteList: NoteModel[] = [];

    /** Data for note popup */
    noteDetails: POFulfillmentNoteModel;

    /** Available options for PO Item auto-complete */
    poItemOptions: Array<POLineItemModel>;

    /** Register all subscriptions then un-subscript at the end */
    private _subscriptions: Array<Subscription> = [];

    constructor(
        public notification: NotificationPopup,
        protected _poFulfillmentService: POFulfillmentFormService,
        protected _noteService: NoteService,
        private _gaService : GoogleAnalyticsService) {
        super(notification, _noteService);
    }

    ngOnInit() {
    }

    ngOnChanges() {
        // Map to correct data model for PO Items options
        this.poItemOptions = this.poFulfillmentOrders.map(i => new POLineItemModel(i.purchaseOrderId, i.customerPONumber, i.poLineItemId, i.productCode));
    }

    ngOnDestroy() {
        this._subscriptions.forEach(i => {
            i.unsubscribe();
        });
    }

    /** Callback to check whether current note can be editable */
    canEditNote = (createdBy: string): boolean => {
        return this.currentUser.isInternal || (this.currentUser.username === createdBy);
    }

    /** Handler as user clicking on Add button */
    onAddNoteClick() {
        this.noteDetails = new POFulfillmentNoteModel(this.currentUser.name);
        this.onAddNoteBase();
    }

    /** Handler as user clicking on Edit button */
    onEditNoteClick(note: POFulfillmentNoteModel) {
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
                    this._gaService.emitEvent('delete_dialog',  GAEventCategory.POBooking, 'Delete Dialog');
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
    onNoteAdded(note: POFulfillmentNoteModel) {
        this.notePopupOpened = false;
        note.poFulfillmentId = this.poFulfillmentId;
        note.owner = this.currentUser.name;

        const sub = this._noteService
            .createNote(DateHelper.formatDate(note))
            .subscribe(
                (newNoteModel) => {
                    this.notification.showSuccessPopup(
                        'save.sucessNotification',
                        'label.message'
                    );

                    const newPOFulfillmentNoteModel = new POFulfillmentNoteModel();
                    newPOFulfillmentNoteModel.MapFrom(newNoteModel);
                    this.noteList.push(newPOFulfillmentNoteModel);
                    this.noteList = Object.assign([], this.noteList);
                    this._gaService.emitEvent('add_dialog',  GAEventCategory.POBooking, 'Add Dialog');
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
    onNoteEdited(note: POFulfillmentNoteModel) {
        this.notePopupOpened = false;

        this._noteService
            .updateNote(note.id,
                note
            )
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

                    this._gaService.emitEvent('edit_dialog',  GAEventCategory.POBooking, 'Edit Dialog');
                }
            );
    }

    /** Handler as note popup opened */
    openNotePopup(note: POFulfillmentNoteModel) {
        this.onEditNoteClick(note);
        this.onViewNoteBase();
    }

    /** Handler as note popup closed */
    onNotePopupClosed() {
        this.notePopupOpened = false;
    }

    /** Call-back to display text of PO Items on the grid */
    displayPOItemText(data: Array<POLineItemModel>) {
        if (!StringHelper.isNullOrEmpty(data)) {
            return data.map(x => x.value).join(', ');
        } else {
            return '';
        }
    }

}
