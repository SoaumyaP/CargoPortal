import { Component, Input, OnChanges, OnDestroy, OnInit, SimpleChange, SimpleChanges } from '@angular/core';
import { Subscription } from 'rxjs';
import { DateHelper } from 'src/app/core';
import { GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { NoteListComponent } from 'src/app/core/notes/note-list.component';
import { NoteService } from 'src/app/core/notes/note.service';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { POFulfillmentFormService } from '../../po-fulfillment/po-fulfillment-form/po-fulfillment-form.service';
import { BulkFulfillmentNoteItemModel } from '../models/bulk-fulfillment-note-item.model';
import { BulkFulfillmentNoteModel } from '../models/bulk-fulfillment-note.model';
import { BulkFulfillmentModel, BulkFulfillmentOrderModel } from '../models/bulk-fulfillment.model';

@Component({
  selector: 'app-bulk-fulfillment-dialog',
  templateUrl: './bulk-fulfillment-dialog.component.html',
  styleUrls: ['./bulk-fulfillment-dialog.component.scss']
})
export class BulkFulfillmentDialogComponent extends NoteListComponent implements OnInit, OnChanges, OnDestroy {
  /** Data for grid of notes */
  @Input() noteList: BulkFulfillmentNoteModel[] = [];
  @Input() bookingModel: BulkFulfillmentModel;
  @Input() fulfillmentOrders: BulkFulfillmentOrderModel[];

  noteDetails: BulkFulfillmentNoteModel;

  itemDropdown: Array<BulkFulfillmentNoteItemModel> = [];

  /** Register all subscriptions then un-subscript at the end */
  private _subscriptions: Array<Subscription> = [];

  constructor(
    public notification: NotificationPopup,
    protected _poFulfillmentService: POFulfillmentFormService,
    protected _noteService: NoteService,
    private _gaService: GoogleAnalyticsService) {
    super(notification, _noteService);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.bookingModel?.currentValue) {
      for (let item of this.bookingModel.orders) {
        this.itemDropdown.push(
          {
            poFulfillmentOrderId: item.id,
            productCode: item.productCode,
            productName: item.productName,
            value: item.productCode ? `${item.productCode}-${item.productName}` : item.productName
          }
        )
      }
    }
  }

  ngOnInit() {

  }

  /** Callback to check whether current note can be editable */
  canEditNote = (createdBy: string): boolean => {
    return this.currentUser.isInternal || (this.currentUser.username === createdBy);
  }

  /** Handler as user clicking on Add button */
  onAddNoteClick() {
    this.noteDetails = new BulkFulfillmentNoteModel(this.currentUser.name);
    this.onAddNoteBase();
  }

  /** Handler as user clicking on Edit button */
  onEditNoteClick(note: BulkFulfillmentNoteModel) {
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
          this._gaService.emitAction('Delete Dialog', GAEventCategory.BulkBooking);

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
  onNoteAdded(note: BulkFulfillmentNoteModel) {
    this.notePopupOpened = false;
    note.poFulfillmentId = this.bookingModel.id;
    note.owner = this.currentUser.name;
    const sub = this._noteService.createNote(DateHelper.formatDate(note)).subscribe(
      (newNoteModel) => {
        this.notification.showSuccessPopup(
          'save.sucessNotification',
          'label.message'
        );
        this._gaService.emitAction('Add Dialog', GAEventCategory.BulkBooking);

        const newBulkFulfillmentNoteModel = new BulkFulfillmentNoteModel();
        newBulkFulfillmentNoteModel.MapFrom(newNoteModel);
        this.noteList.push(newBulkFulfillmentNoteModel);
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
  onNoteEdited(note: BulkFulfillmentNoteModel) {
    this.notePopupOpened = false;

    this._noteService
      .updateNote(note.id, note)
      .subscribe(
        (data) => {
          this.notification.showSuccessPopup(
            'save.sucessNotification',
            'label.message'
          );

          this._gaService.emitAction('Edit Dialog', GAEventCategory.BulkBooking);

          this.noteList.forEach((el, i) => {
            if (el.id === note.id) {
              this.noteList[i] = note;
            }
          });
        }
      );
  }

  /** Handler as note popup opened */
  openNotePopup(note: BulkFulfillmentNoteModel) {
    this.onEditNoteClick(note);
    this.onViewNoteBase();
  }

  /** Handler as note popup closed */
  onNotePopupClosed() {
    this.notePopupOpened = false;
  }

  displayItemText(itemsSelected: Array<BulkFulfillmentNoteItemModel>) {
    if (itemsSelected) {
      return itemsSelected.map(x => x.value).join(', ');
    } else {
      return '';
    }
  }

  ngOnDestroy() {
    this._subscriptions.forEach(i => { i.unsubscribe(); });
  }
}
