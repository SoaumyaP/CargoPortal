import { Component, Output, Input, EventEmitter, ViewChild, OnInit } from '@angular/core';
import { faPlus, faEllipsisV, faPencilAlt, faTrashAlt } from '@fortawesome/free-solid-svg-icons';
import { POFulfillmentStageType, PackageDimensionUnitType } from 'src/app/core/models/enums/enums';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';

@Component({
    selector: 'app-po-fulfillment-cargo-detail',
    templateUrl: './po-fulfillment-cargo-detail.component.html',
    styleUrls: ['./po-fulfillment-cargo-detail.component.scss']
})
export class POFulfillmentCargoDetailComponent implements OnInit {
    @Input() public isViewMode: boolean;
    @Input() public model: [];
    @Input() public loads: Array<any>;
    @Input() public poFulfillmentStage: POFulfillmentStageType;
    @Output() itemAdded: EventEmitter<any> = new EventEmitter<any>();
    @Output() itemDeleted: EventEmitter<any> = new EventEmitter<any>();
    @Output() itemEdited: EventEmitter<any> = new EventEmitter<any>();
    loadOptions: Array<any> = [];

    faPlus = faPlus;
    faEllipsisV = faEllipsisV;
    faPencilAlt = faPencilAlt;
    faTrashAlt = faTrashAlt;

    cargoDetailFormOpened: boolean;
    cargoDetailItem = this.getDeftaulCargoDetail();
    isEditing: boolean;
    isViewing: boolean;
    isEditable = false;

    constructor(public notification: NotificationPopup) { }

    ngOnInit() {
        this.isEditable = this.poFulfillmentStage == POFulfillmentStageType.ForwarderBookingConfirmed;

        this.loads.forEach(item => {
            this.loadOptions.push({label: item.loadReferenceNumber, value: item.loadReferenceNumber});
        });

        this.model.forEach((item:any) =>{
            item.volume = parseFloat(item.volume.toFixed(3));
            item.grossWeight = parseFloat(item.grossWeight.toFixed(2));
            item.netWeight = parseFloat(item.netWeight.toFixed(2));
        })
    }

    onAddCargoDetailClick() {
        this.isEditing = false;
        this.cargoDetailFormOpened = true;
        this.cargoDetailItem = this.getDeftaulCargoDetail();
        const currentItem = this.model[this.model.length - 1];
        if (currentItem) {
            const currentLineOrder = +currentItem['lineOrder'];
            this.cargoDetailItem.lineOrder = currentLineOrder + 1;
        } else {
            this.cargoDetailItem.lineOrder = 1;
        }
    }

    onCargoDetailFormClosed() {
        this.cargoDetailFormOpened = false;
        this.isViewing = false;
    }

    onCargoDetailFormAdded(cargoDetailModel) {
        this.cargoDetailFormOpened = false;
        this.itemAdded.emit(cargoDetailModel);
    }

    onCargoDetailFormEdited(cargoDetailModel) {
        this.cargoDetailFormOpened = false;
        this.itemEdited.emit(cargoDetailModel);
    }

    onEditItemClick(lineOrder) {
        this.cargoDetailFormOpened = true;
        this.isEditing = true;
        const selectedItem = this.model.find(el => {
            return el['lineOrder'] === lineOrder;
        });

        this.cargoDetailItem = Object.assign({}, selectedItem);
    }

    onViewItemClick(lineOrder) {
        this.cargoDetailFormOpened = true;
        this.isViewing = true;
        const selectedItem = this.model.find(el => {
            return el['lineOrder'] === lineOrder;
        });

        this.cargoDetailItem = Object.assign({}, selectedItem);
    }

    onDeleteItemClick(lineOrder) {
        const confirmDialog = this.notification.showConfirmationDialog('msg.deleteCargoDetail', 'label.poFulfillment');
        confirmDialog.result.subscribe((rs: any) => {
            if (rs.value) {
                this.itemDeleted.emit(lineOrder);
            }
        });
    }

    private getDeftaulCargoDetail() {
        return {
            id: 0,
            lineOrder: 0,
            packageQuantity: '',
            packageUOM: '',
            hsCode: '',
            height: '',
            width: '',
            length: '',
            dimensionUnit: PackageDimensionUnitType.CM,
            unitQuantity: '',
            unitUOM: '',
            volume: '',
            grossWeight: '',
            netWeight: '',
            countryCodeOfOrigin: '',
            packageDescription: '',
            shippingMarks: '',
            commodity: '',
            loadReferenceNumber: ''
        };
    }
}
