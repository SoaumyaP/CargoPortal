import { Component, EventEmitter, Input, NgZone, OnInit, Output, Renderer2, OnDestroy } from '@angular/core';
import { faAngleDoubleLeft, faAngleRight, faMinusCircle, faTimes, faCaretUp } from '@fortawesome/free-solid-svg-icons';
import { RowClassArgs } from '@progress/kendo-angular-grid';
import { CheckableSettings, TreeItemLookup } from '@progress/kendo-angular-treeview';
import { fromEvent, Subject, Subscription, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { CargoDetailLoadModel } from 'src/app/core/models/cargo-details/cargo-detail-load.model';
import { IntegrationData } from 'src/app/core/models/forms/integration-data';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { ConsolidationService } from '../consolidation.service';
const tableRow = node => node.tagName.toLowerCase() === 'tr';
const closest = (node, predicate) => {
  while (node && !predicate(node)) {
    node = node.parentNode;
  }

  return node;
};
@Component({
  selector: 'app-load-cargo-detail-popup',
  templateUrl: './load-cargo-detail-popup.component.html',
  styleUrls: ['./load-cargo-detail-popup.component.scss']
})
export class LoadCargoDetailPopupComponent implements OnInit, OnDestroy {
  @Input() public popupOpened: boolean = false;
  @Input() public consolidationId: number;
  @Input() parentIntegration$: Subject<IntegrationData>;
  @Output() close: EventEmitter<any> = new EventEmitter();
  @Output() save: EventEmitter<any> = new EventEmitter();

  faAngleDoubleLeft = faAngleDoubleLeft;
  faAngleRight = faAngleRight;
  faMinusCircle = faMinusCircle;
  faTimes = faTimes;
  faCaretUp = faCaretUp;

  /** List of Cargo Detail including loaded and unloaded item. */
  cargoDetailList: CargoDetailLoadModel[] = [];

  loadedCargoList: CargoDetailLoadModel[] = [];
  unloadedCargoList: CargoDetailLoadModel[] = [];
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
  loadingTreeView: boolean = false;
  checkedKeys: any[] = [];
  expandedKeys: any[] = [];
  isCheckAll: boolean = false;

  // drag row on grid
  acpTimeout: any;
  gridBindingTimeout: any;
  private loadedCargoGridSubscription: Subscription;

  constructor(private zone: NgZone, private renderer: Renderer2, private consolidationService: ConsolidationService, public notification: NotificationPopup) { }

  ngOnInit() {
    this.loadingTreeView = true;
    this.consolidationService.getUnloadedCargoDetails(this.consolidationId)
      .subscribe(
        (res: CargoDetailLoadModel[]) => {
          this.cargoDetailList = res;
          this.unloadedCargoList = [...this.cargoDetailList.filter(c => this.loadedCargoList.findIndex(l => l.id === c.id) === -1)];
          this.buildTree(this.unloadedCargoList);
          this.loadingTreeView = false;
        }
      );
  }

  //#region tree-view handlers
  buildTree(currentList) {
    for (let i = 0; i < currentList.length; i++) {
      let itemNode = {
        id: currentList[i].id,
        text: currentList[i].productCode,
        customIndex: this.cargoDetailDataSource.length + '_0_0',
        customId: `${currentList[i].shipmentId}_${currentList[i].itemId}`,
        items: []
      };
      let poNode = {
        id: currentList[i].id,
        text: currentList[i].poNumber,
        customIndex: this.cargoDetailDataSource.length + '_0',
        customId: `${currentList[i].shipmentId}_${currentList[i].orderId}`,
        items: []
      };
      poNode.items.push(itemNode);

      const existingShipmentNodeIndex = this.cargoDetailDataSource.findIndex(x => x.text === currentList[i].shipmentNo);
      if (existingShipmentNodeIndex < 0) {
        let shipmentNode = {
          id: currentList[i].id,
          text: currentList[i].shipmentNo,
          customIndex: `${this.cargoDetailDataSource.length}`,
          customId: `${currentList[i].shipmentId}`,
          items: []
        };
        shipmentNode.items.push(poNode);
        this.cargoDetailDataSource.push(shipmentNode);
      }
      else {
        const existingPONodeIndex = this.cargoDetailDataSource[existingShipmentNodeIndex].items.findIndex(x => x.text === currentList[i].poNumber);
        if (existingPONodeIndex < 0) {
          let newNodeIndex = `${existingShipmentNodeIndex}_${this.cargoDetailDataSource[existingShipmentNodeIndex].items.length}`;
          poNode.customIndex = newNodeIndex;
          poNode.items[0].customIndex = newNodeIndex + '_' + 0;
          this.cargoDetailDataSource[existingShipmentNodeIndex].items.push(poNode);
        }
        else {
          let existingPONode = this.cargoDetailDataSource[existingShipmentNodeIndex].items[existingPONodeIndex];
          let newNodeIndex = `${existingShipmentNodeIndex}_${existingPONodeIndex}_${existingPONode.items.length}`;
          itemNode.customIndex = newNodeIndex;
          existingPONode.items.push(itemNode);
        }
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
      if (this.checkedKeys[i].split('_').length === 3) {
        res += 1;
      }
    }
    return res;
  }

  //#endregion

  _onLoadClicked() {
    this.isCheckAll = false;
    let sequence = this.loadedCargoList.length;
    for (let i = 0; i < this.checkedKeys?.length; i++) {
      const nodeIndexes = this.checkedKeys[i].split('_');
      if (nodeIndexes.length === 3) {
        let shipmentNode = this.cargoDetailDataSource[nodeIndexes[0]];
        let poNode = shipmentNode.items[nodeIndexes[1]];
        let itemNode = poNode.items[nodeIndexes[2]];

        const cargoDetailItemIndex = this.unloadedCargoList.findIndex(x => x.id === itemNode.id);
        let newLoadedCargo: any = { ...this.unloadedCargoList[cargoDetailItemIndex] };
        newLoadedCargo.sequence = sequence + 1;
        this.loadedCargoList.push(newLoadedCargo);

        this.unloadedCargoList.splice(cargoDetailItemIndex, 1);
        sequence += 1;
      }
    }
    this.resetTree(this.unloadedCargoList);
    this.invokeDragDropLoadedCargos()
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

  _onUnloadAllClicked() {
    this.loadedCargoList = [];
    this.unloadedCargoList = [...this.cargoDetailList];
    this.resetTree(this.unloadedCargoList);
  }

  _onDeleteLoadedCargoClicked(rowIndex) {
    this.unloadedCargoList.push(this.loadedCargoList[rowIndex]);
    this.loadedCargoList.splice(rowIndex, 1);
    this.updateLoadedCargoSequence();
    this.resetTree(this.unloadedCargoList);
  }

  _collapseAllClicked() {
    this.expandedKeys = [];
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
        const dataItem = this.loadedCargoList[draggedItemIndex];
        dataItem.dragging = true;
      }));

      sub.add(dragOver.subscribe((e: any) => {
        e.preventDefault();
        const dataItem = this.loadedCargoList.splice(draggedItemIndex, 1)[0];
        const dropIndex = closest(e.target, tableRow).rowIndex;
        const dropItem = this.loadedCargoList[dropIndex];

        draggedItemIndex = dropIndex;
        this.zone.run(() =>
          this.loadedCargoList.splice(dropIndex, 0, dataItem)
        );
      }));

      sub.add(dragEnd.subscribe((e: any) => {
        e.preventDefault();
        const dataItem = this.loadedCargoList[draggedItemIndex];
        dataItem.dragging = false;
        this.updateLoadedCargoSequence();
      }));
    });

    return sub;
  }

  updateLoadedCargoSequence() {
    let sequence = 1;
    this.loadedCargoList.forEach(element => {
      element.sequence = sequence++;
    });
  }

  //#endregion

  onSave() {
    this.popupOpened = false;
    this.consolidationService.loadCargoDetails(this.consolidationId, this.loadedCargoList).subscribe(
      () => {
        this.notification.showSuccessPopup('save.sucessNotification', 'label.consolidation');
        //emit an event to notify subscribers about the update.
        let emitValue = {
          name: '[load-cargo-detail-popup]cargoDetailLoaded',
          content: this.loadedCargoList
        };
        this.parentIntegration$.next(emitValue);

        this.save.emit();
      },
      (err) => {
        this.notification.showErrorPopup('save.failureNotification', 'label.consolidation');
      }
    );
  }

  onFormClosed() {
    this.popupOpened = false;
    this.close.emit();
  }

  public ngOnDestroy(): void {
    this.loadedCargoGridDestroy();
  }

}