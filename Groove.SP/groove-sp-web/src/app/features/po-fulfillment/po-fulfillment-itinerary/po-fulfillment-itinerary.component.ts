import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { UserContextService, DropDowns, POFulfillmentLoadStatus, POFulfillmentStageType, ModeOfTransport, ModeOfTransportType, ViewSettingModuleIdType } from 'src/app/core';
import { ControlContainer, NgForm } from '@angular/forms';
import { POFulfillmentFormService } from '../po-fulfillment-form/po-fulfillment-form.service';
import { EnumHelper } from 'src/app/core/helpers/enum.helper';
import { TranslateService } from '@ngx-translate/core';
import { DATE_FORMAT, DATE_HOUR_FORMAT_12, StringHelper } from '../../../core/helpers';
import { faCheck } from '@fortawesome/free-solid-svg-icons';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';
import { FormHelper } from 'src/app/core/helpers/form.helper';

@Component({
    selector: 'app-po-fulfillment-itinerary',
    templateUrl: './po-fulfillment-itinerary.component.html',
    styleUrls: ['./po-fulfillment-itinerary.component.scss'],
    viewProviders: [{ provide: ControlContainer, useExisting: NgForm } ]
})
export class POFulfillmentItineraryComponent implements OnInit {
    @Input('data')
    itineraryList: any[];
    @Input()
    poff: any;
    @Input()
    isViewMode: boolean;
    @Input()
    isEnabled: boolean;
    @Input()
    isInheritFromShipment: boolean;
    @Input()
    stage: POFulfillmentStageType;
    @Input()
    formErrors: any;
    @Input()
    validationRules: any;
    @Output()
    onConfirmed: EventEmitter<any> = new EventEmitter<any>();

    defaultValue = DefaultValue2Hyphens;
    
    get isCFSBooking() {
        return this.poff.movementType === 'CFS_CY' || this.poff.movementType === 'CFS_CFS';
    }

    viewSettingModuleIdType = ViewSettingModuleIdType;
    modeOfTransportOptions = DropDowns.ModeOfTransportStringType;
    modeOfTransportType = ModeOfTransportType;
    POFulfillmentLoadStatus = POFulfillmentLoadStatus;
    poFulfillmentStageType = POFulfillmentStageType;
    allCarrierOptions: any[];
    filteredCarrierOptions: any[];
    @Input()
    allPortOptions: any[];
    filteredPortOptions: any[];
    DATE_FORMAT = DATE_FORMAT;
    DATE_HOUR_FORMAT_12 = DATE_HOUR_FORMAT_12;
    faCheck = faCheck;

    constructor(private service: POFulfillmentFormService,
        private userContext: UserContextService,
        public translateService: TranslateService) {
        if (!this.itineraryList) {
            this.itineraryList = [];
        }

        this.userContext.getCurrentUser().subscribe(user => {
            if (user) {
                if (!user.isInternal) {
                    this.service.affiliateCodes = user.affiliates;
                }
            }
        });

        this.service.getAllCarriers().subscribe(data => {
            this.allCarrierOptions = data;
        });
    }

    ngOnInit(): void {
    }

    labelFromEnum(arr, value) {
        return EnumHelper.convertToLabel(arr, value);
    }

    carrierValueChange(value, rowIndex) {
        const carrier = this.allCarrierOptions.find((s) => s.name.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        this.itineraryList[rowIndex].carrierId =  carrier && carrier.id;
    }

    carrierFilterChange(value: string, rowIndex: number) {
        this.filteredCarrierOptions = [];
        if (value.length >= 3) {
            this.filteredCarrierOptions = this.allCarrierOptions.filter((s) => s.name.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        }
    }

    loadingPortValueChange(value, rowIndex) {
        const port = this.allPortOptions.find((s) => s.locationDescription.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        this.itineraryList[rowIndex].loadingPortId =  port && port.id;
    }

    dischargePortValueChange(value, rowIndex) {
        const port = this.allPortOptions.find((s) => s.locationDescription.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        this.itineraryList[rowIndex].dischargePortId =  port && port.id;
    }

    portFilterChange(value, rowIndex) {
        this.filteredPortOptions = [];
        if (value.length >= 3) {
            this.filteredPortOptions = this.allPortOptions.filter((s) => s.locationDescription.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        }
    }

    onETDDateChange(value, rowIndex) {
        this.itineraryList[rowIndex].etdDate = value;
    }

    onETADateChange(value, rowIndex) {
        this.itineraryList[rowIndex].etaDate = value;
    }

    addBlankRow() {
            this.itineraryList.push({
                isAddLine: true,
                status: POFulfillmentLoadStatus.Active
            });

            this.updateValidateGrid();
    }

    private updateValidateGrid() {
        const rowIndex = this.itineraryList.length - 1;
        this.validationRules['modeOfTransport_' + rowIndex] = {
            'required': 'label.modeOfTransport'
        };
        this.validationRules['carrier_' + rowIndex] = {
            'required': 'label.carrier'
        };
        this.validationRules['loadingPort_' + rowIndex] = {
            'required': 'label.loadingPort'
        };
        this.validationRules['etdDate_' + rowIndex] = {
            'required': 'label.etdDates'
        };
        this.validationRules['dischargePort_' + rowIndex] = {
            'required': 'label.dischargePort'
        };
        this.validationRules['etaDate_' + rowIndex] = {
            'required': 'label.etaDates'
        };
    }

    onDeleteRow(value: any) {
        this.itineraryList.splice(this.itineraryList.indexOf(value), 1);
    }

    onConfirmItineraryClick() {
        this.onConfirmed.emit();
    }

    isVisibleField(moduleId: ViewSettingModuleIdType, fieldId: string): boolean {
        if (!this.isViewMode) { // apply for view mode only
            return true;
        }

        return !FormHelper.isHiddenColumn(this.poff.viewSettings, moduleId, fieldId);
    }
}
