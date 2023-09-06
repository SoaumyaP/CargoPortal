import { Component, OnInit, OnDestroy, ViewChild, } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';
import { DropDowns, MasterReportType, POStageType, Roles, StringHelper, UserContextService } from 'src/app/core';
import { ReportCriteriaFormService } from '../report-criteria-form.service';
import { Router, ActivatedRoute } from '@angular/router';
import { AutoCompleteComponent, MultiSelectComponent } from '@progress/kendo-angular-dropdowns';
import * as cloneDeep from 'lodash/cloneDeep';
import { QueryModel } from 'src/app/core/models/forms/query.model';
import { ReportFormBase } from '../report-form-base';
import { TranslateService } from '@ngx-translate/core';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { CommonService } from 'src/app/core/services/common.service';
import { forkJoin } from 'rxjs';
import moment from 'moment';
import { TelerikReportViewerComponent } from '@progress/telerik-angular-report-viewer';
import { environment } from 'src/environments/environment';

@Component({
    selector: 'app-master-summary-report-form',
    templateUrl: './master-summary-report-form.component.html',
    styleUrls: ['./master-summary-report-form.component.scss']
})
export class MasterSummaryReportFormComponent extends ReportFormBase implements OnInit, OnDestroy {
    @ViewChild('portAutoComplete', { static: false }) public portAutoComplete: AutoCompleteComponent;
    @ViewChild('shipFromLocationSelections', { static: false }) shipFromLocationSelections: MultiSelectComponent;
    @ViewChild('shipToLocationSelections', { static: false }) shipToLocationSelections: MultiSelectComponent;

    Roles = Roles;
    portList: any[];
    filteredPortList: any[];

    acpTimeout: any;
    portLoading: boolean;
    masterReportMovementType = DropDowns.MasterReportMovementType;
    masterReportTypeDropdowns = DropDowns.MasterReportType;

    poStageOptions = DropDowns.POStageType;

    allLocations: any[];
    locationsFiltered: any;
    supplier: any[];
    supplierFiltered: any[];

    bookedPOStage = {
        any: 0,
        shipmentDispatch: 50,
        closed: 60
    };

    constructor(
        protected reportCriteriaFormService: ReportCriteriaFormService,
        protected _userContext: UserContextService,

        protected router: Router,
        protected activatedRoute: ActivatedRoute,
        private translateService: TranslateService,
        protected notification: NotificationPopup,
        private commonService: CommonService
    ) {
        super(reportCriteriaFormService, _userContext, router, activatedRoute, notification);
    }

    ngOnInitDataLoaded() {
        super.ngOnInitDataLoaded();
        this.onFormInit();

        forkJoin([this.commonService.getAllLocationSelections(), this.commonService.getSupplierByCustomerId(this.selectedCustomerId)])
            .subscribe(r => {
                const validLocations = r[0].filter(c => c.value);
                this.allLocations = validLocations;
                this.locationsFiltered = validLocations;
                this.isInitDataLoaded = true;

                this.supplier = r[1];
                if (this.supplier && this._userContext.currentUser?.role?.id === Roles.Shipper) {
                    const supplierOrganization = this.supplier.find(s =>
                        s.value === this._userContext.currentUser.organizationId.toString());
                    this.mainForm.controls['supplier'].setValue(supplierOrganization?.text);
                }
            });
    }

    onFormInit() {
        this.mainForm = new FormGroup({

            etdFrom: new FormControl(),
            etdTo: new FormControl(),
            containerNo: new FormControl(),

            etaFrom: new FormControl(),
            etaTo: new FormControl(),
            houseBLNo: new FormControl(),

            atdFrom: new FormControl(),
            atdTo: new FormControl(),

            bookingNo: new FormControl(),
            shipmentNo: new FormControl(),
            masterBLNo: new FormControl(),

            cargoReadyDateFrom: new FormControl(),
            cargoReadyDateTo: new FormControl(),
            poStage: new FormControl(),

            shipFromLocation: new FormControl(),
            shipToLocation: new FormControl(),
            movementType: new FormControl(),

            poNoFrom: new FormControl(),
            poNoTo: new FormControl(),
            supplier: new FormControl(),

            reportType: new FormControl(),
            promotionCode: new FormControl(),

        });

        setTimeout(() => {
            this.setDefaultValueForForm();
        }, 1);
    }

