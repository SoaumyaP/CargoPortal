import { Component, OnInit, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { of } from 'rxjs';
import { faAngleDoubleLeft, faAngleRight, faMinusCircle, faTimes, faCaretUp } from '@fortawesome/free-solid-svg-icons';
import { CheckableSettings, TreeItemLookup } from '@progress/kendo-angular-treeview';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { POFulfillmentCustomerPOModel } from '../po-fulfillment-load-detail-form/po-fulfillment-load-detail-form.model';
import { 
  DragEndEvent,
  DragOverEvent,
  DragStartEvent,
  SortableComponent 
} from '@progress/kendo-angular-sortable';
@Component({
  selector: 'app-load-cargo-popup',
  templateUrl: './load-cargo-popup.component.html',
  styleUrls: ['./load-cargo-popup.component.scss']
})
export class LoadCargoPopupComponent implements OnInit {
  @Input() public popupOpened: boolean = false;
  @Input() public loadNumber: string = '';
  @Input('orders') public poffOrders: Array<POFulfillmentCustomerPOModel> = [];
  @Output() close: EventEmitter<any> = new EventEmitter();
  @Output() save: EventEmitter<any> = new EventEmitter();

  @ViewChild('sortable', {static: false}) sortable: SortableComponent;

  faAngleDoubleLeft = faAngleDoubleLeft;
  faAngleRight = faAngleRight;
  faMinusCircle = faMinusCircle;
  faTimes = faTimes;
  faCaretUp = faCaretUp;

  loadedPOFFOrders: Array<POFulfillmentCustomerPOModel> = [];
  unloadedPOFFOrders: Array<POFulfillmentCustomerPOModel> = [];
  /**Data source for kendo-treeview */
  cargoDetailDataSource: any[] = [];

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
  expandedKeys: string[] = [];
  isCheckAll: boolean = false;

  currentDraggingIndex: number;

  constructor(public notification: NotificationPopup) { }

  ngOnInit() {
    this.unloadedPOFFOrders = [...this.poffOrders];
    this.buildTree(this.unloadedPOFFOrders);
  }

  //#region tree-view handlers
  buildTree(currentList) {
    for (let i = 0; i < currentList.length; i++) {
      let itemNode = {
        id: currentList[i].id,
        text: currentList[i].productCode,
        customIndex: this.cargoDetailDataSource.length + '_0',
        customId: `${currentList[i].purchaseOrderId}_${currentList[i].poLineItemId}`,
        items: []
      };

      const existingPONodeIndex = this.cargoDetailDataSource.findIndex(x => x.text === currentList[i].customerPONumber);
      if (existingPONodeIndex < 0) {
        let poNode = {
          id: currentList[i].id,
          text: currentList[i].customerPONumber,
          customIndex: `${this.cargoDetailDataSource.length}`,
          customId: `${currentList[i].purchaseOrderId}`,
          items: []
        };
        poNode.items.push(itemNode);
        this.cargoDetailDataSource.push(poNode);
      }
      else {
        let existingPONode = this.cargoDetailDataSource[existingPONodeIndex];
        let newNodeIndex = `${existingPONodeIndex}_${existingPONode.items.length}`;
        itemNode.customIndex = newNodeIndex;
        existingPONode.items.push(itemNode);
      }
    }
  }

  resetTree(currentList) {
    const currentCargoDetailDataSource = [...this.cargoDetailDataSource];
    const currentExpandedKeys = [...this.expandedKeys];
    this.cargoDetailDataSource = [];
    this.checkedKeys = [];
    this.expandedKeys = [];
    this.isCheckAll = false;
    this.buildTree(currentList);
    this.updateExpandedKey(currentCargoDetailDataSource, currentExpandedKeys);
  }

  hasChildren(item: any) {
    return item.items && item.items.length > 0;
  }

  fetchChildren(item: any) {
    return of(item.items);
  }

  handleChecking(itemLookup: TreeItemLookup): void {
    this.isCheckAll = false;
    // select parent node
    const currentNode = itemLookup.item.index;
    const isChecking = this.checkedKeys.indexOf(currentNode) === -1;
    const nodeList = currentNode.split('_');
    if (nodeList.length > 0) {
      let currentIndex = nodeList[0];

      if (nodeList.length === 1) {
        isChecking ? this.tickChildList(this.cargoDetailDataSource[currentIndex].items) : this.untickChildList(this.cargoDetailDataSource[currentIndex].items);
      } else if (nodeList.length === 2) {
        isChecking ? this.tickChildList(this.cargoDetailDataSource[currentIndex].items[nodeList[1]].items) : this.untickChildList(this.cargoDetailDataSource[currentIndex].items[nodeList[1]].items)
      }
    }
  }

  _onCheckAllClicked() {
    if (this.isCheckAll) {
      this.tickChildList(this.cargoDetailDataSource);
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

  untickChildList(currentList) {
    for (let i = 0; i < currentList.length; i++) {
      // remove child from checked list
      const index = this.checkedKeys.indexOf(currentList[i].customIndex);
      if (index >= 0) {
        this.checkedKeys.splice(index, 1);
      }
      // has children
      if (currentList[i].items.length > 0) {
        this.untickChildList(currentList[i].items);
      }
    }
  }

  get numberOfCheckedItem() {
    let res = 0;
    for (let i = 0; i < this.checkedKeys?.length; i++) {
      if (this.checkedKeys[i].split('_').length === 2) {
        res += 1;
      }
    }
    return res;
  }

  //#endregion

  _onLoadClicked() {
    this.isCheckAll = false;
    let sequence = this.loadedPOFFOrders.length;
    for (let i = 0; i < this.checkedKeys?.length; i++) {
      const nodeIndexes = this.checkedKeys[i].split('_');
      if (nodeIndexes.length === 2) {
        let poNode = this.cargoDetailDataSource[nodeIndexes[0]];
        let itemNode = poNode.items[nodeIndexes[1]];

        const cargoDetailItemIndex = this.unloadedPOFFOrders.findIndex(x => x.id === itemNode.id);
        let newLoadedCargo: any = { ...this.unloadedPOFFOrders[cargoDetailItemIndex] };
        newLoadedCargo.sequence = sequence + 1;
        this.loadedPOFFOrders.push(newLoadedCargo);

        this.unloadedPOFFOrders.splice(cargoDetailItemIndex, 1);
        sequence += 1;
      }
    }
    // force to UI redraw.
    const tmp = [...this.loadedPOFFOrders];
    this.loadedPOFFOrders = tmp;

    this.resetTree(this.unloadedPOFFOrders);
  }

  updateExpandedKey(treeViewDtsBeforeChanged, expandedKeysBeforeChanged) {
    this.expandedKeys = [];
    for (let i = 0; i < expandedKeysBeforeChanged.length; i++) {
      let nodeIndexs = expandedKeysBeforeChanged[i].split('_');
      let expandedNode = treeViewDtsBeforeChanged[nodeIndexs[0]];
      if (nodeIndexs.length === 2) {
        expandedNode = expandedNode.items[nodeIndexs[1]];
      }
      this.updateExpandedKeyByCustomId(this.cargoDetailDataSource, expandedNode.customId);
    }
  }

  updateExpandedKeyByCustomId(currentList, customId) {
    for (let i = 0; i < currentList.length; i++) {
      if (currentList[i].customId === customId) {
        this.expandedKeys.push(currentList[i].customIndex);
        break;
      }
      else {
        // Has children
        if (currentList[i].items.length > 0) {
          this.updateExpandedKeyByCustomId(currentList[i].items, customId);
        }
      }
    }
  }

  /**To unload all cargo items. */
  public unloadAll(): void {
    this.loadedPOFFOrders = [];
    this.unloadedPOFFOrders = [...this.poffOrders];
    this.resetTree(this.unloadedPOFFOrders);
  }

  /**To unload a specific cargo item. */
  public unloadItem(poffOrderId: number): void {
    let item = this.loadedPOFFOrders.find(x => x.id === poffOrderId);
    if (!item) {
      return;
    }
    this.unloadedPOFFOrders.push(item);
    this.loadedPOFFOrders = this.loadedPOFFOrders.filter(x => x.id !== poffOrderId);

    // update relevant data
    this.updateCargoSequence();
    this.resetTree(this.unloadedPOFFOrders);
  }
  
  /**To collapse all items on kendo tree-view. */
  public collapseAll(): void {
    this.expandedKeys = [];
  }

  updateCargoSequence() {
    let sequence = 1;
    this.loadedPOFFOrders.forEach(element => {
      element.sequence = sequence++;
    });
  }

  //#endregion

  /**On saving dialog. */
  onSave() {
    this.popupOpened = false;
    this.save.emit(this.loadedPOFFOrders);
  }

  onFormClosed() {
    this.popupOpened = false;
    this.close.emit();
  }

  /**
   * On starting to drag an item.
   * */
  public onDragStart(e: DragStartEvent): void {
    this.currentDraggingIndex = e.index;
    let item = this.loadedPOFFOrders[e.index];
    item.dragging = true;
  }

  /**
   * On dragging item over another item
   * */
  public onDragOver(e: DragOverEvent) {
    e.preventDefault();
    this.currentDraggingIndex = e.index;

    this.sortable.moveItem(e.oldIndex, e.index);
  }

  /**When an item has been dragged.
   * 
  */
  public onDragEnd(e: DragEndEvent): void {
    let item = this.loadedPOFFOrders[this.currentDraggingIndex];
    item.dragging = false;

    this.updateCargoSequence();
  }

  public ngOnDestroy(): void {}

  get title() {
    return 'Load Cargo to ' + this.loadNumber;
  }

}
