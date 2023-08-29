import { Component, ViewChild, AfterViewInit, Renderer2,
    NgZone, OnDestroy, ElementRef, QueryList, ViewChildren } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormComponent, UserContextService, BuyerComplianceStatus,
    StringHelper, DropDowns, ApprovalAlertFrequency,
    BuyerComplianceStage,
    ValidationResultPolicy,
    ApproverSetting,
    AgentType,
    YesNoType,
    ApprovalDuration,
    OrganizationType,
    BookingPortType,
    POType,
    BuyerComplianceServiceType,
    AgentAssignmentMethodType,
    ModeOfTransport,
    ModeOfTransportType,
    EmailSettingType,
    EmailSettingTypeName} from 'src/app/core';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { ComplianceFormService } from './compliance-form.service';
import { AutoCompleteComponent, MultiSelectComponent } from '@progress/kendo-angular-dropdowns';
import { faPlus, faEllipsisV, faPencilAlt, faTrashAlt, faPowerOff, faUnlockAlt, faInfoCircle } from '@fortawesome/free-solid-svg-icons';
import { Subscription, fromEvent } from 'rxjs';
import { RowClassArgs } from '@progress/kendo-angular-grid';
import { State, process } from '@progress/kendo-data-query';
import { tap, take } from 'rxjs/operators';
import { EnumHelper } from 'src/app/core/helpers/enum.helper';
import { MultipleEmailDomainValidationPattern, MultipleEmailValidationPattern } from 'src/app/core/models/constants/app-constants';
import { AgentAssignmentComponent } from './agent-assignment/agent-assignment.component';
import { EmailSettingComponent } from '../email-setting/email-setting.component';

const tableRow = node => node.tagName.toLowerCase() === 'tr';
const closest = (node, predicate) => {
    while (node && !predicate(node)) {
        node = node.parentNode;
    }

    return node;
};

@Component({
    selector: 'app-compliance-form',
    templateUrl: './compliance-form.component.html',
    styleUrls: ['./compliance-form.component.scss']
})
export class ComplianceFormComponent extends FormComponent implements AfterViewInit, OnDestroy  {

    readonly BuyerComplianceServiceType = BuyerComplianceServiceType;

    modelName = 'compliances';
    faPlus = faPlus;
    faEllipsisV = faEllipsisV;
    faPencilAlt = faPencilAlt;
    faTrashAlt = faTrashAlt;
    faPowerOff = faPowerOff;
    faUnlockAlt = faUnlockAlt;
    faInfoCircle = faInfoCircle;

    TAB_TYPE = {
        PO_MANAGEMENT: 0,
        CARGO_LOADABILITY: 1,
        SHIPPING_COMPLIANCE: 2,
        AGENT_ASSIGNMENT: 3,
        BOOKING_POLICY: 5
    };

    tabSelected = this.TAB_TYPE.PO_MANAGEMENT;

    tabPrefix = {
        'emailSetting': 'tab7#'
    };

    noSystemEmail = [
        EmailSettingType.BookingImportviaAPI,
        EmailSettingType.BookingRejected,
        EmailSettingType.BookingCargoReceived
    ];

    currentUser: any;

    backupModel: any;
    model = {
        id: 0,

        // tab 1
        name: '',
        status: BuyerComplianceStatus.Active,
        stage: BuyerComplianceStage.Draft,
        stageName: 'label.draft',
        statusName: 'label.active',
        organizationId: null,
        organizationCode: '',
        organizationName: '',
        organizationHasCustomerPrefix: false,
        enforceCommercialInvoiceFormat: false,
        enforcePackingListFormat: false,
        isAssignedAgent: false,
        allowToBookIn: POType.Allocated,
        purchaseOrderTransmissionMethods: null,
        shortShipTolerancePercentage: null,
        overshipTolerancePercentage: null,
        hsCodeShipFromCountryIds: null,
        hsCodeShipToCountryIds: null,
        isProgressCargoReadyDates: false,
        isEmailNotificationToSupplier: false,
        isAllowMissingPO: true,
        emailNotificationTime:'12:00 AM',
        isCompulsory: false,
        progressNotifyDay: 0,
        integrateWithWMS: false,
        serviceType: 10,
        isAllowShowAdditionalInforPOListing: false,

        purchaseOrderVerificationSetting: {
            expectedDeliveryDateVerification: null,
            expectedShipDateVerification: null,
            consigneeVerification: null,
            shipperVerification: null,
            shipFromLocationVerification: null,
            shipToLocationVerification: null,
            paymentTermsVerification: null,
            paymentCurrencyVerification: null,
            modeOfTransportVerification: null,
            incotermVerification: null,
            movementTypeVerification: null,
            preferredCarrierVerification: null,
        },
        productVerificationSetting: {
            productCodeVerification: null,
            commodityVerification: null,
            hsCodeVerification: null,
            countryOfOriginVerification: null,
            isRequireGrossWeight: false,
            isRequireVolume: false
        },

        // tab 2
        bookingTimeless: {
            cyEarlyBookingTimeless: null,
            cyLateBookingTimeless: null,
            cfsEarlyBookingTimeless: null,
            cfsLateBookingTimeless: null,
            airEarlyBookingTimeless: null,
            airLateBookingTimeless: null,
            dateForComparison: DropDowns.BuyerComplianceDateForComparisons[1].value
        },
        cargoLoadabilities: [],

        // tab 3
        shippingCompliance: {
            allowPartialShipment: true,
            allowMixedCarton: false,
            allowMixedPack: null,
            allowMultiplePOPerFulfillment: true,
            isShipperPickup: false,
            isNotifyPartyAsConsignee: false

        },
        complianceSelection: {
            modeOfTransportIds: [],
            commodityIds: [],
            shipFromLocationIds: [],
            shipToLocationIds: [],
            movementTypeIds: [],
            incotermTypeIds: [],
            carrierIds: [],
            carrierSelectionNotes: ''
        },

        // tab 4
        agentAssignments: [],
        airAgentAssignments: [],

        // tab 5
        bookingPolicies: null,
        bookingPolicyAction: 10,
        bookingApproverSetting: 10,
        bookingApproverUser: '',
        approvalAlertFrequency: null,
        approvalDuration: null,
        agentAssignmentMethod: AgentAssignmentMethodType.bySystem,
        bypassEmailDomain: null,

        emailSettings: []
    };

    buyerList: any;
    agentOrgList: any;
    buyerFilter: any;
    agentOrgFilter: any;
    agentAssignmentMethodType = AgentAssignmentMethodType;

    // tab 1
    @ViewChild('buyerAutoComplete', { static: false }) public buyerAutoComplete: AutoCompleteComponent;
    @ViewChild('complianceNameElement', { static: false }) public complianceNameElement: ElementRef;

    // tab 2
    @ViewChild('cyEarlyBookingTimelessElement', { static: false }) public cyEarlyBookingTimelessElement: ElementRef;
    @ViewChild('cfsEarlyBookingTimelessElement', { static: false }) public cfsEarlyBookingTimelessElement: ElementRef;
    @ViewChild('airEarlyBookingTimelessElement', { static: false }) public airEarlyBookingTimelessElement: ElementRef;

    // tab 3
    @ViewChild('commoditySelections', { static: false }) commoditySelections: MultiSelectComponent;
    @ViewChild('shipFromLocationSelections', { static: false }) shipFromLocationSelections: MultiSelectComponent;
    @ViewChild('shipToLocationSelections', { static: false }) shipToLocationSelections: MultiSelectComponent;
    @ViewChild('incotermSelections', { static: false }) incotermSelections: MultiSelectComponent;
    @ViewChild('carrierSelections', { static: false }) carrierSelections: MultiSelectComponent;

    @ViewChildren('agentPorts') agentPorts: QueryList<MultiSelectComponent>;
    @ViewChildren('hsCodeCountrySettings') hsCodeCountrySettings: QueryList<MultiSelectComponent>;
    @ViewChild(AgentAssignmentComponent, { static: false }) AgentAssignmentComponent: AgentAssignmentComponent;
    @ViewChild(EmailSettingComponent, { static: false }) emailSettingComponent: EmailSettingComponent;
    buyerComplianceStatus = BuyerComplianceStatus;
    buyerComplianceStage = BuyerComplianceStage;
    poType = POType;
    approvalAlertFrequencyOptions = DropDowns.ApprovalAlertFrequency;
    approvalDurationOptions = DropDowns.ApprovalDuration;
    poTransmissionMethodOptions = DropDowns.PurchaseOrderTransmissionMethod;
    poTransmissionFrequencyOptions = DropDowns.PurchaseOrderFrequency;
    verificationSettingOptions = DropDowns.VerificationSetting;
    defaultVerificationSettingOptions = DropDowns.VerificationSetting;
    expectedVerificationSettingOptions = DropDowns.ExpectedVerificationSetting;
    buyerComplianceStatusOptions = DropDowns.BuyerComplianceStatus;
    allowMixedPackOptions = DropDowns.AllowMixedPack;
    modeOfTransportOptions = DropDowns.ModeOfTransport;
    commodityOptions = DropDowns.Commodity;
    movementTypeOptions = DropDowns.MovementType;
    incotermTypeOptions = DropDowns.IncotermType;
    validationResultOptions = DropDowns.ValidationResult;
    fulfillmentAccuracyOptions = DropDowns.FulfillmentAccuracy;
    cargoLoadabilityOptions = DropDowns.CargoLoadability;
    bookingTimelessOptions = DropDowns.BookingTimeless;
    serviceTypeOptions = DropDowns.BuyerComplianceServiceType;
    emailNotificationTimeOptions: string[] = ['12:00'];
    allLocationOptions: any;
    mainAllLocationOptions: any;
    allCarrierOptions: any;
    mainAllCarrierOptions: any;
    acpTimeout: any;
    gridBindingTimeout: any;
    buyerLoading: boolean;

