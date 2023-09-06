import { Component, OnInit, Input, Output, EventEmitter, NgZone, Renderer2 } from '@angular/core';
import { Subscription, of, fromEvent } from 'rxjs';
import { faAngleDoubleLeft, faAngleRight, faMinusCircle, faTimes, faCaretUp } from '@fortawesome/free-solid-svg-icons';
import { CheckableSettings, TreeItemLookup } from '@progress/kendo-angular-treeview';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { RowClassArgs } from '@progress/kendo-angular-grid';
import { tap } from 'rxjs/operators';
import { BulkFulfillmentOrderModel } from '../../models/bulk-fulfillment.model';
import { TranslateService } from '@ngx-translate/core';
import { StringHelper } from 'src/app/core';
const tableRow = node => node.tagName.toLowerCase() === 'tr';
const closest = (node, predicate) => {
  while (node && !predicate(node)) {
    node = node.parentNode;
  }

  return node;
};
@Component({
  selector: 'app-load-cargo-dialog',
  templateUrl: './load-cargo-dialog.component.html',
  styleUrls: ['./load-cargo-dialog.component.scss']
})
export class LoadCargoDialogComponent implements OnInit {
  @Input() public popupOpened: boolean = false;
  @Input() public loadNumber: string = '';
  @Input('orders') public poffOrders: Array<BulkFulfillmentOrderModel> = [];
  @Output() close: EventEmitter<any> = new EventEmitter();
  @Output() save: EventEmitter<any> = new EventEmitter();

  faAngleDoubleLeft = faAngleDoubleLeft;
  faAngleRight = faAngleRight;
  faMinusCircle = faMinusCircle;
  faTimes = faTimes;
  faCaretUp = faCaretUp;
  stringHelper = StringHelper;

  loadedOrders: Array<BulkFulfillmentOrderModel> = [];
  unloadedOrders: Array<BulkFulfillmentOrderModel> = [];

  /**BulkFulfillmentOrders data source for Kendo-TreeView. */
  treeViewDataSource: any[] = [];

  // tree view declarations
  public get checkableSettings(): CheckableSettings {
    return {
      checkChildren: false,
      checkParents: true,
      enabled: true,
      mode: 'multiple',
      checkOnClick: false
    };
  }

  checkedKeys: string[] = [];
  isCheckAll: boolean = false;

  // drag row on grid
  acpTimeout: any;
  gridBindingTimeout: any;
  private loadedCargoGridSubscription: Subscription;

  constructor(private zone: NgZone, private renderer: Renderer2, public notification: NotificationPopup, public translateService: TranslateService) { }

  ngOnInit() {
    this.unloadedOrders = [...this.poffOrders];
    this.buildTree(this.unloadedOrders);
  }

  //#region tree-view handlers
  buildTree(currentList) {
    for (let i = 0; i < currentList.length; i++) {
      this.treeViewDataSource.push({
        id: currentList[i].id,
        text: `${(StringHelper.isNullOrEmpty(currentList[i].productCode) ? '' : currentList[i].productCode + ' - ') + currentList[i].productName}`,
        customIndex: `${i}`,
        customId: `${currentList[i].id}`,
        items: []
      });
    }
  }

  resetTree(currentList) {
    this.treeViewDataSource = [];
    this.checkedKeys = [];
    this.isCheckAll = false;
    this.buildTree(currentList);
  }

  hasChildren(item: any) {
    return item.items && item.items.length > 0;
  }

  fetchChildren(item: any) {
    return of(item.items);
  }

  handleChecking(itemLookup: TreeItemLookup): void {
    this.isCheckAll = false;
  }

  private onCheckAllClicked(event) {
    if (this.isCheckAll) {
      this.tickChildList(this.treeViewDataSource);
    } else {
      this.checkedKeys = [];
    }
  }

  tickChildList(currentList) {
    for (let i = 0; i < currentList.length; i++) {
      const index = this.checkedKeys.indexOf(currentList[i].customIndex);
      if (index < 0) {
        this.checkedKeys.push(currentList[i].customIndex);
      }
      // has children
      if (currentList[i].items.length > 0) {
        this.tickChildList(currentList[i].items);
      }
    }
  }

