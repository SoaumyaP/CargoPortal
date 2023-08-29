import {
    Component,
    OnInit,
    Input,
    ViewChild,
    OnChanges,
    Output,
    EventEmitter,
    OnDestroy,
    SimpleChanges,
} from '@angular/core';
import {
    DropDowns,
    VerificationSetting,
    UserContextService,
    StringHelper,
} from 'src/app/core';
import { MissingPOFulfillmentFormService } from '../missing-po-fulfillment-form/missing-po-fulfillment-form.service';
import { AutoCompleteComponent } from '@progress/kendo-angular-dropdowns';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { ControlContainer, NgForm } from '@angular/forms';
import { IntlService } from '@progress/kendo-angular-intl';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { BehaviorSubject, Subject, Subscription } from 'rxjs';
import { OrganizationNameRole, EquipmentType, POType, ModeOfTransportType, ValidationDataType, POFulfillmentStageType, RoleSequence, Roles, ViewSettingModuleIdType } from 'src/app/core/models/enums/enums';
import { isUndefined } from 'util';
import { IntegrationData } from 'src/app/core/models/forms/integration-data';
import { filter } from 'rxjs/operators';
import * as appConstants from 'src/app/core/models/constants/app-constants';
import { ValidationData } from 'src/app/core/models/forms/validation-data';
import { FormHelper } from 'src/app/core/helpers/form.helper';

@Component({
    selector: 'app-missing-po-fulfillment-general-info',
    templateUrl: './missing-po-fulfillment-general-info.component.html',
    styleUrls: ['./missing-po-fulfillment-general-info.component.scss'],
    viewProviders: [{ provide: ControlContainer, useExisting: NgForm }],
})
export class MissingPOFulfillmentGeneralInfoComponent implements OnInit, OnChanges, OnDestroy {
    isCreatedFromPO = false;
    @Input()
    parentIntegration$: Subject<IntegrationData>;
    @Input()
    model: any;
    @Input()
    formErrors: any;
    @Input()
    isViewMode: boolean;
    @Input()
    isEditMode: boolean;
    @Input()
    isAddMode: boolean;
    @Input()
    set isFulfilledFromPO(value: boolean) {
        this.isCreatedFromPO = value;
        if (value !== undefined && value !== null) {
            this._isFulfilledFromPO.next(value);
        }
    }
    // Please trigger this event to change ('add'/'remove') the booking contacts.
    @Output() contactsChanged: EventEmitter<object> = new EventEmitter();

    @ViewChild('customerAutoComplete', { static: true })
    public customerAutoComplete: AutoCompleteComponent;
    modeOfTransportOptions = DropDowns.ModeOfTransportStringType;
    movementTypeOptions = DropDowns.CustomMovementStringType;
    incotermTypeOptions = DropDowns.IncotermStringType;

    @Input()
    allLocationOptions: any;
    mapLocationOptions = [];

    // It is prefix for formErrors and validationRules
    // Use it to detect what tab contains invalid data
    @Input()
    tabPrefix: string;

    @Input()
    poCurrentCarrier: any;
    allCarrierOptions: any;
    carrierOptions = [];

    // Store all subscriptions, then should un-subscribe at the end
    private _subscriptions: Array<Subscription> = [];
    equipmentTypes = EquipmentType;
    modeOfTransportType = ModeOfTransportType;
    locationOptions: any;
    shipFromText: any;
    shipToText: any;
    customerOptions = [];
    customerLoading: boolean;
    acpTimeout: any;
    customerName: String;
    customerId: number;
    customerPrefix: string;
    supplierId: number;

