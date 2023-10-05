import { Component, ViewChild, OnDestroy, TemplateRef, ElementRef, HostListener } from "@angular/core";
import { Router, ActivatedRoute } from "@angular/router";
import {
    FormComponent,
    UserContextService,
    POFulfillmentStageType,
    POFulfillmentStatus,
    StringHelper,
    ValidationResultPolicy,
    POFulfillmentOrderStatus,
    BuyerApprovalStage} from "src/app/core";
import { NotificationPopup } from "src/app/ui/notification-popup/notification-popup";
import { TranslateService } from "@ngx-translate/core";
import {
    faPlus,
    faEllipsisV,
    faPencilAlt,
    faTrashAlt,
    faCheck,
    faBan,
    faShare,
    faCloudUploadAlt,
    faInfoCircle,
    faRedo,
    faCloudDownloadAlt,
    faCaretLeft
} from "@fortawesome/free-solid-svg-icons";
import { MissingPOFulfillmentFormService } from "./missing-po-fulfillment-form.service";
import { AppPermissions } from "src/app/core/auth/auth-constants";
import { concatMap, delay, filter, map } from "rxjs/operators";
import { EMPTY, of, Observable, forkJoin, Subject, Subscription } from "rxjs";
import { MissingPOFulfillmentGeneralInfoComponent } from "../missing-po-fulfillment-general-info/missing-po-fulfillment-general-info.component";
import { RowArgs } from "@progress/kendo-angular-grid";
import { DatePipe } from "@angular/common";
import {
    OrganizationNameRole,
    AgentType,
    EquipmentType,
    PurchaseOrderAdhocChangePriority, POType, ModeOfTransportType, EntityType, AgentAssignmentMode, Roles, VerificationSetting, RoleSequence, AgentAssignmentMethodType, OrganizationType, OrderFulfillmentPolicy, FormModeType, ViewSettingModuleIdType} from "src/app/core/models/enums/enums";
import { SelectPosFormService } from 'src/app/ui/select-pos-form/select-pos-form.service';
import { MathHelper } from 'src/app/core/helpers/math.helper';
import { MissingPOFulfillmentLoadInfoComponent } from '../missing-po-fulfillment-load-info/missing-po-fulfillment-load-info.component';
import { MissingPOFulfillmentContactComponent } from '../missing-po-fulfillment-contact/missing-po-fulfillment-contact.component';
import { IntegrationData } from 'src/app/core/models/forms/integration-data';
import * as appConstants from 'src/app/core/models/constants/app-constants';
import * as cloneDeep from 'lodash/cloneDeep';
import { AttachmentUploadPopupService } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.service';
import { DialogService } from '@progress/kendo-angular-dialog';
import { POAdhocChangedData } from 'src/app/core/models/forms/po-adhoc-change-data';
import { POFulfillmentNoteModel } from '../models/po-fulfillment-note.model';
import { CarrierModel } from 'src/app/core/models/carrier.model';
import { AttachmentKeyPair } from "src/app/core/models/attachment.model";
import { AttachmentFormModeType, AttachmentUploadPopupComponent } from "src/app/ui/attachment-upload-popup/attachment-upload-popup.component";
import { POFulfillmentAttachmentModel, POFulfillmentAttachmentState } from "../models/po-fulfillment-attachment.model";
import { OrganizationReferenceDataModel, UserOrganizationProfileModel } from "src/app/core/models/organization.model";
import { ShipmentTrackingService } from "../../shipment/shipment-tracking/shipment-tracking.service";
import { GoogleAnalyticsService } from "src/app/core/services/google-analytics.service";
import { MissingPOFulfillmentCustomerComponent } from "../missing-po-fulfillment-customer/missing-po-fulfillment-customer.component";
import { POFulfillmentTabModel } from "../../po-fulfillment/po-fulfillment-models/po-fulfullment-tab.model";
import { FormHelper } from "src/app/core/helpers/form.helper";

@Component({
    selector: "app-missing-po-fulfillment-form",
    templateUrl: "./missing-po-fulfillment-form.component.html",
    styleUrls: ["./missing-po-fulfillment-form.component.scss"]
})
export class MissingPOFulfillmentFormComponent extends FormComponent implements OnDestroy {
    modelName = "pofullfillments";

    /**To interact between components by emitting or subscribing events. */
    integration$: Subject<IntegrationData> = new Subject();

    // Store all subscriptions, then should un-subscribe at the end
    private _subscriptions: Array<Subscription> = [];

    /**Owner's Organization info of the booking */
    createdByOrganization: UserOrganizationProfileModel;

    /** Selected attachments by checkboxes */
    selectedAttachments = [];

    /** Data for attachment upload popup */
    attachmentModel: POFulfillmentAttachmentModel = null;

    //Icon definitions   
    faPlus = faPlus;
    faEllipsisV = faEllipsisV;
    faPencilAlt = faPencilAlt;
    faTrashAlt = faTrashAlt;
    faCheck = faCheck;
    faBan = faBan;
    faRedo = faRedo;
    faCloudDownloadAlt = faCloudDownloadAlt;
    faCaretLeft = faCaretLeft;
    faShare = faShare;
    faCloudUploadAlt = faCloudUploadAlt;
    faInfoCircle = faInfoCircle;

    //Constant/enum definitions
    stringHelper = StringHelper;
    readonly AppPermissions = AppPermissions;
    verificationSetting = VerificationSetting;
    modeOfTransportType = ModeOfTransportType;
    equipmentTypes = EquipmentType;
    poFulfillmentStageType = POFulfillmentStageType;
    poFulfillmentStatus = POFulfillmentStatus;
    purchaseOrderAdhocChangePriority = PurchaseOrderAdhocChangePriority;
    private readonly DocumentLevel = appConstants.DocumentLevel;

    firstPOContacts: any[] =[];
    
    isCanClickVesselFlight: boolean;
    shipmentItineraries: any[]=[];
    
    currentUser: any;
    poFulfillmentGeneral: any;
    bookingPOAdhocChange: POAdhocChangedData;

    stages = [
        {
            stage: POFulfillmentStageType.Draft,
            title: "label.draft",
            class: "n-draft",
            active: false,
            current: false
        },
        {
            stage: POFulfillmentStageType.ForwarderBookingRequest,
            title: "label.forwarderBookingRequest",
            class: "n-forwarderBookingRequest",
            active: false,
            current: false
        },
        {
            stage: POFulfillmentStageType.ForwarderBookingConfirmed,
            title: "label.forwarderBookingConfirmed",
            class: "n-forwarderBookingConfirmed",
            active: false,
            current: false
        },
        {
            stage: POFulfillmentStageType.ShipmentDispatch,
            title: "label.shipmentDispatch",
            class: "n-shipmentDispatch",
            active: false,
            current: false
        },
        {
            stage: POFulfillmentStageType.Closed,
            title: "label.closed",
            class: "n-closed",
            active: false,
            current: false
        }
    ];

    tabs : Array<POFulfillmentTabModel> = [
        {
            text: 'label.general',
            sectionId: 'general',
            hidden: false,
            selected: false,
            readonly: false
        },
        {
            text: 'label.contact',
            sectionId: 'contact',
            hidden: false,
            selected: false,
            readonly: false
        },
        {
            text: 'label.customerPO',
            sectionId: 'customerPO',
            hidden: false,
            selected: false,
            readonly: false
        },
        {
            text: 'label.load',
            sectionId: 'load',
            hidden: false,
            selected: false,
            readonly: false
        },
        {
            text: 'label.activity',
            sectionId: 'activity',
            hidden: false,
            selected: false,
            readonly: true
        },
        {
            text: 'label.attachment',
            sectionId: 'attachment',
            hidden: false,
            selected: false,
            readonly: false
        },
        {
            text: 'label.dialog',
            sectionId: 'dialog',
            hidden: false,
            selected: false,
            readonly: false
        }
    ];
    
    attachmentUploadPopupOpened = false;

    attachmentFormMode: AttachmentFormModeType = AttachmentFormModeType.add;

    @ViewChild(AttachmentUploadPopupComponent, { static: false })
    attachmentPopupComponent: AttachmentUploadPopupComponent;

    @ViewChild(MissingPOFulfillmentGeneralInfoComponent, { static: false })
    poFulfillmentGeneralInfoControl: MissingPOFulfillmentGeneralInfoComponent;

    @ViewChild(MissingPOFulfillmentLoadInfoComponent, { static: false })
    poFulfillmentLoadInfoComponent: MissingPOFulfillmentLoadInfoComponent;

    @ViewChild(MissingPOFulfillmentCustomerComponent, { static: false })
    poFulfillmentCustomerInfoComponent: MissingPOFulfillmentCustomerComponent;

    @ViewChild(MissingPOFulfillmentContactComponent, { static: false })
    poFulfillmentContactComponent: MissingPOFulfillmentContactComponent;

    @ViewChild('headerBar', { static: false })
    headerBarElement: ElementRef;

    @ViewChild('stickyBar', { static: false })
    stickyBarElement: ElementRef;

    @ViewChild('sectionContainer', { static: false })
    sectionContainerElement: ElementRef;

    @ViewChild('general', { static: false })
    generalElement: ElementRef;

    @ViewChild('contact', { static: false })
    contactElement: ElementRef;

    @ViewChild('customerPO', { static: false })
    customerPOElement: ElementRef;

    @ViewChild('load', { static: false })
    loadElement: ElementRef;

    @ViewChild('activity', { static: false })
    activityElement: ElementRef;

    @ViewChild('attachment', { static: false })
    attachmentElement: ElementRef;
    
    @ViewChild('dialog', { static: false })
    dialogElement: ElementRef;

    @ViewChild('confirmBookingPendingForApprovalTemplate', {static: false})
    public confirmBookingPendingForApprovalTemplate: TemplateRef<any>;

    @ViewChild('confirmBookingRejectedTemplate', {static: false})
    public confirmBookingRejectedTemplate: TemplateRef<any>;

    ImportStepState = {
        Selecting: 0,
        Selected: 1
    };

    // initial data for add mode
    model: any = {
        id: 0,
        stage: POFulfillmentStageType.Draft,
        status: null,
        createdBy: null,
        // tab 1
        owner: "",
        cargoReadyDate: "",
        incoterm: null,
        isPartialShipment: false,
        bookedBy: null,
        modeOfTransport: null,
        preferredCarrier: null,
        logisticsService: null,
        movementType: null,
        shipFrom: null,
        shipTo: null,
        shipFromName: null,
        shipToName: null,
        receiptPort: "",
        deliveryPort: "",
        expectedShipDate: "",
        expectedDeliveryDate: "",
        remarks: null,
        customerPrefix: null,
        customerList: null,
        currentOrganization: null,
        isForwarderBookingItineraryReady: false,
        isFulfilledFromPO: null,
        orderFulfillmentPolicy: OrderFulfillmentPolicy.AllowMissingPO,
        isContainDangerousGoods: false,
        isShipperPickup: false,
        isNotifyPartyAsConsignee: false,
        isCIQOrFumigation: false,
        isBatteryOrChemical: false,
        isExportLicence: false,
        // tab 2
        contacts: [],
        // tab 3
        orders: [],
        // tab 4
        loads: [],
        selectedCustomer: null,
        buyerCompliance: null,
        customerPOList: null,
        cargoDetails: [],
        itineraries: [],

        attachments: [],
        agentAssignmentMode : AgentAssignmentMode.Default
    };

    allLocationOptions = [];
    
    poCurrentCarrier: CarrierModel = null;
    posObservable = null;
    activityCount: number;
    
    hasCargoDetail: boolean;
    hasLoadDetail: boolean;

    // form validation
    validationRules = {
        customerId: {
            required: "label.customer",
            invalid: "label.customer"
        }
    };
    cancelPOFulfillmentDialog = false;
    isReadyForSubmit: boolean = true;
    isReadyForBook: boolean = true;
    isReadyForCancelBooking: boolean = true;
    isRefreshPOClicked: boolean = false;
    /**Re-load means plan to ship again. */
    isReloadMode: boolean = false;
    private isManualScroll: boolean = true;
    cancelReason = "";

