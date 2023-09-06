import { Component, OnDestroy, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { ComboBoxComponent } from '@progress/kendo-angular-dropdowns';
import { EMPTY, of, Subject, Subscription } from 'rxjs';
import { tap, switchMap, delay } from 'rxjs/operators';
import { DateHelper, DropDownListItemModel, DropdownListModel, DropDowns, FormComponent, ModeOfTransportType, ShipmentStatus, StringHelper } from 'src/app/core';
import { EVENT_2054, EVENT_7001, EVENT_7002, GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { ShipmentMilestoneModel, ShipmentModel } from 'src/app/core/models/shipments/shipment.model';
import { CommonService } from 'src/app/core/services/common.service';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { ContractMasterModel } from '../master-bill-of-lading/models/contract-master-model';
import { ShipmentTrackingService } from '../shipment-tracking/shipment-tracking.service';

/**
 * Component to edit shipment
 */
@Component({
    selector: 'app-shipment-form',
    templateUrl: './shipment-form.component.html',
    styleUrls: ['./shipment-form.component.scss']
})
export class ShipmentFormComponent extends FormComponent implements OnDestroy {

    modelName = 'shipments';
    shipmentStatus = ShipmentStatus;
    model: ShipmentModel;

    validationRules = {
        ciNo: {
            maxLengthInput: 50
        },
        movement: {
            required: "label.movement"
        }
    };

    readonly modeOfTransportType = ModeOfTransportType;
    readonly stringHelper = StringHelper;
    /**
     * Data source for carrier contract no
     */
    carrierContractNoDataSource: Array<DropDownListItemModel<string>> = [];
    movementTypeDataSource: any[] = [];
    carrierContractNoKeyUp$ = new Subject<string>();
    contractMasterList: Array<ContractMasterModel> = [];
    // Master BL number
    @ViewChild('carrierContractNoComboBox', { static: false })
    public carrierContractNoComboBox: ComboBoxComponent;
    isCarrierContractNoLoading: boolean = false;

    buyerCompliance = null;

    private _subscriptions: Array<Subscription> = [];

    constructor(
        protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: ShipmentTrackingService,
        public translateService: TranslateService,
        private _gaService: GoogleAnalyticsService
    ) {
        super(route, service, notification, translateService, router);
        this._registerEventHandlers();
    }

    onInitDataLoaded(data): void {
        this._fetchDataSources();
        this._registerEventHandlers();
    }

    get standardizationShipmentStatus(): string {
        return StringHelper.toUpperCaseFirstLetter(this.model.status);
    }

    get isMaterBLAssignedToShipment(): boolean {
        return this.model?.masterBillNos?.length > 0 || false;
    }

    private _fetchDataSources() {
        this.carrierContractNoDataSource = [];
        if (this.model?.contractMaster) {
            this.carrierContractNoDataSource.push(
                {
                    text : this.model.contractMaster.realContractNo,
                    value : this.model.carrierContractNo
                }
            );
            this.model.carrierContractType = this.model.contractMaster.contractType;
        }

        this.movementTypeDataSource = [];
        if (!StringHelper.caseIgnoredCompare(this.model.modeOfTransport, ModeOfTransportType.Air)) {
            this.movementTypeDataSource = this.model.isFCL ? DropDowns.CYMovementStringType : DropDowns.CFSMovementStringType;
        }
    }

    backList() {
        this.router.navigate(['/shipments']);
    }

    onBtnCancellingShipmentEditClick() {
        const confirmDlg = this.notification.showConfirmationDialog('edit.cancelConfirmation', 'label.shipment');

        const sub = confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.router.navigate([`/shipments/${this.model.id}`]);
                }
            });
        this._subscriptions.push(sub);
    }

    onCarrierContractNoValueChange(carrierContractNo: string) {
        const selectedContractMaster = this.contractMasterList.find(x => x.carrierContractNo === carrierContractNo);
        this.model.carrierContractType = selectedContractMaster?.contractType;
        this.model.carrierContractNo = selectedContractMaster?.carrierContractNo || '' ;
    }

    onBtnSavingShipmentClick() {
        this.validateAllFields(false);
        let isValid = true;
        if (this.mainForm.invalid) {
            isValid = false;
        }
        if (!this.validateDataBeforeSaving())
        {
            isValid = false;
        }

        if (!isValid) {
            return;
        }

        let tmpModel = DateHelper.formatDate(this.model);
        this.service.updateShipment(tmpModel.id, tmpModel).subscribe(
            success => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.shipment');
                this._gaService.emitEvent('edit', GAEventCategory.Shipment, 'Edit');
                this.router.navigate([`/shipments/${tmpModel.id}`]);
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.shipment');
            }
        );
    }

    validateDataBeforeSaving(): boolean {
        let isValid = true;
        if (!this.validateCINoInput()) {
            isValid = false;
        }
        return isValid;
    }
    
    validateCINoInput(): boolean {
        let isValid = true;
        if (this.model.commercialInvoiceNo?.length > 50) {
            this.setInvalidControl('ciNo', 'maxLengthInput');
            isValid = false;
        }
        return isValid;
    }

    ngOnDestroy() {
        this._subscriptions.map(x => x.unsubscribe());
    }

    private _registerEventHandlers(): void {
        const sub = this.carrierContractNoKeyUp$.pipe(
            tap(() => {
                this.carrierContractNoComboBox.toggle(false);
            }),
            switchMap((searchTerm: string) => {
                if (!StringHelper.isNullOrEmpty(searchTerm) && searchTerm.length >= 3) {
                    return of(searchTerm).pipe(delay(1000));
                } else {
                    return EMPTY;
                }
            }
        )).subscribe((searchTerm: string) => {
            this._carrierContractNoFilterChange(searchTerm);
        });
        this._subscriptions.push(sub);
    }

    private _carrierContractNoFilterChange(searchTerm: string) {
        this.isCarrierContractNoLoading = true;
        this.service.getContractMasters(searchTerm, this.model.id, new Date()).subscribe(
            (data) => {
                this.contractMasterList = data;
                this.carrierContractNoDataSource = this.contractMasterList.map(x => new DropDownListItemModel(x.realContractNo, x.carrierContractNo)) || [];
                this.carrierContractNoComboBox.toggle(true);
                this.isCarrierContractNoLoading = false;
            }
        );
    }

    /**To check whether the Commercial Invoice info is able to edit */
    get canEditCIInfo() {
        if (!this.model || this.model.enforceCommercialInvoiceFormat === undefined) {
            return false;
        }
        return !this.model.enforceCommercialInvoiceFormat;
    }
}