import {
    Component,
    OnInit,
    Input,
    OnChanges,
    OnDestroy,
    SimpleChanges,
    Output,
    EventEmitter,
} from '@angular/core';
import {
    DropDowns,
    UserContextService,
    StringHelper,
} from 'src/app/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { Subject, Subscription } from 'rxjs';
import { OrganizationNameRole, EquipmentType, POType, ModeOfTransportType, ValidationDataType, ViewSettingModuleIdType } from 'src/app/core/models/enums/enums';
import { isUndefined } from 'util';
import { IntegrationData } from 'src/app/core/models/forms/integration-data';
import { filter } from 'rxjs/operators';
import * as appConstants from 'src/app/core/models/constants/app-constants';
import { ValidationData } from 'src/app/core/models/forms/validation-data';
import { POFulfillmentFormService } from '../../po-fulfillment/po-fulfillment-form/po-fulfillment-form.service';
import { CommonService } from 'src/app/core/services/common.service';
import { BulkFulfillmentModel } from '../models/bulk-fulfillment.model';
import { ControlContainer, NgForm } from '@angular/forms';
import { ViewSettingModel } from 'src/app/core/models/viewSetting.model';
import { FormHelper } from 'src/app/core/helpers/form.helper';

@Component({
    selector: 'app-bulk-fulfillment-general',
    templateUrl: './bulk-fulfillment-general.component.html',
    styleUrls: ['./bulk-fulfillment-general.component.scss'],
    viewProviders: [{ provide: ControlContainer, useExisting: NgForm }]
})
export class BulkFulfillmentGeneralComponent implements OnInit, OnChanges, OnDestroy {
    StringHelper = StringHelper;
    ModeOfTransportType = ModeOfTransportType;
    formHelper = FormHelper;
    viewSettingModuleIdType = ViewSettingModuleIdType;

    @Input() visibleColumns: ViewSettingModel[];
    @Input() parentIntegration$: Subject<IntegrationData>;
    @Input() model: BulkFulfillmentModel;
    @Input() formErrors: any;
    @Input() isGeneralTabEditable: boolean;
    @Input() isViewMode: boolean;
    @Input() isEditMode: boolean;
    @Input() isAddMode: boolean;
    @Input() allLocationOptions: any;
    @Input() saveAsDraft: boolean;
    @Input() validationRules: any;
    @Output() onModeOfTransportChange: EventEmitter<string> = new EventEmitter<string>();
    @Output() onMovementChange: EventEmitter<string> = new EventEmitter<string>();

    // It is prefix for formErrors and validationRules
    // Use it to detect what tab contains invalid data
    @Input() tabPrefix: string;

    public defaultDropDownItem: { text: string, label: string, description: string, value: number } =
        {
            text: 'label.select',
            label: 'label.select',
            description: 'select',
            value: null
        };
    modeOfTransportOptions = DropDowns.ModeOfTransportStringType.filter(c => c.value === ModeOfTransportType.Sea || c.value === ModeOfTransportType.Air);
    movementTypeOptions = DropDowns.CustomMovementStringType;
    incotermTypeOptions = DropDowns.IncotermStringType;
    mapLocationOptions = [];
    vesselsFiltered: Array<any> = [];
    vesselsSource: Array<any> = [];
    allCarrierOptions: any;
    carrierOptions = [];
    equipmentTypes = EquipmentType;
    locationOptions: any;
    customerOptions = [];
    customerName: String;
    customerId: number;
    customerPrefix: string;
    supplierId: number;

    isReceiptPortDisabled = true;
    isDeliveryPortDisabled = true;

    /**Data from 2 sources:
     * 1. Add mode: PO.pOType
     * 2. Edit mode: POFF.fulfilledFromPOType
     * */
    poType: POType = POType.Bulk;
    _formValidationKeys = {
        shipFromName: 'shipFromName',
        shipToName: 'shipToName',
        receiptPort: 'receiptPort',
        deliveryPort: 'deliveryPort',
        carrier: 'carrier',
        cargoReadyDate: 'cargoReadyDate',
        expectedShipDate: 'expectedShipDate',
        expectedDeliveryDate: 'expectedDeliveryDate'
    };

    // default values are for Sea
    literalLabels = {
        'shipFrom': 'label.portOfLoading',
        'shipTo': 'label.portOfDischarge'
    };