    bookingValidationErrors = {
        /**To contain validation message for general section.
         * Pls use getErrorMessage('general) to get error message with full top at the end.*/
        general: null,
        /**To contain validation message for contact section.
         * Pls use getErrorMessage('contact) to get error message with full top at the end.*/
        contact: null,
        /**To contain validation message for customer po section.
         * Pls use getErrorMessage('customerPO) to get error message with full top at the end.*/
        customerPO: null,
        /**To contain validation message for load section.
         * Pls use getErrorMessage('load) to get error message with full top at the end.*/
        load: null,
        /**To contain validation message for activity section.
         * Pls use getErrorMessage('activity) to get error message with full top at the end.*/
        activity: null,
        /**To contain validation message for attachment.
         * Pls use getErrorMessage('attachment) to get error message with full top at the end.*/
        attachment: null,
        /**To contain validation message for dialog section.
         * Pls use getErrorMessage('dialog) to get error message with full top at the end.*/
        dialog: null,

        /** To auto add a full stop at the end of text */
        getErrorMessage(tabName): string {
            const errorMessage = this[tabName];
            if (!StringHelper.isNullOrEmpty(errorMessage)) {
                if (errorMessage.charAt(errorMessage.length - 1) !== '.') {
                    return errorMessage + '.';
                }
                return errorMessage;
            }
            return errorMessage;
        }
    };
    policyCheckResults: any = null;

    /**Data from 2 sources:
     * 1. Add mode: PO.pOType
     * 2. Edit mode: POFF.fulfilledFromPOType
     * */
    poType: POType = POType.Bulk;

    // Data for Dialog Tab
    noteList: POFulfillmentNoteModel[];

    // Control saving message popup
    saveBookingFailed = false;
    saveBookingErrors: Array<string> = [];

    //15-09-2023 getIsEditPoVal added to get value for bulk edit is click r not
    isBulkEditClick:any;