  //#endregion

  onLoadBtnClicked(event) {
    this.isCheckAll = false;
    let sequence = this.loadedOrders.length;
    for (const key of this.checkedKeys) {
      let selectedItem = this.treeViewDataSource[key];
      const selectedOrderIndex = this.unloadedOrders.findIndex(x => x.id === selectedItem.id);

      // add to loaded list
      let newLoadedCargo: any = { ...this.unloadedOrders[selectedOrderIndex] };
      newLoadedCargo.sequence = sequence + 1;
      this.loadedOrders.push(newLoadedCargo);

      // delete out of unload list
      this.unloadedOrders.splice(selectedOrderIndex, 1);

      sequence += 1;
    }
    this.resetTree(this.unloadedOrders);
    this.invokeDragDropLoadedCargos()
  }

  onUnloadAllBtnClicked(event) {
    this.loadedOrders = [];
    this.unloadedOrders = [...this.poffOrders];
    this.resetTree(this.unloadedOrders);
  }

  deleteLoadedOrder(rowIndex) {
    this.unloadedOrders.push(this.loadedOrders[rowIndex]);
    this.loadedOrders.splice(rowIndex, 1);
    this.updateLoadedCargoSequence();
    this.resetTree(this.unloadedOrders);
  }

  //#region drag drop handlers
  public rowCallback(context: RowClassArgs) {
    return {
      dragging: context.dataItem.dragging
    };
  }

  private loadedCargoGridDestroy() {
    if (this.loadedCargoGridSubscription) {
      this.loadedCargoGridSubscription.unsubscribe();
    }
  }

  invokeDragDropLoadedCargos() {
    clearTimeout(this.gridBindingTimeout);
    this.acpTimeout = setTimeout(() => {
      this.loadedCargoGridDestroy();
      this.loadedCargoGridSubscription = this.handleDragAndDrop();
    }, 100);
  }

  private handleDragAndDrop(): Subscription {
    const sub = new Subscription(() => { });
    let draggedItemIndex;

    const tableRows = Array.from(document.querySelectorAll('.loaded-cargo-items-grid.k-grid tr'));
    tableRows.forEach(item => {
      this.renderer.setAttribute(item, 'draggable', 'true');
      const dragStart = fromEvent<DragEvent>(item, 'dragstart');
      const dragOver = fromEvent(item, 'dragover');
      const dragEnd = fromEvent(item, 'dragend');

      sub.add(dragStart.pipe(
        tap(({ dataTransfer }) => {
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
        const dataItem = this.loadedOrders[draggedItemIndex];
        dataItem.dragging = true;
      }));

      sub.add(dragOver.subscribe((e: any) => {
        e.preventDefault();
        const dataItem = this.loadedOrders.splice(draggedItemIndex, 1)[0];
        const dropIndex = closest(e.target, tableRow).rowIndex;
        const dropItem = this.loadedOrders[dropIndex];

        draggedItemIndex = dropIndex;
        this.zone.run(() =>
          this.loadedOrders.splice(dropIndex, 0, dataItem)
        );
      }));

      sub.add(dragEnd.subscribe((e: any) => {
        e.preventDefault();
        const dataItem = this.loadedOrders[draggedItemIndex];
        dataItem.dragging = false;
        this.updateLoadedCargoSequence();
      }));
    });

    return sub;
  }

  updateLoadedCargoSequence() {
    let sequence = 1;
    this.loadedOrders.forEach(element => {
      element.sequence = sequence++;
    });
  }

  //#endregion

  onSave() {
    this.popupOpened = false;
    this.save.emit(this.loadedOrders);
  }

  onFormClosed() {
    this.popupOpened = false;
    this.close.emit();
  }

  public ngOnDestroy(): void {
    this.loadedCargoGridDestroy();
  }

  get title() {
    return this.translateService.instant('label.loadCargoTo',
      {
        loadRef: this.loadNumber
      }
    );
  }

}