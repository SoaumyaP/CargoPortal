import { Component, Input, ViewChild, OnInit, OnDestroy, OnChanges, SimpleChange, QueryList, ViewChildren } from '@angular/core';
import { faEllipsisV, faPencilAlt, faTrashAlt, faAngleRight, faBars } from '@fortawesome/free-solid-svg-icons';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { EquipmentType, FormMode, ValidationDataType, POFulfillmentStageType, ViewSettingModuleIdType } from 'src/app/core/models/enums/enums';
import { POFulfillmentLoadDetailFormModel, POFulfillmentLoadDetailModel, POFulfillmentCustomerPOModel, POFulfillmentLoadModel } from '../po-fulfillment-load-detail-form/po-fulfillment-load-detail-form.model';
import { PositionUpDownModel } from 'src/app/core/directives/position-up-down.directive';
import { StringHelper, DropDowns } from 'src/app/core';
import { TranslateService } from '@ngx-translate/core';
import { EnumHelper } from 'src/app/core/helpers/enum.helper';
import { NgForm } from '@angular/forms';
import { ValidationData } from 'src/app/core/models/forms/validation-data';
import { Subscription } from 'rxjs';
import {
    DragEndEvent,
    DragOverEvent,
    DragStartEvent,
    SortableComponent 
} from '@progress/kendo-angular-sortable';
import { FormHelper } from 'src/app/core/helpers/form.helper';
@Component({
    selector: 'app-po-fulfillment-load-detail',
    templateUrl: './po-fulfillment-load-detail.component.html',
    styleUrls: ['./po-fulfillment-load-detail.component.scss']
})
export class POFulfillmentLoadDetailComponent implements OnInit, OnDestroy, OnChanges {
    @Input() public model;
    @Input() public isViewMode: boolean;
    @Input() public isReloadMode: boolean = false;
    @Input() isShowContainerInfo = false;

    @ViewChild('mainForm', { static: false }) mainForm: NgForm;
    @ViewChildren('sortable') sortables: QueryList<SortableComponent>;

    loadDetailFormOpened = false;
    /**Reference information on selected load for load plan detail popup & load container popup. */
    selectedLoad: POFulfillmentLoadModel;
    /**Index of selected load in the list */
    selectedLoadIndex: number;
    /**Reference information on customer po for load plan detail popup. */
    selectedCustomerPO: POFulfillmentCustomerPOModel;
    isShowLoadPlan = true;
    /**Model for load plan detail popup */
    selectedLoadDetailForm: POFulfillmentLoadDetailFormModel = null;
    deletedLoadDetail = null;
    equipmentTypes = EquipmentType;
    prevSelectedLoadDetail: POFulfillmentLoadDetailModel = null;
    prevSelectedLoadDetailIndex = null;
    isEditable = false;

    // Update container info popup declarations.
    loadContainerFormMode = '';
    loadContainerFormOpened = false;
    equipmentTypeOptions = DropDowns.EquipmentStringType;

    // Load cargo popup declarations.
    loadCargoPopupOpened: boolean = false;
    selectedLoadNumber: string = '';
    availablePOFFOrders: Array<POFulfillmentCustomerPOModel> = [];

    faEllipsisV = faEllipsisV;
    faPencilAlt = faPencilAlt;
    faTrashAlt = faTrashAlt;
    faAngleRight = faAngleRight;
    faBars = faBars;

    currentDraggingLoadIndex: number;
    currentDraggingLoadDetailIndex: number;

    /**Store all subscriptions, then should un-subscribe at the end */
    private _subscriptions: Array<Subscription> = [];

    viewSettingModuleIdType = ViewSettingModuleIdType;

    constructor(public notification: NotificationPopup, public translateService: TranslateService) { }
    ngOnChanges(changes: { [propName: string]: SimpleChange }): void {
        if (changes['isViewMode'] && changes['isViewMode'].previousValue != changes['isViewMode'].currentValue) {

            this.isEditable = (this.model.stage === POFulfillmentStageType.ForwarderBookingConfirmed
                && !this.model.isGeneratePlanToShip && !this.isViewMode) || this.isReloadMode;

            if (this.isEditable) {
                this.model.loads?.forEach(load => {
                    load.details.map((e, i) => e.originalIndex = i);
                });
            }
        }
    }

    public disabledIndexes(loadRowIndex: number) {
        if (!this.isEditable) {
            return Array.from(Array(this.model.loads[loadRowIndex].details.length), (_,x) => x);
        }
        return [];
    }