    policyFormOpened: boolean;
    PolicyFormModeType = {
        add: 0,
        edit: 1,
        view: 2
    };
    policyFormMode = this.PolicyFormModeType.view;
    policyModel: any;

    dateForComparisonOptions = DropDowns.BuyerComplianceDateForComparisons;
    equipmentTypeOptions = DropDowns.EquipmentStringType;
    allEquipmentTypeOptions = DropDowns.EquipmentStringType;
    validationResult = ValidationResultPolicy;
    approverSettingOptions = DropDowns.ApproverSetting;
    approverSetting = ApproverSetting;

    autoCreateShipmentOption = DropDowns.YesNoType;
    agentTypeOption = DropDowns.AgentType;
    agentType = AgentType;
    yesNoType = YesNoType;
    agentOrganizationOptions = [];
    countryList = [];
    bookingPortOptions = DropDowns.BookingPortType;

    hsCodeSettings = [];
    hsCodeCountryOptions = [];
    patternEmail = MultipleEmailValidationPattern;
    patternEmailDomain = MultipleEmailDomainValidationPattern;
    allAgentAssignment = [];

    validationRules = {
        'organizationIdControl': {
            'required': 'label.customer',
            'invalid': 'label.customer',
            'duplicateCompliance': 'validation.duplicateCompliance',
            'missingCustomerPrefix': 'validation.missingCustomerPrefix',
        },
        'complianceName': {
            'required': 'label.complianceName'
        },
        'cyEarlyBookingTimeless': {
            'greaterThan': 'label.cyLateBookingPeriod',
        },
        'cfsEarlyBookingTimeless': {
            'greaterThan': 'label.cfsLateBookingPeriod',
        },
        'airEarlyBookingTimeless': {
            'greaterThan': 'label.airLateBookingPeriod',
        },
        'bookingApproverUser': {
            'required': 'label.approverUser',
            'pattern': 'label.approverUser'
        },
        'bypassEmailDomain': {
            'pattern': 'label.emailDomainToBypassApproval'
        }
    };

    stringHelper = StringHelper;

    // drag row on grid
    public policyState: State = {
        skip: 0,
        take: 10
    };
    private policyGridSubscription: Subscription;


    isAllowPartialShipmentSelected(id): boolean {
        return this.model.complianceSelection.modeOfTransportIds.some(item => item === id);
    }

    isCommoditySelected(id): boolean {
        return this.model.complianceSelection.commodityIds.some(item => item === id);
    }

    isShipFromLocationSelected(description): boolean {
        return this.model.complianceSelection.shipFromLocationIds.some(item => item === description);
    }

    isShipToLocationSelected(description): boolean {
        return this.model.complianceSelection.shipToLocationIds.some(item => item === description);
    }

    isPortAgentSelected(description, rowIndex): boolean {
        return this.model.agentAssignments[rowIndex].portSelectionIds.some(item => item === description);
    }

    isMovementTypeSelected(id): boolean {
        return this.model.complianceSelection.movementTypeIds.some(item => item === id);
    }

    isIncotermSelected(id): boolean {
        return this.model.complianceSelection.incotermTypeIds.some(item => item === id);
    }

    isCarrierSelected(description): boolean {
        return this.model.complianceSelection.carrierIds.some(item => item === description);
    }

    labelFromEnum(arr, value) {
        return EnumHelper.convertToLabel(arr, value);
    }

