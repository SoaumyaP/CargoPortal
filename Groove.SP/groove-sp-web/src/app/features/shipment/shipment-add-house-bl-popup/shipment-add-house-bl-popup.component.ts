import { Component, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges, ViewChild } from '@angular/core';
import { AbstractControl, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { DateHelper, DATE_FORMAT, DropDowns, ModeOfTransportType, StringHelper, UserContextService } from 'src/app/core';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';
import { LocalDate } from 'src/app/core/models/local-date.model';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { BillOfLadingFormService } from '../bill-of-lading/bill-of-lading-form.service';
import { ShipmentTrackingService } from '../shipment-tracking/shipment-tracking.service';
import { ShipmentAddHouseBLModel } from './models/shipment-add-house-bl.model';
import { ShipmentLoadDetailModel } from 'src/app/core/models/shipments/shipment-load-detail.model';
import { FormHelper } from 'src/app/core/helpers/form.helper';

@Component({
    selector: 'app-shipment-add-house-bl-popup',
    templateUrl: './shipment-add-house-bl-popup.component.html',
    styleUrls: ['./shipment-add-house-bl-popup.component.scss']
})
export class ShipmentAddHouseBLPopupComponent implements OnChanges, OnInit, OnDestroy {
    @Input() isOpenHouseBLPopup: boolean;
    @Input() consignments: Array<any> = [];
    @Input() currentUser: any = {};
    @Input() shipmentModel: any = {};

    @Output() houseBLPopupClosed: EventEmitter<any> = new EventEmitter<any>();
    @Output() houseBLPopupSavedSuccessfully: EventEmitter<any> = new EventEmitter<any>();

    @ViewChild('mainForm', { static: false }) mainForm: NgForm;

    defaultValue: string = DefaultValue2Hyphens;
    dateFormat = DATE_FORMAT;
    defaultDropDownItem: { executionAgent: string, executionAgentId: number } =
    {
        executionAgent: this.translateService.instant('label.select'),
        executionAgentId: null
    };
    houseBLModel: ShipmentAddHouseBLModel = new ShipmentAddHouseBLModel();
    isDisabledResetButton: boolean = true;
    isDisabledAddNewButton: boolean = false;
    isDisabledConsignmentDropdown: boolean;
    isHouseBLNoChanged: boolean;
    isShowSelectMode: boolean;
    isShowAddNewMode: boolean;
    filteredHouseBLs: Array<ShipmentAddHouseBLModel> = [];
    billOfLadingTypes: Array<any> = DropDowns.BillOfLadingType;
    searchHouseBLEvent$ = new Subject<string>();
    checkHouseBLEvent$ = new Subject<string>();
    houseBLNoManualInput: string;
    shipmentLoadDetails: Array<ShipmentLoadDetailModel> = [];
    subscriptions: Array<Subscription> = [];
    modeOfTransport = ModeOfTransportType;
    stringHelper = StringHelper;

  constructor(
    private billOfLadingFormService: BillOfLadingFormService,
    private shipmentTrackingService: ShipmentTrackingService,
    private notification: NotificationPopup,
    private router: Router,
    public translateService: TranslateService,
    private userContext: UserContextService,
  ) {
    this.userContext.getCurrentUser().subscribe(user => {
      if (user) {
        this.currentUser = user;
        if (!user.isInternal) {
          this.billOfLadingFormService.affiliateCodes = user.affiliates;
        }
      }
    });
  }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes?.consignments?.currentValue) {
            // Filter on mode of transport Sea/Air
            this.consignments = this.consignments?.filter(c => StringHelper.caseIgnoredCompare(c.modeOfTransport, ModeOfTransportType.Sea) || StringHelper.caseIgnoredCompare(c.modeOfTransport, ModeOfTransportType.Air));
            this._autoSelectOption();
        }

        // Fetch data for shipment load details to save house bl
        if (changes?.isOpenHouseBLPopup?.currentValue) {
            this.shipmentTrackingService.getShipmentLoadDetails(this.shipmentModel.id).subscribe(
                (data: Array<ShipmentLoadDetailModel>) => {
                    this.shipmentLoadDetails = data;
                }
            );
        }
    }

    ngOnInit() {
        this.handleSearchHouseBLEvent();
        this.handleCheckHouseBLEvent();
        this.houseBLModel.billOfLadingNo = '';
    }

    onSelectHouseBL() {
        FormHelper.ValidateAllFields(this.mainForm);
        if (!(this.mainForm.valid && this.houseBLModel.id)) {
            return;    
        }

        this.shipmentTrackingService.assignHouseBL(this.shipmentModel.id, this.houseBLModel).subscribe(
            r => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.houseBillOfLading');
                this.houseBLPopupSavedSuccessfully.emit();
            },
            err => {
                this.notification.showErrorPopup('save.failureNotification', 'label.houseBillOfLading');
            }
        );
    }

    onAddNewHouseBL() {
        this.isDisabledAddNewButton = true;
        this.isDisabledResetButton = false;
        this.isShowAddNewMode = true;
        this.houseBLModel.billOfLadingNo = null;
        this.houseBLNoManualInput = null;
        this.resetForm();
    }

    onChangeHouseBLNo(value: string) {
        if (this.houseBLNoManualInput.length === 0) {
            this.isDisabledResetButton = true;
            this.isShowSelectMode = false;
            this.setValueHouseBL(null);
            return;
        } else {
            this.isDisabledResetButton = false;
            this.isShowSelectMode = true;

            const isValidHouseBLNo = this.filteredHouseBLs.some(c => c.billOfLadingNo === this.houseBLNoManualInput);
            if (!isValidHouseBLNo) {
                this.isShowSelectMode = false;
                this.setValueHouseBL(null);
                return;
            }
        }

        const data = this.filteredHouseBLs.find(c => c.billOfLadingNo === this.houseBLNoManualInput);
        this.setValueHouseBL(data);
    }

    handleSearchHouseBLEvent() {
        const sub = this.searchHouseBLEvent$.pipe(
            debounceTime(1000),
            tap((houseBLNo: string) => {
                this.onFilterHouseBLNo(houseBLNo);
            }
            )).subscribe();
        this.subscriptions.push(sub);
    }

    handleCheckHouseBLEvent() {
        const sub = this.checkHouseBLEvent$.pipe(
            debounceTime(1000),
            tap((houseBLNo: string) => {
                this.onCheckHouseBLNoAlreadyExists(houseBLNo);
            }
            )).subscribe();
        this.subscriptions.push(sub);
    }

    onFilterHouseBLNo(houseBLNo: string) {
        this.filteredHouseBLs = [];
        this.isDisabledAddNewButton = houseBLNo?.length !== 0;

        if (houseBLNo?.length >= 3) {
            const shipFromETDDate = new LocalDate(this.shipmentModel.shipFromETDDate).toISOString();
            const shipToETADate = new LocalDate(this.shipmentModel.shipToETADate).toISOString();
            this.billOfLadingFormService.filterHouseBL(
                houseBLNo,
                this.shipmentModel.modeOfTransport,
                this.houseBLModel.executionAgentId
            ).subscribe((c: any) => {
                this.filteredHouseBLs = c;
            });
        }
    }

    onCheckHouseBLNoAlreadyExists(houseBLNo: string) {
        if (houseBLNo?.length >= 3) {
            this.billOfLadingFormService.checkHouseBLAlreadyExists(
                houseBLNo
            ).subscribe((c: any) => {
                if (c) {
                    this.getControl('billOfLadingNo').setErrors({ isDuplicated: true });
                } else {
                    this.getControl('billOfLadingNo').setErrors(null);
                }
            });
        }
    }

    onChangeExecutionAgent(value: number) {
        this.onResetHouseBL();
    }

    onSaveHouseBL() {
        FormHelper.ValidateAllFields(this.mainForm);
        if (!this.mainForm.valid) {
            return;
        }

        this.setValueHouseBLBeforeSave();
        this.houseBLModel = DateHelper.formatDate(this.houseBLModel);
        this.shipmentTrackingService.createAndAssignHouseBL(this.shipmentModel.id, this.houseBLModel).subscribe(
            r => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.houseBillOfLading');
                this.houseBLPopupSavedSuccessfully.emit();
            },
            err => {
                this.notification.showErrorPopup('save.failureNotification', 'label.houseBillOfLading');
            }
        );
    }

    setValueHouseBL(data: ShipmentAddHouseBLModel) {
        this.houseBLModel.id = data?.id;
        this.houseBLModel.billOfLadingNo = data?.billOfLadingNo;
        this.houseBLModel.jobNumber = data?.jobNumber;
        this.houseBLModel.issueDate = data?.issueDate;
        this.houseBLModel.modeOfTransport = data?.modeOfTransport;
        this.houseBLModel.billOfLadingType = data?.billOfLadingType;
        this.houseBLModel.shipFromETDDate = data?.shipFromETDDate;
        this.houseBLModel.shipToETADate = data?.shipToETADate;
        this.houseBLModel.originAgent = data?.originAgent;
        this.houseBLModel.destinationAgent = data?.destinationAgent;
        this.houseBLModel.customer = data?.customer;
        this.houseBLModel.shipFrom = data?.shipFrom;
        this.houseBLModel.shipTo = data?.shipTo;
        this.houseBLModel.movement = data?.movement;
        this.houseBLModel.incoterm = data?.incoterm;
    }

    setValueHouseBLBeforeSave() {
        const firstShipmentLoadDetail = this.shipmentLoadDetails[0];
        this.houseBLModel.totalGrossWeight = 0;
        this.houseBLModel.totalGrossWeightUOM = firstShipmentLoadDetail?.grossWeightUOM;
        this.houseBLModel.totalNetWeight = 0;
        this.houseBLModel.totalNetWeightUOM = firstShipmentLoadDetail?.netWeightUOM;
        this.houseBLModel.totalPackage = 0;
        this.houseBLModel.totalPackageUOM = firstShipmentLoadDetail?.packageUOM;
        this.houseBLModel.totalVolume = 0;
        this.houseBLModel.totalVolumeUOM = firstShipmentLoadDetail?.volumeUOM;
        this.houseBLModel.modeOfTransport = this.shipmentModel.modeOfTransport;
        this.houseBLModel.shipFrom = this.shipmentModel.shipFrom;
        this.houseBLModel.shipFromETDDate = this.shipmentModel.shipFromETDDate;
        this.houseBLModel.shipTo = this.shipmentModel.shipTo;
        this.houseBLModel.shipToETADate = this.shipmentModel.shipToETADate;
        this.houseBLModel.movement = this.shipmentModel.movement;
        this.houseBLModel.incoterm = this.shipmentModel.incoterm;
    }

    onCancelHouseBLPopup() {
        this.closeHouseBLPopup();
    }

    closeHouseBLPopup() {
        this.houseBLPopupClosed.emit();
        this.setValueHouseBL(null);
        this.isShowSelectMode = false;
        this.isShowAddNewMode = false;
        this.isDisabledResetButton = true;
        this.isDisabledAddNewButton = false;
        this.houseBLNoManualInput = null;
        this._autoSelectOption();
    }

    onResetHouseBL() {
        this.isDisabledResetButton = true;
        this.isDisabledAddNewButton = false;
        this.isShowSelectMode = false;
        this.isShowAddNewMode = false;
        this.setValueHouseBL(null);
        this.houseBLNoManualInput = null;
    }

    resetForm() {
        this.mainForm.form.setErrors(null);
        this.mainForm.form.markAsPristine();
        this.mainForm.form.markAsUntouched();
    }

    getControl(controlName: string): AbstractControl {
        return this.mainForm?.form?.controls[controlName];
    }

    ngOnDestroy(): void {
        this.subscriptions.map(x => x.unsubscribe());
    }

    private _autoSelectOption() {
        this.houseBLModel.executionAgentId = null;
            if (this.currentUser?.isInternal) {
                this.isDisabledConsignmentDropdown = this.consignments.length === 1;
                if (this.consignments.length === 1) {
                    this.houseBLModel.executionAgentId = this.consignments[0].executionAgentId;
                }
            } else {
                this.isDisabledConsignmentDropdown = true;
                this.houseBLModel.executionAgentId = this.currentUser.organizationId;
            }
    }
}