    constructor(
        protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: MissingPOFulfillmentFormService,
        public translateService: TranslateService,
        private _userContext: UserContextService,
        private datePipe: DatePipe,
        private _selectPOsService: SelectPosFormService,
        private _attachmentService: AttachmentUploadPopupService,
        private shipmentTrackingService: ShipmentTrackingService,
        private _dialogService: DialogService,
        private _gaService: GoogleAnalyticsService
    ) {
        super(route, service, notification, translateService, router);
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
                if (!user.isInternal) {
                    this.service.affiliateCodes = user.affiliates;
                }
                this.isCanClickVesselFlight = this.currentUser?.permissions?.some(c => c.name === AppPermissions.FreightScheduler_List);
            }
        });

        this.getAllLocationOptions().subscribe();

        //Register event handlers
        this._registerInputValidationAutoCompletedHandler();
        this._registerOnBuyerComplianceDataLoadedHandler();
        this._registerShipFromPortChangedHandler();
        this._registerShipToPortChangedHandler();
        this._registerPOFFOrderChangedHandler();
    }

    _registerInputValidationAutoCompletedHandler() {
        /**
         * To handle events for dynamic input validation
         */
        const sub = this.integration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-contact]inputValidationAutoCompleted'
            )).subscribe((eventContent: IntegrationData) => {

                const fieldName = eventContent.content['field'];
                const errorName = eventContent.content['error'];
                const validationType = eventContent.content['type'];

                if (validationType === appConstants.ValidationResultType.valid) {
                    this.setValidControl(fieldName);
                } else if (validationType === appConstants.ValidationResultType.invalid) {
                    this.setInvalidControl(fieldName, errorName);
                }
            });
        this._subscriptions.push(sub);
    }

    _registerShipFromPortChangedHandler() {
        const sub = this.integration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-customer]shipFromPortValueChanged' ||
                eventContent.name === '[po-fulfillment-general-info]shipFromPortValueChanged'
            )).subscribe(() => {
                // remove existing Origin-Agent
                const existingOriginAgentIndex = this.model.contacts
                    .findIndex(c => c.organizationRole === OrganizationNameRole.OriginAgent && (StringHelper.isNullOrEmpty(c.removed) || !c.removed));
                if (existingOriginAgentIndex >= 0) {
                    this.poFulfillmentContactComponent.onDeleteContact(existingOriginAgentIndex);
                }
                this.updateAgentAssignment(
                    OrganizationNameRole.OriginAgent
                );
            });
        this._subscriptions.push(sub);
    }

    _registerShipToPortChangedHandler() {
        const sub = this.integration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-customer]shipToPortValueChanged' ||
                eventContent.name === '[po-fulfillment-general-info]shipToPortValueChanged'
            )).subscribe(() => {
                // remove existing Destination-Agent
                const existingDestinationAgentIndex = this.model.contacts
                    .findIndex(c => c.organizationRole === OrganizationNameRole.DestinationAgent && (StringHelper.isNullOrEmpty(c.removed) || !c.removed));
                if (existingDestinationAgentIndex >= 0) {
                    this.poFulfillmentContactComponent.onDeleteContact(existingDestinationAgentIndex);
                }
                this.updateAgentAssignment(
                    OrganizationNameRole.DestinationAgent
                );
            });
        this._subscriptions.push(sub);
    }

    _registerOnBuyerComplianceDataLoadedHandler() {
        const sub = this.integration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-general-info]buyerComplianceDataLoaded'
            ),
            concatMap(() => this.getAllLocationOptions())
            ).subscribe(() => {
                if (this.isAddMode) {
                    this.generateAgentAssignment();
                }
            });
        this._subscriptions.push(sub);
    }

    _registerPOFFOrderChangedHandler() {
        let sub = this.integration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-customer]poffOrderChanged'
            )).subscribe((eventContent: IntegrationData) => {
                const { expectedShipDateVerification, expectedDeliveryDateVerification } = this.model?.buyerCompliance?.purchaseOrderVerificationSetting;

                // Do not reset expectedShipDate when compliance setting is AllowOverride or ManualInput.
                const isResetExpectedShipDate = ![
                    this.verificationSetting.AsPerPOAllowOverride,
                    this.verificationSetting.ManualInput
                ].includes(expectedShipDateVerification);

                if (isResetExpectedShipDate) {
                    this.model.expectedShipDate = this.getMinExpectedShipDate(this.model.orders);
                }

                // Do not reset expectedDeliveryDate when compliance setting is AllowOverride or ManualInput.
                const isResetExpectedDeliveryDate = ![
                    this.verificationSetting.AsPerPOAllowOverride,
                    this.verificationSetting.ManualInput
                ].includes(expectedDeliveryDateVerification);

                if (isResetExpectedDeliveryDate) {
                    this.model.expectedDeliveryDate = this.getMinExpectedDeliveryDate(this.model.orders);
                }
            });
        this._subscriptions.push(sub);

        sub = this.integration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-customer]add-first-customer-po'
            )).subscribe((eventContent: IntegrationData) => {
                this.firstPOContacts = eventContent.content;
            });
        this._subscriptions.push(sub);
    }

    _registerModeOfTransportChangedHandler() {
        const sub = this.integration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-general-info]modeOfTransportValueChanged'
            )
            ).subscribe(() => {
                this.setTabDisplay('load', !StringHelper.caseIgnoredCompare(this.model?.modeOfTransport, ModeOfTransportType.Air));
            });
        this._subscriptions.push(sub);
    }

    public selectAttachment(context: RowArgs): string {
        return context.dataItem;
    }

    onInitDataLoaded(data): void {
        this.formErrors = {};
        if (this.model != null) {
            this.service.getOwnerOrgInfo(this.model.createdBy).subscribe(
                response => this.createdByOrganization = response
            )

            // Emit event for contact tab when contacts model changed, apply for View/Edit mode booking
            if (!this.isAddMode) {
                // Edit mode, fire event to Ppo-fulfillment-contact.component.ts for handle mode Default or Change Agent
                // Adding some delay on timeout as it needs to execute after another processing
                setTimeout(() => {
                    const emitValue: IntegrationData = {
                        name: '[po-fulfillment-general-info]initializedContacts'
                    };
                    this.integration$.next(emitValue);
                }, 1);
            }

            // It is on edit mode
            /**If PO is revised
             * 1. Show alert on the top of the page
             * 2. Check if whether the user has clicked on the Refresh PO button to refresh booking orders */
            if (this.model.purchaseOrderAdhocChanges && this.model.purchaseOrderAdhocChanges.length > 0) {
                const poAdHocChanges = this.model.purchaseOrderAdhocChanges.sort(
                    (a, b) =>
                        (a.priority > b.priority) ? 1 : ((b.priority > a.priority) ? -1 : 0));
                this.bookingPOAdhocChange = new POAdhocChangedData(
                    poAdHocChanges[0].priority,
                    this.getPOAdhocChangedMessage(poAdHocChanges[0].priority),
                    poAdHocChanges.filter(x => x.priority === poAdHocChanges[0].priority).map(x => x.purchaseOrderId)
                );

                this.isRefreshPOClicked = this.queryParams
                    && this.queryParams['refresh-po']
                    && this.queryParams['refresh-po'] == 'true'
                    && this.isEditMode;

                if (this.isRefreshPOClicked) {
                    setTimeout(() => {
                        const emitValue: IntegrationData = {
                            name: '[po-fulfillment-form]customerPOsRefreshed',
                            content: {
                                'purchaseOrderIds' : this.bookingPOAdhocChange.purchaseOrderIds
                            }
                        };
                        this.integration$.next(emitValue);
                    }, 100); // wait for some delay on UI rendering
                    
                }
            }

            if (this.model.stage >= POFulfillmentStageType.Draft) {
                for (const item of this.stages) {
                    item.active = false;
                    item.current = false;
                }
                for (const item of this.stages) {
                    item.active = true;
                    if (this.model.stage === item.stage) {
                        item.current = true;
                        break;
                    }
                }
            }

            this.bindingDataByPO();

            // binding edit mode
            let index = 1;
            this.model.attachments.forEach(element => {
                element.index = index++;
            });

            const emitValue: IntegrationData = {
                name: '[po-fulfillment-form]onInitDataLoaded',
                content: {
                    'bookingModel': this.model
                }
            };
            
            this.poType = this.model.fulfilledFromPOType;
            this._emitPOTypeChanged(this.poType);
        }
        this.bindingNoteTab();
        this.getShipmentItineraries();

        //set tab display

        this.setTabDisplay('load', !StringHelper.caseIgnoredCompare(this.model?.modeOfTransport, ModeOfTransportType.Air));

        this.service.getTotalActivity(this.modelId).subscribe(
            res => {
                this.activityCount = res
                this.setTabDisplay('activity', this.isShowActivityTab);
            }
        );

        setTimeout(() => {
            this.initTabLink();
        }, 500); // make sure UI has been rendered
    }

    bindingDataByCustomer() {
        this.route.queryParams
        .pipe(
            concatMap(routeParams => {
                const selectedcustomer = routeParams['selectedcustomer'];

                if (selectedcustomer && this.isAddMode) {
                    this.model.isFulfilledFromPO = true;
                    let getCustomerOrg$ = this.service.getOrganizationsByIds([selectedcustomer]);

                    return getCustomerOrg$;
                }

                return of('empty').pipe(delay(1));
            }),
            concatMap(value => {
                this.poFulfillmentGeneral = this.poFulfillmentGeneralInfoControl;
                if (value !== 'empty') {
                    return of(value);
                }
                return EMPTY;
            })
        )
        .subscribe((organization: any) => {
            if (organization[0]) {
                this._bindDataToBookingFromSelectedCustomer(organization[0]);
            }
        });
    }

    bindingDataByPO() {
        this.route.queryParams
            .pipe(
                concatMap(routeParams => {
                    const selectedpos = routeParams['selectedpos'];
                    if (selectedpos) {
                        // Adding new booking from selected Purchase orders
                        const ids = selectedpos.split(',');
                        this.model.isFulfilledFromPO = true;
                        const getPurchaseOrders$ = [];
                        ids.filter(x => +x)
                            .map((x: number) => {
                                if (x) {
                                    getPurchaseOrders$.push(this.service.getPurchaseOrderReplacedByOrgPreferences(x));
                                }
                            });

                        return forkJoin(getPurchaseOrders$);

                    } else if (this.isAddMode) {
                        // Adding new booking directly
                        this.model.isFulfilledFromPO = false;
                    }

                    return of('empty').pipe(delay(1));
                }),
                concatMap(value => {
                    this.poFulfillmentGeneral = this.poFulfillmentGeneralInfoControl;
                    if (value !== 'empty') {
                        return of(value);
                    }

                    this.bindingDataByCustomer();
                    return EMPTY;
                })
            )
            .subscribe((purchaseOrders: Array<any>) => {
                if (purchaseOrders && purchaseOrders.length >= 1) {
                    this._selectPOsService.validatePOsAgainstBuyerCompliance$(purchaseOrders).subscribe((isValid) => {
                        if (isValid) {
                            this._bindDataToBookingFromSelectedPOs(purchaseOrders);
                        } else {
                            this.notification.showErrorPopup(
                                "validation.mandatoryFieldsValidation",
                                "label.poFulfillment"
                            );
                        }
                    });
                }
            });
    }

    private _bindDataToBookingFromSelectedCustomer(customerOrganization: OrganizationReferenceDataModel): void {
        if (customerOrganization.organizationType !== OrganizationType.Principal) {
            return;
        }
        // bind contacts
        this.model.contacts.push({
            organizationId: customerOrganization.id,
            organizationRole: OrganizationNameRole.Principal,
            organizationCode: customerOrganization.code,
            companyName: customerOrganization.name,
            contactName: customerOrganization.contactName,
            contactNumber: customerOrganization.contactNumber,
            contactEmail: customerOrganization.contactEmail,
            contactSequence: this.orgRoleSequenceMapping(OrganizationNameRole.Principal),
            address: this.service.concatenateAddressLines(customerOrganization.address, customerOrganization.addressLine2, customerOrganization.addressLine3, customerOrganization.addressLine4)
        });
        
        // default fulfillment type = Bulk
        this.poType = POType.Bulk;
        if (this.model.isFulfilledFromPO) {
            this.model.fulfilledFromPOType = POType.Bulk;
        }
        this._emitPOTypeChanged(this.poType);

        // turn on load data on tab 1
        this.service.resetCustomerPOs();
        this.poFulfillmentGeneral.ngOnInit();

        const customerOrgId = this.model.contacts.find(x => x.organizationRole === OrganizationNameRole.Principal).organizationId;

        // Load Customer PO list for selected purchase orders
        this.service.getCustomerPOs([0], customerOrgId);
    }

    /**Binding data from selected POs into booking */
    private async _bindDataToBookingFromSelectedPOs(purchaseOrders: Array<any>) {
        const firstPO = purchaseOrders[0];

        // Set some properties for current booking
        this.poType = firstPO.poType;
        if (this.model.isFulfilledFromPO) {
            this.model.fulfilledFromPOType = firstPO.poType;
        }
        this._emitPOTypeChanged(this.poType);

        // General tab
        this.model.shipFrom = firstPO.shipFromId;
        this.model.shipTo = firstPO.shipToId;
        this.model.modeOfTransport = firstPO.modeOfTransport;
        this.setTabDisplay('load', !StringHelper.caseIgnoredCompare(this.model?.modeOfTransport, ModeOfTransportType.Air));
        this.poFulfillmentGeneralInfoControl.modeOfTransportChange(this.model.modeOfTransport);
        this.model.incoterm = firstPO.incoterm;

        this.bindingLocations();

        this.model.expectedShipDate = this.getMinExpectedShipDate(purchaseOrders);
        this.model.expectedDeliveryDate = this.getMinExpectedDeliveryDate(purchaseOrders);
        this.model.owner = this.currentUser.companyName;

        this.model.carrierName = firstPO.carrierName;
        this.model.carrierCode = firstPO.carrierCode;

        this.service.getCarrierByCode(firstPO.carrierCode)
        .subscribe(carrier => {
            this.poCurrentCarrier = carrier;
        });

        // Contact tab

        // Get organization information (from master data) for PO's contacts
        // Company name, Contact name, Contact number and Contact email should be from organization information (NOT from PO)
        const newContacts = firstPO.contacts;
        this.firstPOContacts = firstPO.contacts
        const orgIds = newContacts.map(x => x.organizationId);
        const orgs = await this.service.getOrganizationsByIds(orgIds).toPromise();

        // Set default values (contacts) from the first selected PO
        newContacts.forEach(i => {
            let concatenatedAddress = i.addressLine1;
            let organizationContact: any;
            if (orgs.length > 0) {
                organizationContact = orgs.find(x => x.id === i.organizationId);
            }
            // Apply multilines on address
            if (!this.stringHelper.isNullOrEmpty(organizationContact)) {
                concatenatedAddress = this.service.concatenateAddressLines(organizationContact.address, organizationContact.addressLine2, organizationContact.addressLine3, organizationContact.addressLine4);
            }

            this.model.contacts.push({
                organizationId: i.organizationId,
                organizationRole: i.organizationRole,
                organizationCode: i.organizationCode,
                weChatOrWhatsApp: organizationContact?.weChatOrWhatsApp,
                companyName: (this.stringHelper.isNullOrEmpty(organizationContact) || this.stringHelper.isNullOrEmpty(organizationContact.name)) ? i.companyName : organizationContact.name,
                contactName: (this.stringHelper.isNullOrEmpty(organizationContact) || this.stringHelper.isNullOrEmpty(organizationContact.contactName)) ? i.contactName : organizationContact.contactName,
                contactNumber: (this.stringHelper.isNullOrEmpty(organizationContact) || this.stringHelper.isNullOrEmpty(organizationContact.contactNumber)) ? i.contactNumber : organizationContact.contactNumber,
                contactEmail: (this.stringHelper.isNullOrEmpty(organizationContact) || this.stringHelper.isNullOrEmpty(organizationContact.contactEmail)) ? i.contactEmail : organizationContact.contactEmail,
                contactSequence: this.orgRoleSequenceMapping(i.organizationRole),
                address: concatenatedAddress
            });
            this.poFulfillmentContactComponent.sortBySequence();
            this.poFulfillmentContactComponent.validateGrid(this.model.contacts.length - 1);
            this.poFulfillmentContactComponent.resetOrganizationRoleOptions();
        });

        // turn on load data on tab 1
        this.service.resetCustomerPOs();
        this.poFulfillmentGeneral.ngOnInit();

        const customerOrgId = this.model.contacts.find(x => x.organizationRole === OrganizationNameRole.Principal).organizationId;
        const purchaseOrderIds = purchaseOrders.map(x => x.id);

        // Load Customer PO list for selected purchase orders

        this.service.getCustomerPOs(purchaseOrderIds, customerOrgId);

        // Adding customer PO into the grid
        const posObservable =  this.service.currentCustomerPOs$.subscribe(pos => {
            // It works only in adding mode
            if (pos && this.isAddMode === true) {
                purchaseOrders.map( po => {
                    const lineItems = po.lineItems.filter( x => x.balanceUnitQty > 0 );
                    const originCustomerPO = pos.find(o => o.id === po.id);
                    po.containerType = originCustomerPO.containerType;

                    lineItems.forEach(i => {
                        // auto-calculate and populate data based on [article master]
                        const originLineItem = originCustomerPO.lineItems.find(o => o.id === i.id);
                        const bookedPackage = Math.ceil(!originLineItem.outerQuantity || originLineItem.outerQuantity <= 0 ? 0 : i.balanceUnitQty / originLineItem.outerQuantity);
                        const volume = MathHelper.calculateCBM(originLineItem.outerDepth, originLineItem.outerWidth, originLineItem.outerHeight) * bookedPackage;
                        const grossWeight = originLineItem.outerGrossWeight * bookedPackage;
                        const isExisted = (this.model.orders as Array<any>).some(x => x.poLineItemId === i.id);
                        if (!isExisted) {
                            this.model.orders.push({
                                customerPONumber: po.poNumber,
                                productCode: i.productCode,
                                descriptionOfGoods: i.descriptionOfGoods,
                                commodity: i.commodity,
                                countryCodeOfOrigin: i.countryCodeOfOrigin,
                                currencyCode: i.currencyCode,
                                fulfillmentUnitQty: i.balanceUnitQty,
                                orderedUnitQty: i.orderedUnitQty,
                                balanceUnitQty: 0,
                                outerQuantity: originLineItem.outerQuantity,
                                innerQuantity: originLineItem.innerQuantity,
                                outerDepth: originLineItem.outerDepth,
                                outerHeight: originLineItem.outerHeight,
                                outerWidth: originLineItem.outerWidth,
                                outerGrossWeight: originLineItem.outerGrossWeight,
                                bookedPackage: bookedPackage,
                                volume: isNaN(volume) || volume === 0 ? null : MathHelper.roundToThreeDecimals(volume),
                                grossWeight: isNaN(grossWeight) || grossWeight === 0 ? null : MathHelper.roundToTwoDecimals(grossWeight),
                                unitPrice: i.unitPrice,
                                unitUOM: i.unitUOM,
                                packageUOM: i.packageUOM,
                                hsCode: i.hsCode,
                                chineseDescription: i.chineseDescription,
                                shippingMarks: i.shippingMarks,
                                status: POFulfillmentOrderStatus.Active,
                                purchaseOrderId: po.id,
                                poLineItemId: i.id,
                                poContainerType: po.containerType,
                                expectedShipDate: po.expectedShipDate,
                                expectedDeliveryDate: po.expectedDeliveryDate
                            });
                            originLineItem.balanceUnitQty = 0;
                        }
                    });
                });

                // To enforce immutability for orders.
                // Call it every after changed data of this.model.orders.
                // To work with pipe poFulfillmentCustomerOrder
                this.model.orders = Object.assign([], this.model.orders);

                if (this.model.modeOfTransport === ModeOfTransportType.Sea) {
                        const containerTypes = [...new Set(purchaseOrders.map(x => x.containerType)
                            .filter(x => !this.stringHelper.isNullOrEmpty(x)))];
                        if (containerTypes && containerTypes.length === 1) {
                            const emitValue: IntegrationData = {
                                name: '[po-fulfillment-form]containerTypeChanged',
                                content: {
                                    'containerType': this.poType === POType.Blanket ? EquipmentType.LCLShipment :  containerTypes[0]
                                }
                            };
                            this.integration$.next(emitValue);
                        }
                    }
            }
        });
        this._subscriptions.push(posObservable);
    }

    onContactsChanged($event) {
        if ($event.action === 'add') {
            this.poFulfillmentContactComponent?.validateGrid(this.model.contacts.length - 1);
        } else if ($event.action === 'remove') {
            this.poFulfillmentContactComponent?.onDeleteContact($event.rowIndex);
        } else {
            return;
        }
        this.poFulfillmentContactComponent?.resetOrganizationRoleOptions();
    }

    private orgRoleSequenceMapping(role: string): number {
        let result: number = 0;
        switch (role)
        {
            case OrganizationNameRole.Principal:
                result = RoleSequence.Principal;
                break;
            case OrganizationNameRole.Shipper:
                result = RoleSequence.Shipper;
                break;
            case OrganizationNameRole.Consignee:
                result = RoleSequence.Consignee;
                break;
            case OrganizationNameRole.NotifyParty:
                result = RoleSequence.NotifyParty;
                break;
            case OrganizationNameRole.AlsoNotify:
                result = RoleSequence.AlsoNotifyParty;
                break;
            case OrganizationNameRole.Supplier:
                result = RoleSequence.Supplier;
                break;
            case OrganizationNameRole.Delegation:
                result = RoleSequence.Delegation;
                break;
            case OrganizationNameRole.Pickup:
                result = RoleSequence.PickupAddress;
                break;
            case OrganizationNameRole.BillingParty:
                result = RoleSequence.BillingAddress;
                break;
            case OrganizationNameRole.OriginAgent:
                result = RoleSequence.OriginAgent;
                break;
            case OrganizationNameRole.DestinationAgent:
                result = RoleSequence.DestinationAgent;
                break;
            default:
                break;
        }

        return result;
    }

    private getMinExpectedShipDate(purchaseOrders: any[]): any {
        const defaultDate = new Date('2000-01-01T00:00:00Z');
        if (this.isAddMode) {
            const expectedShipDateList = purchaseOrders.filter(po => po.expectedShipDate && po.expectedShipDate > defaultDate)
                .map(po => po.expectedShipDate);
            return expectedShipDateList.length > 0 ? expectedShipDateList.reduce(function (a, b) { return a < b ? a : b; }) : null;
        } else {
            // If not add mode, must get data from purchase order list

            // POFF.Orders
            const purchaseOrderIds = purchaseOrders?.map(x => x.purchaseOrderId);

            // List of purchase orders
            const purchaseOrderList = this.service.currentCustomerPOs();

            const expectedShipDateList = purchaseOrderList.filter(po => purchaseOrderIds.indexOf(po.id) >= 0 && po.expectedShipDate && po.expectedShipDate > defaultDate)
                                        .map(po => po.expectedShipDate);
            return expectedShipDateList.length > 0 ? expectedShipDateList.reduce(function (a, b) { return a < b ? a : b; }) : null;
        }
    }

    private getMinExpectedDeliveryDate(purchaseOrders: any[]): any {
        const defaultDate = new Date('2000-01-01T00:00:00Z');
        if (this.isAddMode) {
            const expectedDeliveryDateList = purchaseOrders.filter(po => po.expectedDeliveryDate && po.expectedDeliveryDate > defaultDate)
                .map(po => po.expectedDeliveryDate);
            return expectedDeliveryDateList.length > 0 ? expectedDeliveryDateList.reduce(function (a, b) { return a < b ? a : b; }) : null;
        } else {
            // If not add mode, must get data from purchase order list

            // POFF.Orders
            const purchaseOrderIds = purchaseOrders?.map(x => x.purchaseOrderId);

            // List of purchase orders
            const purchaseOrderList = this.service.currentCustomerPOs();

            const expectedDeliveryDateList = purchaseOrderList.filter(po => purchaseOrderIds.indexOf(po.id) >= 0 && po.expectedDeliveryDate && po.expectedDeliveryDate > defaultDate)
                                                .map(po => po.expectedDeliveryDate);
            return expectedDeliveryDateList.length > 0 ? expectedDeliveryDateList.reduce(function (a, b) { return a < b ? a : b; }) : null;

        }
    }

    // Please use this method to make sure data is already initialized
    getAllLocationOptions(): Observable<any> {
        // not initialized data
        if (this.allLocationOptions.length === 0) {
            return this.service.getAllLocations().map(locations => {
                this.allLocationOptions = locations;
                return this.allLocationOptions;
            });
        } else {
            // after data initialized
            return of(this.allLocationOptions);
        }
    }

    bindingLocations(): void {
        this.getAllLocationOptions().subscribe(locations => {
            const shipFrom = locations.find(s => s.id === this.model.shipFrom);
            this.model.shipFromName = shipFrom && shipFrom.locationDescription;
            const shipTo = locations.find(s => s.id === this.model.shipTo);
            this.model.shipToName = shipTo && shipTo.locationDescription;
            const receiptPort = locations.find(s => s.id === this.model.receiptPortId);
            this.model.receiptPort = receiptPort && receiptPort.locationDescription;
            const deliveryPort = locations.find(s => s.id === this.model.deliveryPortId);
            this.model.deliveryPort = deliveryPort && deliveryPort.locationDescription;
        });
    }

    backList() {
        this.router.navigate(["/po-fulfillments"]);
    }

    private formatDate(object: any): any {
        try {
            for (const item in object) {
                if (object[item] && object[item] !== null) {
                    if (typeof object[item].getMonth === "function") {
                        object[item] = this.datePipe.transform(
                            object[item],
                            this.DATE_FORMAT
                        );
                    } else if (typeof object[item] === "object") {
                        this.formatDate(object[item]);
                    }
                }
            }

            return object;
        } catch (ex) {
            console.log(ex);
        }
    }

    async updateAgentAssignment(orgRoleName) {
        // Using isPOHasAgentContact to check if PO has no Origin/Destination (PO Assigned Origin And Destination Agent compliance setting = No)
        const isPOHasAgentContact = this.firstPOContacts?.some(c=>c.organizationRole === orgRoleName);

        if (
            this.model.buyerCompliance.agentAssignmentMethod === AgentAssignmentMethodType.byPO && isPOHasAgentContact && this.model.shipFrom > 0
            || this.model.buyerCompliance.agentAssignmentMethod === AgentAssignmentMethodType.byPO && isPOHasAgentContact && this.model.shipTo > 0
            ) {
            const contact = this.firstPOContacts.find(c=>c.organizationRole === orgRoleName);
            await this.setOrgToContact(contact?.organizationId, contact?.organizationRole);
            return;
        }

        if (this.model.buyerCompliance.agentAssignmentMethod === AgentAssignmentMethodType.bySystem
            || (this.model.buyerCompliance.agentAssignmentMethod === AgentAssignmentMethodType.byPO  && !isPOHasAgentContact)

            ) {
            if (
                (orgRoleName === OrganizationNameRole.OriginAgent &&
                    !StringHelper.isNullOrEmpty(this.model.shipFrom) && this.model.shipFrom > 0) ||
                (orgRoleName === OrganizationNameRole.DestinationAgent &&
                    !StringHelper.isNullOrEmpty(this.model.shipTo) && this.model.shipTo > 0)
            ) {
                const shipId_poff =
                    orgRoleName === OrganizationNameRole.OriginAgent
                        ? this.model.shipFrom
                        : this.model.shipTo;

                const shipId_poffString = shipId_poff.toString();

                const agentType =
                    orgRoleName === OrganizationNameRole.OriginAgent
                        ? AgentType.Origin
                        : AgentType.Destination;

                if (!this.model.buyerCompliance) {
                    return;
                }
                    const agentAssignmentsSettings = this.model.buyerCompliance.agentAssignments.filter(c=>c.modeOfTransport === this.model.modeOfTransport)
                    .filter(a => a.agentType === agentType)
                    .sort((a, b) => a.order - b.order);

                // check by ports
                for (let i = 1; i < agentAssignmentsSettings.length; i++) {
                    const item = agentAssignmentsSettings[i];

                    if (!StringHelper.isNullOrEmpty(item.portSelectionIds)) {
                        const ports = item.portSelectionIds;
                        for (let j = 0; j < ports.length; j++) {
                            const port = ports[j];
                            const locationId = port.split("-")[1];
                            if (locationId === shipId_poffString) {

                                await this.setOrgToContact(
                                    item.agentOrganizationId,
                                    orgRoleName
                                );
                                return;
                            }
                        }
                    }
                }

                // check by country
                const res = this.allLocationOptions;
                for (let i = 1; i < agentAssignmentsSettings.length; i++) {
                    const item = agentAssignmentsSettings[i];

                    if (StringHelper.isNullOrEmpty(item.portSelectionIds) ||
                            // Array[0] = "" -> set on country, not on port
                            (item.portSelectionIds.length === 1 && StringHelper.isNullOrEmpty(item.portSelectionIds[0]))
                        ) {
                        if (res.length > 0) {
                            const location = res.find(x => x.id === shipId_poff);
                            if (location.countryId === item.countryId) {
                                await this.setOrgToContact(
                                    item.agentOrganizationId,
                                    orgRoleName
                                );
                                return;
                            }
                        }
                    }
                }

                // set by default
                const defaultAgentAssignmentsSetting =
                    agentAssignmentsSettings.length > 0
                        ? agentAssignmentsSettings[0]
                        : null;
                if (defaultAgentAssignmentsSetting) {
                    await this.setOrgToContact(
                        defaultAgentAssignmentsSetting.agentOrganizationId,
                        orgRoleName
                    );
                    }


                return;
            }
        }

    }

    async setOrgToContact(agentOrganizationId, orgRoleName) {
        const rs = await this.service
            .getOrganizationsByIds([agentOrganizationId])
            .toPromise();

        if (orgRoleName === OrganizationNameRole.OriginAgent) {
            const customerId = this.model.contacts.find(c=>c.organizationRole === OrganizationNameRole.Principal).organizationId;
            var emailNotificationSetup:any = await this.service.getEmailNotification(agentOrganizationId,customerId,this.model.shipFrom).toPromise();
        }

        if (rs.length > 0) {
            const data = rs[0];
            const concatenatedAddress = this.service.concatenateAddressLines(data.address, data.addressLine2, data.addressLine3, data.addressLine4);
            let contact = {
                organizationId: agentOrganizationId,
                organizationRole: orgRoleName,
                organizationCode: data.code,
                companyName: data.name,
                contactName: data.contactName,
                contactNumber: data.contactNumber,
                weChatOrWhatsApp: data.weChatOrWhatsApp,
                contactEmail: data.contactEmail,
                address: concatenatedAddress
            }
            if (emailNotificationSetup) {
                //Get the first email has been separated by comma.
                //This logic is to make sure the email does not exceed the limit length in the booking.
                var firstEmail = emailNotificationSetup.email.split(",")[0];
                contact.contactEmail = firstEmail.trim();
            }
            this.model.contacts.push(contact);
            this.poFulfillmentContactComponent.validateGrid(this.model.contacts.length - 1);
            this.poFulfillmentContactComponent.resetOrganizationRoleOptions();
        }
    }

    async generateAgentAssignment() {
        // remove existing Origin-Agent
        const existingOriginAgentIndex = this.model.contacts
            .findIndex(c => c.organizationRole === OrganizationNameRole.OriginAgent && (StringHelper.isNullOrEmpty(c.removed) || !c.removed));
        if (existingOriginAgentIndex >= 0) {
            this.poFulfillmentContactComponent.onDeleteContact(existingOriginAgentIndex);
        }
        // remove existing Destination-Agent
        const existingDestinationAgentIndex = this.model.contacts
            .findIndex(c => c.organizationRole === OrganizationNameRole.DestinationAgent && (StringHelper.isNullOrEmpty(c.removed) || !c.removed));
        if (existingDestinationAgentIndex >= 0) {
            this.poFulfillmentContactComponent.onDeleteContact(existingDestinationAgentIndex);
        }
        await this.updateAgentAssignment(
            OrganizationNameRole.OriginAgent
        );
        await this.updateAgentAssignment(
            OrganizationNameRole.DestinationAgent
        );
        // Add mode, fire event to Ppo-fulfillment-contact.component.ts for handle mode Default or Change Agent
        const emitValue: IntegrationData = {
            name: '[po-fulfillment-general-info]initializedContacts'
        };
        this.integration$.next(emitValue);

    }

    onChangeAgentMode(value: string) {
        this.model.agentAssignmentMode = value;
    }

    /**
     * Call api to get CustomerPO DataSource by the selected PurchaseOrder Ids,
     * the list will be stored in the service for further uses.
     */
    private getCustomerPOOnSelectedIds(): void {
        const purchaseOrderIds = this.model.orders.map(x => x.purchaseOrderId);
        if (purchaseOrderIds.length > 0) {
            const principalContact = this.model.contacts.find(
                (c) => c.organizationRole === OrganizationNameRole.Principal
            );
            let customerId = 0;
            if (principalContact) {
                customerId = principalContact.organizationId;
            }
            this.service.getCustomerPOs(purchaseOrderIds,
                customerId);
        }
    }

    // fina Save for New Booking
    async onSubmit() {
        this.isReadyForSubmit = false;

        if (!this.currentUser.isInternal) {
            this.model.organizationId = this.currentUser.organizationId ? this.currentUser.organizationId : null
        }

        if (this.model.contacts.length > 0
            && this.model.isNotifyPartyAsConsignee) {
            const emitValue: IntegrationData = {
                name: '[po-fulfillment-general-info]isNotifyPartyAsConsigneeChanged',
                content: {
                    'isNotifyPartyAsConsignee': this.model.isNotifyPartyAsConsignee
                }
            };
            this.integration$.next(emitValue);
        }
        if (this.isAddMode) {

            // validate input validations: required, greaterthan....
            let isValid = this.mainForm.valid;
            if (!isValid) {
               // To show validation messages
                this.validateAllFields(false);
               // Do not return here
            }
            isValid = this.validatePOFFBeforeSaving();
            if (!isValid) {
                this.isReadyForSubmit = true;
                return;
            }

            let tempModel: any = { ...this.model };

            // remove all record with removed marked
            tempModel.contacts = tempModel.contacts.filter((value) => StringHelper.isNullOrEmpty(value.removed) || !value.removed);

            tempModel.loads = tempModel.loads.filter((value) => StringHelper.isNullOrEmpty(value.removed) || !value.removed);

            // for AIR booking, auto create & calculate load item.
            if (this.stringHelper.caseIgnoredCompare(this.model?.modeOfTransport, ModeOfTransportType.Air)) {
                // to skip add default load if there's any missing PO.
                let skipGenerateDefaultLoad = !this.model.orders || this.model.orders.findIndex(x => x.purchaseOrderId === 0) !== -1;
                
                if (!skipGenerateDefaultLoad) {
                    loadItem = {
                        plannedVolume: 0,
                        plannedGrossWeight: 0,
                        plannedNetWeight: 0,
                        plannedPackageQuantity: 0
                    };
                    loadItem.equipmentType = EquipmentType.LCLShipment;
                    loadItem.packageUOM = appConstants.Carton;
                    this.calculateLoadTotal(loadItem);
                    tempModel.loads = [loadItem];
                }
            }

            // remove redundant fields
            if (tempModel.orders) {
                tempModel.orders.forEach(element => {
                    element.selectedDragItem = null;
                });
            }

            // clone submitted model to keep datetime format
            const attachments = cloneDeep(tempModel.attachments);

            tempModel = this.formatDate(tempModel);

            tempModel.attachments = attachments;

            this.resetBookingValidationErrors();
            this.service.create(tempModel).subscribe(
                data => {
                    this._gaService.emitAction('Add', appConstants.GAEventCategory.POBooking);
                    this.notification.showSuccessPopup(
                        "save.sucessNotification",
                        "label.poFulfillment"
                    );

                    //Assign created id
                    this.modelId = data.id;
                    this.model.id = data.id;

                    this.selectedAttachments = [];
                    this.navigate(FormModeType.View);
                    this.getCustomerPOOnSelectedIds();
                    this.isReadyForSubmit = true;
                },
                () => {
                    this.notification.showErrorPopup(
                        "save.failureNotification",
                        "label.poFulfillment"
                    );
                    this.isReadyForSubmit = true;
                }
            );
        } else if (this.isEditMode) {
            let isValid = this.mainForm.valid;
            if (!isValid) {
                // To show validation messages
                this.validateAllFields(false);
                // Do not return here
            }

            isValid = this.validatePOFFBeforeSaving();
            if (!isValid) {
                this.isReadyForSubmit = true;
                return;
            }

            let tempModel: any = { ...this.model };

            // remove all record with removed marked
            tempModel.contacts = tempModel.contacts.filter((value) => StringHelper.isNullOrEmpty(value.removed) || !value.removed);
            // remove all record with removed marked
            tempModel.loads = tempModel.loads.filter((value) => StringHelper.isNullOrEmpty(value.removed) || !value.removed);

            // for AIR booking, auto create & calculate load item.
            if (this.stringHelper.caseIgnoredCompare(this.model?.modeOfTransport, ModeOfTransportType.Air)) {
                // to skip add/update default load if there's any missing PO.
                let skipGenerateDefaultLoad = !this.model.orders || this.model.orders.findIndex(x => x.purchaseOrderId === 0) !== -1;

                if (!skipGenerateDefaultLoad) {
                    var loadItem = tempModel.loads[0]; // just update if existing load record.
                    if (!loadItem) {
                        loadItem = {
                            plannedVolume: 0,
                            plannedGrossWeight: 0,
                            plannedNetWeight: 0,
                            plannedPackageQuantity: 0
                        };
                    }
                    loadItem.equipmentType = EquipmentType.LCLShipment;
                    loadItem.packageUOM = appConstants.Carton;
                    this.calculateLoadTotal(loadItem);
                    tempModel.loads = [loadItem];
                }
            }

            // ignore update attachments coming from other modules
            tempModel.attachments = tempModel.attachments?.filter(
                a => a.documentLevel === this.DocumentLevel.POFulfillment
            );

            // remove redundant fields
            if (tempModel.orders) {
                tempModel.orders.forEach(element => {
                    element.selectedDragItem = null;
                });
            }

            // clone submitted model to keep datetime format
            const attachments = cloneDeep(tempModel.attachments);

            tempModel = this.formatDate(tempModel);

            tempModel.attachments = attachments;

            this.resetBookingValidationErrors();
            tempModel.IsPurchaseOrderRefreshed = this.isRefreshPOClicked;
            this.service.update(this.modelId, tempModel).subscribe(
                data => {
                    this._gaService.emitAction('Edit', appConstants.GAEventCategory.POBooking);

                    this.notification.showSuccessPopup(
                        "save.sucessNotification",
                        "label.poFulfillment"
                    );

                    if (data.isNeedToPlanToShipAgain) {
                        this.notification.showInfoPopup(
                            "msg.needToPlanToShipAgain",
                            "label.poFulfillment"
                        );
                    }
                    this.selectedAttachments = [];
                    this.bookingPOAdhocChange = null;
                    this.navigate(FormModeType.View);
                    this.getCustomerPOOnSelectedIds();
                    this.isReadyForSubmit = true;
                },
                err => {
                    // #region handle business cases based on the error message.
                    // must be integrated with the server
                    if (err.error) {
                        const errMessage = err.error.errors;
                        if (!this.stringHelper.isNullOrEmpty(errMessage)) {
                            const errHashTags = errMessage.split('#').filter(x => !StringHelper.isNullOrEmpty(x));
                            switch (errHashTags[0]) {
                                case 'POAdhocChanged':
                                    this.navigate(FormModeType.View);
                                    const priority = this.purchaseOrderAdhocChangePriority[errHashTags[1]];
                                    this.notification.showErrorPopup(
                                        this.getPOAdhocChangedMessage(priority),
                                        "label.poFulfillment"
                                    );
                                    this.isReadyForSubmit = true;
                                    this.bookingPOAdhocChange = new POAdhocChangedData(
                                        errHashTags[1],
                                        this.getPOAdhocChangedMessage(priority),
                                        errHashTags[3].split(',').map(x => Number.parseInt(x))
                                    );
                                    break;
                                default:
                                    break;
                            }
                            return;
                        }
                    }
                    this.isReadyForSubmit = true;
                    this.notification.showErrorPopup(
                        "save.failureNotification",
                        "label.poFulfillment"
                    );
                }
            );
        }
    }

    /**
     * To validate all tabs, including input + business validations
     *
     * Validated by other of tabs: General, Contact, CustomerPO, Load, Load Details
     *
     *
     * @returns boolean
     */
    private validatePOFFBeforeSaving(): boolean {
        // validate tab General
        if (!this._validateInputsInTab('general')) {
            return false;
        }
        this.bookingValidationErrors.general = null;
        const generalValidationResult = this.poFulfillmentGeneral.validateTab();
        for (let index = 0; index < generalValidationResult.length; index++) {
            if (!generalValidationResult[index].status) {
                if (generalValidationResult[index].message) {
                    this.bookingValidationErrors.general = generalValidationResult[index].message;
                    this.scrollToSection("general");
                }
                return false;
            }
        }

        // validate tab Contact
        if ( this.model.contacts.find(c => c.organizationRole === OrganizationNameRole.Principal) === undefined) {
            this.notification.showErrorPopup('msg.creatingPOFFRequiredCustomerNotification', 'label.poFulfillment');
            return false;
        }

        if (!this._validateInputsInTab('contact')) {
            return false;
        }

        this.bookingValidationErrors.contact = null;
        const contactValidationResult =  this.poFulfillmentContactComponent.validateTab();
        for (let index = 0; index < contactValidationResult.length; index++) {
            if (!contactValidationResult[index].status) {
                if (contactValidationResult[index].message) {
                    this.bookingValidationErrors.contact = contactValidationResult[index].message;
                    this.scrollToSection("contact");
                }
                return false;
            }
        }
        
        // validate tab Customer PO
        if (!this._validateInputsInTab('customerPO')) {
            return false;
        }

        this.bookingValidationErrors.customerPO = null;
        const customerPOValidationResult = this.poFulfillmentCustomerInfoComponent.validateTab();
        for (let index = 0; index < customerPOValidationResult.length; index++) {
            if (!customerPOValidationResult[index].status) {
                if (customerPOValidationResult[index].message) {
                    this.bookingValidationErrors.customerPO = customerPOValidationResult[index].message;
                    this.scrollToSection("customerPO");
                }
                return false;
            }
        }

        // validate tab Load
        // For Air booking, the tab load is hidden as the load record will auto-generate by default when saving the booking -> don't need to validate
        if (!StringHelper.caseIgnoredCompare(this.model.modeOfTransport, ModeOfTransportType.Air))
        {
            if (!this._validateInputsInTab('load')) {
                return false;
            }
    
            this.bookingValidationErrors.load = null;
            if (this.poFulfillmentLoadInfoComponent) {
                const loadValidationResult =  this.poFulfillmentLoadInfoComponent.validateTab();
                for (let index = 0; index < loadValidationResult.length; index++) {
                    if (!loadValidationResult[index].status) {
                        if (loadValidationResult[index].message) {
                            this.bookingValidationErrors.load = loadValidationResult[index].message;
                            this.scrollToSection("load");
                        }
                        return false;
                    }
                }
            }
        }
        

        // In case there is any error but not belonging to any tab
        const errors = Object.keys(this.formErrors);
        errors.map((key) => {
            const err = Reflect.get(this.formErrors, key);
            if (err && !this.stringHelper.isNullOrEmpty(err)) {
                return false;
            }
        });

        return true;
    }

    /**
     * To validate inputs which belong to tab
     * @param tabName Name of tab to validate
     * @returns boolean
     */
    private _validateInputsInTab(tabName: string): boolean {
        let isValid = true;
        // every formError should contains it's tab, e.x.: load#EquipmentType, general#shipFrom
        const errors = Object.keys(this.formErrors)?.filter(x => x.toLowerCase().startsWith(tabName.toLowerCase())) || [];
        const tabErrors: Array<string> = [];
        errors.map((key) => {
            const currentTab = key.includes('#') ? key.split('#')[0] : '';
            const err = Reflect.get(this.formErrors, key);
            if (err && !StringHelper.isNullOrEmpty(err)) {
                if (!this.stringHelper.isNullOrEmpty(currentTab) && !tabErrors.includes(currentTab)) {
                    tabErrors.push(currentTab);
                }
                isValid = false;
            }
        });

        // Add error message into the tab header
        this.resetBookingValidationErrors();
        if  (!isValid) {
            tabErrors.map(tabError => {
                this.bookingValidationErrors[tabError] = this.translateService.instant('validation.mandatoryFieldsValidation');
            });
            this.scrollToSection(tabName);
            return false;
        }

        return true;
    }

    onEditPOFulfillmentClick() {
        this.navigate(FormModeType.Edit);
        this.poFulfillmentGeneral.forceValidateForm();
    }

    onCancel() {
        const confirmDlg = this.notification.showConfirmationDialog(
            "edit.cancelConfirmation",
            "label.poFulfillment"
        );

        confirmDlg.result.subscribe((result: any) => {
            if (result.value) {
                this.resetBookingValidationErrors();
                this.isRefreshPOClicked = false;
                this.isReloadMode = false;
                if (this.isAddMode) {
                    this.backList();
                } else {
                    if (this.isEditMode) {
                        this.navigate(FormModeType.View);
                    }
                }
            }
        });
    }

    bookingValidation() {
        this.service.bookingValidation(this.model.id).subscribe(
            (data: any) => {
                switch (data.bookingRequestResult) {
                    case ValidationResultPolicy.PendingForApproval:
                        this.notification.showWarningPopup(
                            "msg.pendingForApprovalResult",
                            "label.poFulfillment"
                        );
                        break;
                    case ValidationResultPolicy.BookingAccepted:
                        this.notification.showSuccessPopup(
                            "msg.bookingAcceptedResult",
                            "label.poFulfillment"
                        );
                        break;
                    case ValidationResultPolicy.BookingRejected:
                        this.notification.showErrorPopup(
                            "msg.bookingRejectedResult",
                            "label.poFulfillment"
                        );
                        break;
                }
                this.ngOnInit();
            },
            () => {
                this.notification.showErrorPopup(
                    "save.failureNotification",
                    "label.poFulfillment"
                );
            }
        );
    }

    get hiddenBtnEdit() {
        if (
            !this.currentUser.isInternal &&
            this.model.stage === POFulfillmentStageType.ForwarderBookingRequest
        ) {
            return true;
        }

        if (this.model.stage === POFulfillmentStageType.Draft && this.isPendingStatus()) {
            return true;
        }

        if (this.model.stage === POFulfillmentStageType.Closed) {
            return true;
        }

        if (this.model.status === POFulfillmentStatus.Inactive) {
            return true;
        }

        if (!this.currentUser.isInternal) {
            if ((this.currentUser.role.id !== Roles.Shipper && this.currentUser.role.id !== Roles.Factory) && this.currentUser.role.id !== Roles.Agent) {
                return true;
            }
            if (this.currentUser.organizationId !== this.createdByOrganization?.organizationId ?? 0) {
                return true;
            }
        }
        return false;
    }

    get hiddenBtnCancel() {
        if (this.model.stage > POFulfillmentStageType.ForwarderBookingRequest) {
            return true;
        }

        if (this.model.status === POFulfillmentStatus.Inactive) {
            return true;
        }

        if (!this.currentUser.isInternal) {
            if ((this.currentUser.role.id !== Roles.Shipper && this.currentUser.role.id !== Roles.Factory) && this.currentUser.role.id !== Roles.Agent) {
                return true;
            }
            if (this.currentUser.organizationId !== this.createdByOrganization?.organizationId ?? 0) {
                return true;
            }
        }
        return false;
    }

    get hiddenBtnRefreshPO() {
        if (this.model.stage > POFulfillmentStageType.Draft) {
            return true;
        }

        if (!this.bookingPOAdhocChange ||
            this.bookingPOAdhocChange.priority !== this.purchaseOrderAdhocChangePriority.Level2) {
            return true;
        }

        if (!this.currentUser.isInternal) {
            if (this.currentUser.role.id !== Roles.Shipper && this.currentUser.role.id !== Roles.Factory) {
                return true;
            }
            if (this.currentUser.organizationId !== this.createdByOrganization?.organizationId ?? 0) {
                return true;
            }
        }

        return false;
    }

    get isOnlyCancelBooking() {
        if (this.bookingPOAdhocChange &&
            this.bookingPOAdhocChange.priority === this.purchaseOrderAdhocChangePriority.Level1) {
            return true;
        }
        return false;
    }

    get hiddenBtnBook() {
        if (this.model.stage !== POFulfillmentStageType.Draft) {
            return true;
        }

        if (this.model.stage === POFulfillmentStageType.Draft && this.isPendingStatus()) {
            return true;
        }

        if (this.model.status === POFulfillmentStatus.Inactive) {
            return true;
        }

        if (!this.currentUser.isInternal) {
            if ((this.currentUser.role.id !== Roles.Shipper && this.currentUser.role.id !== Roles.Factory) && this.currentUser.role.id !== Roles.Agent) {
                return true;
            }
            if (this.currentUser.organizationId !== this.createdByOrganization?.organizationId ?? 0) {
                return true;
            }
        }
        return false;
    }

    get hiddenBtnPlanToShip() {
        if (this.model.status === POFulfillmentStatus.Inactive) {
            return true;
        }

        if (StringHelper.caseIgnoredCompare(this.model.modeOfTransport, ModeOfTransportType.Air)) {
            return true;
        }

        if (
            this.model.stage !==
            POFulfillmentStageType.ForwarderBookingConfirmed
        ) {
            return true;
        }

        if (this.model.isGeneratePlanToShip) {
            return true;
        }

        if (!this.currentUser.isInternal) {
            if ((this.currentUser.role.id !== Roles.Shipper && this.currentUser.role.id !== Roles.Factory) && this.currentUser.role.id !== Roles.Agent) {
                return true;
            }
            if (this.currentUser.organizationId !== this.createdByOrganization?.organizationId ?? 0) {
                return true;
            }
        }

        if (this.model.movementType === appConstants.MovementTypes.CFSUnderscoreCY) {
            return true;
        }

        return false;
    }

    get hiddenCustomerPOTab() {
        return !this.isViewMode && (!this.service.currentCustomerPOs || !this.model.buyerCompliance);
    }


    // After 'Plan To Ship', show Container Info
    get isShowContainerInfo() {
        if (this.model.stage >=
            POFulfillmentStageType.ForwarderBookingConfirmed
        ) {
            return true;
        }

        return false;
    }

    // After 'Plan To Ship'
    // only update container info when stage = FB Confirmed and Shipment Dispatch
    get canUpdateContainer() {
        if (!this.hiddenBtnPlanToShip) {
            return false;
        }

        if (
            this.model.stage ===
            POFulfillmentStageType.ForwarderBookingConfirmed ||
            this.model.stage === POFulfillmentStageType.ShipmentDispatch
        ) {
            return true;
        }
        return false;
    }

    get isShowLoadInfoTab() {
        return !StringHelper.caseIgnoredCompare(this.model?.modeOfTransport, ModeOfTransportType.Air);
    }

    get isGeneralTabEditable() {
        if (this.model.stage > POFulfillmentStageType.Draft) {
            return false;
        }

        if (this.isViewMode) {
            return false;
        }

        return true;
    }

    get isContactTabEditable() {
        if (this.model.stage > POFulfillmentStageType.Draft) {
            return false;
        }

        if (this.isViewMode) {
            return false;
        }

        return true;
    }

    get isCustomerTabEditable() {
        if (this.model.stage > POFulfillmentStageType.Draft) {
            return false;
        }

        if (this.isViewMode) {
            return false;
        }

        return true;
    }

    get isLoadTabEditable() {
        if (this.model.stage > POFulfillmentStageType.Draft) {
            return false;
        }

        if (this.isViewMode) {
            return false;
        }

        return true;
    }

    get isShowActivityTab() {
        return (
            this.model.stage > POFulfillmentStageType.Draft ||
            this.activityCount > 0
        );
    }

    onCancelPOFulfillmentClick() {
        this.cancelPOFulfillmentDialog = true;
    }

    onRefreshPOClick() {
        this.navigate(FormModeType.Edit, {name: "refresh-po", value: 'true'});
        this.scrollToSection("customerPO");
    }

    onNoOfCancelDialogClick() {
        this.cancelPOFulfillmentDialog = false;
        this.cancelReason = "";
    }

    onYesOfCancelDialogClick() {
        this.isReadyForCancelBooking = false;
        this.cancelPOFulfillmentDialog = false;
        if (
            this.model.stage < POFulfillmentStageType.ForwarderBookingConfirmed
        ) {
            this.service
                .cancelPOFulfillment(this.model.id, this.cancelReason)
                .subscribe(
                    () => {
                        this._gaService.emitAction('Cancel', appConstants.GAEventCategory.POBooking);
                        this.cancelReason = "";
                        this.notification.showSuccessPopup(
                            "confirmation.cancelSuccessfully",
                            "label.poFulfillment"
                        );
                        // The current URL is in view mode so no need to navigate anymore.
                        // Just need to reload form data
                        this.loadInitData();
                        
                        this.bookingPOAdhocChange = null;
                        this.isReadyForCancelBooking = true;
                    },
                    (err) => {
                        this.cancelReason = '';
                        this.isReadyForCancelBooking = true;

                        if (err.error) {
                            const errorsDetails = err.error.errors;
                            const errors = errorsDetails.split(/(?=,\d+:\/)/);
                            this.saveBookingErrors = [];
                            if (errors) {
                                this.saveBookingErrors = errors.map(x => x.replace(',', '', 1));
                            }
                            this.saveBookingFailed = true;
                            this.saveBookingErrors.unshift(this.translateService.instant('msg.cancelledBookingFailedDetails'));
                        } else {
                            this.notification.showErrorPopup(
                            'confirmation.cancelUnsuccessfully',
                            'label.poFulfillment'
                            );
                        }
                    });
        }
    }

    onBookButtonClick() {
        this.isReadyForBook = false;
        let valid = true;
        let currentErrorTab = null;
        // Validate General tab
        const mandatoryFields = [
            "incoterm",
            "cargoReadyDate",
            "modeOfTransport",
            "logisticsService",
            "shipFrom",
            "shipTo",
            "receiptPort",
            "deliveryPort",
            "expectedShipDate"
        ];

        const translateLabelsMapping = {
            incoterm : 'incoterm',
            cargoReadyDate : 'bookingCargoReadyDates',
            modeOfTransport : 'modeOfTransport',
            logisticsService : 'logisticsServiceType',
            shipFrom : 'shipFrom',
            shipTo : 'shipTo',
            receiptPort : 'receiptPort',
            deliveryPort : 'deliveryPort',
            expectedDeliveryDate : 'expectedDeliveryDates',
            expectedShipDate : 'expectedShipDates'
        };

        mandatoryFields.forEach(f => {
            if (StringHelper.isNullOrEmpty(this.model[f]) || this.model[f] === 0) {
                const currentValue = this.bookingValidationErrors.general;
                const labelMapped = translateLabelsMapping[f];
                if (StringHelper.isNullOrEmpty(currentValue)) {
                    this.bookingValidationErrors.general = 'Missing ' + this.translateService.instant(`label.${labelMapped}`);
                } else {
                    this.bookingValidationErrors.general = currentValue + ', ' + this.translateService.instant(`label.${labelMapped}`);
                }
                valid = false;
            }
        });
        if (!StringHelper.isNullOrEmpty(this.bookingValidationErrors.general)) {
            this.bookingValidationErrors.general += '.';
            currentErrorTab = StringHelper.isNullOrEmpty(currentErrorTab) ? 'general' : currentErrorTab;
        }

        // Validate Contact tab
        const requiredOrgRoles = [
            OrganizationNameRole.Supplier,
            OrganizationNameRole.Principal,
            OrganizationNameRole.Shipper,
            OrganizationNameRole.OriginAgent,
            OrganizationNameRole.DestinationAgent
        ];
        requiredOrgRoles.forEach(organizationRole => {
            if (!this.model.contacts.find(c => c.organizationRole === organizationRole)) {
                const currentValue = this.bookingValidationErrors.general;
                if (StringHelper.isNullOrEmpty(currentValue)) {
                    this.bookingValidationErrors.general = 'Missing ' + organizationRole;
                } else {
                    this.bookingValidationErrors.general = currentValue + ', ' + organizationRole;
                }
                valid = false;
                currentErrorTab = StringHelper.isNullOrEmpty(currentErrorTab) ? 'general' : currentErrorTab;
            }
        });

        const contactConsignee = this.model.contacts.find(c => c.organizationRole === OrganizationNameRole.Consignee);
        if (!this.model.isNotifyPartyAsConsignee && !contactConsignee) {
            const currentValue = this.bookingValidationErrors.contact;
            if (StringHelper.isNullOrEmpty(currentValue)) {
                this.bookingValidationErrors.contact = 'Missing ' + OrganizationNameRole.Consignee.toString();
            } else {
                this.bookingValidationErrors.contact = currentValue + ', ' + OrganizationNameRole.Consignee.toString();
            }
            valid = false;
        }
        if (!StringHelper.isNullOrEmpty(this.bookingValidationErrors.contact)) {
            this.bookingValidationErrors.contact += '.';
            currentErrorTab = StringHelper.isNullOrEmpty(currentErrorTab) ? 'contact' : currentErrorTab;
        }

        // Validate Customer PO tab
        if (this.model.orders.length === 0) {
            this.bookingValidationErrors.customerPO = this.translateService.instant('validation.atLeastOneRecord');
            valid = false;
            currentErrorTab = StringHelper.isNullOrEmpty(currentErrorTab) ? 'customerPO' : currentErrorTab;
        }
        else {
            // submit booking doesn't allow missing POs
            let hasMissingPO = this.model.orders.findIndex(x => x.purchaseOrderId === 0) !== -1;
            if (hasMissingPO) {
                this.bookingValidationErrors.customerPO = this.translateService.instant('validation.missingPOsInformation');
                valid = false;
                currentErrorTab = StringHelper.isNullOrEmpty(currentErrorTab) ? 'customerPO' : currentErrorTab;
            }
        }

        // Validate Load tab
        if (this.model.loads.length === 0) {
            this.bookingValidationErrors.load = this.translateService.instant('validation.atLeastContainerLoad');
            valid = false;
            currentErrorTab = StringHelper.isNullOrEmpty(currentErrorTab) ? 'load' : currentErrorTab;
        }

        // validate tab attachment
        if (this.model.isContainDangerousGoods && (
            !this.model.attachments || !this.model.attachments.some(a => a.attachmentType === 'MSDS'))) {

            this.bookingValidationErrors.attachment = this.translateService.instant('validation.requireMSDSWithDangerousGoods');
            valid = false;
            currentErrorTab = StringHelper.isNullOrEmpty(currentErrorTab) ? 'attachment' : currentErrorTab;
        }

        if (!valid) {
            this.notification.showErrorPopup(
                "msg.validateCreateBookingRequestFailed",
                "label.poFulfillment"
            );
            const editBtn: HTMLElement = document.getElementById('editBtn') as HTMLElement;
            editBtn.click();
            this.isReadyForBook = true;
            setTimeout(() => {
                this.scrollToSection(currentErrorTab);
            }, 100);
            return;
        }

        // Check booking validation result
        this.service.trialValidateBooking(this.model.id).subscribe(
            (response) => {
                const validationResult = response;
                const actionType = validationResult.actionType;
                const policyCheckResults = validationResult.policyCheckResults;
                let actions: Array<any>;
                let title: string;
                let dialogOptions: any;
                let confirmDlg: any;
                switch (actionType) {
                    // It is Pending For Approval
                    case ValidationResultPolicy.PendingForApproval:
                        this.policyCheckResults = policyCheckResults;
                        actions = [
                            { text: this.translateService.instant('label.yes'), value: true, primary: true },
                            { text: this.translateService.instant('label.no'), value: false }
                        ];
                        title =  this.translateService.instant('label.poFulfillment');
                        dialogOptions = {
                            title : title,
                            content: this.confirmBookingPendingForApprovalTemplate,
                            actions: actions
                        };
                        confirmDlg = this._dialogService.open(dialogOptions);
                        confirmDlg.result.subscribe(
                            (result: any) => {
                                this.policyCheckResults = null;
                                if (result.value) {
                                    this.forceCreateBooking();
                                } else {
                                    this.isReadyForBook = true;
                                }
                            });

                        break;
                    // It is Rejected
                    case ValidationResultPolicy.BookingRejected:
                        this.policyCheckResults = policyCheckResults;

                        actions = [
                            { text: this.translateService.instant('label.ok'), value: false, primary: true }
                        ];
                        title = this.translateService.instant('label.poFulfillment');
                        dialogOptions = {
                            title : title,
                            content: this.confirmBookingRejectedTemplate,
                            actions: actions
                        };
                        confirmDlg = this._dialogService.open(dialogOptions);
                        confirmDlg.result.subscribe(
                            (result: any) => {
                                this.policyCheckResults = null;
                                if (result.value) {
                                } else {
                                    this.isReadyForBook = true;
                                }
                            });

                        break;
                    default:
                        this.forceCreateBooking();
                        break;
                }

            },
            err => this._createPOFulfillmentBookingRequestFailedHandler(err)
        );
    }

    /**
    * To create booking, including booking validation
    */
    private forceCreateBooking() {
        this.service.createPOFulfillmentBookingRequest(this.model.id).subscribe(
            (response) => {
                const poff = response;
                const isRejected = poff.isRejected;
                const isPending = this.isPendingStatus(poff);

                // POFF is rejected after validating
                if (isRejected) {
                    this.notification.showErrorPopup(
                        "msg.bookingRejectedResult",
                        "label.poFulfillment"
                    );
                } else if (isPending) {
                    // POFF is pending for approval after validating (stage of POFF is in stage Forwarder Booking Request)
                    this.notification.showWarningPopup(
                        "msg.pendingForApprovalResult",
                        "label.poFulfillment"
                    );
                } else {
                    // POFF is moved to Forwarder Booking Request
                    this.notification.showSuccessPopup(
                        "msg.bookingAcceptedResult",
                        "label.poFulfillment"
                    );
                }
                this.bookingPOAdhocChange = null;
                this.isReadyForBook = true;

                this._gaService.emitEvent('submit', appConstants.GAEventCategory.POBooking, 'Submit');

                this.router.navigate([`/po-fulfillments/view/${this.model.id}`], {
                    queryParams: {
                        formType: FormModeType.View
                    }
                });
            },
            err => {
                this._createPOFulfillmentBookingRequestFailedHandler(err);
            }
        );
    }

    /**
    * Calculate load total by sum-up all booking orders.
    */
    private calculateLoadTotal(loadItem) {
        let tmpSubTotal = 0;
        this.model.orders.filter(x => x.volume && x.volume > 0).map(x => tmpSubTotal += x.volume);
        loadItem.plannedVolume = tmpSubTotal;

        tmpSubTotal = 0;
        this.model.orders.filter(x => x.grossWeight && x.grossWeight > 0).map(x => tmpSubTotal += x.grossWeight);
        loadItem.plannedGrossWeight = tmpSubTotal;

        tmpSubTotal = 0;
        const netWeightValues = this.model.orders.filter(x => !StringHelper.isNullOrEmpty(x.netWeight));
        if (netWeightValues && netWeightValues.length > 0) {
            this.model.orders.filter(x => x.netWeight && x.netWeight > 0).map(x => tmpSubTotal += x.netWeight);
            loadItem.plannedNetWeight = tmpSubTotal;
        }

        tmpSubTotal = 0;
        this.model.orders.map(x => tmpSubTotal += (x.bookedPackage ?? 0));
        loadItem.plannedPackageQuantity = tmpSubTotal;
    }

    private _createPOFulfillmentBookingRequestFailedHandler(err) {
                // #region handle business cases based on the error message.
                // must be integrated with the server
                if (err.error) {
                    const errMessage = err.error.errors;
                    if (!this.stringHelper.isNullOrEmpty(errMessage)) {
                        const errHashTags = errMessage.split('#').filter(x => !StringHelper.isNullOrEmpty(x));
                        switch (errHashTags[0]) {
                            case 'POAdhocChanged':
                                const priority = this.purchaseOrderAdhocChangePriority[errHashTags[1]];
                                this.notification.showErrorPopup(
                                    this.getPOAdhocChangedMessage(priority),
                                    "label.poFulfillment"
                                );
                                this.ngOnInit();
                                this.bookingPOAdhocChange = new POAdhocChangedData(
                                    errHashTags[1],
                                    this.getPOAdhocChangedMessage(priority),
                                    errHashTags[3].split(',').map(x => Number.parseInt(x))
                                );
                                break;
                            case 'POBookedFully':
                                this.notification.showErrorPopup(
                                    "msg.createBookingRequestFailed.pOBookedFully",
                                    "label.poFulfillment"
                                );
                                break;
                            case 'Unauthorized':
                                this.notification.showErrorPopup(
                                    "msg.createBookingRequestFailed.unauthorized",
                                    "label.poFulfillment"
                                );
                                break;
                            case 'BlanketBooking':
                                    this.notification.showErrorPopup(
                                        "msg.createBookingRequestFailed.allocatedPOQtyIncorrect",
                                        "label.poFulfillment"
                                    );
                                    break;
                            case 'AllocatedBooking':
                                    this.notification.showErrorPopup(
                                        "msg.createBookingRequestFailed.allocatedPOQtyIncorrect",
                                        "label.poFulfillment"
                                    );
                                    break;
                            default:
                                const errors = errHashTags[0].split(/(?=,\d+:\/)/);
                                this.saveBookingErrors = [];
                                if (errors) {
                                    this.saveBookingErrors = errors.map(x => x.replace(',', '', 1));
                                }
                                this.saveBookingFailed = true;
                                this.saveBookingErrors.unshift(this.translateService.instant('msg.submittedBookingFailedDetails'));
                                break;
                        }
                        this.isReadyForBook = true;
                        return;
                    }
                }
                this.notification.showErrorPopup(
                    "msg.createBookingRequestFailed",
                    "label.poFulfillment"
                );
                this.isReadyForBook = true;
    }

    private getPOAdhocChangedMessage(priority) {
        let msg = '';
        if (priority <= PurchaseOrderAdhocChangePriority.Level2 &&
            (this.model.stage === POFulfillmentStageType.ForwarderBookingConfirmed ||
                this.model.stage === POFulfillmentStageType.ShipmentDispatch)) {
            msg = this.translateService.instant('msg.poBeingRevised');
        } else {
            msg = this.translateService.instant(appConstants.POAdhocChangeErrorMsgs[`level${priority}`]);
        }
        return msg;
    }

    private getDocumentLevelText(documentLevel: string) : string {
        return this._attachmentService.translateDocumentLevel(documentLevel);
    }

    uploadAttachment() {
        this.attachmentFormMode = AttachmentFormModeType.add;
        this.attachmentPopupComponent.updateStyle(
            this.ImportStepState.Selecting
        );
        this.attachmentUploadPopupOpened = true;
        this.attachmentModel = {
            id: 0,
            fileName: '',
            poFulfillmentId: this.model.id,
            entityType: EntityType.POFulfillment,
            documentLevel: this.DocumentLevel.POFulfillment,
            otherDocumentTypes: this.model.attachments?.map(x => new AttachmentKeyPair(x.attachmentType, x.referenceNo))
        };
    }

    downloadAttachments() {
        this._attachmentService.downloadAttachments(`Booking ${this.model.number} Documents` , this.selectedAttachments).subscribe();
    }

    editAttachment(attachment: POFulfillmentAttachmentModel) {
        // get index of current attachment
        const index = this.model.attachments.indexOf(attachment);
        if (index >= 0) {
            this.attachmentModel = Object.assign({}, attachment);
            this.attachmentModel.otherDocumentTypes = this.model.attachments?.filter(x => this.model.attachments.indexOf(x) !== index)?.map(x => new AttachmentKeyPair(x.attachmentType, x.referenceNo));
            this.attachmentModel.entityType = EntityType.POFulfillment;
            // store link to original attachments
            this.attachmentModel.sourceIndex = index;
            this.attachmentModel.poFulfillmentId =  this.model.id;
            this.attachmentFormMode = AttachmentFormModeType.edit;
            this.attachmentUploadPopupOpened = true;
            this.attachmentPopupComponent.updateStyle(
                this.ImportStepState.Selected
            );
        }
    }

    attachmentClosedHandler() {
        this.attachmentUploadPopupOpened = false;
    }

    attachmentAddedHandler(attachment: POFulfillmentAttachmentModel) {
        const ignoreAttachmentTypes = this.attachmentPopupComponent.ignoreUploadWarningAttachmentTypes;
        attachment.state = POFulfillmentAttachmentState.added;
        let attachments: Array<POFulfillmentAttachmentModel> = this.model.attachments;
        attachments = attachments.filter(x => ignoreAttachmentTypes.indexOf(x.attachmentType) >= 0 || x.attachmentType !== attachment.attachmentType || !this.stringHelper.caseIgnoredCompare(x.referenceNo, attachment.referenceNo));
        attachments.unshift(attachment);
        this.model.attachments = attachments;
        this.attachmentUploadPopupOpened = false;
        this._gaService.emitEvent('upload_attachment', appConstants.GAEventCategory.POBooking, 'Upload Attachment');
    }

    attachmentEditedHandler(attachment: POFulfillmentAttachmentModel) {
        const ignoreAttachmentTypes = this.attachmentPopupComponent.ignoreUploadWarningAttachmentTypes;
        attachment.state = attachment.state || POFulfillmentAttachmentState.edited;
        attachment.uploadedDateTime = (new Date()).toISOString();
        let attachments: Array<POFulfillmentAttachmentModel> = this.model.attachments;
        // get link to original attachment
        const index = attachment.sourceIndex;
        if (index >= 0) {
            attachments = attachments.filter(x => this.model.attachments.indexOf(x) !== index && (ignoreAttachmentTypes.indexOf(x.attachmentType) >= 0 || x.attachmentType !== attachment.attachmentType || !this.stringHelper.caseIgnoredCompare(x.referenceNo, attachment.referenceNo)));
            // push at the top of grid
            attachments.unshift(attachment);
            this.model.attachments = attachments;
        }
        this.attachmentUploadPopupOpened = false;
        this._gaService.emitEvent('edit_attachment', appConstants.GAEventCategory.POBooking, 'Edit Attachment');
    }

    attachmentDeletedHandler(attachment: POFulfillmentAttachmentModel) {
        const confirmDlg = this.notification.showConfirmationDialog(
            "msg.deleteAttachmentConfirmation",
            "label.poFulfillment"
        );

        confirmDlg.result.subscribe((result: any) => {
            if (result.value) {
                const index = this.model.attachments.indexOf(attachment);
                if (index >= 0) {
                    this.model.attachments.splice(index, 1);
                }
                this._gaService.emitEvent('delete_attachment', appConstants.GAEventCategory.POBooking, 'Delete Attachment');
            }
        });
    }

    downloadFile(id, fileName) {
        this._attachmentService.downloadFile(id, fileName).subscribe();
    }

    isRejectedStatus() {
        return (
            this.model.isRejected &&
            this.model.status !== POFulfillmentStatus.Inactive
        );
    }

    isPendingStatus(data?: any) {
        let poff = data;
        if (this.stringHelper.isNullOrEmpty(poff)) {
            // Get from component model data
            poff = this.model;
        }
        return (
            poff.buyerApprovals &&
            poff.buyerApprovals.length > 0 &&
            poff.buyerApprovals.find(
                x => x.stage === BuyerApprovalStage.Pending
            ) &&
            poff.status !== POFulfillmentStatus.Inactive && // cancel
            poff.stage === POFulfillmentStageType.ForwarderBookingRequest
        );
    }
    private resetBookingValidationErrors() {
        this.bookingValidationErrors.general = null;
        this.bookingValidationErrors.contact = null;
        this.bookingValidationErrors.customerPO = null;
        this.bookingValidationErrors.load = null;
        this.bookingValidationErrors.attachment = null;
    }

    private _emitPOTypeChanged(poType: POType) {
        const emitValue: IntegrationData = {
            name: '[po-fulfillment-form]poTypeChanged',
            content: {
                'poType': poType
            }
        };
        this.integration$.next(emitValue);
    }

    bindingNoteTab() {
        var noteObs$ = this.service.getNotes(this.model.id)
            .pipe(
                map((res:any) => {
                    return res.map(x => {
                        const newPOFulfillmentNoteModel = new POFulfillmentNoteModel();
                        newPOFulfillmentNoteModel.MapFrom(x);
                        return newPOFulfillmentNoteModel;
                    })
                })
            )

        var masterNote$ = this.service.getMasterNotes(this.model.id)
            .pipe(
                map((res: any) => {
                    return res.map(x => {
                        const newPOFulfillmentNoteModel =  new POFulfillmentNoteModel();
                        newPOFulfillmentNoteModel.MapFromMasterNote(x);
                        return newPOFulfillmentNoteModel;
                    })
                })
            )

        forkJoin([noteObs$, masterNote$]).subscribe(
            (note) => {
                this.noteList = note[0].concat(note[1]);
            });
    }

    getShipmentItineraries(): void {
        let viewSettingModuleId = "";
        if (this.formType === FormModeType.View) {
            viewSettingModuleId = ViewSettingModuleIdType.FREIGHTBOOKING_DETAIL_SHIPMENT_SHIPMENTITINERARIES;
        }
        this.shipmentItineraries = []; // reset data before
        if (this.model?.shipments?.length > 0) {
            this.model.shipments.forEach(c => {
                this.service.getItineraryByShipmentId(c.id, viewSettingModuleId).subscribe(
                    (response: any) => {
                        const data = response.sort((a, b) => parseInt(a.sequence) - parseInt(b.sequence));
                        this.shipmentItineraries = this.shipmentItineraries.concat(data);
                    });
            })
        }
    }

    /**Turn on/off the tab display. */
    setTabDisplay(sectionId: string, isDisplay: boolean) {
        let tab = this.tabs.filter(x => x.sectionId == sectionId);
        tab[0].hidden = !isDisplay;
        
        if (isDisplay) {
            // delay time to make sure this tab section is fully shown.
            setTimeout(() => {
                this.linkTab(tab[0]);
            }, 500);
        }
    }

    /**Return true if tab is able to display.
     ** Usage: tabDisplay['sectionId'] */
    get tabDisplay() : any {
        let result = {};
        this.tabs.map(({ sectionId, hidden }) => result[sectionId] = !hidden);
        return result;
    }

    /**Link UI element to tabs object
    Must make sure that it is correct order */
    private initTabLink(): void {
        this.tabs.map(tab => {
            this.linkTab(tab);
        });
    }

    private linkTab(tab : POFulfillmentTabModel) {
        switch (tab.sectionId) {
            case 'general':
                tab.sectionElementRef = this.generalElement;
                break;
            case 'contact':
                tab.sectionElementRef = this.contactElement;
                break;
            case 'customerPO':
                tab.sectionElementRef = this.customerPOElement;
                break;
            case 'load':
                tab.sectionElementRef = this.loadElement;
                break;
            case 'activity':
                tab.sectionElementRef = this.activityElement;
                break;
            case 'attachment':
                tab.sectionElementRef = this.attachmentElement;
                break;
            case 'dialog':
                tab.sectionElementRef = this.dialogElement;
                break;
        }
    }

    /**To scroll to a specific section. */
    private scrollToSection(sectionId: string) : void {
        let tabIndex = this.tabs.findIndex(x => x.sectionId === sectionId);
        if (tabIndex !== -1){
            this.onClickStickyBar(null, this.tabs[tabIndex]);
        }
    }
    
    onClickStickyBar(event, tab: POFulfillmentTabModel) {
        this.isManualScroll = false;
        for (let i = 0; i < this.tabs.length; i++) {
            const element = this.tabs[i];
            if (element.sectionId === tab.sectionId) {
                element.selected = true;
            } else {
                element.selected = false;
            }
        }
        // If the first section, move to the top
        if (tab.text === 'label.general') {
            window.scrollTo({ behavior: 'smooth', top: 0 });
        } else {
            const headerHeight = this.headerBarElement?.nativeElement?.clientHeight;
            window.scrollTo({ behavior: 'smooth', top: tab.sectionElementRef?.nativeElement?.offsetTop - headerHeight - 36 });
        }

        // After 1s, reset isManualScroll = true -> it scrolls to target position
        setTimeout(() => {
            this.isManualScroll = true;
        }, 1000);
    }

    @HostListener('window:scroll', ['$event'])
    onScroll(event) {
        const currentYPosition = window.scrollY;
        const headerHeight = this.headerBarElement?.nativeElement?.clientHeight;

        // Make header sticky
        if (currentYPosition >= headerHeight - 30) {
            this.headerBarElement?.nativeElement?.style.setProperty('position', 'sticky');
            this.headerBarElement?.nativeElement?.style.setProperty('top', '60px');
        } else {
            this.headerBarElement?.nativeElement?.style.setProperty('position', 'relative');
            this.headerBarElement?.nativeElement?.style.removeProperty('top');
        }

        // Make sticky bar

        if (currentYPosition >= headerHeight + 30) {
            this.stickyBarElement?.nativeElement?.style.setProperty('position', 'sticky');
            this.stickyBarElement?.nativeElement?.style.setProperty('top', headerHeight + 60 + 'px');
            this.stickyBarElement?.nativeElement?.style.removeProperty('display');
        } else {
            this.stickyBarElement?.nativeElement?.style.setProperty('display', 'none');
        }

        //#region Auto update sticky bar status

        // If user clicks on sticky menu, do not update status
        if (!this.isManualScroll) {
            return;
        }

        this.tabs.forEach(c => {
            c.selected = false;
        });

        for (let i = 0; i < this.tabs.length; i++) {
            const element = this.tabs[i];
            // adding 240px to make update sticky bar earlier
            if (currentYPosition + headerHeight + 40 <= element.sectionElementRef?.nativeElement?.offsetTop + element.sectionElementRef?.nativeElement?.clientHeight) {
                element.selected = true;
                break;
            }
        }
        //#endregion
    }
    
    /**
     * To check whether the field is able to display on current user role. 
     * */
    isVisibleField(moduleId: ViewSettingModuleIdType, fieldId: string): boolean {
        if (!this.isViewMode) { // apply for view mode only
            return true;
        }
        
        return !FormHelper.isHiddenColumn(this.model.viewSettings, moduleId, fieldId);
    }

    /** Internal use only. Navigate to another mode in the same component.* */
    private navigate(modeType: FormModeType, ...additionalParams: {name: string, value: string}[]) : void {
        // The current URL is in the same mode so no need to navigate anymore.
        // Just need to reload form data
        if (modeType.toLowerCase() === this.paramMode.toLowerCase()) {
            this.loadInitData();
            return;
        }
        // Append more params on navigate url
        let params = {
            formType: modeType
        };
        if (additionalParams && additionalParams.length)
        {
            for (var param of additionalParams)
            {
                params[param.name] = param.value
            }
        }
        this.router.navigate([`/missing-po-fulfillments/${modeType}/${this.model.id}`], {
            queryParams: params
        });
    }

    ngOnDestroy(): void {
        this.service.resetCustomerPOs();
        this._subscriptions.map(x => x.unsubscribe());
    }

    //15-09-2023 getIsEditPoVal added to get value for bulk edit is click r not
    getIsEditPoVal(val){
        this.isBulkEditClick = val;
    }
}

