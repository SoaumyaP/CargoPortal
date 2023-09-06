import { Component, OnInit, Input, Output, EventEmitter, NgZone, Renderer2 } from '@angular/core';
import { Subscription, of, fromEvent } from 'rxjs';
import { faAngleDoubleLeft, faAngleRight, faMinusCircle, faTimes, faCaretUp } from '@fortawesome/free-solid-svg-icons';
import { CheckableSettings, TreeItemLookup } from '@progress/kendo-angular-treeview';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { RowClassArgs } from '@progress/kendo-angular-grid';
import { tap } from 'rxjs/operators';
import { TranslateService } from '@ngx-translate/core';
import { StringHelper } from 'src/app/core';
import { ColumnOptionModel } from '../../models/column.option.model';
const tableRow = node => node.tagName.toLowerCase() === 'tr';
const closest = (node, predicate) => {
  while (node && !predicate(node)) {
    node = node.parentNode;
  }

  return node;
};
@Component({
  selector: 'app-column-options-dialog',
  templateUrl: './column-options-dialog.component.html',
  styleUrls: ['./column-options-dialog.component.scss']
})
export class ColumnOptionsDialogComponent implements OnInit {
  @Input() public popupOpened: boolean = false;
  @Input() public columns: Array<ColumnOptionModel> = [];
  @Output() close: EventEmitter<any> = new EventEmitter();
  @Output() save: EventEmitter<any> = new EventEmitter();

  faAngleDoubleLeft = faAngleDoubleLeft;
  faAngleRight = faAngleRight;
  faMinusCircle = faMinusCircle;
  faTimes = faTimes;
  faCaretUp = faCaretUp;
  stringHelper = StringHelper;

  selectedColumns: Array<ColumnOptionModel> = [];
  unselectedColumns: Array<ColumnOptionModel> = [];

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
  private selectedColumnsGridSubscription: Subscription;

  constructor(private zone: NgZone, private renderer: Renderer2, public notification: NotificationPopup, public translateService: TranslateService) { }

  ngOnInit() {
    this.selectedColumns = this.columns.filter(x => x.selected)?.sort((a,b) => (a.sequence > b.sequence) ? 1 : ((b.sequence > a.sequence) ? -1 : 0));
    this.unselectedColumns = this.columns?.filter(x => !x.selected);
    this.buildTree(this.unselectedColumns);
    this.invokeDragDrop();
  }

  //#region tree-view handlers
  buildTree(currentList: Array<ColumnOptionModel>) {
    for (let i = 0; i < currentList.length; i++) {
      this.treeViewDataSource.push({
        id: currentList[i].name,
        text: `${currentList[i].name}`,
        customIndex: `${i}`,
        customId: `${currentList[i].name}`,
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

  onAddColumnBtnClicked(event) {
    this.isCheckAll = false;
    let sequence = this.selectedColumns.length;
    for (const key of this.checkedKeys) {
      let selectedItem = this.treeViewDataSource[key];
      const selectedColumnIndex = this.unselectedColumns.findIndex(x => x.name === selectedItem.id);

      // add to selected column list
      let newColumn: any = this.unselectedColumns[selectedColumnIndex];
      newColumn.sequence = sequence + 1;
      this.selectedColumns.push(newColumn);

      // delete out of unselected column list
      this.unselectedColumns.splice(selectedColumnIndex, 1);

      sequence += 1;
    }
    this.resetTree(this.unselectedColumns);
    this.invokeDragDrop()
  }

  onRevertAllBtnClicked(event) {
    this.selectedColumns = [];
    this.unselectedColumns = [...this.columns];
    this.resetTree(this.unselectedColumns);
  }

  deleteSelectedColumn(rowIndex) {
    this.unselectedColumns.push(this.selectedColumns[rowIndex]);
    this.selectedColumns.splice(rowIndex, 1);
    this.updateColumnSequence();
    this.resetTree(this.unselectedColumns);
  }

  //#region drag drop handlers
  public rowCallback(context: RowClassArgs) {
    return {
      dragging: context.dataItem.dragging
    };
  }

  private selectedColumnsGridDestroy() {
    if (this.selectedColumnsGridSubscription) {
      this.selectedColumnsGridSubscription.unsubscribe();
    }
  }

  invokeDragDrop() {
    clearTimeout(this.gridBindingTimeout);
    this.acpTimeout = setTimeout(() => {
      this.selectedColumnsGridDestroy();
      this.selectedColumnsGridSubscription = this.handleDragAndDrop();
    }, 100);
  }

  private handleDragAndDrop(): Subscription {
    const sub = new Subscription(() => { });
    let draggedItemIndex;

    const tableRows = Array.from(document.querySelectorAll('.selected-columns-grid.k-grid tr'));
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
        const dataItem = this.selectedColumns[draggedItemIndex];
        dataItem.dragging = true;
      }));

      sub.add(dragOver.subscribe((e: any) => {
        e.preventDefault();
        const dataItem = this.selectedColumns.splice(draggedItemIndex, 1)[0];
        const dropIndex = closest(e.target, tableRow).rowIndex;
        const dropItem = this.selectedColumns[dropIndex];

        draggedItemIndex = dropIndex;
        this.zone.run(() =>
          this.selectedColumns.splice(dropIndex, 0, dataItem)
        );
      }));

      sub.add(dragEnd.subscribe((e: any) => {
        e.preventDefault();
        const dataItem = this.selectedColumns[draggedItemIndex];
        dataItem.dragging = false;
        this.updateColumnSequence();
      }));
    });

    return sub;
  }

  updateColumnSequence() {
    let sequence = 1;
    this.selectedColumns.forEach(element => {
      element.sequence = sequence++;
    });
  }

  //#endregion

  onSave() {
    this.popupOpened = false;
    this.save.emit(this.selectedColumns);
  }

  onFormClosed() {
    this.popupOpened = false;
    this.close.emit();
  }

  public ngOnDestroy(): void {
    this.selectedColumnsGridDestroy();
  }
}