    selectedCarrierName: string = null;

    // Store all subscriptions, then should un-subscribe at the end
    private _subscriptions: Array<Subscription> = [];

    constructor(
        protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: POFulfillmentFormService,
        public translateService: TranslateService,
        private _userContext: UserContextService,
        private commonService: CommonService
    ) { }

    ngOnInit(): void {
        this.commonService.searchRealActiveVessels("").subscribe(
            r => {
                this.vesselsSource = r;
                this.vesselsFiltered = r;
            }
        )

        if (this._userContext.currentUser) {
            const user = this._userContext.currentUser;
            const isEditAllowed = user.permissions.find(
                (s) => s.name === AppPermissions.PO_Fulfillment_Detail_Edit
            );
            this.model.currentOrganization = {
                id: user.organizationId,
                name: user.organizationName,
            };

            if (this.model.currentOrganization.id) {
                // external user
                if (isEditAllowed) {
                    this.service
                        .getBuyersByOrgId(this.model.currentOrganization.id)
                        .subscribe((data) => {
                            // this.model.customerList = data;
                        });
                }

                this.bindSupplier();
                this.model.owner = this.model.currentOrganization.name;
            } else {
                // internal user
                this.service.getBuyers().subscribe((data) => {
                    // this.model.customerList = data;
                });
            }
        }

        this.updateCarrierValue();

        this.onChangeLogisticsServiceType(this.model.logisticsService, false);

        this._registerEventHandlers();

        this._registerContainerTypeChangedHandler();

        this._registerBookingDataLoadedHandler();
    }


    get logisticServiceTypeOptions() {
        if (this.model?.modeOfTransport === ModeOfTransportType.Air) {
            return DropDowns.AirLogisticServiceStringType;
        }

        return DropDowns.LogisticsServiceStringType;
    }