    setDefaultValueForForm() {
        this.mainForm.controls['poStage'].setValue(0);
        this.mainForm.controls['movementType'].setValue('');

        const reportTypeControl = this.mainForm.controls['reportType'];
        reportTypeControl.setValue(MasterReportType.POLevel);

        const promotionCodeControl = this.mainForm.controls['promotionCode'];
        if (reportTypeControl.value === MasterReportType.POLevel) {
            promotionCodeControl.disable();
        }
        this.poStageOptions = DropDowns.POStageType;
    }

    validateRangeDateInput(fromName: string, toName: string, errorMessage: string, errorDurationMessage: string) {
        delete this.formErrors[toName];
        const dateTo = this.fieldValue(toName);
        const dateFrom = this.fieldValue(fromName);
        this.onChangeBookedPoCard();

        if (StringHelper.isNullOrEmpty(dateTo)) {
            return;
        }

        if (StringHelper.isNullOrEmpty(dateFrom) || new Date(dateFrom).setHours(0, 0, 0, 0) > new Date(dateTo).setHours(0, 0, 0, 0)) {
            this.formErrors[toName] = this.translateService.instant(errorMessage);
            return;
        }

        const monthDiff = moment(dateTo).diff(moment(dateFrom), 'months', true);
        if (monthDiff >= 2) {
            this.formErrors[toName] = this.translateService.instant(errorDurationMessage);
        }
    }

    onChangeBookedPoCard() {
        const allControlsOnBookedPoCard = [
            'etdFrom', 'etdTo', 'containerNo',
            'etaFrom', 'etaTo', 'houseBLNo',
            'bookingNo', 'shipmentNo', 'masterBLNo',
        ];
        const valueOnBookedPOCard = [];
        for (let index = 0; index < allControlsOnBookedPoCard.length; index++) {
            valueOnBookedPOCard.push(this.fieldValue(allControlsOnBookedPoCard[index]));
        }

        // if user typing on any controls of BookedPO card then reload poStageOptions
        let isHaveValueOnBookedPOCard = false;
        for (let i = 0; i < valueOnBookedPOCard.length; i++) {
            if (valueOnBookedPOCard[i]) {
                isHaveValueOnBookedPOCard = true;
                this.poStageOptions = DropDowns.POStageType.filter(
                    c =>
                        c.value === this.bookedPOStage.any ||
                        c.value === this.bookedPOStage.shipmentDispatch ||
                        c.value === this.bookedPOStage.closed
                );
                break;
            }
        }
        if (!isHaveValueOnBookedPOCard) {
            this.poStageOptions = DropDowns.POStageType;
        }
    }

