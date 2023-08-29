import {
  Component,
  OnInit,
  Output,
  EventEmitter,
  Input,
  OnChanges,
  OnDestroy,
  ElementRef,
  ViewChild,
} from '@angular/core';
import { faTimes, faEllipsisV } from '@fortawesome/free-solid-svg-icons';
import { AssignPOsFormService } from './assign-pos-form.service';
import { OrganizationNameRole, StringHelper } from 'src/app/core';
import {
  tap,
  debounceTime,
} from 'rxjs/operators';
import { Subscription, Subject } from 'rxjs';
import { NotificationPopup } from '../notification-popup/notification-popup';

interface SelectPurchaseOrderModel {
  id: number;
  poNumber: string;
  poKey: string;
  shipFrom: string;
}

@Component({
  selector: 'app-assign-pos-form',
  templateUrl: './assign-pos-form.component.html',
  styleUrls: ['./assign-pos-form.component.scss']
})
export class AssignPOsFormComponent
  implements OnInit, OnChanges, OnDestroy {
  @Input() popupOpened: boolean;
  @Input() hintText: string = '';
  @Input() popupTitle: string = 'label.message';

  @Input() customerId: number = 0;
  @Input() supplierId: number;

  @Output() popupClosing: EventEmitter<any> = new EventEmitter();

  faTimes = faTimes;
  faEllipsisV = faEllipsisV;

  selectedPOs: Array<SelectPurchaseOrderModel> = [];
  sourcePOs: Array<SelectPurchaseOrderModel> = [];
  sourcePOsFiltered: Array<SelectPurchaseOrderModel> = [];

  @ViewChild('searchInput', { read: ElementRef, static: false })
  searchInput: ElementRef;
  searchTerm: string = '';
  treeData: any = null;
  selectedDragItem: SelectPurchaseOrderModel = null;

  treeViewPagination: ILoadMoreRequestArgs = {
    skip: 0,
    take: 20,
    loadedRecordCount: 0,
    maximumRecordCount: 0,
    loadingData: false
  };

  public searchTermKeyUp$ = new Subject<string>();
  private _subscriptions: Array<Subscription> = [];

  constructor(
    private _service: AssignPOsFormService,
    private _notification: NotificationPopup
  ) {

    const sub = this.searchTermKeyUp$.pipe(
      debounceTime(1000),
      tap((searchTearm: string) => {
        if (StringHelper.isNullOrEmpty(searchTearm) || searchTearm.length === 0 || searchTearm.length >= 3) {
          this._fetchSourcePOsDataSource(false, searchTearm);
        }
      }
      )).subscribe();
    this._subscriptions.push(sub);
  }

  ngOnInit() { }

  ngOnChanges() {
    // Clean-up current values as opening popup
    if (this.popupOpened) {
      this._cleanupWorkingState();
      this._fetchSourcePOsDataSource();
    }
  }

  ngOnDestroy() {
    this._subscriptions.map((x) => x.unsubscribe());
  }

  // Event handlers on poup
  onDragStart(event) {
    event.dataTransfer.setData('text', '');
  }

  onDragEnd() {
    this.selectedDragItem = null;
  }

  clickItem(selectedPO) {
    this.selectedDragItem = selectedPO;
  }

  // need statements to drop work
  allowDrop(event) {
    event.stopPropagation();
    event.preventDefault();
  }

  onDrop() {
    const value = this.selectedDragItem;
    this.selectedPOs.push(value);
    this._filterSourcePOs(true);
  }

  unselectPO(currentPO: {
    id: number;
    poNumber: string;
    poKey: string;
    itemsCount: number;
  }) {
    const po = this.selectedPOs.filter((item) => item.id === currentPO.id);

    this.selectedPOs = this.selectedPOs.filter(
      (item) => item.id !== currentPO.id
    );

    const check = this.sourcePOs.some((item) => item.id === currentPO.id);
    if (!check) {
      this.sourcePOs.push(po[0]);
    }

    this._filterSourcePOs();
  }

  principalSelectionChanged() {

    // clean-up selected POs
    this.selectedPOs = [];

    // clean-up searching text
    this.searchTerm = '';

    this._fetchSourcePOsDataSource();
  }

  onClosing() {
    this.popupClosing.emit();
  }

  onAssign() {
    if (!this.selectedPOs || this.selectedPOs.length === 0) {
      return;
    }

    const seletedPOs = this.selectedPOs.map((x) => x.id);
    this._service.assignPOs(
      seletedPOs,
      this.supplierId,
      OrganizationNameRole.Supplier
    ).subscribe(
      (success) => this._notification.showSuccessPopup('save.sucessNotification', this.popupTitle),
      (error) => this._notification.showErrorPopup('save.failureNotification', this.popupTitle)
    );
    this.popupClosing.emit();
  }

  loadMorePO() {
    if (
      this.treeViewPagination.loadedRecordCount <
      this.treeViewPagination.maximumRecordCount
    ) {
      this._fetchSourcePOsDataSource(true);
    }
  }

  private _fetchSourcePOsDataSource(
    loadMoreMode?: boolean,
    searchValue?: string
  ) {
    // Set status here to make show loading icon
    this.treeViewPagination.loadingData = true;

    // Reset data if it is not loading more POs
    if (!loadMoreMode) {

      this.sourcePOs = [];
      this.sourcePOsFiltered = [];

      this.treeViewPagination.skip = 0;
      this.treeViewPagination.loadedRecordCount = 0;
      this.treeViewPagination.maximumRecordCount = 0;
    }

    const skip = this.treeViewPagination.skip;
    const take = this.treeViewPagination.take;

    const sub = this._service
      .getSourcePOsDataSource(
        this.customerId,
        this.supplierId,
        skip,
        take,
        // searchTeam will be from direct value of input or data model
        StringHelper.isNullOrEmpty(searchValue)
          ? this.searchTerm
          : searchValue
      )
      .pipe(
        tap((data) => {
          const filterData = data.filter(
            (x) => !this.selectedPOs.some((y) => y.id === x.id)
          );

          if (loadMoreMode) {
            this.sourcePOs = this.sourcePOs.concat(filterData);
            this.sourcePOsFiltered = this.sourcePOsFiltered.concat(
              filterData
            );
          } else {
            this.sourcePOs = filterData;
            this.sourcePOsFiltered = filterData;
            this.treeViewPagination.maximumRecordCount =
              StringHelper.isNullOrEmpty(data) ||
                StringHelper.isNullOrEmpty(data[0])
                ? 0
                : data[0].recordCount;
          }

          // update treeview pagging
          this.treeViewPagination.loadedRecordCount += data.length;
          this.treeViewPagination.skip = this.treeViewPagination.loadedRecordCount;

          // set other status
          this.treeViewPagination.loadingData = false;

        })
      )
      .subscribe();
    this._subscriptions.push(sub);
  }

  private _filterSourcePOs(addingMode?: boolean) {

    // If selected POs count > 1, then locally filter
    if (this.selectedPOs.length > 1) {
      // apply filter on selected POs
      this.sourcePOsFiltered = this.sourcePOs.filter(
        (x) => !this.selectedPOs.some((y) => y.id === x.id)
      );
    } else if (this.selectedPOs.length === 1) {
      // If it is selecting PO, then server filter
      if (addingMode) {
        this._fetchSourcePOsDataSource();
      } else {
        // If it is unselecting PO, then local filter
        // apply filter on selected POs
        this.sourcePOsFiltered = this.sourcePOs.filter(
          (x) => !this.selectedPOs.some((y) => y.id === x.id)
        );
      }
    } else {
      // Server filter
      this._fetchSourcePOsDataSource();
    }
  }

  private _cleanupWorkingState() {
    this.selectedPOs = [];
    this.sourcePOsFiltered = [];
    this.sourcePOs = [];
    this.selectedDragItem = null;
    this.searchTerm = '';
  }
}