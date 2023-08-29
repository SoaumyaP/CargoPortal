import { Component, NgZone, OnDestroy, OnInit, Renderer2 } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ColumnSetting, EventCodeStatus, ListComponent } from 'src/app/core';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { MasterEventListService } from './master-event-list.service';
import { Location } from '@angular/common';
import { faPencilAlt, faPlus, faPowerOff, faSortAmountUp } from '@fortawesome/free-solid-svg-icons';
import { fromEvent, Subscription } from 'rxjs';
import { tap } from 'rxjs/operators';
import { closest, tableRow } from 'src/app/core/models/constants/app-constants';
import { FormMode } from 'src/app/core/models/enums/enums';
import { EventCodeModel } from '../models/master-event.model';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { DataStateChangeEvent, GridDataResult, RowClassArgs } from '@progress/kendo-angular-grid';

@Component({
  selector: 'app-master-event-list',
  templateUrl: './master-event-list.component.html',
  styleUrls: ['./master-event-list.component.scss']
})
export class MasterEventListComponent extends ListComponent implements OnInit, OnDestroy {

  listName = "events"

  faPlus = faPlus;
  faPencilAlt = faPencilAlt;
  faPowerOff = faPowerOff;
  faSort = faSortAmountUp;

  readonly AppPermissions = AppPermissions;
  isOpenEventDialog: boolean = false;
  formMode: string;
  eventCodeStatus = EventCodeStatus;
  isSortMode: boolean = false;
  model: EventCodeModel;

  private acpTimeout: any;
  private gridBindingTimeout: any;
  private dragDropSubscription: Subscription;

  defaultColumns: ColumnSetting[] = [
    {
      field: 'activityCode',
      title: 'label.eventCode',
      filter: 'text',
      width: '10%',
      sortable: true
    },
    {
      field: 'activityDescription',
      title: 'label.eventDescription',
      filter: 'text',
      width: '23%',
      sortable: true
    },
    {
      field: 'activityTypeDescription',
      title: 'label.eventType',
      filter: 'text',
      width: '23%',
      sortable: true
    },
    {
      field: 'locationRequired',
      title: 'label.locationRequired',
      filter: 'text',
      width: '12%',
      sortable: true
    },
    {
      field: 'remarkRequired',
      title: 'label.remarkRequired',
      filter: 'text',
      width: '12%',
      sortable: true
    },
    {
      field: 'statusName',
      title: 'label.status',
      filter: 'text',
      width: '10%',
      sortable: true
    },
    {
      field: 'action',
      title: 'label.action',
      filter: 'text',
      width: '10%',
      sortable: false
    }
  ];

  columns = this.defaultColumns;

  gridDataRes: GridDataResult;

  constructor(
    public service: MasterEventListService,
    public notification: NotificationPopup,
    private zone: NgZone,
    private renderer: Renderer2,
    route: ActivatedRoute,
    location: Location) {
    super(service, route, location);
  }

  ngOnInit() {
    super.ngOnInit();
  }

  updateSequence(): void {
    this.service.updateSortSequences(this.gridDataRes.data).subscribe(
      () => {
        this.notification.showSuccessPopup("save.sucessNotification", "label.listOfMasterEvents");
        this.cancelSortMode();
      },
      (err) => this.notification.showErrorPopup('save.failureNotification', "label.listOfMasterEvents")
    );
  }

  onAddEventClick(): void {
    this.formMode = FormMode.Add;
    this.isOpenEventDialog = true;
  }

  onCloseEventDialog() {
    this.isOpenEventDialog = false;
  }

  onSaveDialogSuccessfully() {
    this.isOpenEventDialog = false;
    this.ngOnInit();
  }

  onEditEventClick(dataItem): void {
    this.formMode = FormMode.Edit;
    this.isOpenEventDialog = true;
    this.model = { ...dataItem };
    return;
  }

  updateStatus(activityCode, status: EventCodeStatus): void {
    this.service.updateStatus(activityCode, status).subscribe(
      () => {
        this.notification.showSuccessPopup("save.sucessNotification", "label.listOfMasterEvents")
        this.service.dataStateChange(<DataStateChangeEvent>this.service.state);
      },
      (err) =>
        this.notification.showErrorPopup('save.failureNotification', "label.listOfMasterEvents")
    );
  }