    isModeOfTransportDisabled = false;
    isExpectedShipDateDisabled = false;
    isExpectedDeliveryDateDisabled = false;
    isIncotermDisabled = false;
    isPreferredCarrierDisabled = false;
    isLogisticsServiceTypeDisabled = false;
    isShipFromDisabled = false;
    isShipToDisabled = false;
    isReceiptPortDisabled = true;
    isDeliveryPortDisabled = true;
    isPartialShipmentDisabled = false;
    _isFulfilledFromPO = new BehaviorSubject(null);

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
        expectedDeliveryDate: 'expectedDeliveryDate',
    };

    // default values are for Sea
    literalLabels = {
        'shipFrom': 'label.portOfLoading',
        'shipTo': 'label.portOfDischarge'
    };

    viewSettingModuleIdType = ViewSettingModuleIdType;

    constructor(
        protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: MissingPOFulfillmentFormService,
        public translateService: TranslateService,
        private _userContext: UserContextService,
        private intl: IntlService
    ) {}

    ngOnInit(): void {
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
                        this.model.customerPrefix = this.customerPrefix =
                            data[0].customerPrefix;
                    }
                });

            this.applyBuyerCompliance(this.customerId);
        }

        if (this._userContext.currentUser) {
            const user = this._userContext.currentUser;
            const isEditAllowed = user.permissions.find(
                (s) => s.name === AppPermissions.PO_Fulfillment_Detail_Edit
            );
            this.model.currentOrganization = {
                id: user.organizationId,
                name: user.organizationName,
                isInternal: user.isInternal,
                role: user.role.id
            };
            if (this.model.currentOrganization.id) {
                // external user
                if (isEditAllowed) {
                    this.service
                        .getBuyersByOrgId(this.model.currentOrganization.id)
                        .subscribe((data) => {
                            this.model.customerList = data;
                        });
                }

                this.bindSupplier();
                this.model.owner = this.model.currentOrganization.name;
            } else {
                // internal user
                this.service.getBuyers().subscribe((data) => {
                    this.model.customerList = data;
                });
            }

            if (!this.service.currentCustomerPOs()) {
                if (isEditAllowed) {
                    // check if add mode
                    if (this.customerId) {
                        this.supplierId =
                            this.model && this.model.currentOrganization.id
                                ? this.model.currentOrganization.id
                                : this.model.contacts.find(
                                        (c) =>
                                            c.organizationRole ===
                                            OrganizationNameRole.Supplier
                                    )?.organizationId;
                                    
                        if (!this.isAddMode) {
                            
                            const purchaseOrderIds = this.model.orders.map(x => x.purchaseOrderId);

                            // In case it is adding from purchase order, this.model.orders will be blank.
                            if (purchaseOrderIds.length > 0) {
                                this.service.getCustomerPOs(purchaseOrderIds,
                                    this.customerId);
                            }
                            else {
                                this.service.getCustomerPOs([0],
                                    this.customerId);
                            }
                        }
                        
                    }
                }
            }
        }

        this.updateCarrierValue();

        //Register event handlers.
        this.logisticsServiceTypeChange(this.model.logisticsService, false, this.isAddMode);
        this._registerEventHandlers();
        this._registerContainerTypeChangedHandler();
        this._registerBookingDataLoadedHandler();
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
                this.logisticsServiceTypeChange(this.model.logisticsService, false, true);
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
                                this.model.customerPrefix = this.customerPrefix =
                                    data[0].customerPrefix;
                            }
                        });

                    this.applyBuyerCompliance(this.customerId);
                }
            });
        this._subscriptions.push(sub);
    }

    ngOnChanges(changes: SimpleChanges): void {
        this.mapLocationOptions = this.allLocationOptions.map((l) => ({
            id: l.id,
            label: l.locationDescription,
        }));

        // get carrier from selected PO
        if (!StringHelper.isNullOrEmpty(this.poCurrentCarrier)) {

            // do not reset carrier if already
            if (StringHelper.isNullOrEmpty(this.model.preferredCarrier)) {
                const selectedCarrier = this.poCurrentCarrier;
                this.model.preferredCarrier = selectedCarrier && selectedCarrier.id;
                this.model.carrier = selectedCarrier && selectedCarrier.name;
            }

            // set value for Carrier if it's name not initialized
            if (!StringHelper.isNullOrEmpty(this.model.preferredCarrier) && StringHelper.isNullOrEmpty(this.model.carrier)) {
                this.updateCarrierValue();
            }
        }

        this.updateLiteralLabels(this.model?.modeOfTransport);

        // set poType from model (edit mode)
        this.poType = this.model.fulfilledFromPOType;
    }

    updateCarrierValue() {
        this.service.getAllCarriers()
        .map((data) => {
            this.allCarrierOptions = data;
            this.carrierOptions = this.allCarrierOptions;

            // get carrier from POFF
            if (!StringHelper.isNullOrEmpty(this.model.preferredCarrier)) {
                const carrier = this.carrierOptions.find((x) => +x.id === +this.model.preferredCarrier);
                this.model.carrier = carrier && carrier.name;
            }
        }).subscribe();
    }

    get supplierName() {
        const supplier = this.model.contacts.find(
            (c) => c.organizationRole === OrganizationNameRole.Supplier
        );
        if (!supplier) {
            return null;
        }
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

        this._isFulfilledFromPO.subscribe((val) => {
            let currentOrganization = this.model.currentOrganization;
            if (val !== null && !val && this.isAddMode && currentOrganization && currentOrganization.role === Roles.Shipper) {
                this.service
                    .getOrganizationsByIds([currentOrganization.id])
                    .subscribe((data) => {
                        this.model.contacts.push({
                            organizationId: currentOrganization.id,
                            organizationRole: OrganizationNameRole.Supplier,
                            organizationCode: data[0].code,
                            companyName: data[0].name,
                            contactName: data[0].contactName,
                            contactNumber: data[0].contactNumber,
                            contactEmail: data[0].contactEmail,
                            contactSequence: RoleSequence.Supplier,
                            address: data[0].address,
                        });
                        this.contactsChanged.emit({action: 'add'});
                    });
            }
        });
    }

    customerFilterChange(value) {
        this.customerId = null;
        if (value.length >= 3) {
            this.customerAutoComplete.toggle(false);
            this.customerOptions = [];
            clearTimeout(this.acpTimeout);

            this.acpTimeout = setTimeout(() => {
                this.customerLoading = true;
                value = value.toLowerCase();
                let takeRecords = 10;
                if (this.model.customerList != null) {
                    for (
                        let i = 0;
                        i < this.model.customerList.length && takeRecords > 0;
                        i++
                    ) {
                        if (
                            this.model.customerList[i].name
                                .toLowerCase()
                                .indexOf(value) !== -1
                        ) {
                            this.customerOptions.push(
                                this.model.customerList[i]
                            );
                            takeRecords--;
                        }
                    }
                    this.customerAutoComplete.toggle(true);
                    this.customerLoading = false;
                }
            }, 400);
        } else {
            this.customerLoading = false;
            if (this.acpTimeout) {
                clearTimeout(this.acpTimeout);
            }
            this.customerAutoComplete.toggle(false);
        }
    }

    customerValueChange(value) {
        this.customerPrefix = '';
        this.model.contacts = this.model.contacts.filter(
            (c) => c.organizationRole !== OrganizationNameRole.Principal
        );
        this.model.selectedCustomer = this.customerOptions.find(
            (s) => s.name.toLowerCase() === value.toLowerCase()
        );

        this.service.resetCustomerPOs();
        this.model.buyerCompliance = null;
        if (!this.model.selectedCustomer) {
            return;
        }

        this.customerId = this.model.selectedCustomer.id;
        this.model.customerPrefix = this.customerPrefix = this.model.selectedCustomer.customerPrefix;
        this.model.contacts.push({
            organizationId: this.model.selectedCustomer.id,
            organizationRole: OrganizationNameRole.Principal,
            organizationCode: this.model.selectedCustomer.organizationCode,
            companyName: this.model.selectedCustomer.name,
            contactName: this.model.selectedCustomer.contactName,
            contactNumber: this.model.selectedCustomer.contactNumber,
            contactEmail: this.model.selectedCustomer.contactEmail,
            contactSequence: RoleSequence.Principal,
            address: this.model.selectedCustomer.address,
        });
        this.contactsChanged.emit({action: 'add'});

        if (!this.supplierId) {
            this.supplierId =
                this.model && this.model.currentOrganization.id
                    ? this.model.currentOrganization.id
                    : this.model.contacts.find(
                            (c) =>
                                c.organizationRole ===
                                OrganizationNameRole.Supplier
                        )?.organizationId;
        }
        const purchaseOrderIds = this.model.orders.map(x => x.purchaseOrderId);
        this.service.getCustomerPOs(purchaseOrderIds, this.model.selectedCustomer.id);
        this.applyBuyerCompliance(this.model.selectedCustomer.id);
    }

    applyBuyerCompliance(customerId: any) {
        this.service.getBuyerCompliance(customerId)
        .subscribe((data) => {
            this.model.buyerCompliance = data[0];
            if (this.model.buyerCompliance) {

                this.applyPOVerificationSetting(this.model.buyerCompliance);
                this.isPartialShipmentDisabled = true;
                this.model.isPartialShipment = this.model.buyerCompliance.shippingCompliance.allowPartialShipment;
                const emitValue: IntegrationData = {
                    name: '[po-fulfillment-general-info]buyerComplianceDataLoaded',
                    content: data[0]
                };
                this.parentIntegration$.next(emitValue);
            }
        });
    }

    applyPOVerificationSetting(buyerCompliance: any) {
        if (buyerCompliance.purchaseOrderVerificationSetting) {
            const poVerificationSetting =
                buyerCompliance.purchaseOrderVerificationSetting;

            if (
                poVerificationSetting.modeOfTransportVerification ===
                VerificationSetting.AsPerPO
            ) {
                this.isModeOfTransportDisabled = true;
            }

            if (
                poVerificationSetting.expectedShipDateVerification === VerificationSetting.AsPerPO 
                || poVerificationSetting.expectedShipDateVerification === VerificationSetting.AsPerPODefault
            ) {
                this.isExpectedShipDateDisabled = true;
            }

            if (
                poVerificationSetting.expectedDeliveryDateVerification === VerificationSetting.AsPerPO 
                || poVerificationSetting.expectedDeliveryDateVerification === VerificationSetting.AsPerPODefault
            ) {
                this.isExpectedDeliveryDateDisabled = true;
            }

            if (
                poVerificationSetting.shipFromLocationVerification ===
                VerificationSetting.AsPerPO
            ) {
                this.isShipFromDisabled = true;
            }

            if (
                poVerificationSetting.shipToLocationVerification ===
                VerificationSetting.AsPerPO
            ) {
                this.isShipToDisabled = true;
            }

            if (
                poVerificationSetting.incotermVerification ===
                VerificationSetting.AsPerPO
            ) {
                this.isIncotermDisabled = true;
            }

            if (
                poVerificationSetting.preferredCarrierVerification ===
                VerificationSetting.AsPerPO
            ) {
                this.isPreferredCarrierDisabled = true;
            }

            const emitValue: IntegrationData = {
                name: '[po-fulfillment-general-info]applyPOContactVerificationSetting',
                content: {
                    'consigneeVerification': poVerificationSetting.consigneeVerification,
                    'shipperVerification': poVerificationSetting.shipperVerification
                }
            };
            this.parentIntegration$.next(emitValue);
        }
    }

    carrierFilter(value) {
        this.carrierOptions = [];
        if (value.length >= 3) {
            this.carrierOptions = this.allCarrierOptions.filter(
                (s) => s.name.toLowerCase().indexOf(value.toLowerCase()) !== -1
                    && StringHelper.caseIgnoredCompare(s.modeOfTransport, this.model.modeOfTransport)
            );
        }
    }

    carrierValueChange(value) {
        const carrier = this.carrierOptions.find((s) => s.name === value);
        if (carrier == null) {
            this.model.preferredCarrier = 0;
        } else {
            this.model.preferredCarrier = carrier && carrier.id;
        }
        this.validateForm(this._formValidationKeys.carrier);
    }

    locationFilter(value) {
        this.locationOptions = this.mapLocationOptions.filter(
            (s) => s.label.toLowerCase().indexOf(value.toLowerCase()) !== -1
        );
    }

    shipFromValueChange(value) {
        const shipFrom = this.mapLocationOptions.find((s) => s.label === value);
        if (shipFrom == null) {
            this.model.shipFrom = 0;
        } else {
            this.model.shipFrom = shipFrom.id;
            this.shipFromPortValueChangeEmitter();
        }
        this.model.receiptPort = value;
        this.model.receiptPortId = this.model.shipFrom;
        this.validateForm(this._formValidationKeys.shipFromName);
        this.validateForm(this._formValidationKeys.receiptPort);
    }

    shipToValueChange(value) {
        const shipTo = this.mapLocationOptions.find((s) => s.label === value);
        if (shipTo == null) {
            this.model.shipTo = 0;
        } else {
            this.model.shipTo = shipTo.id;
            this.shipToPortValueChangeEmitter();
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
            if (this.isViewMode) {
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
                            this.formErrors[this.tabPrefix + fieldKey] = null;
                        } else {
                            this.formErrors[this.tabPrefix + fieldKey] = this.translateService.instant(
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
                        this.formErrors[this.tabPrefix + fieldKey] = null;
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
                            this.formErrors[this.tabPrefix + fieldKey] = null;
                        } else {
                            this.formErrors[this.tabPrefix + fieldKey] = this.translateService.instant(
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
                        this.formErrors[this.tabPrefix + fieldKey] = null;
                    }
                    break;
                case this._formValidationKeys.carrier:
                    const carrier = this.carrierOptions.find(
                        (s) => s.name === fieldValue
                    );
                    if (carrier == null) {
                        if (fieldValue === '') {
                            this.formErrors[this.tabPrefix + fieldKey] = null;
                        } else {
                            this.formErrors[this.tabPrefix + fieldKey] = this.translateService.instant(
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
                        this.formErrors[this.tabPrefix + fieldKey] = null;
                    }
                    break;
                 case this._formValidationKeys.cargoReadyDate:
                    if (StringHelper.isNullOrEmpty(this.model.cargoReadyDate)){
                        this.formErrors[this.tabPrefix + fieldKey] = '';
                    }else{
                        if (this.model.cargoReadyDate.getFullYear().toString().length === 4) {
                            if (!StringHelper.isNullOrEmpty(this.model.cargoReadyDate) && !StringHelper.isNullOrEmpty(this.model.expectedShipDate) && !(new Date(this.model.cargoReadyDate) <= new Date( this.model.expectedShipDate))
                            && this.model.expectedShipDate.getFullYear().toString().length === 4 ) {
                                this.formErrors[this.tabPrefix + fieldKey] =  this.translateService.instant(
                                    'validation.notLaterThan',
                                    {
                                        currentFieldName: this.translateService.instant('label.bookingCargoReadyDates'),
                                        fieldName: this.translateService.instant('label.expectedShipDates')
                                    });
        
                                return false;
                             } else {
                                this.formErrors[this.tabPrefix + fieldKey] = '';
                             }
                        } else{
                            this.formErrors[this.tabPrefix + fieldKey] =  this.translateService.instant(
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
                    }else{
                        if (this.model.expectedShipDate.getFullYear().toString().length === 4) {
                            this.formErrors[this.tabPrefix + fieldKey + 'invalid'] = null;
                            if (!StringHelper.isNullOrEmpty(this.model.cargoReadyDate) && !StringHelper.isNullOrEmpty(this.model.expectedShipDate) && !(new Date(this.model.cargoReadyDate) <= new Date(this.model.expectedShipDate)) ) {
                                this.formErrors[this.tabPrefix + fieldKey + 'notEarlierThan'] =  this.translateService.instant(
                                    'validation.notEarlierThan',
                                    {
                                        currentFieldName: this.translateService.instant('label.expectedShipDates'),
                                        fieldName: this.translateService.instant('label.bookingCargoReadyDates'),
                                    });
        
                                    isValidate = false;
                             } else {
                                this.formErrors[this.tabPrefix + fieldKey + 'notEarlierThan'] = null;
                             }
        
                             if (!StringHelper.isNullOrEmpty(this.model.expectedShipDate) && !StringHelper.isNullOrEmpty(this.model.expectedDeliveryDate) && !(new Date(this.model.expectedShipDate) <= new Date( this.model.expectedDeliveryDate)) && this.model.expectedDeliveryDate.getFullYear().toString().length === 4 ) {
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
                        else{
                            this.formErrors[this.tabPrefix + fieldKey + 'notEarlierThan'] = null;
                            this.formErrors[this.tabPrefix + fieldKey + 'notLaterThan'] = null;
                            this.formErrors[this.tabPrefix + fieldKey + 'invalid'] =  this.translateService.instant(
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
                    if (StringHelper.isNullOrEmpty(this.model.expectedDeliveryDate)) {
                        this.formErrors[this.tabPrefix + fieldKey] = '';
                    }else{
                        if (this.model.expectedDeliveryDate.getFullYear().toString().length === 4) {
                            if (!StringHelper.isNullOrEmpty(this.model.expectedDeliveryDate) && !StringHelper.isNullOrEmpty(this.model.expectedShipDate) && !(new Date(this.model.expectedShipDate) <= new Date(this.model.expectedDeliveryDate))) {
                                this.formErrors[this.tabPrefix + fieldKey] =  this.translateService.instant(
                                 'validation.notEarlierThan',
                                 {
                                     currentFieldName: this.translateService.instant('label.expectedDeliveryDates'),
                                     fieldName: this.translateService.instant('label.expectedShipDates'),
                                 });
         
                                return false;
                             } else {
                                this.formErrors[this.tabPrefix + fieldKey] = '';
                             }
                        }else{
                            this.formErrors[this.tabPrefix + fieldKey] =  this.translateService.instant(
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

    logisticsServiceTypeChange(event, needToValidateForm: boolean, overwritePort: boolean) {       
        if (event === 'InternationalDoorToPort' || event === 'InternationalDoorToAirport') {
            this.isReceiptPortDisabled = false;
            this.isDeliveryPortDisabled = true;
            if (overwritePort) {
                this.model.deliveryPort = this.model.shipToName;
                this.model.deliveryPortId = this.model.shipTo;
            }
        }
        else if (event === 'InternationalPortToDoor' || event === 'InternationalAirportToDoor') {
            this.isReceiptPortDisabled = true;
            // Not allow to change Delivery Port if "to-Door" service.
            this.isDeliveryPortDisabled = this.poType === POType.Blanket;
            if (overwritePort) {
                this.model.receiptPort = this.model.shipFromName;
                this.model.receiptPortId = this.model.shipFrom;
            }
        }
        else if (event === 'InternationalDoorToDoor') {
            this.isReceiptPortDisabled = false;
            // Not allow to change Delivery Port if "to-Door" service.
            this.isDeliveryPortDisabled = this.poType === POType.Blanket;
        }
        else {
            this.isReceiptPortDisabled = true;
            this.isDeliveryPortDisabled = true;
            if (overwritePort) {
                this.model.receiptPort = this.model.shipFromName;
                this.model.receiptPortId = this.model.shipFrom;
                this.model.deliveryPort = this.model.shipToName;
                this.model.deliveryPortId = this.model.shipTo;
            }
        }
        if (needToValidateForm) {
            this.validateForm();
        }
    }

    modeOfTransportChange($event) {
        if ($event !== ModeOfTransportType.Sea) {
            this.model.movementType = undefined;
        } else {
            const containerTypes = [...new Set(this.model.orders.map(x => x.poContainerType)
                .filter(x => !StringHelper.isNullOrEmpty(x)))];
            if (containerTypes && containerTypes.length === 1) {
                const emitValue: IntegrationData = {
                    name: '[po-fulfillment-general-info]containerTypeChanged',
                    content: {
                        'containerType': containerTypes[0]
                    }
                };
                this.parentIntegration$.next(emitValue);
            }
        }

        this.updateLiteralLabels($event);

        //reset logistic service
        let isValidLogisticService =  this.logisticServiceTypeOptions.some(x => x.value === this.model.logisticsService);
        if (!isValidLogisticService) {
            this.model.logisticsService = null;
            this.logisticsServiceTypeChange(this.model.logisticsService, true, true);
        }

        //validate the carrier belongs to the mode of transport
        if (!StringHelper.isNullOrEmpty(this.model.carrier)) {
            this.carrierFilter(this.model.carrier);
            this.validateForm(this._formValidationKeys.carrier);
        }

        const emitValue: IntegrationData = {
            name: '[po-fulfillment-general-info]modeOfTransportValueChanged',
            content: {
                'modeOfTransport': $event
            }
        };
        this.parentIntegration$.next(emitValue);

        
        if (this.model.buyerCompliance) {
            this.shipFromPortValueChangeEmitter();
            this.shipToPortValueChangeEmitter();
        }
    }

    requirePickupChange($event) {
        const emitValue: IntegrationData = {
            name: '[po-fulfillment-general-info]isRequirePickupValueChanged',
            content: {
                'isRequirePickup': this.model.isShipperPickup,
            }
        };
        this.parentIntegration$.next(emitValue);
    }

    isNotifyPartyAsConsigneeChange($event) {
        const emitValue: IntegrationData = {
            name: '[po-fulfillment-general-info]isNotifyPartyAsConsigneeChanged',
            content: {
                'isNotifyPartyAsConsignee': this.model.isNotifyPartyAsConsignee,
            }
        };
        this.parentIntegration$.next(emitValue);
    }

    /**Using to publish event when the ShipFrom Port has been changed. */
    shipFromPortValueChangeEmitter() {
        const emitValue: IntegrationData = {
            name: '[po-fulfillment-general-info]shipFromPortValueChanged'
        };
        this.parentIntegration$.next(emitValue);
    }

    /**Using to publish event when the ShipTo Port has been changed. */
    shipToPortValueChangeEmitter() {
        const emitValue: IntegrationData = {
            name: '[po-fulfillment-general-info]shipToPortValueChanged'
        };
        this.parentIntegration$.next(emitValue);
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

    updateLiteralLabels(mode) {
        if (!mode) {
            mode = "";
        }
        switch (mode.toLowerCase()) {
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

    get logisticServiceTypeOptions() {
        if (this.model?.modeOfTransport === ModeOfTransportType.Air) {
            return DropDowns.AirLogisticServiceStringType;
        }

        return DropDowns.LogisticsServiceStringType;
    }

    isVisibleField(moduleId: ViewSettingModuleIdType, fieldId: string): boolean {
        if (this.isAddMode || this.isEditMode) { // apply for view mode only
            return true;
        }

        return !FormHelper.isHiddenColumn(this.model.viewSettings, moduleId, fieldId);
    }

    ngOnDestroy(): void {
        this._subscriptions.map(x => x.unsubscribe());
    }
}
