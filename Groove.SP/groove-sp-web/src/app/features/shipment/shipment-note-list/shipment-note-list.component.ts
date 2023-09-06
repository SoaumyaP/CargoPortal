import { Component, Input, OnChanges, OnDestroy, OnInit } from '@angular/core';
import { forkJoin } from 'rxjs';
import { Subscription } from 'rxjs';
import { map } from 'rxjs/operators';
import { StringHelper } from 'src/app/core';
import { DateHelper } from 'src/app/core/helpers/date.helper';
import { GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { NoteModel } from 'src/app/core/models/note.model';
import { POLineItemModel } from 'src/app/core/models/po-lineitem.model';
import { NoteListComponent } from 'src/app/core/notes/note-list.component';
import { NoteService } from 'src/app/core/notes/note.service';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { ShipmentNoteModel } from '../models/shipment-note.model';
import { ShipmentTrackingService } from '../shipment-tracking/shipment-tracking.service';

@Component({
    selector: 'app-shipment-note-list',
    templateUrl: './shipment-note-list.component.html',
    styleUrls: ['./shipment-note-list.component.scss']
})
export class ShipmentNoteListComponent extends NoteListComponent implements OnInit, OnChanges, OnDestroy {

    /** Shipment id which notes linking */
    @Input()
    shipmentId: number;

    /** List of cargo detail of shipment */
    @Input()
    cargoDetail: any[];

    /** Data for grid of notes */
    noteList: NoteModel[] = [];

    /** Data for note popup */
    noteDetails: ShipmentNoteModel;

    /** Available options for PO Item auto-complete */
    poItemOptions: Array<POLineItemModel>;

    /** Register all subscriptions then un-subscript at the end */
    private _subscriptions: Array<Subscription> = [];

    constructor(
        public notification: NotificationPopup,
        protected _shipmentTrackingService: ShipmentTrackingService,
        protected _noteService: NoteService,
        private _gaService: GoogleAnalyticsService) {
        super(notification, _noteService);
    }

    ngOnInit() {
        // Map to correct data model for shipment notes
        // Convert from note model to shipment's

        var noteObs$ = this._shipmentTrackingService.getNotes(this.shipmentId)
            .pipe(
                map((res:any) => {
                    return res.map(x => {
                        const newShipmentNoteModel = new ShipmentNoteModel();
                        newShipmentNoteModel.MapFrom(x);
                        return newShipmentNoteModel;
                    })
                })
            )

        var masterNote$ = this._shipmentTrackingService.getMasterNotes(this.shipmentId)
            .pipe(
                map((res: any) => {
                    return res.map(x => {
                        const newShipmentNoteModel =  new ShipmentNoteModel();
                        newShipmentNoteModel.MapFromMasterNote(x);
                        return newShipmentNoteModel;
                    })
                })
            )

        forkJoin([noteObs$, masterNote$]).subscribe(
            (note) => {
                this.noteList = note[0].concat(note[1]);
            });
    }

    ngOnChanges() {
        // Map to correct data model for PO Items options
        this.poItemOptions = new Array<POLineItemModel>();
        const map = new Map();
        for (const item of this.cargoDetail) {
            if(item.poLineItemId && !map.has(item.poLineItemId)) {
                map.set(item.poLineItemId, true);
                this.poItemOptions.push(new POLineItemModel(item.purchaseOrderId, item.customerPONumber, item.poLineItemId, item.productCode));
            }
        }
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
        this.noteDetails = new ShipmentNoteModel(this.currentUser.name);
        this.onAddNoteBase();
    }

    /** Handler as user clicking on Edit button */
    onEditNoteClick(note: ShipmentNoteModel) {
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
                    this._gaService.emitAction('Delete Dialog', GAEventCategory.Shipment);
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
    onNoteAdded(note: ShipmentNoteModel) {
        this.notePopupOpened = false;
        note.shipmentId = this.shipmentId;
        note.owner = this.currentUser.name;

        const sub = this._noteService
            .createNote(DateHelper.formatDate(note))
            .subscribe(
                (newNoteModel) => {
                    this.notification.showSuccessPopup(
                        'save.sucessNotification',
                        'label.message'
                    );
                    this._gaService.emitAction('Add Dialog', GAEventCategory.Shipment);

                    const newShipmentNoteModel = new ShipmentNoteModel();
                    newShipmentNoteModel.MapFrom(newNoteModel);
                    this.noteList.push(newShipmentNoteModel);
                    this.noteList = Object.assign([], this.noteList);
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
    onNoteEdited(note: ShipmentNoteModel) {
        this.notePopupOpened = false;

        this._noteService
            .updateNote(note.id, note)
            .subscribe(
                (data) => {
                    this.notification.showSuccessPopup(
                        'save.sucessNotification',
                        'label.message'
                    );

                    this._gaService.emitAction('Edit Dialog', GAEventCategory.Shipment);

                    this.noteList.forEach((el, i) => {
                        if (el.id === note.id) {
                            this.noteList[i] = note;
                        }
                    });
                }
            );
    }

    /** Handler as note popup opened */
    openNotePopup(note: ShipmentNoteModel) {
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
