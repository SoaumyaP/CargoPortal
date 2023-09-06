import { Component, EventEmitter, Input, OnInit, Output, ViewChild, NgZone, Renderer2 } from '@angular/core';
import { NgForm } from '@angular/forms';
import { faPencilAlt, faRedo } from '@fortawesome/free-solid-svg-icons';
import { DataStateChangeEvent, GridDataResult, PageChangeEvent, RowClassArgs } from '@progress/kendo-angular-grid';
import { DataSourceRequestState, SortDescriptor, toDataSourceRequestString } from '@progress/kendo-data-query';
import { Subject, Subscription, fromEvent } from 'rxjs';
import { ConsolidationStage, FormMode, HttpService, StringHelper } from 'src/app/core';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { IntegrationData } from 'src/app/core/models/forms/integration-data';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { environment } from 'src/environments/environment';
import { filter, tap } from 'rxjs/operators';
import { GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
const tableRow = node => node.tagName.toLowerCase() === 'tr';
const closest = (node, predicate) => {
  while (node && !predicate(node)) {
    node = node.parentNode;
  }

  return node;
};
@Component({
  selector: 'app-consolidation-cargo-detail-list',
  templateUrl: './consolidation-cargo-detail-list.component.html',
  styleUrls: ['./consolidation-cargo-detail-list.component.scss']
})
export class ConsolidationCargoDetailListComponent implements OnInit {
  @Input() consolidationId: number;
  @Input() stage: ConsolidationStage;
  @Input() parentIntegration$: Subject<IntegrationData>;

  @Output() changeMode: EventEmitter<FormMode> = new EventEmitter();
  @ViewChild('mainForm', { static: false }) mainForm: NgForm;

  isGridLoading: boolean = false;
  isEditing: boolean = false;

  openCargoDescriptionDetailPopup: boolean = false;
  cargoDescriptionDetail: string;

  faPencilAlt = faPencilAlt;
  faRedo = faRedo;
  readonly ConsolidationStage = ConsolidationStage;
  readonly AppPermissions = AppPermissions;

  // Store all subscriptions, then should un-subscribe at the end
  private _subscriptions: Array<Subscription> = [];

  loadCargoDetailPopupOpened: boolean = false;
  reloadCargoDetailPopupOpened: boolean = false;


  // drag row on grid
  acpTimeout: any;
  gridBindingTimeout: any;
  private dragDropSubscription: Subscription;

  // kendo-grid declarations
  gridSort: SortDescriptor[] = [
    {
      field: 'sequence',
      dir: 'asc'
    }
  ];

  private defaultGridState: DataSourceRequestState = {
    sort: this.gridSort,
    skip: 0,
    take: 20
  };

  gridState: DataSourceRequestState;

  public gridData: GridDataResult = {
    data: [],
    total: 0
  };

  public pageSizes: Array<number> = [20, 50, 100];
  public pagerType: 'numeric' | 'input' = 'numeric';
  public buttonCount: number = 9;

  constructor(
    private _httpService: HttpService,
    public _notification: NotificationPopup,
    private zone: NgZone,
    private renderer: Renderer2,
    private _gaService: GoogleAnalyticsService) { }

  ngOnInit() {
    this.gridState = { ...this.defaultGridState };
    this.fetchGridData();
    this._registerEventHandlers();
  }

  private _registerEventHandlers() {

    /* Handle when the consignment linking to this consolidation has been deleted successfully.
    */
    const sub = this.parentIntegration$.pipe(
      filter((eventContent: IntegrationData) =>
        eventContent.name === '[consolidation-consignment-form]consignmentDeleted'
      )).subscribe((eventContent: IntegrationData) => {
        this.fetchGridData();
      });
    this._subscriptions.push(sub);
  }

  public fetchGridData() {
    let url = `${environment.apiUrl}/consolidations/internal/${this.consolidationId}/shipmentLoadDetails?${toDataSourceRequestString(this.gridState)}`;
    this.isGridLoading = true;
    this._httpService
      .get(`${url}`)
      .map(({ data, total }: GridDataResult) =>
        (<GridDataResult>{
          data: data,
          total: total
        }))
      .subscribe((res) => {
        this.gridData = res;
        this.isGridLoading = false;
        if (this.isEditing) {
          this.invokeDragDrop();
        } else {
          this.removeDragDrop();
        }
      });
  }

  public gridPageChange(event: PageChangeEvent): void {
    this.gridState.skip = event.skip;
    this.fetchGridData();
  }

  public gridSortChange(sort: SortDescriptor[]): void {
    this.gridState.sort = sort;
    this.fetchGridData();
  }

  gridStateChange(state: DataStateChangeEvent) {
    this.gridState = state;
    this.fetchGridData();
  }

  public pageSizeChange(value: any): void {
    this.gridState.skip = 0;
    this.gridState.take = value;
    this.fetchGridData();
  }

  _onEditClicked() {
    this.isEditing = true;
    this.gridState.take = null;
    this.changeMode.emit(FormMode.Edit);
    this.fetchGridData();
  }

  _onLoadClicked() {
    this.loadCargoDetailPopupOpened = true;
  }

  _onLoadCargoDetailPopupClosed() {
    this.loadCargoDetailPopupOpened = false;
    this.reloadCargoDetailPopupOpened = false;
  }

  _onLoadCargoDetailPopupSaved() {
    if (this.reloadCargoDetailPopupOpened) {
        this._gaService.emitAction('Re-load Cargo Details', GAEventCategory.Consolidation);
    } else {
        this._gaService.emitAction('Load Cargo Details', GAEventCategory.Consolidation);
    }
    this.loadCargoDetailPopupOpened = false;
    this.reloadCargoDetailPopupOpened = false;
    this.fetchGridData();
  }

  _onReloadClicked() {
    this.reloadCargoDetailPopupOpened = true;
  }

  _onCancelClick() {
    const confirmDlg = this._notification.showConfirmationDialog(
      'edit.cancelConfirmation',
      'label.consolidation'
    );

    confirmDlg.result.subscribe((result: any) => {
      if (result.value) {
        this.isEditing = false;
        this.gridState = { ...this.defaultGridState };
        this.fetchGridData();
        this.changeMode.emit(FormMode.View);
      }
    });
  }

  _onSaveClicked() {
    let isValid = this.validateAllGridInputs();
    if (!isValid) {
      this._notification.showErrorPopup('validation.mandatoryFieldsValidation', 'label.consolidation');
      return;
    }
    let url = `${environment.apiUrl}/consolidations/internal/${this.consolidationId}/shipmentLoadDetails`;
    this.isGridLoading = true;
    this._httpService
      .update(`${url}`, this.filteredData)
      .subscribe(
        (s) => {

          // emit an event to notify subscriber about the update.
          let emitValue = {
            name: '[consolidation-cargo-detail-list]cargoDetailListUpdated',
            content: this.filteredData
          };
          this.parentIntegration$.next(emitValue);

          this._notification.showSuccessPopup('save.sucessNotification', 'label.consolidation');
          this.isEditing = false;
          this.gridState = { ...this.defaultGridState };
          this.fetchGridData();
          this._gaService.emitAction('Edit Cargo Details', GAEventCategory.Consolidation);
        },
        (e) => {
          this.isGridLoading = false;
          this._notification.showErrorPopup('save.failureNotification', 'label.consolidation')
        },
        () => {
          this.changeMode.emit(FormMode.View);
        }
      );
  }

  _onDeleteLoadRow(rowIndex) {
    this.gridData.data[rowIndex].removed = true;
    this.f['unit_' + rowIndex].setErrors(null);
    this.f['package_' + rowIndex].setErrors(null);
    this.f['volume_' + rowIndex].setErrors(null);
    this.f['grossWeight_' + rowIndex].setErrors(null);
    this.updateCargoSequence();
  }

  _onUnitValueChange(value, rowIndex) {
    this.validateUnitValue(value, rowIndex);
  }
  validateUnitValue(value, rowIndex) {
    /* Temporary comment for PSP-2733

    const { balanceUnitQty } = this.gridData.data[rowIndex]
    if (value > balanceUnitQty) {
      this.f['unit_' + rowIndex].setErrors({ 'invalid': true });
      return false;
    }
    */
    if (value <= 0) {
      this.f['unit_' + rowIndex].setErrors({ 'required': true });
      return false;
    }
    return true;
  }

  _onPackageValueChange(value, rowIndex) {
    this.validatePackageValue(value, rowIndex);
  }
  validatePackageValue(value, rowIndex) {
    /* Temporary comment for PSP-2733

    const { balancePackageQty } = this.gridData.data[rowIndex]
    if (value > balancePackageQty) {
      this.f['package_' + rowIndex].setErrors({ 'invalid': true });
      return false;
    }
    */
    if (StringHelper.isNullOrEmpty(value))
    {
      this.f['package_' + rowIndex].setErrors({ 'required': true });
      return false;
    }
    return true;
  }

  _onVolumeValueChange(value, rowIndex) {
    this.validateVolumeValue(value, rowIndex);
  }
  validateVolumeValue(value, rowIndex) {
    if (value <= 0) {
      this.f['volume_' + rowIndex].setErrors({ 'required': true });
      return false;
    }
    return true;
  }

  _onGrossWeightValueChange(value, rowIndex) {
    this.validateGrossWeightValue(value, rowIndex);
  }
  validateGrossWeightValue(value, rowIndex) {
    if (value <= 0) {
      this.f['grossWeight_' + rowIndex].setErrors({ 'required': true });
      return false;
    }
    return true;
  }

  validateAllGridInputs() {
    let isValidAllGridInputs = true;
    this.gridData.data?.forEach((d, i) => {
      if (StringHelper.isNullOrEmpty(d.removed) || !d.removed) {
        let isValidUnit = this.validateUnitValue(d.unit, i);
        if (!isValidUnit) {
          isValidAllGridInputs = false;
        }
        let isValidPackage =  this.validatePackageValue(d.package, i);
        if (!isValidPackage) {
          isValidAllGridInputs = false;
        }
        let isValidVolume = this.validateVolumeValue(d.volume, i);
        if (!isValidVolume){
          isValidAllGridInputs = false;
        }
        let isValidGrossWeight = this.validateGrossWeightValue(d.grossWeight, i);
        if (!isValidGrossWeight) {
          isValidAllGridInputs = false;
        }
      }
    });
    return isValidAllGridInputs;
  }

  invokeDragDrop() {
    clearTimeout(this.gridBindingTimeout);
    this.acpTimeout = setTimeout(() => {
      this.dragDropSubscriptionDestroy();
      this.dragDropSubscription = this.handleDragAndDrop();
    }, 1000);
  }

  removeDragDrop() {
    const tableRows = Array.from(document.querySelectorAll('.consolidation-cargo-detail-grid.k-grid tr'));
    tableRows.forEach(item => {
      this.renderer.setAttribute(item, 'draggable', 'false');
    });
  }

  private handleDragAndDrop(): Subscription {
    const sub = new Subscription(() => { });
    let draggedItemIndex;

    const tableRows = Array.from(document.querySelectorAll('.consolidation-cargo-detail-grid.k-grid tr'));
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
        const dataItem = this.gridData.data[draggedItemIndex];
        dataItem.dragging = true;
      }));

      sub.add(dragOver.subscribe((e: any) => {
        e.preventDefault();
        const dataItem = this.gridData.data.splice(draggedItemIndex, 1)[0];
        const dropIndex = closest(e.target, tableRow).rowIndex;

        draggedItemIndex = dropIndex;
        this.zone.run(() =>
          this.gridData.data.splice(dropIndex, 0, dataItem)
        );
      }));

      sub.add(dragEnd.subscribe((e: any) => {
        e.preventDefault();
        const dataItem = this.gridData.data[draggedItemIndex];
        dataItem.dragging = false;
        this.updateCargoSequence();
        this.updateDeletedRowsIsValid();
        this.validateAllGridInputs();
      }));
    });

    return sub;
  }

  updateCargoSequence() {
    let sequence = 1;
    this.filteredData.forEach(element => {
      element.sequence = sequence++;
    });
  }

  updateDeletedRowsIsValid() {
    this.gridData.data.forEach((x, i) => {
      if (x.removed) {
        this.f['unit_' + i].setErrors(null);
      }
    })
  }

  get filteredData() {
    return this.gridData.data.filter(d => StringHelper.isNullOrEmpty(d.removed) || !d.removed);
  }

  get isShowEditButton() {
    if (this.stage === ConsolidationStage.Confirmed) {
      return false;
    }
    if (!this.gridData.data || this.gridData.data?.length <= 0) {
      return false;
    }
    return true;
  }

  get isShowLoadButton() {
    if (this.stage === ConsolidationStage.Confirmed) {
      return false;
    }
    if (this.gridData.data?.length > 0) {
      return false;
    }
    return true;
  }

  get isShowReloadButton() {
    if (this.stage === ConsolidationStage.Confirmed) {
      return false;
    }
    if (!this.gridData.data || this.gridData.data?.length <= 0) {
      return false;
    }
    return true;
  }

  public rowCallback(args: RowClassArgs) {
    // Deleted row will be marked with removed property.
    return {
      'hide-row': args.dataItem.removed,
      'dragging': args.dataItem.dragging
    };
  }

  seeMoreCargoDescription(description: string) {
    this.openCargoDescriptionDetailPopup = true;
    this.cargoDescriptionDetail = description;
  }
  onCargoDescriptionDetailPopupClosed() {
      this.openCargoDescriptionDetailPopup = false;
  }

  // convenience getter for easy access to form fields
  get f() { return this.mainForm.controls; }

  private dragDropSubscriptionDestroy() {
    if (this.dragDropSubscription) {
      this.dragDropSubscription.unsubscribe();
    }
  }

  public ngOnDestroy(): void {
    this._subscriptions.push(this.dragDropSubscription);
    this._subscriptions.map(x => x?.unsubscribe());
  }
}