    constructor(protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: ComplianceFormService,
        public translateService: TranslateService,
        private _userContext: UserContextService,
        private renderer: Renderer2, private zone: NgZone) {
        super(route, service, notification, translateService, router);
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
                if (!user.isInternal) {
                    this.service.affiliateCodes = user.affiliates;
                }
            }
        });

        this.service.getAllLocations().subscribe(data => {
            this.mainAllLocationOptions = data;
            this.mainAllLocationOptions.forEach(item => {
                const des = item.description.indexOf('-') >= 0 ? item.description.split('-') : [];
                if (des.length >= 2) {
                    item.countryId = des[0];
                    item.locationiId = des[1];
                } else {
                    item.countryId = null;
                    item.locationiId = null;
                }
            });
            this.bindingAgentPortLocation();
            this.bindingAirAgentPortLocation();
            this.allLocationOptions = data;
        });
        this.service.getAllCarriers().subscribe(data => {
            this.mainAllCarrierOptions = data;
            this.allCarrierOptions = data;
        });
        this.service.getCountries().subscribe((data: any) => {
            this.countryList = data;
            this.hsCodeCountryOptions = data;
        });
        this.defaultVerificationSettingOptions = this.verificationSettingOptions.filter(x => x.default);
        this.emailNotificationTimeOptions = this.service.initializeTime();
    }

    bindingAgentPortLocation() {
        if (this.model.agentAssignments && this.model.agentAssignments.length > 0 &&
            this.mainAllLocationOptions && this.mainAllLocationOptions.length > 0) {
            this.model.agentAssignments.forEach(element => {
                if (!StringHelper.isNullOrEmpty(element.countryId)) {
                    const agentCountryId = element.countryId.toString();
                    element.portLocations = this.mainAllLocationOptions
                    .filter(s => s.locationiId && s.countryId === agentCountryId);
                }
            });
        }
    }

    bindingAirAgentPortLocation() {
        if (this.model.airAgentAssignments && this.model.airAgentAssignments.length > 0 &&
            this.mainAllLocationOptions && this.mainAllLocationOptions.length > 0) {
            this.model.airAgentAssignments.forEach(element => {
                if (!StringHelper.isNullOrEmpty(element.countryId)) {
                    const agentCountryId = element.countryId.toString();
                    element.portLocations = this.mainAllLocationOptions
                    .filter(s => s.locationiId && s.countryId === agentCountryId);
                }
            });
        }
    }

    addBlankLoadabilityRow() {
        if (!this.model.cargoLoadabilities) {
            this.model.cargoLoadabilities = [];
        }
        this.model.cargoLoadabilities.push({ isAddLine: true });
        // Using rowIndex as a key
        const rowIndex = this.model.cargoLoadabilities.length - 1;
        this.formErrors['gridValidationRules'][rowIndex] = this.getGridRuleValidation();
    }

    addBlankAssignmentRow() {
        if (!this.model.agentAssignments) {
            this.model.agentAssignments = [];
        }
        this.model.agentAssignments.push({
            portSelectionIds: [],
            countryId: null,
            order: this.model.agentAssignments.length + 1,
            modeOfTransport: ModeOfTransportType.Sea
        });
        // Using rowIndex as a key
        const rowIndex = this.model.agentAssignments.length - 1;
        this.formErrors['agentValidationRules'][rowIndex] = this.getAgentGridValidation();
    }

    removeLoadability(index) {
        this.model.cargoLoadabilities.splice(index, 1);
        this.model.cargoLoadabilities = Object.assign([], this.model.cargoLoadabilities);
        this.updateEquipmentTypeOptions();
    }

    removeAssignment(index) {
        this.model.agentAssignments.splice(index, 1);
        this.model.agentAssignments = Object.assign([], this.model.agentAssignments);
    }

    onSelectTabChanged(event) {
        this.tabSelected = event.index;
        if (this.tabSelected === this.TAB_TYPE.BOOKING_POLICY && this.isEditMode) {
            this.invokeDragDropPolicies();
        }
    }

    onInitDataLoaded(data): void {
        if (this.model != null) {
            this.allAgentAssignment = Object.assign([], this.model.agentAssignments);
            this.backupModel = Object.assign({}, this.model); // backup for any further revert case.
            this.bindingData();
        }
        if (this.isAddMode) {
            /*
            Init data for Email Settings.
            */
            Object.values(EmailSettingType).filter((v) => !isNaN(Number(v))).forEach(
                (type: number) => {;
                    this.model.emailSettings.push({
                        emailType: type,
                        emailTypeName: EmailSettingTypeName[type],
                        defaultSendTo: 1
                    });
                }
            );

        }
        this.bindingHSCodeSetting();
        this.serviceTypeValueChanged(this.model?.serviceType);
    }

    bindingData() {
        if (this.model.cargoLoadabilities && this.model.cargoLoadabilities.length) {
            this.model.cargoLoadabilities = Object.assign([], this.model.cargoLoadabilities);
            this.formErrors['gridValidationRules'] = [];
            this.model.cargoLoadabilities.forEach((element, index) => {
                this.formErrors['gridValidationRules'][index] = this.getGridRuleValidation();
            });
        } else {
            this.model.cargoLoadabilities = [];
            this.formErrors['gridValidationRules'] = [];
        }

        if (this.model.agentAssignments && this.model.agentAssignments.length && this.model.agentAssignments.some(c=>c.modeOfTransport === ModeOfTransportType.Sea)) {
            this.model.agentAssignments = Object.assign([], this.model.agentAssignments);
            this.model.agentAssignments = this.model.agentAssignments.filter(c=>c.modeOfTransport === ModeOfTransportType.Sea);
        } else {
            this.model.agentAssignments = [
                {
                    autoCreateShipment: YesNoType.No,
                    agentType: AgentType.Origin,
                    countryId: null,
                    portSelectionIds: [],
                    agentOrganizationId: null,
                    agentOrganizationName: '',
                    portLocations: [],
                    order: 1,
                    modeOfTransport:ModeOfTransportType.Sea
                },
                {
                    autoCreateShipment: null,
                    agentType: AgentType.Destination,
                    countryId: null,
                    portSelectionIds: [],
                    agentOrganizationId: null,
                    agentOrganizationName: '',
                    portLocations: [],
                    order: 2,
                    modeOfTransport:ModeOfTransportType.Sea
                }
            ];
        }
        this.formErrors['agentValidationRules'] = [];
        this.model.agentAssignments.forEach((element, index) => {
            this.formErrors['agentValidationRules'][index] = this.getAgentGridValidation();
        });

        if (this.allAgentAssignment?.some(c=>c.modeOfTransport === ModeOfTransportType.Air)) {
            this.model.airAgentAssignments = this.allAgentAssignment?.filter(c=>c.modeOfTransport === ModeOfTransportType.Air);
        } else {
            this.model.airAgentAssignments = [
                {
                    autoCreateShipment: YesNoType.No,
                    agentType: AgentType.Origin,
                    countryId: null,
                    portSelectionIds: [],
                    agentOrganizationId: null,
                    agentOrganizationName: '',
                    portLocations: [],
                    order: 1,
                    modeOfTransport:ModeOfTransportType.Air
                },
                {
                    autoCreateShipment: null,
                    agentType: AgentType.Destination,
                    countryId: null,
                    portSelectionIds: [],
                    agentOrganizationId: null,
                    agentOrganizationName: '',
                    portLocations: [],
                    order: 2,
                    modeOfTransport:ModeOfTransportType.Air
                }
            ];
        }
        this.formErrors['airAgentValidationRules'] = [];
        this.model.airAgentAssignments.forEach((element, index) => {
            this.formErrors['airAgentValidationRules'][index] = this.getAgentGridValidation();
        });

        this.bindingAgentPortLocation();
        this.bindingAirAgentPortLocation();

        this.updateEquipmentTypeOptions();

        this.service.getOrganizations().subscribe(
            (data: any) => {
                this.buyerList = data;
                this.agentOrgList = this.buyerList.filter(x => x.organizationType === OrganizationType.Agent);

                // show Org name, start at row 1
                for (let i = 0; i < this.model.agentAssignments.length; i++) {
                    const item = this.model.agentAssignments[i];
                    const agentOrganizationId = +item.agentOrganizationId;
                    const destinationOrg = this.agentOrgList.find(x => x.id === agentOrganizationId);
                    item.agentOrganizationName = destinationOrg ? destinationOrg.name : '';
                }

                for (let i = 0; i < this.model.airAgentAssignments.length; i++) {
                    const item = this.model.airAgentAssignments[i];
                    const agentOrganizationId = +item.agentOrganizationId;
                    const destinationOrg = this.agentOrgList.find(x => x.id === agentOrganizationId);
                    item.agentOrganizationName = destinationOrg ? destinationOrg.name : '';
                }

                const organization = this.buyerList.find(x => x.id === this.model.organizationId);
                this.model.organizationCode = organization && organization.code;
                this.model.organizationHasCustomerPrefix = true;
            }
        );
        if (this.model.serviceType === BuyerComplianceServiceType.WareHouse) {
            this.bindingEmailSetting();
        }
    }

    bindingEmailSetting() {
        if (this.model.emailSettings && this.model.emailSettings.length > 0) {
            this.model.emailSettings.forEach((e, i) => {
                if (!e.defaultSendTo && !this.noSystemEmail.includes(e.emailType)){
                    this.validationRules[this.tabPrefix.emailSetting + 'sendTo_' + i] = {
                        'pattern': 'label.sendTo',
                        'required': 'label.sendTo'
                    };
                }
                else {
                    this.validationRules[this.tabPrefix.emailSetting + 'sendTo_' + i] = {
                        'pattern': 'label.sendTo'
                    };
                }
                this.validationRules[this.tabPrefix.emailSetting + 'cc_' + i] = {
                    'pattern': 'label.cc'
                };
            });
        }
    }

    clearFormErrors(tabPrefix) {
        Object.keys(this.formErrors)
        .filter(x => x.startsWith(tabPrefix))
        .map(x => {
            delete this.formErrors[x];
        });
    }

    invokeDragDropPolicies() {
        clearTimeout(this.gridBindingTimeout);
        this.acpTimeout = setTimeout(() => {
            this.policyGridDestroy();
            this.policyGridSubscription = this.handleDragAndDrop();
        }, 1000);
    }

    backList() {
        this.router.navigate(['/compliances']);
    }

    buyerFilterChange(filterName: string) {
        if (filterName.length >= 3) {
            this.buyerAutoComplete.toggle(false);
            this.buyerFilter = [];
            clearTimeout(this.acpTimeout);
            this.acpTimeout = setTimeout(() => {
                this.buyerLoading = true;
                filterName = filterName.toLowerCase();
                let takeRecords = 10;
                if (this.buyerList != null) {
                    for (let i = 0; i < this.buyerList.length && takeRecords > 0; i++) {
                        if (this.buyerList[i].code.toLowerCase().indexOf(filterName) !== -1) {
                            this.buyerFilter.push({ value: this.buyerList[i].value, code: this.buyerList[i].code });
                            takeRecords--;
                        }
                    }
                    this.buyerAutoComplete.toggle(true);
                    this.buyerLoading = false;
                }
            }, 400);
        } else {
            this.buyerLoading = false;
            if (this.acpTimeout) {
                clearTimeout(this.acpTimeout);
            }
            this.buyerAutoComplete.toggle(false);
        }
    }

    agentOrgFilterChange(value) {
        this.agentOrgFilter = [];
        if (value.length >= 3) {
            this.agentOrgFilter = this.agentOrgList.filter((s) => s.name.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        }
    }

    agentOrgValueChange(value, rowIndex) {
        const selectedItem = this.agentOrgList.find(
            (element) => {
                return element.name === value;
            });

        if (StringHelper.isNullOrEmpty(selectedItem)) {
            this.model.agentAssignments[rowIndex].agentOrganizationId = null;
            this.model.agentAssignments[rowIndex].agentOrganizationName = '';
            if (!StringHelper.isNullOrEmpty(value)) {
                this.formErrors['agentValidationRules'][rowIndex].agentOrganizationId.required =
                this.translateService.instant('validation.requiredField',
                {
                    fieldName: this.translateService.instant('label.organization')
                });
            }
            return;
        }
        this.formErrors['agentValidationRules'][rowIndex].agentOrganizationId.required = '';
        this.formErrors['agentValidationRules'][rowIndex].agentOrganizationId.notExists = '';
        this.service.checkAgentOrgHasContactEmail(selectedItem.id).subscribe(data => {
            if (!data) {
                this.formErrors['agentValidationRules'][rowIndex].agentOrganizationId.notExists =
                this.translateService.instant('validation.assignAgentMissingContactEmail');
                return;
            }

            if (!StringHelper.isNullOrEmpty(selectedItem)) {
                this.model.agentAssignments[rowIndex].agentOrganizationId = selectedItem.id;
                this.model.agentAssignments[rowIndex].agentOrganizationName = selectedItem.name;
                this.formErrors['agentValidationRules'][rowIndex].agentOrganizationId.required = '';
                this.formErrors['agentValidationRules'][rowIndex].agentOrganizationId.notExists = '';
            }
        });
    }

    buyerValueChange(value) {
        const selectedItem = this.buyerList.find(
            (element) => {
                return element.code === value;
            });

        if (StringHelper.isNullOrEmpty(selectedItem)) {
            this.setOrganizationIdError(value);
            return;
        }

        // check customer prefix
        this.service.checkOrganizationHasCustomerPrefix(selectedItem.id).subscribe(hasCustomerPrefix => {
            if (!hasCustomerPrefix) {
                this.model.organizationHasCustomerPrefix = false;
                this.setInvalidControl('organizationIdControl', 'missingCustomerPrefix');
                return;
            }

            // check duplicate
            this.service.checkOrganizationExists(this.model.id, selectedItem.id).subscribe(isExists => {
                if (isExists) {
                    this.setInvalidControl('organizationIdControl', 'duplicateCompliance');
                    return;
                }

                if (!StringHelper.isNullOrEmpty(selectedItem)) {
                    this.model.organizationId = selectedItem.id;
                    this.model.organizationCode = selectedItem.code;
                    this.model.name = selectedItem.name;
                    this.model.organizationName = selectedItem.name;
                    this.model.organizationHasCustomerPrefix = true;
                    this.formErrors['organizationIdControl'] = '';
                }
                this.buyerLoading = false;
            });
        });
    }

    setOrganizationIdError(value) {
        this.model.organizationId = null;
        this.model.organizationCode = '';
        this.model.organizationName = '';
        this.model.organizationHasCustomerPrefix = false;
        if (!StringHelper.isNullOrEmpty(value)) {
            this.setInvalidControl('organizationIdControl');
        }
        this.buyerLoading = false;
    }

    check_cyEarlyBookingTimeless_Validates() {
        if (!StringHelper.isNullOrEmpty(this.model.bookingTimeless.cyEarlyBookingTimeless)) {
            if (!StringHelper.isNullOrEmpty(this.model.bookingTimeless.cyLateBookingTimeless) &&
                this.model.bookingTimeless.cyEarlyBookingTimeless <= this.model.bookingTimeless.cyLateBookingTimeless) {
                this.setInvalidControl('cyEarlyBookingTimeless', 'greaterThan');
            } else {
                this.formErrors['cyEarlyBookingTimeless'] = '';
            }
        }
    }

    check_cfsEarlyBookingTimeless_Validates() {
        if (!StringHelper.isNullOrEmpty(this.model.bookingTimeless.cfsEarlyBookingTimeless)) {
            if (!StringHelper.isNullOrEmpty(this.model.bookingTimeless.cfsLateBookingTimeless) &&
                this.model.bookingTimeless.cfsEarlyBookingTimeless <= this.model.bookingTimeless.cfsLateBookingTimeless) {
                this.setInvalidControl('cfsEarlyBookingTimeless', 'greaterThan');
            } else {
                this.formErrors['cfsEarlyBookingTimeless'] = '';
            }
        }
    }

    check_airEarlyBookingTimeless_Validates() {
        if (!StringHelper.isNullOrEmpty(this.model.bookingTimeless.airEarlyBookingTimeless)) {
            if (!StringHelper.isNullOrEmpty(this.model.bookingTimeless.airLateBookingTimeless) &&
                this.model.bookingTimeless.airEarlyBookingTimeless <= this.model.bookingTimeless.airLateBookingTimeless) {
                this.setInvalidControl('airEarlyBookingTimeless', 'greaterThan');
            } else {
                this.formErrors['airEarlyBookingTimeless'] = '';
            }
        }
    }

    // check validate on grid
    checkGridNameValidates(rowIndex) {
        if (this.model.cargoLoadabilities.length === 0) {
            return;
        }
        const currentRow = this.model.cargoLoadabilities[rowIndex];
        currentRow.isNameValid = false;
        if (StringHelper.isNullOrEmpty(currentRow.name)) {
            this.formErrors['gridValidationRules'][rowIndex].name.required = this.translateService.instant('validation.requiredField',
            {
                fieldName: this.translateService.instant('label.name')
            });
            return;
        }
        this.formErrors['gridValidationRules'][rowIndex].name.required = '';
        currentRow.isNameValid = true;
    }

    /**
     * To validate Organization of current row data of Agent Assignment
     * @param rowIndex Row index number
     * @param airAgentAssignment If it is Air agent assignment. Leave null, it is on Sea agent assignment
     * @returns Void
     */
    checkAgentOrgValidates(rowIndex, airAgentAssignment?: boolean) {
        const gridData = airAgentAssignment ? this.model.airAgentAssignments : this.model.agentAssignments;

        if (gridData.length === 0) {
            return;
        }

        const currentRow = gridData[rowIndex];
        if (StringHelper.isNullOrEmpty(currentRow.agentOrganizationId)) {
            this.formErrors[`${airAgentAssignment ? 'airAgent' : 'agent'}ValidationRules`][rowIndex].agentOrganizationId.required =
            this.translateService.instant('validation.requiredField',
            {
                fieldName: this.translateService.instant('label.organization')
            });
            return;
        }
        this.formErrors[`${airAgentAssignment ? 'airAgent' : 'agent'}ValidationRules`][rowIndex].agentOrganizationId.required = '';
    }

    /**
     * To validate Country of current row data of Agent Assignment
     * @param rowIndex Row index number
     * @param airAgentAssignment If it is Air agent assignment. Leave null, it is on Sea agent assignment
     * @returns Void
     */
    checkAgentCountryValidates(rowIndex, airAgentAssignment?: boolean) {
        const gridData = airAgentAssignment ? this.model.airAgentAssignments : this.model.agentAssignments;

        if (gridData.length === 0) {
            return;
        }
        const currentRow = gridData[rowIndex];
        if (StringHelper.isNullOrEmpty(currentRow.countryId)) {
            this.formErrors[`${airAgentAssignment ? 'airAgent' : 'agent'}ValidationRules`][rowIndex].countryId.required =
            this.translateService.instant('validation.requiredField',
            {
                fieldName: this.translateService.instant('label.country')
            });
            return;
        }
        this.formErrors[`${airAgentAssignment ? 'airAgent' : 'agent'}ValidationRules`][rowIndex].countryId.required = '';
    }

    /**
     * To validate "Auto create shipment" of current row data of Agent Assignment
     * @param rowIndex Row index number
     * @param airAgentAssignment If it is Air agent assignment. Leave null, it is on Sea agent assignment
     * @returns Void
     */
    checkAgentAutoCreateShipmentValidates(rowIndex, airAgentAssignment?: boolean) {
        const gridData = airAgentAssignment ? this.model.airAgentAssignments : this.model.agentAssignments;

        if (gridData.length === 0) {
            return;
        }
        const currentRow = this.model.agentAssignments[rowIndex];
        if (StringHelper.isNullOrEmpty(currentRow.autoCreateShipment)) {
            this.formErrors[`${airAgentAssignment ? 'airAgent' : 'agent'}ValidationRules`][rowIndex].autoCreateShipment.required =
            this.translateService.instant('validation.requiredField',
            {
                fieldName: this.translateService.instant('label.autoCreateShipment')
            });
            return;
        }
        this.formErrors[`${airAgentAssignment ? 'airAgent' : 'agent'}ValidationRules`][rowIndex].autoCreateShipment.required = '';
    }

    /**
     * To validate Agent type of current row data of Agent Assignment
     * @param rowIndex Row index number
     * @param airAgentAssignment If it is Air agent assignment. Leave null, it is on Sea agent assignment
     * @returns Void
     */
    checkAgentAgentTypeValidates(rowIndex, airAgentAssignment?: boolean) {
        const gridData = airAgentAssignment ? this.model.airAgentAssignments : this.model.agentAssignments;

        if (gridData.length === 0) {
            return;
        }
        const currentRow = gridData[rowIndex];
        if (StringHelper.isNullOrEmpty(currentRow.agentType)) {
            this.formErrors[`${airAgentAssignment ? 'airAgent' : 'agent'}ValidationRules`][rowIndex].agentType.required =
            this.translateService.instant('validation.requiredField',
            {
                fieldName: this.translateService.instant('label.agentType')
            });
            return;
        }
        this.formErrors[`${airAgentAssignment ? 'airAgent' : 'agent'}ValidationRules`][rowIndex].agentType.required = '';
    }

    checkGridEquipmentTypeValidates(rowIndex) {
        if (this.model.cargoLoadabilities.length === 0) {
            return;
        }
        const currentRow = this.model.cargoLoadabilities[rowIndex];
        currentRow.isEquipmentTypeValid = false;
        if (StringHelper.isNullOrEmpty(currentRow.equipmentType)) {
            this.formErrors['gridValidationRules'][rowIndex].equipmentType.required = this.translateService
            .instant('validation.requiredField',
            {
                fieldName: this.translateService.instant('label.equipmentType')
            });
            return;
        }
        this.formErrors['gridValidationRules'][rowIndex].equipmentType.required = '';
        currentRow.isEquipmentTypeValid = true;
    }

    checkGridCYMinimumCBMValidates(rowIndex) {
        if (this.model.cargoLoadabilities.length === 0) {
            return;
        }
        const currentRow = this.model.cargoLoadabilities[rowIndex];
        currentRow.isCYMinimumCBMValid = false;
        if (StringHelper.isNullOrEmpty(currentRow.cyMinimumCBM)) {
            this.formErrors['gridValidationRules'][rowIndex].cyMinimumCBM.required = this.translateService
            .instant('validation.requiredField',
            {
                fieldName: this.translateService.instant('label.cyMinimumCBM')
            });
            return;
        }
        this.formErrors['gridValidationRules'][rowIndex].cyMinimumCBM.required = '';
        if (this.checkCY_CBMValidate(currentRow, rowIndex)) {
            currentRow.isCYMinimumCBMValid = true;
            currentRow.isCyMaximumCBMValid = true;
        }
    }

    checkGridCyMaximumCBMValidates(rowIndex) {
        if (this.model.cargoLoadabilities.length === 0) {
            return;
        }
        const currentRow = this.model.cargoLoadabilities[rowIndex];
        currentRow.isCyMaximumCBMValid = false;
        if (StringHelper.isNullOrEmpty(currentRow.cyMaximumCBM)) {
            this.formErrors['gridValidationRules'][rowIndex].cyMaximumCBM.required = this.translateService
            .instant('validation.requiredField',
            {
                fieldName: this.translateService.instant('label.cyMaximumCBM')
            });
            return;
        }
        this.formErrors['gridValidationRules'][rowIndex].cyMaximumCBM.required = '';
        if (this.checkCY_CBMValidate(currentRow, rowIndex)) {
            currentRow.isCYMinimumCBMValid = true;
            currentRow.isCyMaximumCBMValid = true;
        }
    }

    checkGridCFSMinimumCBMValidates(rowIndex) {
        if (this.model.cargoLoadabilities.length === 0) {
            return;
        }
        const currentRow = this.model.cargoLoadabilities[rowIndex];
        currentRow.isCFSMinimumCBMValid = false;
        if (StringHelper.isNullOrEmpty(currentRow.cfsMinimumCBM)) {
            this.formErrors['gridValidationRules'][rowIndex].cfsMinimumCBM.required = this.translateService
            .instant('validation.requiredField',
            {
                fieldName: this.translateService.instant('label.cfsMinimumCBM')
            });
            return;
        }
        this.formErrors['gridValidationRules'][rowIndex].cfsMinimumCBM.required = '';
        if (this.checkCFS_CBMValidate(currentRow, rowIndex)) {
            currentRow.isCFSMinimumCBMValid = true;
            currentRow.isCFSMaximumCBMValid = true;
        }
    }

    checkGridCFSMaximumCBMValidates(rowIndex) {
        if (this.model.cargoLoadabilities.length === 0) {
            return;
        }
        const currentRow = this.model.cargoLoadabilities[rowIndex];
        currentRow.isCFSMaximumCBMValid = false;
        if (StringHelper.isNullOrEmpty(currentRow.cfsMaximumCBM)) {
            this.formErrors['gridValidationRules'][rowIndex].cfsMaximumCBM.required = this.translateService
            .instant('validation.requiredField',
            {
                fieldName: this.translateService.instant('label.cfsMaximumCBM')
            });
            return;
        }
        this.formErrors['gridValidationRules'][rowIndex].cfsMaximumCBM.required = '';
        if (this.checkCFS_CBMValidate(currentRow, rowIndex)) {
            currentRow.isCFSMinimumCBMValid = true;
            currentRow.isCFSMaximumCBMValid = true;
        }
    }

    checkCY_CBMValidate(currentRow, rowIndex) {
        if (!StringHelper.isNullOrEmpty(currentRow.cyMaximumCBM) &&
            currentRow.cyMinimumCBM >= currentRow.cyMaximumCBM) {
            this.formErrors['gridValidationRules'][rowIndex].cyMinimumCBM.lessThanMaxCBM = this.translateService
            .instant('validation.lessThan',
            {
                fieldName: this.translateService.instant('label.cyMaximumCBM')
            });
            return false;
        }
        this.formErrors['gridValidationRules'][rowIndex].cyMinimumCBM.lessThanMaxCBM = '';
        return true;
    }

    checkCFS_CBMValidate(currentRow, rowIndex) {
        if (!StringHelper.isNullOrEmpty(currentRow.cfsMaximumCBM) &&
            currentRow.cfsMinimumCBM >= currentRow.cfsMaximumCBM) {
            this.formErrors['gridValidationRules'][rowIndex].cfsMinimumCBM.lessThanMaxCBM = this.translateService
            .instant('validation.lessThan',
            {
                fieldName: this.translateService.instant('label.cfsMaximumCBM')
            });
            return false;
        }
        this.formErrors['gridValidationRules'][rowIndex].cfsMinimumCBM.lessThanMaxCBM = '';
        return true;
    }

    isAllFieldValidates(currentRow) {
        return currentRow.isNameValid && currentRow.isEquipmentTypeValid && currentRow.isCYMinimumCBMValid &&
        currentRow.isCyMaximumCBMValid && currentRow.isCFSMinimumCBMValid && currentRow.isCFSMaximumCBMValid;
    }
    // check validate on grid END

    policyFormClosedHandler() {
        this.policyFormOpened = false;
    }

    policyAddHandler(modelPopup: any) {
        modelPopup.order = this.model.bookingPolicies.length + 1;
        this.model.bookingPolicies.push(modelPopup);
        this.policyFormOpened = false;
        this.policyFormMode = this.PolicyFormModeType.add;

        this.invokeDragDropPolicies();
    }

    policyEditHandler(modelPopup: any) {
        const currentIndex = this.model.bookingPolicies.findIndex(x => x.order === modelPopup.order);
        if (currentIndex >= 0) {
            this.model.bookingPolicies[currentIndex] = modelPopup;
            this.policyFormOpened = false;

            this.invokeDragDropPolicies();
        }
    }

    public policyStateChange(state: State): void {
        this.policyState = state;
        const result = process([], this.policyState);
        this.model.bookingPolicies = result.data;
        this.policyGridDestroy();
        this.zone.onStable.pipe(take(1))
            .subscribe(() => this.policyGridSubscription = this.handleDragAndDrop());
    }

    public ngAfterViewInit(): void {
        const result = process([], this.policyState);
        this.model.bookingPolicies = result.data;
    }
    public rowCallback(context: RowClassArgs) {
        return {
            dragging: context.dataItem.dragging
        };
    }

    public ngOnDestroy(): void {
        this.policyGridDestroy();
    }

    private policyGridDestroy() {
        if (this.policyGridSubscription) {
            this.policyGridSubscription.unsubscribe();
        }
    }

    private handleDragAndDrop(): Subscription {
        const sub = new Subscription(() => {});
        let draggedItemIndex;

        // Register drag-drop for table booking policy setting
        const tableRows = Array.from(document.querySelectorAll('#booking-policy-table.k-grid tr'));
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
                const dataItem = this.model.bookingPolicies[draggedItemIndex];
                dataItem.dragging = true;
            }));

            sub.add(dragOver.subscribe((e: any) => {
                e.preventDefault();
                const dataItem = this.model.bookingPolicies.splice(draggedItemIndex, 1)[0];
                const dropIndex = closest(e.target, tableRow).rowIndex;
                const dropItem = this.model.bookingPolicies[dropIndex];

                draggedItemIndex = dropIndex;
                this.zone.run(() =>
                this.model.bookingPolicies.splice(dropIndex, 0, dataItem)
                );
            }));

            sub.add(dragEnd.subscribe((e: any) => {
                e.preventDefault();
                const dataItem = this.model.bookingPolicies[draggedItemIndex];
                dataItem.dragging = false;
                this.updateOrderBookingPolicy();
            }));
        });

        return sub;
    }

    updateOrderBookingPolicy() {
        let order = 1;
        this.model.bookingPolicies.forEach(element => {
            element.order = order++;
        });
    }

    viewPolicy(order) {
        this.editPolicy(order);
        this.policyFormMode = this.PolicyFormModeType.view;
    }

    addPolicy() {
        this.policyFormMode = this.PolicyFormModeType.add;
        this.policyModel = {
            name: '',
            modeOfTransportIds: [],
            movementTypeIds: [],
            incotermTypeIds: [],
            shipFromIds: [],
            shipToIds: [],
            carrierIds: [],
            fulfillmentAccuracyIds: [],
            logisticsServiceSelectionIds: [],
            cargoLoadabilityIds: [],
            bookingTimelessIds: [],
            approverSetting: 10,
            action: null
        };
        this.policyFormOpened = true;
    }

    editPolicy(order) {
        this.policyFormMode = this.PolicyFormModeType.edit;
        const result = this.model.bookingPolicies.filter(x => x.order === order);
        if (result && result.length > 0) {
            this.policyModel = result[0];
            this.policyFormOpened = true;
        }
    }

    deletePolicy(order) {
        const currentIndex = this.model.bookingPolicies.findIndex(x => x.order === order);
        if (currentIndex >= 0) {
            this.model.bookingPolicies.splice(currentIndex, 1);
            this.updateOrderBookingPolicy();
        }
    }

    checkValidateSave_Tab1_2() {
        return !StringHelper.isNullOrEmpty(this.model.organizationId) && this.model.organizationHasCustomerPrefix &&
                !StringHelper.isNullOrEmpty(this.model.name);
    }

    checkValidateToActivate_Tab1_2() {
        return this.checkValidateSave_Tab1_2() &&
                !StringHelper.isNullOrEmpty(this.model.shortShipTolerancePercentage) &&
                !StringHelper.isNullOrEmpty(this.model.shortShipTolerancePercentage) &&
                this.model.purchaseOrderTransmissionMethods > 0 &&

                // PO Verification Setting
                this.model.purchaseOrderVerificationSetting.expectedShipDateVerification > 0 &&
                this.model.purchaseOrderVerificationSetting.expectedDeliveryDateVerification > 0 &&
                this.model.purchaseOrderVerificationSetting.shipperVerification > 0 &&
                this.model.purchaseOrderVerificationSetting.consigneeVerification > 0 &&
                this.model.purchaseOrderVerificationSetting.shipFromLocationVerification > 0 &&
                this.model.purchaseOrderVerificationSetting.shipToLocationVerification > 0 &&
                this.model.purchaseOrderVerificationSetting.paymentTermsVerification > 0 &&
                this.model.purchaseOrderVerificationSetting.paymentCurrencyVerification > 0 &&
                this.model.purchaseOrderVerificationSetting.modeOfTransportVerification > 0 &&
                this.model.purchaseOrderVerificationSetting.incotermVerification > 0 &&
                this.model.purchaseOrderVerificationSetting.movementTypeVerification > 0 &&
                this.model.purchaseOrderVerificationSetting.preferredCarrierVerification > 0 &&

                 // PO Verification Setting
                 this.model.productVerificationSetting.productCodeVerification > 0 &&
                 this.model.productVerificationSetting.commodityVerification > 0 &&
                 this.model.productVerificationSetting.hsCodeVerification > 0 &&
                 this.model.productVerificationSetting.countryOfOriginVerification > 0;
    }

    checkValidateSave_Tab3() {
        if (this.model.cargoLoadabilities) {
            let gridValid = true;
            for (let i = 0; i < this.model.cargoLoadabilities.length; i++) {
                const cargoLoadability = this.model.cargoLoadabilities[i];
                if (StringHelper.isNullOrEmpty(cargoLoadability.name) ||
                    StringHelper.isNullOrEmpty(cargoLoadability.equipmentType) ||
                    StringHelper.isNullOrEmpty(cargoLoadability.cyMinimumCBM) ||
                    StringHelper.isNullOrEmpty(cargoLoadability.cyMaximumCBM) ||
                    StringHelper.isNullOrEmpty(cargoLoadability.cfsMinimumCBM) ||
                    StringHelper.isNullOrEmpty(cargoLoadability.cfsMaximumCBM)) {
                    gridValid = false;
                }

                this.checkGridNameValidates(i);
                this.checkGridEquipmentTypeValidates(i);
                this.checkGridCYMinimumCBMValidates(i);
                this.checkGridCyMaximumCBMValidates(i);
                this.checkGridCFSMinimumCBMValidates(i);
                this.checkGridCFSMaximumCBMValidates(i);
            }

            if (!gridValid) {
                return false;
            }
        }

        if (!StringHelper.isNullOrEmpty(this.model.bookingTimeless.cyEarlyBookingTimeless) &&
            !StringHelper.isNullOrEmpty(this.model.bookingTimeless.cyLateBookingTimeless)) {
            if (this.model.bookingTimeless.cyEarlyBookingTimeless <= this.model.bookingTimeless.cyLateBookingTimeless) {
                return false;
            }
        }
        if (!StringHelper.isNullOrEmpty(this.model.bookingTimeless.cfsEarlyBookingTimeless) &&
            !StringHelper.isNullOrEmpty(this.model.bookingTimeless.cfsLateBookingTimeless)) {
            if (this.model.bookingTimeless.cfsEarlyBookingTimeless <= this.model.bookingTimeless.cfsLateBookingTimeless) {
                return false;
            }
        }
        if (!StringHelper.isNullOrEmpty(this.model.bookingTimeless.airEarlyBookingTimeless) &&
            !StringHelper.isNullOrEmpty(this.model.bookingTimeless.airLateBookingTimeless)) {
            if (this.model.bookingTimeless.airEarlyBookingTimeless <= this.model.bookingTimeless.airLateBookingTimeless) {
                return false;
            }
        }

        return true;
    }

    /**
     * To activate buyer compliance, validate Agent Assignments tab (for both Sea and Air)
     * @returns boolean
     */
    checkValidateActivate_Tab5(): boolean  {

    let gridValid = true;

    // Sea Agent Assignment
    const seaAgentAssignments = this.model.agentAssignments;

    // check origin and destination org input
    for (let i = 0; i <= 1; i++) {
        if (StringHelper.isNullOrEmpty(seaAgentAssignments[i].agentOrganizationId) || seaAgentAssignments[i].agentOrganizationId === 0) {
            this.checkAgentOrgValidates(i);
            gridValid = false;
        }
    }
    for (let i = 2; i < seaAgentAssignments.length; i++) {
        const agentAssignment = seaAgentAssignments[i];
        if (StringHelper.isNullOrEmpty(agentAssignment.agentType) ||
            StringHelper.isNullOrEmpty(agentAssignment.countryId) ||
            StringHelper.isNullOrEmpty(agentAssignment.agentOrganizationId) ||
            agentAssignment.agentOrganizationId === 0) {
            gridValid = false;
        }
        if (StringHelper.isNullOrEmpty(agentAssignment.agentType) || agentAssignment.agentType === AgentType.Origin) {
            if (StringHelper.isNullOrEmpty(agentAssignment.autoCreateShipment)) {
                gridValid = false;
            }
            this.checkAgentAutoCreateShipmentValidates(i);
        }

        this.checkAgentAgentTypeValidates(i);
        this.checkAgentCountryValidates(i);
        this.checkAgentOrgValidates(i);
    }


    // Air Agent Assignment
    const airAgentAssignments = this.model.airAgentAssignments;

    // check origin and destination org input
    for (let i = 0; i <= 1; i++) {
        if (StringHelper.isNullOrEmpty(airAgentAssignments[i].agentOrganizationId) || airAgentAssignments[i].agentOrganizationId === 0) {
            this.checkAgentOrgValidates(i, true);
            gridValid = false;
        }
    }

    for (let i = 2; i < airAgentAssignments.length; i++) {
        const agentAssignment = airAgentAssignments[i];
        if (StringHelper.isNullOrEmpty(agentAssignment.agentType) ||
            StringHelper.isNullOrEmpty(agentAssignment.countryId) ||
            StringHelper.isNullOrEmpty(agentAssignment.agentOrganizationId ||
            agentAssignment.agentOrganizationId === 0)) {
            gridValid = false;
        }
        if (StringHelper.isNullOrEmpty(agentAssignment.agentType) || agentAssignment.agentType === AgentType.Origin) {
            if (StringHelper.isNullOrEmpty(agentAssignment.autoCreateShipment)) {
                gridValid = false;
            }
            this.checkAgentAutoCreateShipmentValidates(i, true);
        }

        this.checkAgentAgentTypeValidates(i, true);
        this.checkAgentCountryValidates(i, true);
        this.checkAgentOrgValidates(i, true);
    }

    return gridValid;
    }

    /**
     * Email Setting tab.
     * */
    checkValidateSave_Tab7() {
        const isValid = !Object.keys(this.formErrors)
                            .some(x => x.startsWith(this.tabPrefix.emailSetting) && !StringHelper.isNullOrWhiteSpace(this.formErrors[x]));
        if (!isValid) {
            return false;
        }

        if (this.model && this.model.emailSettings?.length > 0) {
            for (let index = 0; index < this.model.emailSettings.length; index++) {
                const element = this.model.emailSettings[index];
                if (!element.defaultSendTo && !this.noSystemEmail.includes(element.emailType) && StringHelper.isNullOrWhiteSpace(element.sendTo)) {
                    return false;
                }
                const reg = new RegExp(MultipleEmailValidationPattern);
                if (!StringHelper.isNullOrWhiteSpace(element.sendTo) && !reg.test(element.sendTo)) {
                    return false;
                }
                if (!StringHelper.isNullOrWhiteSpace(element.cc) && !reg.test(element.cc)) {
                    return false;
                }
            }
        }
        return true;
    }

    /**
     * To validate all tab data prior to activate buyer compliance
     * @returns Array<number>
     */
    checkValidateToActivateBuyerCompliance(): Array<number> {
        const invalidTabs = [];
        // tab 1 + 2: General + PO Management
        if (!this.checkValidateToActivate_Tab1_2()) {
            invalidTabs.push(this.TAB_TYPE.PO_MANAGEMENT);
        }
        // tab 3: Cargo Loadability
        if (StringHelper.isNullOrEmpty(this.model.bookingTimeless.cyEarlyBookingTimeless) ||
            StringHelper.isNullOrEmpty(this.model.bookingTimeless.cyLateBookingTimeless) ||
            StringHelper.isNullOrEmpty(this.model.bookingTimeless.cfsEarlyBookingTimeless) ||
            StringHelper.isNullOrEmpty(this.model.bookingTimeless.cfsLateBookingTimeless) ||
            StringHelper.isNullOrEmpty(this.model.bookingTimeless.airEarlyBookingTimeless) ||
            StringHelper.isNullOrEmpty(this.model.bookingTimeless.airLateBookingTimeless) ||
            !this.checkValidateSave_Tab3() || this.model.cargoLoadabilities === null || this.model.cargoLoadabilities.length === 0) {
            invalidTabs.push(this.TAB_TYPE.CARGO_LOADABILITY);
        }

        // tab 5: Agent Assignment
        if (!this.checkValidateActivate_Tab5()) {
            invalidTabs.push(this.TAB_TYPE.AGENT_ASSIGNMENT);
        }

        // tab 6: Booking Validation Policy
        if (!(this.model.approvalAlertFrequency in ApprovalAlertFrequency) ||
            !(this.model.approvalDuration in ApprovalDuration) ||
            (this.model.bookingPolicyAction === ValidationResultPolicy.WarehouseApproval && StringHelper.isNullOrWhiteSpace(this.model.bypassEmailDomain))) {
            invalidTabs.push(this.TAB_TYPE.BOOKING_POLICY);
        }

        return invalidTabs;
    }

    onChangeIsProgressCargoReadyDates() {
        if (!this.model.isProgressCargoReadyDates) {
            this.model.isCompulsory = false;
            this.model.isEmailNotificationToSupplier = false;
        }
    }

    /**
     * To save buyer compliance.
     * It will ignore all required validations.
     * @returns
     */
    onSubmit() {
        this.model.enforcePackingListFormat = this.model.enforceCommercialInvoiceFormat;

        switch (this.tabSelected) {
            case this.TAB_TYPE.PO_MANAGEMENT:
                this.elements = {
                    'organizationIdControl': this.buyerAutoComplete,
                    'complianceName': this.complianceNameElement,
                };

                // Validate form of current tab PO MANAGEMENT
                if (!this.mainForm.valid) {
                    this.validateAllFields(false);
                    return;
                }
                break;
            case this.TAB_TYPE.CARGO_LOADABILITY:
                this.elements = {
                    'cyEarlyBookingTimeless': this.cyEarlyBookingTimelessElement,
                    'cfsEarlyBookingTimeless': this.cfsEarlyBookingTimelessElement,
                    'airEarlyBookingTimeless': this.airEarlyBookingTimelessElement,
                };
                break;
            default:
                this.elements = {};
                break;
        }

        if (!this.checkValidateSave_Tab1_2()) {
            this.notification.showErrorPopup('validation.mandatoryBuyerCompliance', 'label.' + this.modelName);
            this.validateAllFields(false);
            this.focusFirstErrorElement();
            return;
        }

        if (!this.checkValidateSave_Tab3()) {
            this.notification.showErrorPopup('validation.mandatoryFieldsValidation', 'label.' + this.modelName);
            this.validateAllFields(false);
            this.focusFirstErrorElement();
            return;
        }

        // Validate tab 6: Booking Validation Policy
        const formName = [
            'bypassEmailDomain'
        ];
        const isValid = !Object.keys(this.formErrors)
                        .filter(x => formName.includes(x))
                        .some(x => !StringHelper.isNullOrWhiteSpace(this.formErrors[x]));
        if (!isValid) {
            this.notification.showErrorPopup('validation.mandatoryFieldsValidation', 'label.' + this.modelName);
            this.validateAllFields(false);
            this.focusFirstErrorElement();
            return;
        }

        if (this.model?.serviceType === BuyerComplianceServiceType.WareHouse && !this.checkValidateSave_Tab7()) {
            this.notification.showErrorPopup('validation.mandatoryFieldsValidation', 'label.emailSetting');
            this.validateAllFields(false);
            this.focusFirstErrorElement();
            return;
        }

        this.saveHSCodeSetting();
        if (this.isAddMode) {
            this.service.create(this.model).subscribe(data => {
                    this.notification.showSuccessPopup('save.sucessNotification', 'label.compliances');
                    this.backList();
                },
                error => {
                    this.notification.showErrorPopup('save.failureNotification', 'label.compliances');
                });
        }
        if (this.isEditMode) {
            this.service.update(this.modelId, this.model).subscribe(data => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.compliances');
                this.router.navigate([`/compliances/view/${this.model.id}`]);
                this.tabSelected = this.TAB_TYPE.PO_MANAGEMENT;
                this.ngOnInit();
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.compliances');
            });
        }
    }

    /**
     * To activate buyer compliance
     * @returns
     */
    activateCompliance () {
        let errorMessage = null;

        const invalidTab = this.checkValidateToActivateBuyerCompliance();
        if (invalidTab.length > 0) {
            let failedSections = '';
            for (let i = 0; i < invalidTab.length; i++) {
                switch (invalidTab[i]) {
                    case this.TAB_TYPE.PO_MANAGEMENT:
                        failedSections += this.translateService.instant('label.poManagement');
                        break;
                    case this.TAB_TYPE.CARGO_LOADABILITY:
                        failedSections += this.translateService.instant('label.cargoLoadability');
                        break;
                    case this.TAB_TYPE.AGENT_ASSIGNMENT:
                        failedSections += this.translateService.instant('label.agentAssignment');
                        break;
                    case this.TAB_TYPE.BOOKING_POLICY:
                        failedSections += this.translateService.instant('label.bookingValidationPolicy');
                        break;
                }
                if (i < invalidTab.length - 1) {
                    failedSections += ', ';
                }
            }
            errorMessage = this.translateService.instant('validation.activateComplianceFailed',
                {
                    failedSection: failedSections
                });
            this.notification.showErrorPopup(errorMessage, 'label.' + this.modelName);
            this.validateAllFields(false);
            this.focusFirstErrorElement();
            return;
        }

        this.model.stage = BuyerComplianceStage.Activated;

        this.service.activate(this.model).subscribe(data => {
            const org = {
                id: this.model.organizationId,
                isBuyer: true
            };
            this.service.updateOrganizationBuyer(org).subscribe(success => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.compliances');
                this.model.stage = BuyerComplianceStage.Activated;
                this.model.stageName = 'label.activated';
            }, error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.compliances');
                this.model.stage = BuyerComplianceStage.Draft;
            });
        },
        error => {
            this.notification.showErrorPopup('save.failureNotification', 'label.compliances');
            this.model.stage = BuyerComplianceStage.Draft;
        });
    }

    cancelBuyerCompliance () {
        const confirmDlg = this.notification.showConfirmationDialog('edit.cancelConfirmation', 'label.compliances');

        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    if (this.isAddMode) {
                        this.backList();
                    } else {
                        if (this.isEditMode) {
                            this.router.navigate([`/compliances/view/${this.model.id}`]);
                            this.ngOnInit();
                            this.clearFormErrors(this.tabPrefix.emailSetting);
                            this.tabSelected = this.TAB_TYPE.PO_MANAGEMENT;
                        }
                    }
                }
            });
    }

    deactivateCompliance() {
        const confirmDlg = this.notification.showConfirmationDialog('msg.deactivateComplianceConfirmation', 'label.compliances');

        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.model.stage = BuyerComplianceStage.Draft;
                    this.service.update(this.modelId, this.model).subscribe(
                        data => {
                            const org = {
                                id: this.model.organizationId,
                                isBuyer: false
                            };
                            this.service.updateOrganization(org).subscribe(success => {
                                this.notification.showSuccessPopup('save.sucessNotification', 'label.compliances');
                                this.model.stage = BuyerComplianceStage.Draft;
                                this.model.stageName = 'label.draft';
                            }, error => {
                                this.notification.showErrorPopup('save.failureNotification', 'label.compliances');
                                this.model.stage = BuyerComplianceStage.Activated;
                            });
                        },
                        error => {
                            this.notification.showErrorPopup('save.failureNotification', 'label.compliances');
                            this.model.stage = BuyerComplianceStage.Activated;
                        });
                }
            });
    }

    updateEquipmentTypeOptions() {
        const currentEquipmentTypes = this.model.cargoLoadabilities.map(({equipmentType}) => equipmentType);
        this.equipmentTypeOptions = this.allEquipmentTypeOptions.filter(x => !currentEquipmentTypes.includes(x.value));
    }

    editCompliance() {
        this.router.navigate([`/compliances/edit/${this.model.id}`]);
        if (this.tabSelected === this.TAB_TYPE.BOOKING_POLICY) {
            this.invokeDragDropPolicies();
        }
    }

    getGridRuleValidation() {
        return {
            name: {
                required: ''
            },
            equipmentType: {
                required: ''
            },
            cyMinimumCBM: {
                required: '',
                lessThanMaxCBM: ''
            },
            cyMaximumCBM: {
                required: ''
            },
            cfsMinimumCBM: {
                required: '',
                lessThanMaxCBM: ''
            },
            cfsMaximumCBM: {
                required: ''
            }
        };
    }

    getAgentGridValidation() {
        return {
            autoCreateShipment: {
                required: ''
            },
            countryId: {
                required: '',
            },
            agentType: {
                required: '',
            },
            agentOrganizationId: {
                required: '',
                notExists: ''
            }
        };
    }

    onCommoditySelectionFilterChange(value) {
        if (value.length >= 3) {
            this.commodityOptions = DropDowns.Commodity
            .filter(s => this.translateService.instant(s.label).toLowerCase().indexOf(value.toLowerCase()) !== -1);
        } else if (!value) {
            this.commodityOptions = DropDowns.Commodity;
            this.commoditySelections.toggle(true);
        } else {
            this.commoditySelections.toggle(false);
        }
    }

    onHSCodeSettingCountrySelectionFilterChange(text: string, rowIndex: number) {
        if (text.length >= 3) {
            this.hsCodeSettings[rowIndex].countryOptions = this.hsCodeCountryOptions
            .filter(s => s.label.toLowerCase().indexOf(text.toLowerCase()) !== -1);
        } else if (!text) {
            this.hsCodeSettings[rowIndex].countryOptions = this.hsCodeCountryOptions;
            this.hsCodeCountrySettings.toArray()[rowIndex].toggle(true);
        } else {
            this.hsCodeCountrySettings.toArray()[rowIndex].toggle(false);
        }
    }

    onShipFromLocationSelectionFilterChange(value) {
        if (value.length >= 3) {
            this.allLocationOptions = this.mainAllLocationOptions
            .filter(s => this.translateService.instant(s.label).toLowerCase().indexOf(value.toLowerCase()) !== -1);
        } else if (!value) {
            this.allLocationOptions = this.mainAllLocationOptions;
            this.shipFromLocationSelections.toggle(true);
        } else {
            this.shipFromLocationSelections.toggle(false);
        }
    }

    get hiddenBtnBookingLogs() {
        if (!this.currentUser.isInternal) {
            return true;
        }
        return false;
    }


    onShipToLocationSelectionFilterChange(value) {
        if (value.length >= 3) {
            this.allLocationOptions = this.mainAllLocationOptions
            .filter(s => this.translateService.instant(s.label).toLowerCase().indexOf(value.toLowerCase()) !== -1);
        } else if (!value) {
            this.allLocationOptions = this.mainAllLocationOptions;
            this.shipToLocationSelections.toggle(true);
        } else {
            this.shipToLocationSelections.toggle(false);
        }
    }

    onPortSelectionFilterChange(value, rowIndex) {
        const agentPortComps = this.agentPorts.toArray();
        if (value.length >= 3) {
            this.model.agentAssignments[rowIndex].portLocations = this.mainAllLocationOptions
            .filter(s => s.locationiId && s.countryId === this.model.agentAssignments[rowIndex].countryId.toString() &&
                s.label.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        } else if (!value) {
            this.model.agentAssignments[rowIndex].portLocations = this.mainAllLocationOptions
            .filter(s => s.locationiId && s.countryId === this.model.agentAssignments[rowIndex].countryId.toString());
            agentPortComps[rowIndex - 2].toggle(true);
        } else {
            agentPortComps[rowIndex  - 2].toggle(false);
        }
    }

    onCountryChange(value, rowIndex) {
        const countryId = value.toString();
        this.model.agentAssignments[rowIndex].portLocations = this.mainAllLocationOptions
            .filter(s => s.locationiId && s.countryId === countryId);
        this.model.agentAssignments[rowIndex].portSelectionIds = [];
        this.checkAgentCountryValidates(rowIndex);
    }

    onAgentTypeChange(value, rowIndex) {
        if (value === AgentType.Destination) {
            this.model.agentAssignments[rowIndex].autoCreateShipment = null;
            this.formErrors['agentValidationRules'][rowIndex].autoCreateShipment.required = '';
        }
        this.checkAgentAgentTypeValidates(rowIndex);
    }

    onIncotermSelectionFilterChange(value) {
        if (value.length >= 3) {
            this.incotermTypeOptions = DropDowns.IncotermType
            .filter(s => s.label.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        } else if (!value) {
            this.incotermTypeOptions = DropDowns.IncotermType;
            this.incotermSelections.toggle(true);
        } else {
            this.incotermSelections.toggle(false);
        }
    }

    onCarrierSelectionFilterChange(value) {
        if (value.length >= 3) {
            this.allCarrierOptions = this.mainAllCarrierOptions
            .filter(s => s.label.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        } else if (!value) {
            this.allCarrierOptions = this.mainAllCarrierOptions;
            this.carrierSelections.toggle(true);
        } else {
            this.carrierSelections.toggle(false);
        }
    }

    onBookingPolicyActionChanged(value) {
        if (value !== this.validationResult.WarehouseApproval) {
            this.model.bypassEmailDomain = '';
            delete this.formErrors['bypassEmailDomain'];
        }
    }

    showBookingLogs() {
        this.router.navigate(['/compliances/booking-validation-logs'], { queryParams: { parentid: this.model.id } });
    }

    bindingHSCodeSetting() {
        if (this.model != null) {
            this.hsCodeSettings = [];
            if (!StringHelper.isNullOrEmpty(this.model.hsCodeShipFromCountryIds)) {
                this.hsCodeSettings.push({
                    bookingPort: BookingPortType.ShipFrom,
                    countryIds: this.model.hsCodeShipFromCountryIds,
                    countryOptions: this.hsCodeCountryOptions
                });
                this.setHSCodeSettingValidationRules(this.hsCodeSettings.length - 1);
            }

            if (!StringHelper.isNullOrEmpty(this.model.hsCodeShipToCountryIds)) {
                this.hsCodeSettings.push({
                    bookingPort: BookingPortType.ShipTo,
                    countryIds: this.model.hsCodeShipToCountryIds,
                    countryOptions: this.hsCodeCountryOptions
                });
                this.setHSCodeSettingValidationRules(this.hsCodeSettings.length - 1);
            }
            this.bookingPortOptions = DropDowns.BookingPortType;
        }
    }

    addBlankHSCodeSettingRow() {
        this.hsCodeSettings.push({ isAddLine: true, countryIds: [], countryOptions: this.hsCodeCountryOptions });
        const rowIndex = this.hsCodeSettings.length - 1;
        const currentBookingPorts = this.hsCodeSettings.map(({bookingPort}) => bookingPort);
        this.bookingPortOptions = DropDowns.BookingPortType.filter(x => !currentBookingPorts.includes(x.value));
        this.setHSCodeSettingValidationRules(rowIndex);
    }

    removeHSCodeSettingRow(rowIndex) {
        if (this.hsCodeSettings[rowIndex].bookingPort === BookingPortType.ShipFrom) {
            this.model.hsCodeShipFromCountryIds = [];
        }

        if (this.hsCodeSettings[rowIndex].bookingPort === BookingPortType.ShipTo) {
            this.model.hsCodeShipToCountryIds = [];
        }
        this.hsCodeSettings[rowIndex].isRemoved = true;
        this.deleteFormControls(`hsCodeSetting_bookingPort_${rowIndex}`, `hsCodeSetting_country_${rowIndex}`);
    }

    setHSCodeSettingValidationRules(rowIndex) {
        const prefix = 'hsCodeSetting';
        this.validationRules[prefix + '_bookingPort_' + rowIndex] = {
            'required': 'label.bookingPort'
        };
        this.validationRules[prefix + '_country_' + rowIndex] = {
            'required': 'label.country'
        };
    }

    hsCodeSettingsRowCallback(args) {
        // Deleted row will be marked with removed property.
        return { 'hide-row': args.dataItem.isRemoved };
    }

    get currentHSCodeBookingPorts() {
        const currentBookingPorts = this.hsCodeSettings.filter(x => StringHelper.isNullOrEmpty(x.isRemoved) || !x.isRemoved).map(({bookingPort}) => bookingPort);
        return currentBookingPorts;
    }

    onHSCodeBookingPortChange(value, rowIndex) {
        this.bookingPortOptions = DropDowns.BookingPortType.filter(x => !this.currentHSCodeBookingPorts.includes(x.value));
    }

    onHSCodeBookingPortOpen(rowIndex) {
        const addingOption = DropDowns.BookingPortType.filter(x => x.value === this.hsCodeSettings[rowIndex].bookingPort);
        this.bookingPortOptions = DropDowns.BookingPortType.filter(x => !this.currentHSCodeBookingPorts.includes(x.value));
        this.bookingPortOptions.concat(addingOption);
    }


    isCountrySelected(value, rowIndex): boolean {
        return this.hsCodeSettings[rowIndex].countryIds.some(item => item === value);
    }

    saveHSCodeSetting() {
        this.model.hsCodeShipFromCountryIds = [];
        this.model.hsCodeShipToCountryIds = [];

        this.hsCodeSettings.filter(x => StringHelper.isNullOrEmpty(x.isRemoved) || !x.isRemoved).forEach(item => {
            if (item.bookingPort === 1) {
                this.model.hsCodeShipFromCountryIds = item.countryIds;
            }

            if (item.bookingPort === 2) {
                this.model.hsCodeShipToCountryIds = item.countryIds;
            }
        });
    }

    serviceTypeValueChanged(value: number): void {
        if (value === BuyerComplianceServiceType.WareHouse) {
            this.bindingEmailSetting();
        }
        else {
            this.clearFormErrors(this.tabPrefix.emailSetting);
            this.model.emailSettings = this.backupModel.emailSettings;
        }

        if (value === BuyerComplianceServiceType.Freight) {
            this.validationResultOptions = DropDowns.ValidationResult.filter(x => x.value !== this.validationResult.WarehouseApproval);
            this.model.integrateWithWMS = false;
        } else {
            this.validationResultOptions = DropDowns.ValidationResult.filter(x => x.value !== this.validationResult.BookingRejected);
        }

        const isExisting = this.validationResultOptions.some(x => x.value === this.model?.bookingPolicyAction);
        if (this.model && !isExisting) {
            this.model.bookingPolicyAction = 10;
            this.model.bookingApproverSetting = 10;
            this.model.bookingApproverUser = '';
        }
    }
}