    ngOnInit(): void { }

    /**Closed popup without Saving or Cancelled */
    onLoadDetailFormClosed() {
        this.loadDetailFormOpened = false;
        this.selectedLoadDetailForm = null;
    }

    /**Add new Load plan details */
    onLoadDetailItemAdded(addedItem: POFulfillmentCustomerPOModel) {
        const formModel = new POFulfillmentLoadDetailFormModel(addedItem, this.selectedLoad, this.model.loads);

        this.model.loads.forEach((el, i) => {
            if (el.id === this.selectedLoad.id) {
                if (el.details === undefined) {
                    el['details'] = [];
                }

                // Remove reference information to prevent loop on saving
                formModel.load = null;
                formModel.customerPO = null;
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
        this.validateLoadedPackageByPOFFOrderId(formModel.poFulfillmentOrderId);
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
                load.details[this.prevSelectedLoadDetailIndex].unitQuantity = editedItem.unitQuantity;
                load.details[this.prevSelectedLoadDetailIndex].packageQuantity = editedItem.packageQuantity;
                load.details[this.prevSelectedLoadDetailIndex].grossWeight = editedItem.grossWeight;
                load.details[this.prevSelectedLoadDetailIndex].volume = editedItem.volume;
                if (editedItem.netWeight) {
                    load.details[this.prevSelectedLoadDetailIndex].netWeight = editedItem.netWeight;
                }
                if (editedItem.length) {
                    load.details[this.prevSelectedLoadDetailIndex].length = editedItem.length;
                }
                if (editedItem.width) {
                    load.details[this.prevSelectedLoadDetailIndex].width = editedItem.width;
                }
                if (editedItem.height) {
                    load.details[this.prevSelectedLoadDetailIndex].height = editedItem.height;
                }

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
        this.isShowLoadPlan = true;
    }

    /**Click edit Load plan details */
    onLoadDetailItemEditClick(loadId, index) {
        this.model.loads.forEach(el => {
            if (el.id === loadId) {
                this.prevSelectedLoadDetail = { ...el.details[index] };
                this.prevSelectedLoadDetailIndex = index;
                this.selectedLoad = { ...el };
            }
        });

        this.model.orders.forEach(el => {
            if (el.id === this.prevSelectedLoadDetail.poFulfillmentOrderId) {
                this.selectedCustomerPO = { ...el };
            }
        });
        this.selectedCustomerPO.openQty += this.prevSelectedLoadDetail.packageQuantity;

        this.selectedLoadDetailForm = { ...this.prevSelectedLoadDetail } as POFulfillmentLoadDetailFormModel;
        this.selectedLoadDetailForm.load = this.selectedLoad;
        this.selectedLoadDetailForm.customerPO = this.selectedCustomerPO;
        this.loadDetailFormOpened = true;
    }

    /**Click delete load plan details */
    onLoadDetailItemDeleteClick(loadId, index) {
        this.model.loads.forEach(el => {
            if (el.id === loadId) {
                this.deletedLoadDetail = el.details[index];
            }
        });

        const confirmDialog = this.notification.showConfirmationDialog('msg.deleteLoadDetail', 'label.poFulfillment');
        confirmDialog.result.subscribe((rs: any) => {
            if (rs.value) {
                this.model.orders.forEach(el => {
                    if (el.id === this.deletedLoadDetail.poFulfillmentOrderId) {
                        this.selectedCustomerPO = el;
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
                this.validateLoadedPackageByPOFFOrderId(this.selectedCustomerPO.id);
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

    onLoadContainerEdited(loadContainer: POFulfillmentLoadModel) {
        this.loadContainerFormOpened = false;
        this.model.loads[this.selectedLoadIndex].containerNumber = loadContainer.containerNumber;
        this.model.loads[this.selectedLoadIndex].sealNumber = loadContainer.sealNumber;
        this.model.loads[this.selectedLoadIndex].sealNumber2 = loadContainer.sealNumber2;
        this.model.loads[this.selectedLoadIndex].loadingDate = loadContainer.loadingDate;
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
        this.loadCargoPopupOpened = true;
        this.availablePOFFOrders = []
        this.selectedLoad = this.model.loads.find(l => l.id === loadId);

        this.model.orders.forEach(order => {
            let otherLoads = this.model.loads.filter(ld => ld.id != loadId);
            if(order.bookedPackage === 0) { // mixed carton case
                let hasLoad = otherLoads.findIndex(ld => ld.details && ld.details.some(x => x.poFulfillmentOrderId === order.id)) !== -1;
                if (!hasLoad) {
                    let model: POFulfillmentCustomerPOModel = { ...order };
                    model.loadedQty = 0;
                    model.openQty = model.bookedPackage - model.loadedQty;
                    this.availablePOFFOrders.push(model);
                }
            }
            else {
                let usedPackageQty = 0;
                otherLoads.forEach(el => {
                    usedPackageQty += el.details.find(
                        x => x.poFulfillmentOrderId === order.id)?.packageQuantity ?? 0;
                });
                if (usedPackageQty < order.bookedPackage) {
                    let model: POFulfillmentCustomerPOModel = { ...order };
                    model.loadedQty = usedPackageQty;
                    model.openQty = model.bookedPackage - model.loadedQty;
                    this.availablePOFFOrders.push(model);
                }
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
        this.validateLoadedPackageByPOFFOrderId(currentLoad.details[loadDetailIndex].poFulfillmentOrderId);
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

    onLoadCargoPopupSaved(data: POFulfillmentCustomerPOModel[]): void {
        this.loadCargoPopupOpened = false;

        if (this.selectedLoad.details?.length > 0) {
            this.reloadLoadDetails();
        }
        data.forEach(el => {
            this.onLoadDetailItemAdded({ ...el });
        });
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

    private validateLoadedPackageByPOFFOrderId(poFulfillmentOrderId: number): boolean {
        let isValid = true;
        /* Temporary comment for PSP-2733

        const poffOrder = this.model.orders.find(o => o.id === poFulfillmentOrderId);
        this.model.loads.forEach((el, i) => {
            let loadDetailIndex = el.details.findIndex(x => x.poFulfillmentOrderId === poFulfillmentOrderId);
            if (loadDetailIndex !== -1) {
                let control = this.getControlByName(`packageQuantity_${i}_${loadDetailIndex}`);
                if (poffOrder.openQty < 0) {
                    control?.setErrors({ 'greaterThanBookedQty': true });
                    el.details[loadDetailIndex].bookedPackage = poffOrder.bookedPackage
                    control?.markAsPristine();
                    control?.markAsDirty();
                } else if (control?.errors?.greaterThanBookedQty){
                    control?.setErrors(null);
                    isValid = false;
                }
            }
        });
        */
        return isValid;
    }

    private validateLoadGridInput(loadIndex: number): void {
        // reset form controls
        Object.keys(this.mainForm.controls).forEach((key) => {
            const control = this.mainForm.controls[key];
            control.markAsPristine();
            control.markAsUntouched();
        });

        this.model.loads[loadIndex].details?.forEach((el, i) => {
            el.sequence = i + 1; // update sequence
            let isValidPackageQty = this.validatePackageQty(el.packageQuantity, el.originalIndex, loadIndex);
            if (isValidPackageQty) {
                this.getControlByName(`packageQuantity_${loadIndex}_${el.originalIndex}`).setErrors(null);
            }
            this.validateUnitQty(el.unitQuantity, el.originalIndex, loadIndex);
            this.validateVolume(el.volume, el.originalIndex, loadIndex);
            this.validateGrossWeight(el.grossWeight, el.originalIndex, loadIndex);
        });
    }

    /**Validate packageQuantity value and throw error*/
    private validatePackageQty(value: number, loadDetailIndex: number, loadIndex: number): boolean {
        let control = this.getControlByName(`packageQuantity_${loadIndex}_${loadDetailIndex}`);
        if (StringHelper.isNullOrEmpty(value)) {
            control?.setErrors({ 'required': true });
            return false;
        }
        /* Temporary comment for PSP-2733

        // Business validation
        const currentLoadDetail = this.model.loads[loadIndex].details[loadDetailIndex];
        const poffOrder = this.model.orders.find(o => o.id === currentLoadDetail.poFulfillmentOrderId);
        if (poffOrder.openQty < 0) {
            control?.setErrors({ 'greaterThanBookedQty': true });
            currentLoadDetail.bookedPackage = poffOrder.bookedPackage;
            control?.markAsPristine();
            control?.markAsDirty();
            return false;
        }
        */
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
                const { packageQuantity, unitQuantity, volume, grossWeight, originalIndex } = load.details[j];
                let rs = this.validatePackageQty(packageQuantity, originalIndex, i);
                if (!rs) {
                    isValid = false;
                }
                rs = this.validateUnitQty(unitQuantity, originalIndex, i);
                if (!rs) {
                    isValid = false;
                }
                rs = this.validateVolume(volume, originalIndex, i);
                if (!rs) {
                    isValid = false;
                }
                rs = this.validateGrossWeight(grossWeight, originalIndex, i);
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
        this.loadCargoPopupOpened = false;
    }

    /**
     * On starting to drag an item.
     * */
    public onDragStart(e: DragStartEvent, loadIndex: number): void {
        if (!this.isEditable) {
            return;
        }
        this.currentDraggingLoadIndex = loadIndex;
        this.currentDraggingLoadDetailIndex = e.index;

        let item = this.model.loads[loadIndex].details[e.index];
        item.dragging = true;
    }

    /**
     * On dragging item over another item.
     * */
    public onDragOver(e: DragOverEvent, loadIndex: number) {
        e.preventDefault();
        let currentLoad = this.model.loads[this.currentDraggingLoadIndex];

        // avoid dragging to another container.
        if (this.currentDraggingLoadIndex !== loadIndex) {
            currentLoad.details[e.oldIndex].dragging = false;
            return;
        }
        this.currentDraggingLoadDetailIndex = e.index;
        
        const sortables = this.sortables.toArray();
        sortables[loadIndex].moveItem(e.oldIndex, e.index);
    }

    /**
     * When an item has been dragged.
     * */
    public onDragEnd(e: DragEndEvent): void {
        if (!this.isEditable) {
            return;
        }
        let currentLoadDetails = this.model.loads[this.currentDraggingLoadIndex].details;
        currentLoadDetails[this.currentDraggingLoadDetailIndex].dragging = false;

        this.validateLoadGridInput(this.currentDraggingLoadIndex);
    }

    currentFocusingLoadId: any;
    currentFocusingLoadItemIndex: any;
    /**
     * To set data for current focusing load detail item
     * @param dataItem Data of current cruise item row
     */
    setCurrentFocusingInfo(loadId: number, rowIndex: number) {
        this.currentFocusingLoadId = loadId;
        this.currentFocusingLoadItemIndex = rowIndex;
    }

    /**
     * To render sub-menu for "More actions" button
     * @param dataItem Data of each load detail item row
     * @returns Array of menu options
     */
    getMoreActionMenu(dataItem: any): Array<any> {
        const result = [{
            actionName: this.translateService.instant('label.edit'),
            icon: 'edit',
            click: () => {
                this.onLoadDetailItemEditClick(this.currentFocusingLoadId, this.currentFocusingLoadItemIndex);
            }
        }, {
            actionName: this.translateService.instant('tooltip.delete'),
            icon: 'delete',
            click: () => {
                this.onLoadDetailItemDeleteClick(this.currentFocusingLoadId, this.currentFocusingLoadItemIndex);
            }
        }];

        return result;
    }

    /**Subtotal quantity of Loaded Package must be greater than 0 */
    validateAllSubtotalPackageQty(): boolean {
        for (let i = 0; i < this.model.loads.length; i++) {
            let load = this.model.loads[i];
            if (!load.details || load.details.length === 0) {
                continue;
            }
            let totalPackage = 0;
            for (let j = 0; j < load.details.length; j++) {
                const loadDetail = load.details[j];
                totalPackage+=loadDetail.packageQuantity;
            }
            if (totalPackage <= 0)
            {
                return false;
            }
        }

        return true;
    }

    validateTab(): ValidationData {
        let result: ValidationData = null;

        const isValid: boolean = this.validateAllFields();
        if (!isValid) {
            result = new ValidationData(
                ValidationDataType.Input,
                false,
                this.translateService.instant('validation.mandatoryFieldsValidation')
            );
            return result;
        }

        if (!this.validateAllSubtotalPackageQty())
        {
            result = new ValidationData(ValidationDataType.Input, false, this.translateService.instant('msg.subtotalPackageMustBeGreaterThanZero'));
            return result;
        }
        
        return result;
    }

    isVisibleField(moduleId: ViewSettingModuleIdType, fieldId: string): boolean {
        if (!this.isViewMode) { // apply for view mode only
            return true;
        }
        
        return !FormHelper.isHiddenColumn(this.model.viewSettings, moduleId, fieldId);
    }

    ngOnDestroy(): void {
        this._subscriptions.map(x => x.unsubscribe());
    }
}