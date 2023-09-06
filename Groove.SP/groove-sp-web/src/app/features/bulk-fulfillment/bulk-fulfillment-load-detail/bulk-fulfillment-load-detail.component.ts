import { Component, Input, NgZone, Renderer2, SimpleChange, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { faAngleRight, faEllipsisV, faPencilAlt, faTrashAlt } from '@fortawesome/free-solid-svg-icons';
import { TranslateService } from '@ngx-translate/core';
import { RowClassArgs } from '@progress/kendo-angular-grid';
import { fromEvent, Subscription } from 'rxjs';
import { tap } from 'rxjs/operators';
import { DropDowns, EquipmentType, FormMode, POFulfillmentStageType, StringHelper, ValidationDataType, ViewSettingModuleIdType } from 'src/app/core';
import { PositionUpDownModel } from 'src/app/core/directives/position-up-down.directive';
import { EnumHelper } from 'src/app/core/helpers/enum.helper';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { ValidationData } from 'src/app/core/models/forms/validation-data';
import { ViewSettingModel } from 'src/app/core/models/viewSetting.model';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import {
  BulkFulfillmentLoadDetailFormModel,
  BulkFulfillmentLoadDetailModel,
  BulkFulfillmentLoadModel,
  BulkFulfillmentModel,
  BulkFulfillmentOrderModel
} from '../models/bulk-fulfillment.model';

const tableRow = node => node.tagName.toLowerCase() === 'tr';
const closest = (node, predicate) => {
  while (node && !predicate(node)) {
    node = node.parentNode;
  }

  return node;
};
@Component({
  selector: 'app-bulk-fulfillment-load-detail',
  templateUrl: './bulk-fulfillment-load-detail.component.html',
  styleUrls: ['./bulk-fulfillment-load-detail.component.scss']
})
export class BulkFulfillmentLoadDetailComponent {
  @Input() public model: BulkFulfillmentModel;
  @Input() public isViewMode: boolean;
  @Input() public isReloadMode: boolean = false;
  @Input() isShowContainerInfo = false;
  @Input() viewSettings: ViewSettingModel[];
  formHelper = FormHelper;
  viewSettingModuleIdType = ViewSettingModuleIdType;

  @ViewChild('tooltip', { static: false }) tooltip;
  @ViewChild('mainForm', { static: false }) mainForm: NgForm;

  /**Store all subscriptions, then should un-subscribe at the end */
  private _subscriptions: Array<Subscription> = [];

  equipmentTypes = EquipmentType;
  equipmentTypeOptions = DropDowns.EquipmentStringType;
  faEllipsisV = faEllipsisV;
  faPencilAlt = faPencilAlt;
  faTrashAlt = faTrashAlt;
  faAngleRight = faAngleRight;

  loadDetailFormOpened: boolean = false;
  isEditable: boolean = false;

  /*Reference information on selected load for the dialogs. */
  selectedLoad: BulkFulfillmentLoadModel;
  selectedLoadIndex: number;
  selectedLoadNumber: string;

  /**Reference information on selected order for load plan detail dialog. */
  selectedOrder: BulkFulfillmentOrderModel;

  /**Model for load plan detail dialog */
  selectedLoadDetailForm: BulkFulfillmentLoadDetailFormModel = null;
  deletedLoadDetail = null;

  prevSelectedLoadDetail: BulkFulfillmentLoadDetailModel = null;

  // Update container info dialog declarations.
  loadContainerFormMode = '';
  loadContainerFormOpened = false;

  // Load cargo dialog declarations.
  loadCargoDialogOpened: boolean = false;

  availableOrders: Array<BulkFulfillmentOrderModel> = [];

  // drag row on grid
  acpTimeout: any;
  gridBindingTimeout: any;
  private dragDropSubscription: Subscription;

  constructor(
    public notification: NotificationPopup,
    public translateService: TranslateService,
    private zone: NgZone,
    private renderer: Renderer2) {
  }

  ngOnChanges(changes: { [propName: string]: SimpleChange }): void {
    this.isEditable = (this.model.stage === POFulfillmentStageType.ForwarderBookingConfirmed
      && !this.model.isGeneratePlanToShip && !this.isViewMode) || this.isReloadMode;

    if (this.isEditable) {
      this.invokeDragDrop();
    } else {
      this.removeDragDrop();
    }
  }

  /**Closed popup without Saving or Cancelled */
  onLoadDetailFormClosed() {
    this.loadDetailFormOpened = false;
    this.selectedLoadDetailForm = null;
  }

  /**Add new Load plan details */
  onLoadDetailItemAdded(addedItem: BulkFulfillmentOrderModel) {
    const formModel = new BulkFulfillmentLoadDetailFormModel(addedItem, this.selectedLoad, this.model.loads);

    this.model.loads.forEach((el, i) => {
      if (el.id === this.selectedLoad.id) {
        if (el.details === undefined) {
          el['details'] = [];
        }

        // Remove reference information to prevent loop on saving
        formModel.load = null;
        formModel.order = null;
        // Update related data
        formModel.poFulfillmentOrderId = addedItem.id;
        formModel.poFulfillmentLoadId = el.id;
        this.selectedLoad = { ...el };
        this.model.loads.splice(i, 1);

        this.selectedLoad.subtotalPackageQuantity += formModel.packageQuantity;
        this.selectedLoad.subtotalUnitQuantity += formModel.unitQuantity;
        this.selectedLoad.subtotalGrossWeight += formModel.grossWeight;
        this.selectedLoad.subtotalNetWeight += formModel.netWeight || 0;
        this.selectedLoad.subtotalVolume += formModel.volume;

        this._updateSequenceForLoadDetails(this.selectedLoad.details);
        this.selectedLoad.details.push(formModel);
        this.model.loads.splice(i, 0, this.selectedLoad);

        addedItem.loadedQty += formModel.packageQuantity;
        addedItem.openQty = addedItem.bookedPackage - addedItem.loadedQty;
      }
    });

    this.model.orders.forEach(order => {
      if (order.id === formModel.poFulfillmentOrderId) {
        order.loadedQty += formModel.packageQuantity;
        order.openQty = addedItem.bookedPackage - addedItem.loadedQty;
      }
    });
  }

  reloadLoadDetails() {
    // Update related data
    this.selectedLoad.subtotalPackageQuantity = 0;
    this.selectedLoad.subtotalUnitQuantity = 0;
    this.selectedLoad.subtotalNetWeight = 0;
    this.selectedLoad.subtotalGrossWeight = 0;
    this.selectedLoad.subtotalVolume = 0;
    this.selectedLoad.details.forEach(el => {
      let customerPO = this.model.orders.find(o => o.id === el.poFulfillmentOrderId);
      customerPO.loadedQty -= el.packageQuantity;
      customerPO.openQty = customerPO.bookedPackage - customerPO.loadedQty;
    });
    this.selectedLoad.details = [];
  }

  /**Edit Load plan details */
  onLoadDetailItemEdited(editedItem) {
    this.model.loads.forEach(load => {
      if (load.id === this.selectedLoad.id) {

        // Remove reference information to prevent loop on saving
        editedItem.load = null;
        editedItem.customerPO = null;

        // Update related data
        load.details[load.details.indexOf(this.prevSelectedLoadDetail)] = editedItem;

        this._updateSequenceForLoadDetails(load.details);

        load.subtotalPackageQuantity += editedItem.packageQuantity - this.prevSelectedLoadDetail.packageQuantity;
        load.subtotalUnitQuantity += editedItem.unitQuantity - this.prevSelectedLoadDetail.unitQuantity;
        load.subtotalNetWeight += (editedItem.netWeight || 0) - (this.prevSelectedLoadDetail.netWeight || 0);
        load.subtotalGrossWeight += editedItem.grossWeight - this.prevSelectedLoadDetail.grossWeight;
        load.subtotalVolume += editedItem.volume - this.prevSelectedLoadDetail.volume;
      }
    });

    this.model.orders.forEach(order => {
      if (order.id === this.selectedLoadDetailForm.poFulfillmentOrderId) {
        order.loadedQty += editedItem.packageQuantity - this.prevSelectedLoadDetail.packageQuantity;
        order.openQty = order.bookedPackage - order.loadedQty;
      }
    });

    this.selectedLoadDetailForm = null;
    this.loadDetailFormOpened = false;
  }

  /**Click edit Load plan details */
  onLoadDetailItemEditClick(loadId, index) {
    this.tooltip.hide();
    this.model.loads.forEach(el => {
      if (el.id === loadId) {
        this.prevSelectedLoadDetail = el.details[index];
        this.selectedLoad = { ...el };
      }
    });

    this.model.orders.forEach((el: any) => {
      if (el.id === this.prevSelectedLoadDetail.poFulfillmentOrderId) {
        this.selectedOrder = { ...el };
      }
    });
    this.selectedOrder.openQty += this.prevSelectedLoadDetail.packageQuantity;

    this.selectedLoadDetailForm = { ...this.prevSelectedLoadDetail } as BulkFulfillmentLoadDetailFormModel;
    this.selectedLoadDetailForm.load = this.selectedLoad;
    this.selectedLoadDetailForm.order = this.selectedOrder;

    this.loadDetailFormOpened = true;
  }

  /**Click delete load plan details */
  onLoadDetailItemDeleteClick(loadId, index) {
    this.tooltip.hide();
    this.model.loads.forEach(el => {
      if (el.id === loadId) {
        this.deletedLoadDetail = el.details[index];
      }
    });

    const confirmDialog = this.notification.showConfirmationDialog('msg.deleteLoadDetail', 'label.poFulfillment');
    confirmDialog.result.subscribe((rs: any) => {
      if (rs.value) {
        this.model.orders.forEach((el: any) => {
          if (el.id === this.deletedLoadDetail.poFulfillmentOrderId) {
            this.selectedOrder = el;
          }
        });

        this.model.loads.forEach(el => {
          if (el.id === loadId) {
            el.details = el.details.filter((e, ind) => {
              return ind !== index;
            });

            this._updateSequenceForLoadDetails(el.details);

            // Update related data
            el.subtotalUnitQuantity -= this.deletedLoadDetail.unitQuantity;
            el.subtotalNetWeight -= (this.deletedLoadDetail.netWeight || 0);
            el.subtotalGrossWeight -= this.deletedLoadDetail.grossWeight;
            el.subtotalVolume -= this.deletedLoadDetail.volume;
            this.updateSubtotalPackageAmount(el.id);
          }
        });
        this.updateAllPOFFOrderPackageQty();
      }
    });
  }

  /**To re-seed sequence of load details, from 1 -> n.
   * Please call it after integrating to the list of load details
   */
  private _updateSequenceForLoadDetails(loadDetails: Array<any>) {
    if (!StringHelper.isNullOrEmpty(loadDetails) && loadDetails.length > 0) {
      for (let index = 0; index < loadDetails.length; index++) {
        const element = loadDetails[index];
        element.sequence = index + 1;
      }
    }
  }

  handleLoadDetailsSequenceChanged(positionEventData: PositionUpDownModel) {
    const loadDetails = positionEventData.arrayObject;

    const oldData = loadDetails[positionEventData.currentIndex];
    const newData = loadDetails[positionEventData.newIndex];

    // swap data
    loadDetails[positionEventData.currentIndex] = newData;
    loadDetails[positionEventData.newIndex] = oldData;

    // reset load sequence number
    for (let index = 1; index <= loadDetails.length; index++) {
      const element = loadDetails[index - 1];
      element.sequence = index;
    }
  }

  editContainerPopup(index: number) {
    this.loadContainerFormMode = !this.isViewMode ? FormMode.Edit : FormMode.View;
    this.loadContainerFormOpened = true;
    this.selectedLoadIndex = index;
    this.selectedLoad = { ...this.model.loads[index] };
    this.selectedLoad.equipmentType = this.translateService
      .instant(this.labelFromEnum(this.equipmentTypeOptions, this.selectedLoad.equipmentType));
  }

  viewContainerPopup(index: number) {
    this.editContainerPopup(index);
    this.loadContainerFormMode = FormMode.View;
  }

  labelFromEnum(arr, value) {
    return EnumHelper.convertToLabel(arr, value);
  }

  onLoadContainerFormClosed() {
    this.loadContainerFormOpened = false;
  }

  onLoadContainerEdited(loadContainer: BulkFulfillmentLoadModel) {
    this.loadContainerFormOpened = false;
    this.model.loads[this.selectedLoadIndex].containerNumber = loadContainer.containerNumber;
    this.model.loads[this.selectedLoadIndex].sealNumber = loadContainer.sealNumber;
    this.model.loads[this.selectedLoadIndex].sealNumber2 = loadContainer.sealNumber2;
    this.model.loads[this.selectedLoadIndex].loadingDate = loadContainer.loadingDate;
    this.model.loads[this.selectedLoadIndex].gateInDate = loadContainer.gateInDate;
  }

  get canUpdateContainer() {

    if (this.isReloadMode) {
      return true;
    }

    if (this.isViewMode) {
      return false;
    }

    if (this.model.isGeneratePlanToShip) {
      return false;
    }

    if (this.model.stage
      !== POFulfillmentStageType.ForwarderBookingConfirmed
    ) {
      return false;
    }

    return true;
  }

  onLoadBtnClick(loadId: number) {
    this.loadCargoDialogOpened = true;
    this.availableOrders = []
    this.selectedLoad = this.model.loads.find(l => l.id === loadId);

    this.model.orders.forEach((poffOrder: any) => {
      let usedPackageQty = 0;
      this.model.loads.filter(l => l.id !== loadId).forEach(el => {
        usedPackageQty += el.details?.find(
          x => x.poFulfillmentOrderId === poffOrder.id)?.packageQuantity ?? 0;
      });
      if (usedPackageQty < poffOrder.bookedPackage) {
        let model: BulkFulfillmentOrderModel = { ...poffOrder };
        model.loadedQty = usedPackageQty;
        model.openQty = model.bookedPackage - model.loadedQty;
        this.availableOrders.push(model);
      }
    });
    this.selectedLoadNumber = !this.selectedLoad.containerNumber ? this.selectedLoad.loadReferenceNumber : this.selectedLoad.containerNumber;
  }

  onPackageQtyValueChange(value: number, loadDetailIndex: number, loadIndex: number) {
    const currentLoad = this.model.loads[loadIndex];
    this.updateSubtotalPackageAmount(currentLoad.id);
    this.updateAllPOFFOrderPackageQty();

    const control = this.getControlByName(`packageQuantity_${loadIndex}_${loadDetailIndex}`);
    if (StringHelper.isNullOrEmpty(value)) {
      control?.setErrors({ 'required': true });
      return;
    }
    if (value <= 0) {
      control?.setErrors({ 'lessThanOrEqualZero': true });
      return;
    }
  }

  onUnitQtyValueChange(value: number, loadDetailIndex: number, loadIndex: number) {
    this.updateSubtotalLoadedQty(this.model.loads[loadIndex].id);
    this.validateUnitQty(value, loadDetailIndex, loadIndex);
  }

  onVolumeValueChange(value: number, loadDetailIndex: number, loadIndex: number) {
    this.updateSubtotalVolume(this.model.loads[loadIndex].id);
    this.validateVolume(value, loadDetailIndex, loadIndex);
  }

  onGrossWeightValueChange(value: number, loadDetailIndex: number, loadIndex: number) {
    this.updateSubtotalGrossWeight(this.model.loads[loadIndex].id);
    this.validateGrossWeight(value, loadDetailIndex, loadIndex);
  }

  onLoadCargoPopupSaved(data: BulkFulfillmentOrderModel[]): void {
    this.loadCargoDialogOpened = false;
    this.removeDragDrop();
    if (this.selectedLoad.details?.length > 0) {
      this.reloadLoadDetails();
    }
    data.forEach(el => {
      this.onLoadDetailItemAdded({ ...el });
    });
    this.invokeDragDrop();
  }

  private updateSubtotalPackageAmount(loadId: number): void {
    let loadPlan = this.model.loads.find(l => l.id === loadId);
    loadPlan.subtotalPackageQuantity = loadPlan?.details.reduce((a, b) => a + (b['packageQuantity'] || 0), 0);
  }

  private updateSubtotalLoadedQty(loadId: number): void {
    let loadPlan = this.model.loads.find(l => l.id === loadId);
    loadPlan.subtotalUnitQuantity = loadPlan?.details.reduce((a, b) => a + (b['unitQuantity'] || 0), 0);
  }

  private updateSubtotalVolume(loadId: number): void {
    let loadPlan = this.model.loads.find(l => l.id === loadId);
    loadPlan.subtotalVolume = loadPlan?.details.reduce((a, b) => a + (b['volume'] || 0), 0);
  }

  private updateSubtotalGrossWeight(loadId: number): void {
    let loadPlan = this.model.loads.find(l => l.id === loadId);
    loadPlan.subtotalGrossWeight = loadPlan?.details.reduce((a, b) => a + (b['grossWeight'] || 0), 0);
  }

  /**Call to update remaining package qty of all POFF Orders */
  private updateAllPOFFOrderPackageQty(): void {
    const loadDetails = [].concat.apply([], this.model.loads.map(x => x.details));
    this.model.orders.forEach(o => {
      const usedPackageQty: number = loadDetails.filter(od => od.poFulfillmentOrderId === o.id).reduce((a, b) => a + (b['packageQuantity'] || 0), 0);
      o.loadedQty = usedPackageQty;
      o.openQty = o.bookedPackage - o.loadedQty;
    });
  }

  private validateLoadGridInput(loadIndex: number): void {
    this.model.loads[loadIndex].details?.forEach((el, i) => {
      let isValidPackageQty = this.validatePackageQty(el.packageQuantity, i, loadIndex);
      if (isValidPackageQty) {
        this.getControlByName(`packageQuantity_${loadIndex}_${i}`).setErrors(null);
      }
      this.validateUnitQty(el.unitQuantity, i, loadIndex);
      this.validateVolume(el.volume, i, loadIndex);
      this.validateGrossWeight(el.volume, i, loadIndex);
    });
  }

  /**Validate packageQuantity value and throw error*/
  private validatePackageQty(value: number, loadDetailIndex: number, loadIndex: number): boolean {
    let control = this.getControlByName(`packageQuantity_${loadIndex}_${loadDetailIndex}`);
    if (StringHelper.isNullOrEmpty(value)) {
      control?.setErrors({ 'required': true });
      return false;
    }
    if (value <= 0) {
      control?.setErrors({ 'lessThanOrEqualZero': true });
      return false;
    }
    return true;
  }

  /**Validate unitQuantity value and throw error. */
  private validateUnitQty(value: number, loadDetailIndex: number, loadIndex: number): boolean {
    let control = this.getControlByName(`unitQuantity_${loadIndex}_${loadDetailIndex}`);
    if (StringHelper.isNullOrEmpty(value)) {
      control?.setErrors({ 'required': true });
      return false;
    }
    if (value <= 0) {
      control?.setErrors({ 'lessThanOrEqualZero': true });
      return false;
    }
    return true;
  }

  /**Validate volume value and throw error. */
  private validateVolume(value: number, loadDetailIndex: number, loadIndex: number) {
    let control = this.getControlByName(`volume_${loadIndex}_${loadDetailIndex}`);
    if (StringHelper.isNullOrEmpty(value)) {
      control?.setErrors({ 'required': true });
      return false;
    }
    if (value <= 0) {
      control?.setErrors({ 'lessThanOrEqualZero': true });
      return false;
    }
    return true;
  }

  /**Validate grossWeight value and throw error. */
  private validateGrossWeight(value: number, loadDetailIndex: number, loadIndex: number) {
    let control = this.getControlByName(`grossWeight_${loadIndex}_${loadDetailIndex}`);
    if (StringHelper.isNullOrEmpty(value)) {
      control?.setErrors({ 'required': true });
      return false;
    }
    if (value <= 0) {
      control?.setErrors({ 'lessThanOrEqualZero': true });
      return false;
    }
    return true;
  }

  /** Validate all grid input */
  validateAllFields(): boolean {
    let isValid = true;
    for (let i = 0; i < this.model.loads.length; i++) {
      const load = this.model.loads[i];
      for (let j = 0; j < load.details.length; j++) {
        const { packageQuantity, unitQuantity, volume, grossWeight } = load.details[j];
        let rs = this.validatePackageQty(packageQuantity, j, i);
        if (!rs) {
          isValid = false;
        }
        rs = this.validateUnitQty(unitQuantity, j, i);
        if (!rs) {
          isValid = false;
        }
        rs = this.validateVolume(volume, j, i);
        if (!rs) {
          isValid = false;
        }
        rs = this.validateGrossWeight(grossWeight, j, i);
        if (!rs) {
          isValid = false;
        }
      }
    }
    return isValid;
  }

  // convenience getter for easy access to form fields
  private getControlByName(controlName: string) {
    if (this.mainForm?.controls) {
      return this.mainForm.controls[controlName];
    }
    return null;
  }

  onLoadCargoPopupClosed() {
    this.loadCargoDialogOpened = false;
  }

  invokeDragDrop() {
    clearTimeout(this.gridBindingTimeout);
    this.acpTimeout = setTimeout(() => {
      this.dragDropSubscriptionDestroy();
      this.dragDropSubscription = this.handleDragAndDrop();
    }, 1);
  }

  private dragDropSubscriptionDestroy() {
    if (this.dragDropSubscription) {
      this.dragDropSubscription.unsubscribe();
    }
  }

  removeDragDrop() {
    const tableRows = Array.from(document.querySelectorAll('.load-details-kendo-grid.k-grid tr'));
    tableRows.forEach(item => {
      this.renderer.setAttribute(item, 'draggable', 'false');
    });
  }

  private handleDragAndDrop(): Subscription {
    const sub = new Subscription(() => { });
    let draggedItemIndex: number;
    let currentLoadIndex: number;

    const tableRows = Array.from(document.querySelectorAll('.load-details-kendo-grid.k-grid tr'));
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
        const loadClass = Array.from(row.classList).find(c => c.startsWith('load_'));
        const poFulfillmentLoadId = parseInt(loadClass.split('_')[1]);
        currentLoadIndex = this.model.loads.findIndex(l => l.id === poFulfillmentLoadId);
        const dataItem = this.model.loads[currentLoadIndex].details[draggedItemIndex];
        dataItem.dragging = true;
      }));

      sub.add(dragOver.subscribe((e: any) => {
        e.preventDefault();
        const row: HTMLTableRowElement = <HTMLTableRowElement>closest(e.target, tableRow);
        const loadClass = Array.from(row.classList).find(c => c.startsWith('load_'));
        const poFulfillmentLoadId = parseInt(loadClass.split('_')[1]);
        // Prevent drag over the other loads.
        if (poFulfillmentLoadId != this.model.loads[currentLoadIndex].id) {
          return;
        }
        const dataItem = this.model.loads[currentLoadIndex].details.splice(draggedItemIndex, 1)[0];
        const dropIndex = row.rowIndex;
        const dropItem = this.model.loads[currentLoadIndex].details[dropIndex];

        draggedItemIndex = dropIndex;
        this.zone.run(() =>
          this.model.loads[currentLoadIndex].details.splice(dropIndex, 0, dataItem)
        );
      }));

      sub.add(dragEnd.subscribe((e: any) => {
        e.preventDefault();
        const dataItem = this.model.loads[currentLoadIndex].details[draggedItemIndex];
        dataItem.dragging = false;
        this._updateSequenceForLoadDetails(this.model.loads[currentLoadIndex].details);
        this.validateLoadGridInput(currentLoadIndex);
      }));
    });

    return sub;
  }

  public rowCallback(args: RowClassArgs) {
    return {
      'dragging': args.dataItem.dragging, // dragging row will be marked with dragging property.
      [`load_${args.dataItem.poFulfillmentLoadId}`]: true // mark as which load this row belongs to.
    };
  }

  validateBeforeSaving(): ValidationData[] {
    let result: ValidationData[] = [];

    const isValid: boolean = this.validateAllFields();
    if (!isValid) {
      result.push(new ValidationData(
        ValidationDataType.Input,
        false,
        this.translateService.instant('validation.mandatoryFieldsValidation')
      ));
      return result;
    }

    return result;
  }


  ngOnDestroy(): void {
    this.dragDropSubscriptionDestroy();
    this._subscriptions.map(x => x.unsubscribe());
  }
}