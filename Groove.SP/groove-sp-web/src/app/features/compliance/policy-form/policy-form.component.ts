import { Component, Input, Output, EventEmitter, OnInit, ViewChild } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { FormComponent, UserContextService, DropDowns, ApproverSetting, ValidationResultPolicy, BuyerComplianceServiceType } from 'src/app/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { ComplianceFormService } from '../compliance-form/compliance-form.service';
import { MultiSelectComponent } from '@progress/kendo-angular-dropdowns';
import { EmailValidationPattern, MultipleEmailValidationPattern } from 'src/app/core/models/constants/app-constants';

@Component({
    selector: 'app-policy-form',
    templateUrl: './policy-form.component.html',
    styleUrls: ['./policy-form.component.scss']
})
export class PolicyFormComponent extends FormComponent implements OnInit {
    @Input() public policyFormOpened: boolean = false;
    @Output() close: EventEmitter<any> = new EventEmitter();
    @Output() add: EventEmitter<any> = new EventEmitter<any>();
    @Output() edit: EventEmitter<any> = new EventEmitter<any>();
    @Input() public serviceType: BuyerComplianceServiceType;

    @Input() public formMode: any;
    PolicyFormModeType = {
        add: 0,
        edit: 1,
        view: 2
    };

    get viewFormMode() {
        return this.formMode === this.PolicyFormModeType.view;
    }

    @Input() public model: any;

    validationRules = {
        'name': {
            'required': 'label.name'
        },
        'action': {
            'required': 'label.action'
        },
        'approverUser': {
            'required': 'label.approverUser',
            'pattern': 'label.approverUser'
        }
    };

    @ViewChild('policyLogisticsServiceType', { static: false }) policyLogisticsServiceType: MultiSelectComponent;
    @ViewChild('incoterm', { static: false }) incoterm: MultiSelectComponent;
    @ViewChild('shipFrom', { static: false }) shipFrom: MultiSelectComponent;
    @ViewChild('shipTo', { static: false }) shipTo: MultiSelectComponent;
    @ViewChild('carrier', { static: false }) carrier: MultiSelectComponent;

    modeOfTransportOptions = DropDowns.ModeOfTransport;
    movementTypeOptions = DropDowns.MovementType;
    incotermTypeOptions = DropDowns.IncotermType;
    logicticsServiceTypeOptions = DropDowns.LogisticsService;
    allLocationOptions: any;
    allCarrierOptions: any;
    itineraryIsEmptyTypeOptions = DropDowns.ItineraryType;
    fulfillmentAccuracyOptions = DropDowns.FulfillmentAccuracy;
    cargoLoadabilityOptions = DropDowns.CargoLoadability;
    bookingTimelessOptions = DropDowns.BookingTimeless;
    approverSettingOptions = DropDowns.ApproverSetting;
    approverSetting = ApproverSetting;
    validationResult = ValidationResultPolicy;
    mainAllLocationOptions: any;
    mainAllCarrierOptions: any;
    patternEmail = MultipleEmailValidationPattern;

    public get actionOptions() {
        switch (this.serviceType) {
            case BuyerComplianceServiceType.Freight:
                return DropDowns.ValidationResult.filter(x => x.value !== this.validationResult.WarehouseApproval);
            case BuyerComplianceServiceType.WareHouse:
                return DropDowns.ValidationResult.filter(x => x.value !== this.validationResult.BookingRejected);
            default:
                return DropDowns.ValidationResult;
        }
    }

    isMovementTypeSelected(id): boolean {
        return this.model.movementTypeIds.some(item => item === id);
    }

    isIncotermSelected(id): boolean {
        return this.model.incotermTypeIds.some(item => item === id);
    }

    isShipFromSelected(description): boolean {
        return this.model.shipFromIds.some(item => item === description);
    }

    isShipToSelected(description): boolean {
        return this.model.shipToIds.some(item => item === description);
    }

    isCarrierSelected(description): boolean {
        return this.model.carrierIds.some(item => item === description);
    }

    isFulfillmentAccuracySelected(id): boolean {
        return this.model.fulfillmentAccuracyIds.some(item => item === id);
    }

    isLogisticsServiceTypeSelected(id): boolean {
        return this.model.logisticsServiceSelectionIds.some(item => item === id);
    }

    isModeOfTransportSelected(id): boolean {
        return this.model.modeOfTransportIds.some(item => item === id);
    }

    isCargoLoadabilitySelected(id): boolean {
        return this.model.cargoLoadabilityIds.some(item => item === id);
    }

    isBookingTimelessSelected(id): boolean {
        return this.model.bookingTimelessIds.some(item => item === id);
    }

    constructor(protected route: ActivatedRoute,
                public notification: NotificationPopup,
                public router: Router,
                public service: ComplianceFormService,
                public translateService: TranslateService,
                private _userContext: UserContextService) {
        super(route, service, notification, translateService, router);

        this.service.getAllLocations().subscribe(data => {
            this.mainAllLocationOptions = data;
            this.allLocationOptions = data;
        });
        this.service.getAllCarriers().subscribe(data => {
            this.mainAllCarrierOptions = data;
            this.allCarrierOptions = data;
        });
    }

    ngOnInit() {
    }

    onFormClosed() {
        this.policyFormOpened = false;
        this.close.emit();
    }

    onSave () {
        if (this.mainForm.valid) {
            switch (this.formMode) {
                case this.PolicyFormModeType.add:
                    this.add.emit(this.model);
                    break;
                case this.PolicyFormModeType.edit:
                    this.edit.emit(this.model);
                    break;
            }
        }
    }

    onLogisticsServiceTypeFilterChange(value) {
        if (value.length >= 3) {
            this.logicticsServiceTypeOptions = DropDowns.LogisticsService
            .filter((s) => this.translateService.instant(s.label).toLowerCase().indexOf(value.toLowerCase()) !== -1);
        } else if (!value) {
            this.logicticsServiceTypeOptions = DropDowns.LogisticsService;
            this.policyLogisticsServiceType.toggle(true);
        } else {
            this.policyLogisticsServiceType.toggle(false);
        }
    }

    onIncotermFilterChange(value) {
        if (value.length >= 3) {
            this.incotermTypeOptions = DropDowns.IncotermType
            .filter((s) => s.label.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        } else if (!value) {
            this.incotermTypeOptions = DropDowns.IncotermType;
            this.incoterm.toggle(true);
        } else {
            this.incoterm.toggle(false);
        }
    }

    onShipFromFilterChange(value) {
        if (value.length >= 3) {
            this.allLocationOptions = this.mainAllLocationOptions
            .filter((s) => s.label.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        } else if (!value) {
            this.allLocationOptions = this.mainAllLocationOptions;
            this.shipFrom.toggle(true);
        } else {
            this.shipFrom.toggle(false);
        }
    }

    onShipToFilterChange(value) {
        if (value.length >= 3) {
            this.allLocationOptions = this.mainAllLocationOptions
            .filter((s) => s.label.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        } else if (!value) {
            this.allLocationOptions = this.mainAllLocationOptions;
            this.shipTo.toggle(true);
        } else {
            this.shipTo.toggle(false);
        }
    }

    onCarrierFilterChange(value) {
        if (value.length >= 3) {
            this.allCarrierOptions = this.mainAllCarrierOptions
            .filter((s) => s.label.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        } else if (!value) {
            this.allCarrierOptions = this.mainAllCarrierOptions;
            this.carrier.toggle(true);
        } else {
            this.carrier.toggle(false);
        }
    }
}