    private _registerEventHandlers() {
        // Handler for value changing on the Equipment Type of the first load
        let sub = this.parentIntegration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-load-info]equipmentTypeValueChanged'
            )).subscribe((eventContent: IntegrationData) => {
                this.model.movementType = eventContent.content['movementType'];
            });
        this._subscriptions.push(sub);

        // Handler for value changing on poType
        sub = this.parentIntegration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-form]poTypeChanged'
            )).subscribe((eventContent: IntegrationData) => {
                this.poType = eventContent.content['poType'];
                this.onChangeLogisticsServiceType(this.model.logisticsService, false);
            });
        this._subscriptions.push(sub);
    }

    private _registerContainerTypeChangedHandler() {
        // Handler for value changing on the Container Type
        const sub = this.parentIntegration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-customer]containerTypeChanged' ||
                eventContent.name === '[po-fulfillment-form]containerTypeChanged' ||
                eventContent.name === '[po-fulfillment-general-info]containerTypeChanged'
            )).subscribe((eventContent: IntegrationData) => {
                if (this.model.modeOfTransport === ModeOfTransportType.Sea) {
                    const containerType = eventContent.content['containerType'];
                    if (!StringHelper.isNullOrEmpty(containerType)) {
                        if (appConstants.CYEquipmentTypes.findIndex(x => x === containerType) !== -1) {
                            this.model.movementType = appConstants.MovementTypes.CYUnderscoreCY;
                        } else if (appConstants.CFSEquipmentTypes.findIndex(x => x === containerType) !== -1) {
                            this.model.movementType = appConstants.MovementTypes.CFSUnderscoreCY;
                        }
                    }
                }
            });
        this._subscriptions.push(sub);
    }

    _registerBookingDataLoadedHandler() {
        const sub = this.parentIntegration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-form]onInitDataLoaded'
            )).subscribe((eventContent: IntegrationData) => {
                this.model = eventContent.content['bookingModel'];
                const principalContact = this.model.contacts.find(
                    (c) => c.organizationRole === OrganizationNameRole.Principal
                );

                if (principalContact) {
                    this.customerName = principalContact.companyName;
                    this.customerId = principalContact.organizationId;
                    this.service
                        .getOrganizationsByIds([this.customerId])
                        .subscribe((data) => {
                            if (data.length > 0) {
                                // this.model.customerPrefix = this.customerPrefix = data[0].customerPrefix;
                            }
                        });
                }
            });
        this._subscriptions.push(sub);
    }

    ngOnChanges(changes: SimpleChanges) {
        if (changes.saveAsDraft?.currentValue === false) {
            this.validationRules[this.tabPrefix + 'modeOfTransport'] = {
                'required': 'label.modeOfTransport'
            };
            this.validationRules[this.tabPrefix + 'incoterm'] = {
                'required': 'label.incoterm'
            };
            this.validationRules[this.tabPrefix + 'logisticsService'] = {
                'required': 'label.logisticsServiceType'
            };
            this.validationRules[this.tabPrefix + 'movementType'] = {
                'required': 'label.movementType'
            };
            this.validationRules[this.tabPrefix + 'shipFromName'] = {
                'required': 'label.shipFrom'
            };
            this.validationRules[this.tabPrefix + 'shipToName'] = {
                'required': 'label.shipTo'
            };
            this.validationRules[this.tabPrefix + 'receiptPort'] = {
                'required': 'label.receiptPort'
            };
            this.validationRules[this.tabPrefix + 'deliveryPort'] = {
                'required': 'label.deliveryPort'
            };
            this.validationRules[this.tabPrefix + 'expectedShipDate'] = {
                'required': 'label.expectedShipDates'
            };
            this.validationRules[this.tabPrefix + 'expectedDeliveryDate'] = {
                'required': 'label.expectedDeliveryDates'
            };
            this.validationRules[this.tabPrefix + 'cargoReadyDate'] = {
                'required': 'label.bookingCargoReadyDates'
            };
        }
        if (changes.model?.currentValue.preferredCarrier) {
            // get carrier from Booking
            if (!StringHelper.isNullOrEmpty(this.model.preferredCarrier)) {
                const carrier = this.carrierOptions?.find((x) => +x.id === +this.model.preferredCarrier);
                this.selectedCarrierName = carrier && carrier.name;
            }
        }
        if (changes.model?.currentValue.logisticsService) {
            this.onChangeLogisticsServiceType(this.model.logisticsService, false);
        }

        this.mapLocationOptions = this.allLocationOptions.map((l) => ({
            id: l.id,
            label: l.locationDescription,
        }));

        this.updateLiteralLabels(this.model?.modeOfTransport);
    }

    updateCarrierValue() {
        this.service.getAllCarriers()
            .map((data) => {
                this.allCarrierOptions = data;
                this.carrierOptions = this.allCarrierOptions;

                // get carrier from Booking
                if (!StringHelper.isNullOrEmpty(this.model.preferredCarrier)) {
                    const carrier = this.carrierOptions.find((x) => +x.id === +this.model.preferredCarrier);
                    this.selectedCarrierName = carrier && carrier.name;
                }
            }).subscribe();
    }

    get supplierName() {
        const supplier = this.model.contacts.find(
            (c) => c.organizationRole === OrganizationNameRole.Supplier
        );
        return supplier && supplier.companyName;
    }

    private bindSupplier() {
        if (
            this.model.contacts.find(
                (c) => c.organizationRole === OrganizationNameRole.Supplier
            )
        ) {
            return;
        }
    }

    onFilterCarrier(value) {
        this.carrierOptions = [];
        if (value?.length >= 3) {
            this.carrierOptions = this.allCarrierOptions.filter(
                (s) => s.name.toLowerCase().indexOf(value.toLowerCase()) !== -1
                    && s.modeOfTransport === this.model.modeOfTransport
            );
        }
    }

    onChangeCarrier(value) {
        const carrier = this.carrierOptions.find((s) => s.name === value);
        if (carrier == null) {
            this.model.preferredCarrier = 0;
        } else {
            this.model.preferredCarrier = carrier && carrier.id;
        }
        this.validateForm(this._formValidationKeys.carrier);
    }

    onChangeVesselName(value) {

    }

    onFilterVesselName(value) {
        this.vesselsFiltered = [];
        if (value?.length >= 3) {
            this.vesselsFiltered = this.vesselsSource.filter(c => c.value.toLowerCase().indexOf(value.toLowerCase()) !== -1)
        }
    }

    locationFilter(value) {
        this.locationOptions = this.mapLocationOptions.filter(
            (s) => s.label.toLowerCase().indexOf(value.toLowerCase()) !== -1
        );
    }

    onChangeShipFrom(value) {
        const shipFrom = this.mapLocationOptions.find((s) => s.label === value);
        if (shipFrom == null) {
            this.model.shipFrom = 0;
        } else {
            this.model.shipFrom = shipFrom.id;
        }
        this.model.receiptPort = value;
        this.model.receiptPortId = this.model.shipFrom;
        this.validateForm(this._formValidationKeys.shipFromName);
        this.validateForm(this._formValidationKeys.receiptPort);
    }

    onChangeShipTo(value) {
        const shipTo = this.mapLocationOptions.find((s) => s.label === value);
        if (shipTo == null) {
            this.model.shipTo = 0;
        } else {
            this.model.shipTo = shipTo.id;
        }
        this.model.deliveryPort = value;
        this.model.deliveryPortId = this.model.shipTo;
        this.validateForm(this._formValidationKeys.shipToName);
        this.validateForm(this._formValidationKeys.deliveryPort);
    }

    receiptPortValueChange(value) {
        const receiptPort = this.mapLocationOptions.find((s) => s.label === value);
        this.model.receiptPortId = receiptPort && receiptPort.id;
        this.validateForm(this._formValidationKeys.receiptPort);
    }

    deliveryPortValueChange(value) {
        const deliveryPort = this.mapLocationOptions.find((s) => s.label === value);
        this.model.deliveryPortId = deliveryPort && deliveryPort.id;
        this.validateForm(this._formValidationKeys.deliveryPort);
    }

    forceValidateForm() {
        return this.validateForm(null, true);
    }

    validateTab() {
        const validationResults = [];
        let isValid = true;
        this.onCargoReadyDateChanged();
        // Clear current formErrors for current row
        Object.keys(this.formErrors)
            .filter(x => x.startsWith(this.tabPrefix) && !StringHelper.isNullOrEmpty(this.formErrors[x]))
            .map(x => {
                isValid = false;
            });

        if (!isValid) {
            const result = new ValidationData(ValidationDataType.Business, true);
            result.status = false;
            result.message = this.translateService.instant('validation.mandatoryFieldsValidation');
            validationResults.push(result);
        }
        return validationResults;
    }

    private validateForm(fieldKey?: string, forceValidate?: boolean) {
        if (forceValidate !== true) {
            if (this.isGeneralTabEditable) {
                const keys = Object.keys(this._formValidationKeys);
                keys.map((key) => {
                    this.formErrors[this.tabPrefix + key] = null;
                });
                return true;
            }
        }
        if (fieldKey) {
            const fieldValue = this.model[fieldKey] || '';
            const field = this._formValidationKeys[fieldKey];
            if (isUndefined(field)) {
                return true;
            }

            switch (fieldKey) {
                case this._formValidationKeys.shipFromName:
                case this._formValidationKeys.shipToName:
                    const location = this.mapLocationOptions.find(
                        (s) =>
                            s.label === fieldValue
                    );
                    if (location == null) {
                        if (fieldValue === '') {
                            this.formErrors[this.tabPrefix + fieldKey + '_custom'] = null;
                        } else {
                            this.formErrors[this.tabPrefix + fieldKey + '_custom'] = this.translateService.instant(
                                'validation.invalid',
                                {
                                    fieldName: this.translateService.instant(
                                        `validation.${fieldKey}`
                                    ),
                                }
                            );
                            return false;
                        }
                    } else {
                        this.formErrors[this.tabPrefix + fieldKey + '_custom'] = null;
                    }
                    break;
                case this._formValidationKeys.receiptPort:
                case this._formValidationKeys.deliveryPort:
                    const port = this.mapLocationOptions.find(
                        (s) =>
                            s.label === fieldValue
                    );
                    if (port == null) {
                        if (fieldValue === '') {
                            this.formErrors[this.tabPrefix + fieldKey + '_custom'] = null;
                        } else {
                            this.formErrors[this.tabPrefix + fieldKey] = null;
                            this.formErrors[this.tabPrefix + fieldKey + '_custom'] = this.translateService.instant(
                                'validation.invalid',
                                {
                                    fieldName: this.translateService.instant(
                                        `validation.${fieldKey}`
                                    ),
                                }
                            );
                            return false;
                        }
                    } else {
                        this.formErrors[this.tabPrefix + fieldKey + '_custom'] = null;
                    }
                    break;
                case this._formValidationKeys.carrier:
                    const carrier = this.carrierOptions.find(
                        (s) => s.name === this.selectedCarrierName
                    );
                    if (!carrier) {
                        if (this.selectedCarrierName === '' || !this.selectedCarrierName) {
                            this.formErrors[this.tabPrefix + fieldKey + '_custom'] = null;
                        } else {
                            this.formErrors[this.tabPrefix + fieldKey + '_custom'] = this.translateService.instant(
                                'validation.invalid',
                                {
                                    fieldName: this.translateService.instant(
                                        `validation.${fieldKey}`
                                    ),
                                }
                            );
                            return false;
                        }
                    } else {
                        this.formErrors[this.tabPrefix + fieldKey + '_custom'] = null;
                    }
                    break;
                case this._formValidationKeys.cargoReadyDate:
                    if (StringHelper.isNullOrEmpty(this.model.cargoReadyDate)) {
                        this.formErrors[this.tabPrefix + fieldKey + '_custom'] = '';
                    } else {
                        if (this.model.cargoReadyDate.getFullYear().toString().length === 4) {
                            if (!StringHelper.isNullOrEmpty(this.model.cargoReadyDate) && !StringHelper.isNullOrEmpty(this.model.expectedShipDate) && !(new Date(this.model.cargoReadyDate) <= new Date(this.model.expectedShipDate))
                                && this.model.expectedShipDate.getFullYear().toString().length === 4) {
                                this.formErrors[this.tabPrefix + fieldKey + '_custom'] = this.translateService.instant(
                                    'validation.notLaterThan',
                                    {
                                        currentFieldName: this.translateService.instant('label.bookingCargoReadyDates'),
                                        fieldName: this.translateService.instant('label.expectedShipDates')
                                    });

                                return false;
                            } else {
                                this.formErrors[this.tabPrefix + fieldKey + '_custom'] = '';
                            }
                        } else {
                            this.formErrors[this.tabPrefix + fieldKey + '_custom'] = this.translateService.instant(
                                'validation.invalid',
                                {
                                    currentFieldName: this.translateService.instant('label.bookingCargoReadyDates'),
                                    fieldName: this.translateService.instant('label.bookingCargoReadyDates')
                                });

                            return false;
                        }
                    }
                    break;
                case this._formValidationKeys.expectedShipDate:
                    let isValidate = true;
                    if (StringHelper.isNullOrEmpty(this.model.expectedShipDate)) {
                        this.formErrors[this.tabPrefix + fieldKey + 'notEarlierThan'] = null;
                        this.formErrors[this.tabPrefix + fieldKey + 'notLaterThan'] = null;
                    } else {
                        if (this.model.expectedShipDate.getFullYear().toString().length === 4) {
                            this.formErrors[this.tabPrefix + fieldKey + 'invalid'] = null;
                            if (!StringHelper.isNullOrEmpty(this.model.cargoReadyDate) && !StringHelper.isNullOrEmpty(this.model.expectedShipDate) && !(new Date(this.model.cargoReadyDate) <= new Date(this.model.expectedShipDate))) {
                                this.formErrors[this.tabPrefix + fieldKey + 'notEarlierThan'] = this.translateService.instant(
                                    'validation.notEarlierThan',
                                    {
                                        currentFieldName: this.translateService.instant('label.expectedShipDates'),
                                        fieldName: this.translateService.instant('label.bookingCargoReadyDates'),
                                    });

                                isValidate = false;
                            } else {
                                this.formErrors[this.tabPrefix + fieldKey + 'notEarlierThan'] = null;
                            }

                            if (!StringHelper.isNullOrEmpty(this.model.expectedShipDate) && !StringHelper.isNullOrEmpty(this.model.expectedDeliveryDate) && !(new Date(this.model.expectedShipDate) <= new Date(this.model.expectedDeliveryDate)) && this.model.expectedDeliveryDate.getFullYear().toString().length === 4) {
                                this.formErrors[this.tabPrefix + fieldKey + 'notLaterThan'] = this.translateService.instant(
                                    'validation.notLaterThan',
                                    {
                                        currentFieldName: this.translateService.instant('label.expectedShipDates'),
                                        fieldName: this.translateService.instant('label.expectedDeliveryDates')
                                    });

                                isValidate = false;
                            } else {
                                this.formErrors[this.tabPrefix + fieldKey + 'notLaterThan'] = null;
                            }
                        }
                        else {
                            this.formErrors[this.tabPrefix + fieldKey + 'notEarlierThan'] = null;
                            this.formErrors[this.tabPrefix + fieldKey + 'notLaterThan'] = null;
                            this.formErrors[this.tabPrefix + fieldKey + 'invalid'] = this.translateService.instant(
                                'validation.invalid',
                                {
                                    currentFieldName: this.translateService.instant('label.expectedShipDates'),
                                    fieldName: this.translateService.instant('label.expectedShipDates')
                                });

                            isValidate = false;
                        }
                    }
                    return isValidate;

                case this._formValidationKeys.expectedDeliveryDate:
                    // if (!StringHelper.isNullOrEmpty(this.model.expectedDeliveryDate) && !StringHelper.isNullOrEmpty(this.model.expectedShipDate) && !(new Date(this.model.expectedShipDate) <= new Date(this.model.expectedDeliveryDate))) {
                    //     this.formErrors[this.tabPrefix + fieldKey + 'notEarlierThan'] = this.translateService.instant(
                    //         'validation.notEarlierThan',
                    //         {
                    //             currentFieldName: this.translateService.instant('label.expectedDeliveryDates'),
                    //             fieldName: this.translateService.instant('label.expectedShipDates'),
                    //         });

                    //     return false;
                    // } else {
                    //     this.formErrors[this.tabPrefix + fieldKey + 'notEarlierThan'] = '';
                    // }
                    // break;

                    if (StringHelper.isNullOrEmpty(this.model.expectedDeliveryDate)) {
                        this.formErrors[this.tabPrefix + fieldKey + 'notEarlierThan'] = '';
                        this.formErrors[this.tabPrefix + fieldKey + 'invalid'] = '';
                    } else {
                        if (this.model.expectedDeliveryDate.getFullYear().toString().length === 4) {
                            this.formErrors[this.tabPrefix + fieldKey + 'invalid'] = '';
                            if (!StringHelper.isNullOrEmpty(this.model.expectedDeliveryDate) && !StringHelper.isNullOrEmpty(this.model.expectedShipDate) && !(new Date(this.model.expectedShipDate) <= new Date(this.model.expectedDeliveryDate))) {
                                this.formErrors[this.tabPrefix + fieldKey + 'notEarlierThan'] = this.translateService.instant(
                                    'validation.notEarlierThan',
                                    {
                                        currentFieldName: this.translateService.instant('label.expectedDeliveryDates'),
                                        fieldName: this.translateService.instant('label.expectedShipDates'),
                                    });

                                return false;
                            } else {
                                this.formErrors[this.tabPrefix + fieldKey + 'notEarlierThan'] = '';
                            }
                        } else {
                            this.formErrors[this.tabPrefix + fieldKey + 'notEarlierThan'] = '';
                            this.formErrors[this.tabPrefix + fieldKey + 'invalid'] = this.translateService.instant(
                                'validation.invalid',
                                {
                                    currentFieldName: this.translateService.instant('label.expectedDeliveryDates'),
                                    fieldName: this.translateService.instant('label.expectedDeliveryDates')
                                });

                            return false;
                        }
                    }
                    break;

                default:
                    break;
            }
            return true;
        } else {
            // It is going to verify all inputs/fields
            let isValid = true;
            const validationKeys = Object.keys(this._formValidationKeys);
            validationKeys.map((key) => {
                if (!this.validateForm(key, forceValidate)) {
                    isValid = false;
                }
            });
            return isValid;
        }
    }



    onChangeLogisticsServiceType(event, needToValidateForm: boolean) {
        if (event === 'InternationalDoorToPort' || event === 'InternationalDoorToAirport') {
            this.isReceiptPortDisabled = false;
            this.isDeliveryPortDisabled = true;
            this.model.deliveryPort = this.model.shipToName;
            this.model.deliveryPortId = this.model.shipTo;
        }
        else if (event === 'InternationalPortToDoor' || event === 'InternationalAirportToDoor') {
            this.isReceiptPortDisabled = true;
            // Not allow to change Delivery Port if "to-Door" service.
            this.isDeliveryPortDisabled = this.poType === POType.Blanket;
            this.model.receiptPort = this.model.shipFromName;
            this.model.receiptPortId = this.model.shipFrom;
        }
        else if (event === 'InternationalDoorToDoor') {
            this.isReceiptPortDisabled = false;
            this.isDeliveryPortDisabled = false;
        }
        else {
            this.isReceiptPortDisabled = true;
            this.isDeliveryPortDisabled = true;
            this.model.receiptPort = this.model.shipFromName;
            this.model.receiptPortId = this.model.shipFrom;
            this.model.deliveryPort = this.model.shipToName;
            this.model.deliveryPortId = this.model.shipTo;
        }

        if (needToValidateForm) {
            this.validateForm();
        }
    }

    updateLiteralLabels(mode) {
        switch (mode?.toLowerCase()) {
            case ModeOfTransportType.Sea.toLowerCase():
                this.literalLabels.shipFrom = 'label.portOfLoading';
                this.literalLabels.shipTo = 'label.portOfDischarge';
                break;
            case ModeOfTransportType.Air.toLowerCase():
                this.literalLabels.shipFrom = 'label.origin';
                this.literalLabels.shipTo = 'label.destination';
                break;
            default:
                this.literalLabels.shipFrom = 'label.portOfLoading';
                this.literalLabels.shipTo = 'label.portOfDischarge';
                break;
        }
    }

    onChangeModeOfTransport(event) {
        if (event === ModeOfTransportType.Air) {
            this.model.movementType = null;
            this.model.vesselName = null;
            this.model.voyageNo = null;
        }
        //reset logistic service
        let isValidLogisticService = this.logisticServiceTypeOptions.some(x => x.value === this.model.logisticsService);
        if (!isValidLogisticService) {
            this.model.logisticsService = null;
            this.onChangeLogisticsServiceType(this.model.logisticsService, !this.saveAsDraft);
        }

        this.updateLiteralLabels(event);

        this.onModeOfTransportChange.emit();
    }

    onChangeMovementType(event) {
        this.onMovementChange.emit();
    }

    onCargoReadyDateChanged(value?: string) {
        this.validateForm(this._formValidationKeys.cargoReadyDate);
        this.validateForm(this._formValidationKeys.expectedShipDate);
        this.validateForm(this._formValidationKeys.expectedDeliveryDate);
    }


    onExpectedShipDateChanged(value?: string) {
        this.validateForm(this._formValidationKeys.cargoReadyDate);
        this.validateForm(this._formValidationKeys.expectedShipDate);
        this.validateForm(this._formValidationKeys.expectedDeliveryDate);
    }

    onExpectedDeliveryDateChanged(value?: string) {
        this.validateForm(this._formValidationKeys.cargoReadyDate);
        this.validateForm(this._formValidationKeys.expectedShipDate);
        this.validateForm(this._formValidationKeys.expectedDeliveryDate);
    }

    public validateBeforeSaving(): ValidationData[] {
        let result: ValidationData[] = [];
        // In case there is any error
        const errors = Object.keys(this.formErrors)?.filter(x => x.startsWith(this.tabPrefix));
        for (let index = 0; index < errors.length; index++) {
            const err = Reflect.get(this.formErrors, errors[index]);
            if (err && !StringHelper.isNullOrEmpty(err)) {
                result.push(new ValidationData(ValidationDataType.Input, false));
            }
        }
        return result;
    }

    public validateBeforeSubmitting(): ValidationData[] {
        let result: ValidationData[] = [];
        const mandatoryFields = [
            "incoterm",
            "cargoReadyDate",
            "modeOfTransport",
            "logisticsService",
            "shipFrom",
            "shipTo",
            "receiptPort",
            "deliveryPort",
            "expectedShipDate",
            "expectedDeliveryDate"
        ];
        if (this.model.modeOfTransport === ModeOfTransportType.Sea) {
            mandatoryFields.push('movementType');
        }
        let isValid = true;
        mandatoryFields.forEach(f => {
            if (StringHelper.isNullOrEmpty(this.model[f]) || this.model[f] === 0) {
                isValid = false;
            }
        });
        if (!isValid) {
            result.push(new ValidationData(ValidationDataType.Input, false));
        }
        return result;
    }

    ngOnDestroy(): void {
        this._subscriptions.map(x => x.unsubscribe());
    }
}