    onFilterSupplier(value) {
        if (!value) {
            delete this.formErrors['supplier'];
        }

        this.supplierFiltered = [];
        if (value.length >= 3) {
            this.supplierFiltered = this.supplier.filter(
                (s) => s.text.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        }
    }

    onSelectSupplier(value) {
        delete this.formErrors['supplier'];
        if (!value) {
            return;
        }

        const isValidSupplier = this.supplier.some(c => c.text === value);
        if (!isValidSupplier) {
            this.formErrors['supplier'] = this.translateService.instant('validation.supplierInvalid');
        }
    }

    onFilterShipFrom(value) {
        this.locationsFiltered = [];
        if (value.length >= 3) {
            this.locationsFiltered = this.allLocations
                .filter(s => s.label.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        } else if (!value) {
            this.locationsFiltered = this.allLocations;
            this.shipFromLocationSelections.toggle(true);
        } else {
            this.shipFromLocationSelections.toggle(false);
        }

    }

    onFilterShipTo(value) {
        this.locationsFiltered = [];
        if (value.length >= 3) {
            this.locationsFiltered = this.allLocations
                .filter(s => s.label.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        } else if (!value) {
            this.locationsFiltered = this.allLocations;
            this.shipToLocationSelections.toggle(true);
        } else {
            this.shipToLocationSelections.toggle(false);
        }
    }

    isShipFromLocationSelected(value) {
        const shipFromLocationControl = this.mainForm.controls['shipFromLocation'];

        if (shipFromLocationControl.value) {
            return shipFromLocationControl.value.some(item => item === value);
        }
    }

    isShipToLocationSelected(value) {
        const shipToLocationControl = this.mainForm.controls['shipToLocation'];

        if (shipToLocationControl.value) {
            return shipToLocationControl.value.some(item => item === value);
        }
    }

    onReportTypeChange(value) {
        const promotionCodeControl = this.mainForm.controls['promotionCode'];
        if (value === MasterReportType.ItemLevel) {
            promotionCodeControl.enable();
        } else {
            promotionCodeControl.disable();
            promotionCodeControl.setValue('');
        }
    }

    validatePORangeInput() {
        delete this.formErrors['poNoTo'];
        let poNoFrom = this.fieldValue('poNoFrom');
        let poNoTo = this.fieldValue('poNoTo');


        if (StringHelper.isNullOrEmpty(poNoTo)) {
            return;
        }

        // let to compare number
        if (!isNaN(poNoFrom) && !isNaN(poNoTo)) {
            poNoFrom = Number.parseInt(poNoFrom);
            poNoTo = Number.parseInt(poNoTo);
        }

        if (StringHelper.isNullOrEmpty(poNoFrom) || poNoFrom > poNoTo) {
            this.formErrors['poNoTo'] = this.translateService.instant('validation.poNoRangeInvalid');
        }
    }

    validateRangeDateBeforeRunReport() {
        const etdFromControl = this.mainForm.controls['etdFrom'];
        const etdToControl = this.mainForm.controls['etdTo'];
        const isEtdRangeValid = etdFromControl.value && etdToControl.value;

        const etaFromControl = this.mainForm.controls['etaFrom'];
        const etaToControl = this.mainForm.controls['etaTo'];
        const isEtaRangeValid = etaFromControl.value && etaToControl.value;

        const cargoReadyDateFromControl = this.mainForm.controls['cargoReadyDateFrom'];
        const cargoReadyDateToControl = this.mainForm.controls['cargoReadyDateTo'];
        const isCargoReadyDateRangeValid = cargoReadyDateFromControl.value && cargoReadyDateToControl.value;

        const atdFromControl = this.mainForm.controls['atdFrom'];
        const atdToControl = this.mainForm.controls['atdTo'];
        const isAtdRangeValid = atdFromControl.value && atdToControl.value;

        const isValid =
            !isEtdRangeValid &&
                !isEtaRangeValid &&
                !isCargoReadyDateRangeValid &&
                !isAtdRangeValid
                ? false : true;

        return isValid;
    }

    private handleValueAllLocation(array) {
        // split '-' if All Location have no value
        // convert string array to string separated by comma
        const newValue =
            array
                .map(c => (c.indexOf('-') !== -1) ? c.split('-')[1] : c)
                .filter(c => c)
                .toString();

        return newValue;
    }

    onExportClick() {
        const isRangeDateValid = this.validateRangeDateBeforeRunReport();
        if (!isRangeDateValid) {
            this.notification.showErrorPopup('validation.dateRangeToRunReport', 'Error');
            return;
        }

        super.onExportClick();
        const submitData = cloneDeep(this.mainForm.value);
        // Preprocessing data before it's sent to the server

        const filterDsModel = new MasterSummaryReport(
            this.selectedCustomerId,

            submitData.etdFrom,
            submitData.etdTo,
            submitData.containerNo,

            submitData.etaFrom,
            submitData.etaTo,
            submitData.houseBLNo,

            submitData.atdFrom,
            submitData.atdTo,

            submitData.bookingNo,
            submitData.shipmentNo,
            submitData.masterBLNo,

            submitData.cargoReadyDateFrom,
            submitData.cargoReadyDateTo,
            submitData.poStage,

            submitData.shipFromLocation ? this.handleValueAllLocation(submitData.shipFromLocation) : submitData.shipFromLocation,
            submitData.shipToLocation ? this.handleValueAllLocation(submitData.shipToLocation) : submitData.shipToLocation,
            submitData.movementType,

            submitData.poNoFrom,
            submitData.poNoTo,
            this.supplier?.find(s =>
                s.text == submitData.supplier)?.value,

            submitData.reportType,
            submitData.promotionCode,
        );

        this.reportCriteriaFormService.exportXlsx(
            this.selectedReportId,
            'Master Summary Report.xlsx',
            filterDsModel.buildToQueryParams,
        ).subscribe(response => {
            if (!response) {
                this.notification.showWarningPopup(
                    'msg.noPOFound',
                    'label.result'
                );
            }
            this.resetAfterDownload();
        }, error => {
            this.notification.showErrorPopup(
                'msg.exportFailed',
                'label.result'
            );
            this.resetAfterDownload();
        });
    }

}


export class MasterSummaryReport extends QueryModel {
    private SelectedCustomerId: number;

    private ETDFrom: string;
    private ETDTo: string;
    private ContainerNo: string;

    private ETAFrom: string;
    private ETATo: string;
    private HouseBLNo: string;

    private ATDFrom: string;
    private ATDTo: string;

    private BookingNo: string;
    private ShipmentNo: string;
    private MasterBLNo: string;

    private CargoReadyDateFrom: string;
    private CargoReadyDateTo: string;
    private POStage: number;

    private ShipFromLocation: string;
    private ShipToLocation: string;
    private MovementType: string;

    private PONoFrom: string;
    private PONoTo: string;
    private Supplier: number;

    private ReportType: string;
    private PromotionCode: string;
    /**
     * CONSTRUCTOR
     */
    constructor(
        customerId: number,

        etdFrom: Date,
        etdTo: Date,
        containerNo: string,

        etaFrom: Date,
        etaTo: Date,
        houseBLNo: string,

        atdFrom: Date,
        atdTo: Date,

        bookingNo: string,
        shipmentNo: string,
        masterBLNo: string,

        cargoReadyDateFrom: Date,
        cargoReadyDateTo: Date,
        poStage: number,

        shipFromLocation: string,
        shipToLocation: string,
        movementType: string,

        poNoFrom: string,
        poNoTo: string,
        supplier: number,

        reportType: string,
        promotionCode: string,
    ) {
        super();
        this.SelectedCustomerId = customerId;

        this.ETDFrom = StringHelper.isNullOrEmpty(etdFrom) ? null : this.convertToQueryDateString(etdFrom);
        this.ETDTo = StringHelper.isNullOrEmpty(etdTo) ? null : this.convertToQueryDateString(etdTo);
        this.ContainerNo = StringHelper.isNullOrEmpty(containerNo) ? null : containerNo;

        this.ETAFrom = StringHelper.isNullOrEmpty(etaFrom) ? null : this.convertToQueryDateString(etaFrom);
        this.ETATo = StringHelper.isNullOrEmpty(etaTo) ? null : this.convertToQueryDateString(etaTo);
        this.HouseBLNo = StringHelper.isNullOrEmpty(houseBLNo) ? null : houseBLNo;

        this.ATDFrom = StringHelper.isNullOrEmpty(atdFrom) ? null : this.convertToQueryDateString(atdFrom);
        this.ATDTo = StringHelper.isNullOrEmpty(atdTo) ? null : this.convertToQueryDateString(atdTo);

        this.BookingNo = StringHelper.isNullOrEmpty(bookingNo) ? null : bookingNo;
        this.ShipmentNo = StringHelper.isNullOrEmpty(shipmentNo) ? null : shipmentNo;
        this.MasterBLNo = StringHelper.isNullOrEmpty(masterBLNo) ? null : masterBLNo;

        this.CargoReadyDateFrom = StringHelper.isNullOrEmpty(cargoReadyDateFrom) ? null : this.convertToQueryDateString(cargoReadyDateFrom);
        this.CargoReadyDateTo = StringHelper.isNullOrEmpty(cargoReadyDateTo) ? null : this.convertToQueryDateString(cargoReadyDateTo);
        this.POStage = poStage <= 0 ? null : poStage;

        this.ShipFromLocation = StringHelper.isNullOrEmpty(shipFromLocation) ? null : shipFromLocation;
        this.ShipToLocation = StringHelper.isNullOrEmpty(shipToLocation) ? null : shipToLocation;
        this.MovementType = StringHelper.isNullOrEmpty(movementType) ? null : movementType;

        this.PONoFrom = StringHelper.isNullOrEmpty(poNoFrom) ? null : poNoFrom;
        this.PONoTo = StringHelper.isNullOrEmpty(poNoTo) ? null : poNoTo;
        this.Supplier = supplier <= 0 ? null : supplier;

        this.ReportType = StringHelper.isNullOrEmpty(reportType) ? null : reportType;
        this.PromotionCode = StringHelper.isNullOrEmpty(promotionCode) ? null : promotionCode;
    }
}
