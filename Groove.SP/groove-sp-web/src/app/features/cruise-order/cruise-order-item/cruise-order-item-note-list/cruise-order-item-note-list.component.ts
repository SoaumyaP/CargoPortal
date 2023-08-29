import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { IntegrationData } from 'src/app/core/models/forms/integration-data';
import { NoteModel } from 'src/app/core/models/note.model';
import { NoteListComponent } from 'src/app/core/notes/note-list.component';
import { NoteService } from 'src/app/core/notes/note.service';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { CruiseOrderDetailService } from '../../cruise-order-detail/cruise-order-detail.service';
import { CruiseOrderItemService } from '../cruise-order-item.service';
export interface RemoveCruiseOrderItemNoteEventModel {
  note: NoteModel,
  itemId: number,
  latestNote: string
}
export interface EditCruiseOrderItemNoteEventModel {
  note: NoteModel,
  itemId: number,
  isLatestNote: boolean
}
@Component({
  selector: 'app-cruise-order-item-note-list',
  templateUrl: './cruise-order-item-note-list.component.html',
  styleUrls: ['./cruise-order-item-note-list.component.scss']
})
export class CruiseOrderItemNoteListComponent extends NoteListComponent implements OnInit, OnDestroy {

  @Input() itemId: number;
  @Input() line: number;
  @Input() loadingGrids: {};
  @Input() rowIndex: number;
  @Output() editNoteEvent = new EventEmitter<EditCruiseOrderItemNoteEventModel>();
  @Output() removeNoteEvent = new EventEmitter<RemoveCruiseOrderItemNoteEventModel>();

  noteList: NoteModel[];

  // Store all subscriptions, then should un-subscribe at the end
  private _subscriptions: Array<Subscription> = [];


  constructor(public notification: NotificationPopup,
    protected _noteService: NoteService,
    private _cruiseOrderItemService: CruiseOrderItemService,
    private _cruiseOrderDetailService: CruiseOrderDetailService) {
    super(notification, _noteService);
  }

  ngOnInit() {
    this.loadInitData();
    this._registerEventHandlers();
  }

  loadInitData() {
    this.loadingGrids[this.rowIndex] = true;
    this._cruiseOrderItemService.getNotes(this.itemId).subscribe(data => {
      this.noteList = data;
      delete this.loadingGrids[this.rowIndex];
    });
  }

  private _registerEventHandlers() {
    let sub1 = this._cruiseOrderDetailService.integration$.pipe(
      filter((eventContent: IntegrationData) =>
        eventContent.name === '[cruise-order-item]onDialogUpdated'
      )).subscribe((eventContent: IntegrationData) => {
        if (eventContent.content['lineItems'].includes(this.line)) {
          this.loadInitData();
        }
      });
    this._subscriptions.push(sub1);
  }

  /** Call back to check whether current note can be editable */
  canEditNote = (createdBy: string): boolean => {
    return this.currentUser.isInternal || (this.currentUser.username === createdBy);
  }

  /** Fired when the user clicks the edit button */
  onEditNoteClick(dataItem: NoteModel) {
    const latestDialog = this.noteList[0];
    this.editNoteEvent.emit({ note: dataItem, itemId: this.itemId, isLatestNote: latestDialog.id === dataItem.id });
  }

  onDeleteNoteClick(dataItem: NoteModel) {
    const confirmDlg = this.notification.showConfirmationDialog('msg.deleteNoteConfirm', 'label.message');
    confirmDlg.result.subscribe(
      (result: any) => {
        if (result.value) {
          const currentNoteList = this.noteList.filter(x => x.id !== dataItem.id);
          let latestNote = "";
          if (currentNoteList && currentNoteList.length > 0) {
            latestNote = currentNoteList[0].category;
          }
          this.removeNoteEvent.emit({ note: dataItem, itemId: this.itemId, latestNote });
        }
    });
  }

  ngOnDestroy(): void {
    this._subscriptions.map((x) => x.unsubscribe());
  }
}