  OnSortEventClick() {
    this.view = this.view.pipe(
      tap((x) => this.gridDataRes = x),
      tap(() => this.invokeDragDrop('.k-event-grid.k-grid tr'))
    );
    this.isSortMode = true;
    this.columns = this.columns.filter(x => x.field !== 'action');
    this.service.enableSortMode();
  }

  onCancelSortModeClick() {
    const confirmDlg = this.notification.showConfirmationDialog('edit.cancelConfirmation', 'label.listOfMasterEvents');

    confirmDlg.result.subscribe(
      (result: any) => {
        if (result.value) {
          this.cancelSortMode();
        }
      });
  }

  cancelSortMode() {
    this.isSortMode = false;
    this.clearDragDrop('.k-event-grid.k-grid tr');

    this.columns = this.defaultColumns;
    this.ngOnInit();
  }

  /**
   * Trigger after the item has been dragged
   * */
  private onDragEnd(): void {

    // TODO: update sort sequence

    for (let index = 0; index < this.gridDataRes.data.length; index++) {
      this.gridDataRes.data[index].sortSequence = index + 1;
    }
  }

  public rowCallback(args: RowClassArgs) {
    return {
      'dragging': args.dataItem.dragging
    };
  }

  //#region drag/drop

  invokeDragDrop(selector: string) {
    clearTimeout(this.gridBindingTimeout);
    this.acpTimeout = setTimeout(() => {
      this.dragDropSubscriptionDestroy();
      this.dragDropSubscription = this.handleDragAndDrop(selector);
    }, 1);
  }

  clearDragDrop(selector: string) {
    const tableRows = Array.from(document.querySelectorAll(selector));
    tableRows.forEach(item => {
      this.renderer.setAttribute(item, 'draggable', 'false');
    });
    this.dragDropSubscriptionDestroy();
  }

  private handleDragAndDrop(selector: string): Subscription {
    const sub = new Subscription(() => { });
    let draggedItemIndex;

    const tableRows = Array.from(document.querySelectorAll(selector));
    tableRows.forEach(item => {
      this.renderer.setAttribute(item, 'draggable', 'true');

      sub.add(
        fromEvent<DragEvent>(item, 'dragstart').pipe(
          tap(
            ({ dataTransfer }) => {
              try {
                const dragImgEl = document.createElement('span');
                dragImgEl.setAttribute('style', 'position: absolute; display: block; top: 0; left: 0; width: 0; height: 0;');
                document.body.appendChild(dragImgEl);
                dataTransfer.setDragImage(dragImgEl, 0, 0);
              } catch (err) {
                // IE doesn't support setDragImage
              }
              try {
                // Firefox won't drag without setting data
                dataTransfer.setData('application/json', '');
              } catch (err) {
                // IE doesn't support MIME types in setData
              }
            })
        ).subscribe(({ target }) => {
          const row: HTMLTableRowElement = <HTMLTableRowElement>target;
          draggedItemIndex = row.rowIndex;
          const dataItem = this.gridDataRes.data[draggedItemIndex];
          dataItem.dragging = true;
        })
      );

      sub.add(
        fromEvent(item, 'dragover').subscribe((e: any) => {
          e.preventDefault();
          const dataItem = this.gridDataRes.data.splice(draggedItemIndex, 1)[0];
          const dropIndex = closest(e.target, tableRow).rowIndex;
          draggedItemIndex = dropIndex;

          this.zone.run(() =>
            this.gridDataRes.data.splice(dropIndex, 0, dataItem)
          );
        })
      );

      sub.add(
        fromEvent(item, 'dragend').subscribe((e: any) => {
          e.preventDefault();
          const dataItem = this.gridDataRes.data[draggedItemIndex];
          dataItem.dragging = false;
          this.onDragEnd();
        })
      );

    });

    return sub;
  }

  private dragDropSubscriptionDestroy() {
    if (this.dragDropSubscription) {
      this.dragDropSubscription.unsubscribe();
    }
  }

  //#endregion

  ngOnDestroy(): void {
    this.dragDropSubscriptionDestroy();
  }
}