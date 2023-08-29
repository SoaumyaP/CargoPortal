import { AfterViewChecked, ChangeDetectorRef, Component, ViewChild } from '@angular/core';
import { faShare, faBoxes, faCloudUploadAlt, faTrashAlt, faPencilAlt, faEllipsisV, faPlus, faCheck, faBan, faUnlink, faMinus, faCloudDownloadAlt, faInfo } from '@fortawesome/free-solid-svg-icons';
import { ShipmentTrackingService } from './shipment-tracking.service';
import { Router, ActivatedRoute } from '@angular/router';
import { MilestoneComponent } from 'src/app/ui/milestone/milestone.component';
import {
    DATE_FORMAT, FormComponent, UserContextService, MilestoneType, ActivityType,
    DateHelper, StringHelper, OrganizationNameRole, Roles, POFulfillmentStageType, OrderType, ModeOfTransportType, ShipmentTab, EntityType, StatusStyle, OrganizationType, ShipmentStatus, ConsolidationStage, FulfillmentType, FormModeType
} from 'src/app/core';
import { RowArgs } from '@progress/kendo-angular-grid';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { AttachmentUploadPopupComponent } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.component';
import { AttachmentUploadPopupService } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.service';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { VerificationSetting } from 'src/app/core';
import { ConsignmentItineraryFormComponent } from '../consignment-itinerary-form/consignment-itinerary-form.component';
import { TabStripComponent } from '@progress/kendo-angular-layout';
import { ConsignmentFormDialogComponent } from '../consignment-form-dialog/consignment-form-dialog.component';
import { AttachmentKeyPair, AttachmentModel } from 'src/app/core/models/attachment.model';
import { ConsignmentModel } from '../../consignment/models/consignment.model';
import { ShipmentAddMasterBLPopupComponentMetadata } from '../shipment-add-master-bl-popup/shipment-add-master-bl-popup.component';
import { DocumentLevel, GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';
import { ShipmentModel } from 'src/app/core/models/shipments/shipment.model';
import { groupBy} from 'lodash';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { ConfirmItineraryModel, ShipmentItineraryConfirmPopupMode } from '../shipment-itinerary-confirm-popup/shipment-itinerary-confirm-popup.component';
import { OrderByPipe } from 'src/app/core/pipes/order-by.pipe';
import { delay, map } from 'rxjs/operators';
@Component({
    selector: 'app-shipment-tracking',
    templateUrl: './shipment-tracking.component.html',
    styleUrls: ['./shipment-tracking.component.scss']
})
export class ShipmentTrackingComponent extends FormComponent implements AfterViewChecked {
    modelName = 'shipments';
    model: ShipmentModel;
    milestoneType: MilestoneType;
    modeOfTransportType = ModeOfTransportType;
    formModeType = FormModeType;
    documentLevel = DocumentLevel;
    @ViewChild('milestone', { static: false }) milestone: MilestoneComponent;
    @ViewChild('tab', { static: false }) allTabs: TabStripComponent;

    DATE_FORMAT = DATE_FORMAT;
    readonly ConsolidationStage = ConsolidationStage;
    readonly fulfillmentType = FulfillmentType;
    readonly poFulfillmentStageType = POFulfillmentStageType;
    faShare = faShare;
    /**
     * Data source for consignment grid
     */
    consignment: Array<ConsignmentModel> = [];
    itineraries = [];
    consolidation = [];
    cargoDetail = [];
    contacts = [];
    /**All available attachment on grid */
    attachments: Array<AttachmentModel> = [];
    container = [];
    containerLimit = [];
    /**Selected attachment via check-boxes */
    selectedAttachments: Array<AttachmentModel> = [];
    faCloudUploadAlt = faCloudUploadAlt;
    faCloudDownloadAlt = faCloudDownloadAlt;
    faTrashAlt = faTrashAlt;
    faPencilAlt = faPencilAlt;
    faEllipsisV = faEllipsisV;
    faPlus = faPlus;
    faCheck = faCheck;
    faBoxes = faBoxes;
    faBan = faBan;
    faMinus = faMinus;
    faUnlink = faUnlink;
    faInfo = faInfo;
    importFormOpened = false;
    /**Data for attachment model upload popup */
    attachmentModel: AttachmentModel = null;
    attachmentFormMode = 0;
    currentUser = null;
    AttachmentFormModeType = {
        add: 0,
        edit: 1
    };
    consignmentItineraryFormMode = 'view';
    itineraryDetails: any = {};
    consignmentFormDialogOpened: boolean;
    consignmentItineraryFormDialogOpened: boolean;
    readonly AppPermissions = AppPermissions;
    defaultValue = DefaultValue2Hyphens;
    shipmentStatus = ShipmentStatus;

    confirmItineraryPopupMode: ShipmentItineraryConfirmPopupMode;
    confirmItineraryModel: ConfirmItineraryModel;
    isItineraryInfoMouseEnter: boolean = false;

    activityFormOpened: boolean;
    consolidationPopupOpened: boolean;
    confirmItineraryPopupOpened: boolean;
    heightActivity = 530;
    activityFormMode = 'view';
    activityDetails: any = {};
    executionAgentOptions: any[];
    executionAgentName = '';
    allLocationOptions: any;
    allEventOptions: any;
    activityList = [];
    groupedActivityList = [];
    isCanClickHouseBL: boolean;
    isCanClickMasterBL: boolean;
    isCanClickContainer: boolean;
    isCanClickBooking: boolean;
    isCanClickPO: boolean;
    isCanClickVesselFlight: boolean;
    isOpenHouseBLPopup: boolean;
    isShowBillOfLadingButton: boolean;
    isShowHAWBButton: boolean;
    isReadyForCancelShipment: boolean = true;
    isFullLoadShipment: boolean = true;
    isShowNestedActivityGrid: boolean = false;

    openCargoDescriptionDetailPopup: boolean = false;
    cargoDescriptionDetail: string;

    stringHelper = StringHelper;

    @ViewChild(AttachmentUploadPopupComponent, { static: false }) attachmentPopupComponent: AttachmentUploadPopupComponent;
    @ViewChild(ConsignmentItineraryFormComponent, { static: false }) consignmentItineraryFormComponent: ConsignmentItineraryFormComponent;
    @ViewChild(ConsignmentFormDialogComponent, { static: false }) consignmentFormComponent: ConsignmentFormDialogComponent;
    ImportStepState = {
        Selecting: 0,
        Selected: 1,
    };

    customerFormOpened = false;

    productVerificationSetting: any = {
        commodityVerification: VerificationSetting.AsPerPO
    };
    buyerCompliance = null;

    /**
     * Metadata for Add Master bill of lading popup
     */
    masterBLPopupMetaData: ShipmentAddMasterBLPopupComponentMetadata = {
        isFormOpened: false,
        itineraryDataSource: [],
        shipmentId: null,
        modeOfTransport: null,
        executionAgentDataSource: []
    };
    
    constructor(
        private cdr: ChangeDetectorRef,
        protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: ShipmentTrackingService,
        public orderByPipe: OrderByPipe,
        public translateService: TranslateService,
        private _userContext: UserContextService,
        public attachmentService: AttachmentUploadPopupService,
        private _gaService: GoogleAnalyticsService) {
        super(route, service, notification, translateService, router);
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
                if (!user.isInternal) {
                    this.service.affiliateCodes = user.affiliates;
                }
                this.setDataAccessRight();
            }
        });

        this.service.getEvents().subscribe(data => {
            this.allEventOptions = data;
        });
    }

    onInitDataLoaded(data): void {
        if (this.model != null) {
            this.setMilestoneType();
            this.getActivityTab();
            this.getItineraryTab();
            this.getCargoDetailsTab();
            this.getContactTab(this.model.contacts);
            this.getAttachmentTab();
            this.setDataForHouseBLPopup();

            // buyer compliance
            const principalContact = this.model.contacts.find((c) => c.organizationRole === OrganizationNameRole.Principal);
            if (principalContact) {
                const customerId = principalContact.organizationId;
                this.service.getBuyerCompliance(customerId).subscribe(res => {
                    this.buyerCompliance = res[0];
                    this.productVerificationSetting = this.buyerCompliance ? this.buyerCompliance.productVerificationSetting : null;
                });
            }

            this.service.checkFullLoadShipment(this.model.id).subscribe((rsp: boolean) => {
                this.isFullLoadShipment = rsp;
            });
        }
    }

    setDataForHouseBLPopup() {
        this.setIsShowBillOfLadingButton();
        this.setIsShowHAWBButton();
    }

    setIsShowBillOfLadingButton() {
        const isActiveShipment = this.model.status.toLowerCase() === StatusStyle.Active;
        const isSeaOrAirShipment = this.model.modeOfTransport === ModeOfTransportType.Sea || this.model.modeOfTransport === ModeOfTransportType.Air;
        const isShipmentStillNotAssigned = this.model.billOfLadingNos?.length === 0 && this.model.masterBillNos?.length === 0;
        const isUserHasPermission =  this.currentUser?.permissions?.some(c => c.name === AppPermissions.BillOfLading_HouseBLDetail_Add || c.name === AppPermissions.BillOfLading_MasterBLDetail_Add);

        if ( isActiveShipment && isSeaOrAirShipment && isShipmentStillNotAssigned && isUserHasPermission) {
            this.isShowBillOfLadingButton = true;
        } else {
            this.isShowBillOfLadingButton = false;
        }
    }

    setIsShowHAWBButton() {
        const isActiveShipment = this.model.status.toLowerCase() === StatusStyle.Active;
        const isSeaOrAirShipment = this.model.modeOfTransport === ModeOfTransportType.Air;
        const isShipmentStillNotAssigned = this.model.billOfLadingNos?.length === 0;
        const isUserHasPermission =  this.currentUser?.permissions?.some(c => c.name === AppPermissions.BillOfLading_HouseBLDetail_Add);

        if ( isActiveShipment && isSeaOrAirShipment && isShipmentStillNotAssigned && isUserHasPermission) {
            this.isShowHAWBButton = true;
        } else {
            this.isShowHAWBButton = false;
        }
    }

    setDataAccessRight() {
        this.isCanClickHouseBL = this.currentUser?.permissions?.some(c => c.name === AppPermissions.BillOfLading_HouseBLDetail);
        this.isCanClickMasterBL = this.currentUser?.permissions?.some(c => c.name === AppPermissions.BillOfLading_MasterBLDetail);
        this.isCanClickContainer = this.currentUser?.permissions?.some(c => c.name === AppPermissions.Shipment_ContainerDetail);
        this.isCanClickBooking = this.currentUser?.permissions?.some(c => c.name === AppPermissions.PO_Fulfillment_Detail);
        this.isCanClickPO = this.currentUser?.permissions?.some(c => c.name === AppPermissions.PO_Detail);
        this.isCanClickVesselFlight = this.currentUser?.permissions?.some(c => c.name === AppPermissions.FreightScheduler_List);
    }

    setMilestoneType() {
        const shipmentType = +this.model.orderType;
        if (shipmentType === OrderType.Freight) {
            this.milestoneType = this.model.modeOfTransport === ModeOfTransportType.Air ? MilestoneType.AirFreightShipment : MilestoneType.ShipmentFreight;
        }

        if (shipmentType === OrderType.Cruise) {
            if (!this.model.modeOfTransport) {
                this.milestoneType = MilestoneType.ShipmentCruiseSeaOrOcean;
                return;
            }

            const modeOfTransport = this.model.modeOfTransport.toLowerCase();

            if (modeOfTransport === ModeOfTransportType.Sea.toLowerCase() || modeOfTransport === ModeOfTransportType.Ocean.toLowerCase()) {
                this.milestoneType = MilestoneType.ShipmentCruiseSeaOrOcean;
            }
            if (modeOfTransport === ModeOfTransportType.Air.toLowerCase() || modeOfTransport === ModeOfTransportType.Courier.toLowerCase()) {
                this.milestoneType = MilestoneType.ShipmentCruiseAirOrCourier;
            }
            if (modeOfTransport === ModeOfTransportType.Road.toLowerCase()) {
                this.milestoneType = MilestoneType.ShipmentCruiseRoad;
            }
        }
    }

    updateExceptionRemark() {
        this.model.isException = this.activityList.some(x => !StringHelper.isNullOrEmpty(x.resolved) && x.resolved === false &&
            !StringHelper.isNullOrEmpty(x.activityType) && x.activityType[x.activityType.length - 1] === 'E');
    }

    public selectAttachment(context: RowArgs): string {
        return context.dataItem;
    }

    getActivityTab() {
        this.service.getActivity(this.model.id).subscribe(
            (response: any) => {
                this.activityList = response;
                this.groupActivity();
                this.updateMilestone();
                this.updateExceptionRemark();
            }
        );
    }

    groupActivity() {
        this.groupedActivityList = [];

        // group by activity code & location
        this.activityList.map(
            x => {x.activityCodeLocation =  `${x.activityCode}_${x.location}`;
            }
        );
        const groupedByCodeObj = groupBy([...this.activityList], 'activityCodeLocation');

        for (const property in groupedByCodeObj) {
            let groupedActivities = groupedByCodeObj[property];
            if (groupedActivities.length > 1) {
                groupedActivities.sort((a, b) =>
                    Date.parse(a.activityDate) - Date.parse(b.activityDate) || Date.parse(a.createdDate) - Date.parse(b.createdDate)
                );
                let activity = groupedActivities[0];
                activity.nestedList = groupedActivities.filter(a => a.id !== activity.id);
                this.groupedActivityList.push(activity);
            } else {
                let activity = groupedActivities[0];
                activity.nestedList = [];
                this.groupedActivityList.push(activity);
            }
        }
        this.groupedActivityList = this.sortActivity(this.groupedActivityList);
        this.isShowNestedActivityGrid = this.groupedActivityList.some(x => x.nestedList.length > 0);
    }

    /**Sort by activityDate descending, if it is equal then sort by activityCode. */
    sortActivity([...activityList]) {
        if (activityList?.length > 0) {
            activityList.sort((a, b) => {
                if (new Date(Date.parse(a.activityDate)).setHours(0,0,0,0) < new Date(Date.parse(b.activityDate)).setHours(0,0,0,0)) {
                    return 1;
                } else if (new Date(Date.parse(a.activityDate)).setHours(0,0,0,0) > new Date(Date.parse(b.activityDate)).setHours(0,0,0,0)) {
                    return -1;
                }
                // Else go to compare sortSequence
                if (a.sortSequence < b.sortSequence) {
                    return 1;
                } else if (a.sortSequence > b.sortSequence) {
                    return -1;
                } else { // nothing to split them
                    return 0;
                }
            });
        }
        return activityList;
    }

    public rowCallback(args) {
        return {
            'expandable': args.dataItem.nestedList.length > 0
        };
    }

    updateMilestone() {
        if (this.milestone != null) {
            let activityType;
            const typeShipment = +this.model.orderType;
            if (typeShipment === OrderType.Freight) {
                activityType = ActivityType.FreightShipment;
            }

            if (typeShipment === OrderType.Cruise) {
                activityType = ActivityType.CruiseShipment;
            }
            this.milestone.data = this.activityList.filter(a => a.activityType === activityType || a.activityType === ActivityType.VesselActivity);
            this.milestone.reload();
        }
    }

    get isAgentUser() {
        return this.currentUser && this.currentUser.userRoles.find(x => x.role.id === Roles.Agent || x.role.id === Roles.CruiseAgent) != null;
    }

    get canAddActivity() {
        return this.currentUser
            && this.currentUser.userRoles.find(x => x.role.id !== Roles.Principal && x.role.id !== Roles.Shipper && x.role.id !== Roles.CruisePrincipal) != null
            && StringHelper.caseIgnoredCompare(this.model.status, ShipmentStatus.Active);
    }

    get isViewableConsigneeDetail() {
        return this.currentUser && !this.currentUser.userRoles.some(x => x.role.id === Roles.Shipper || x.role.id === Roles.Principal || x.role.id === Roles.CruisePrincipal);
    }

    get isShowCancelButton() {
        return this.currentUser.isInternal && this.model.fulfillmentNumber && this.model.fulfillmentStage !== POFulfillmentStageType.Closed
            && this.model.status.toLowerCase() === 'active';
    }

    get isDisableConsolidateButton() {
        if (!this.cargoDetail || this.cargoDetail.length <= 0) {
            return true;
        }
        return false;
    }

    get firstItinerary() {
        let itineraries = this.orderByPipe.transform([...this.itineraries], 'sequence');
        return itineraries[0]
    }

    getItineraryTab() {
        this.service.getConsignment(this.model.id).subscribe(
            (response: any) => {
                this.consignment = response;
            }
        );
        this.service.getItinerary(this.model.id).subscribe(
            (response: any) => {
                this.itineraries = response;
            }
        );
    }

    getCargoDetailsTab() {
        this.service.getCargoDetail(this.model.id).subscribe(
            (response: any) => {
                this.cargoDetail = response;
            }
        );
        if (this.model.isFCL) {
            this.service.getContainer(this.model.id).subscribe(
                (response: any) => {
                    this.container = response?.map(x => ({ ...x, isConfirmed : true }));
                    this.containerLimit = this.container.slice(0, 2);
                }
            );
        } else {
            this.service.getConsolidation(this.model.id).subscribe(
                (response: any) => {
                    this.consolidation = response;

                    this.container = this.consolidation.filter(c => c.containerId).map(c => {
                        return {
                            id: c.containerId,
                            containerNo: c.containerNo,
                            isConfirmed: c.stage === ConsolidationStage.Confirmed
                        };
                    });
                    this.containerLimit = this.container.slice(0, 2);
                }
            );
        }
    }

    getContactTab(contacts: any[] = null) {
        if (contacts != null) {
            return this.contacts = contacts;
        }

        this.service.getContact(this.model.id).subscribe(
            (response: any) => {
                this.contacts = response;
            }
        );
    }

    getAttachmentTab() {
        this.service.getAttachment(this.model.id).subscribe(
            (response: any) => {
                this.attachments = response;
            }
        );
    }

    private getDocumentLevelText(documentLevel: string): string {
        return this.attachmentService.translateDocumentLevel(documentLevel);
    }

    backList() {
        this.router.navigate(['/shipments']);
    }

    downloadFile(id, fileName) {
        this.attachmentService.downloadFile(id, fileName).subscribe();
    }

    uploadAttachment() {
        this.attachmentFormMode = this.AttachmentFormModeType.add;
        this.attachmentPopupComponent.updateStyle(this.ImportStepState.Selecting);
        this.importFormOpened = true;
        this.attachmentModel = {
            id: 0,
            fileName: '',
            shipmentId: this.model.id,
            entityType: EntityType.Shipment,
            documentLevel: this.documentLevel.Shipment,
            otherDocumentTypes: this.attachments?.map(x => new AttachmentKeyPair(x.attachmentType, x.referenceNo))
        };
    }

    attachmentAddHandler(attachment: AttachmentModel) {
        this.importFormOpened = false;
        this.attachmentService.create(attachment).subscribe(
            data => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.attachment');
                this.getAttachmentTab();
                this._gaService.emitAction('Add Attachment', GAEventCategory.Shipment);
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.attachment');
            });
    }

    attachmentEditHandler(attachment: AttachmentModel) {
        this.importFormOpened = false;
        this.attachmentService.update(attachment.id, attachment).subscribe(
            data => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.attachment');
                this.getAttachmentTab();
                this._gaService.emitAction('Edit Attachment', GAEventCategory.Shipment);
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.attachment');
            });
    }

    editAttachment(id) {
        const result = this.attachments.find(x => x.id === id);
        if (result) {

            // clone object to dis-coupling on data reference from current page to Attachment popup
            this.attachmentModel = Object.assign({}, result);
            this.attachmentModel.shipmentId = this.model.id;
            this.attachmentModel.entityType = EntityType.Shipment;
            this.attachmentModel.otherDocumentTypes = this.attachments?.filter(x => x.id !== this.attachmentModel.id)?.map(x => new AttachmentKeyPair(x.attachmentType, x.referenceNo));

            this.attachmentPopupComponent.updateStyle(this.ImportStepState.Selected);
            this.attachmentFormMode = this.AttachmentFormModeType.edit;
            this.importFormOpened = true;
        }
    }

    deleteAttachment(attachmentId: number) {
        const confirmDlg = this.notification.showConfirmationDialog('msg.deleteAttachmentConfirmation', 'label.attachment');
        const globalId = `${EntityType.Shipment}_${this.model.id}`;
        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.attachmentService.deleteAttachment(globalId, attachmentId).subscribe(
                        data => {
                            this.notification.showSuccessPopup('msg.attachmentDeleteSuccessfullyNotification', 'label.attachment');
                            this.getAttachmentTab();
                            this._gaService.emitAction('Delete Attachment', GAEventCategory.Shipment);
                        },
                        error => {
                            this.notification.showErrorPopup('save.failureNotification', 'label.attachment');
                        });
                }
            });
    }

    downloadAttachments() {
        this.attachmentService.downloadAttachments(`Shipment ${this.model.shipmentNo} Documents` , this.selectedAttachments).subscribe();
    }

    onAddConsignmentClick() {
        this.consignmentFormDialogOpened = true;
    }

    onConsignmentFormDialogClosed() {
        this.consignmentFormDialogOpened = false;
    }

    onConsignmentAdded(consignment: any) {
        this.consignmentFormDialogOpened = false;
        consignment.shipmentId = this.model.id;
        consignment.movement = this.model.movement;
        consignment.unit = this.model.totalUnit;
        consignment.package = this.model.totalPackage;
        consignment.volume = this.model.totalVolume;
        consignment.netWeight = this.model.totalNetWeight;
        consignment.grossWeight = this.model.totalGrossWeight;
        consignment.triangleTradeFlag = false;
        consignment.memoBOLFlag = false;
        consignment.packageUOM = this.model.totalPackageUOM;
        consignment.volumeUOM = this.model.totalVolumeUOM;
        consignment.grossWeightUOM = this.model.totalGrossWeightUOM;
        consignment.netWeightUOM = this.model.totalNetWeightUOM;
        consignment.unitUOM = this.model.totalUnitUOM;

        this.service.createConsignment(DateHelper.formatDate(consignment)).subscribe(
            data => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.consignment');
                this._gaService.emitAction('Add Consignment', GAEventCategory.Shipment);
                this.service.getConsignment(this.model.id).subscribe(
                    (response: any) => {
                        this.consignment = response;
                    }
                );
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.consignment');
            });
    }

    get isDisabledAddItineraryButton() {
        if (this.model) {
            const isAirShipment = StringHelper.caseIgnoredCompare(this.model?.modeOfTransport, ModeOfTransportType.Air)
            const hasHAWB = this.model.billOfLadingNos?.length > 0 || false;
            // If the AIR Shipment does not link to HAWB# yet
            if (isAirShipment && !hasHAWB) {
                return true;
            }
        }
        return !this.consignment || this.consignment.length <= 0;
    }

    get isCFSMovement() {
        // Checking by isFCL.
        // True -> CY, False -> CFS
        return this.model.isFCL === false;
    }

    get isItineraryTooltipEmpty() {
        if (this.isCFSMovement) {
            return StringHelper.isNullOrEmpty(this.model.cfsClosingDate) && StringHelper.isNullOrEmpty(this.model.cfsWarehouseCode);
        }
        else {
            return StringHelper.isNullOrEmpty(this.model.cyClosingDate) && StringHelper.isNullOrEmpty(this.model.cyEmptyPickupTerminalCode);
        }
    }

    onAddItineraryClick() {
        this.consignmentItineraryFormMode = 'add';
        this.consignmentItineraryFormDialogOpened = true;
        this.itineraryDetails = {};
    }

    onEditItineraryClick(itinerary) {
        this.consignmentItineraryFormMode = 'edit';
        this.consignmentItineraryFormDialogOpened = true;
        this.itineraryDetails = Object.assign({}, itinerary);
    }

    onDeleteItineraryClick(itinerary) {
        if (this.itineraries.length === 1) {
            this.notification.showErrorPopup('msg.deleteLastItineraryNotification', 'label.itinerary');
            return;
        }

        const confirmDlg = this.notification.showConfirmationDialog('msg.deleteItineraryConfirmation', 'label.itinerary');
        const affiliates = this.currentUser.affiliates;
        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.service.deleteItinerary(itinerary.consignmentId, itinerary.id, affiliates).subscribe(
                        data => {
                            this.itineraries = this.itineraries.filter(el => {
                                return el.id !== itinerary.id;
                            });
                            this._gaService.emitAction('Delete Itinerary', GAEventCategory.Shipment);
                            this.notification.showSuccessPopup('msg.itineraryDeleteSuccessfullyNotification', 'label.itinerary');
                            this.getActivityTab();
                        },
                        error => {
                            this.notification.showErrorPopup('save.failureNotification', 'label.itinerary');
                        });
                }
            });
    }

    onConsignmentItineraryFormDialogClosed() {
        this.consignmentItineraryFormDialogOpened = false;
    }

    onConsignmentItineraryAdded(itinerary) {
        this.consignmentItineraryFormDialogOpened = false;
        const affiliates = this.currentUser.affiliates;
        this.service.createItinerary(itinerary.consignmentId, DateHelper.formatDate(itinerary), affiliates).subscribe(
            data => {
                this._gaService.emitAction('Add Itinerary', GAEventCategory.Shipment);
                this.notification.showSuccessPopup('save.sucessNotification', 'label.itinerary');
                const clonedItineraries = [...this.itineraries];
                clonedItineraries.push(data);
                this.itineraries = clonedItineraries;
                this.getActivityTab();
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.itinerary');
            });
    }

    onConsignmentItineraryEdited(itinerary) {
        this.consignmentItineraryFormDialogOpened = false;
        const prevItinerary = this.itineraries.find(el => el.id === itinerary.id);
        const affiliates = this.currentUser.affiliates;

        this.service.updateItinerary(prevItinerary.consignmentId, itinerary.id, DateHelper.formatDate(itinerary), affiliates).subscribe(
            data => {
                this._gaService.emitAction('Edit Itinerary', GAEventCategory.Shipment);
                this.notification.showSuccessPopup('save.sucessNotification', 'label.itinerary');
                const clonedItineraries = [...this.itineraries];
                clonedItineraries.forEach((el, i) => {
                    if (el.id === itinerary.id) {
                        clonedItineraries[i] = data;
                    }
                });
                this.itineraries = clonedItineraries;
                this.getActivityTab();
                
                this.loadInitData(false);
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.itinerary');
            });
    }

    onEditItineraryConfirmClick(): void {
        this.confirmItineraryPopupOpened = true;
        this.confirmItineraryPopupMode = ShipmentItineraryConfirmPopupMode.Update;
        this.confirmItineraryModel = {
            cyClosingDate: this.model.cyClosingDate,
            cfsClosingDate: this.model.cfsClosingDate,
            cyEmptyPickupTerminalCode: this.model.cyEmptyPickupTerminalCode,
            cyEmptyPickupTerminalDescription: this.model.cyEmptyPickupTerminalDescription,
            cfsWarehouseCode: this.model.cfsWarehouseCode,
            cfsWarehouseDescription: this.model.cfsWarehouseDescription
        }
    }

    onConfirmItineraryClick(): void {
        this.confirmItineraryModel = {
            cyClosingDate: null,
            cfsClosingDate: null,
            cyEmptyPickupTerminalCode: null,
            cyEmptyPickupTerminalDescription: null,
            cfsWarehouseCode: null,
            cfsWarehouseDescription: null
        }
        
        if (this.model.modeOfTransport !== ModeOfTransportType.Air) {
            //If CY Shipment: auto-populated The CY Closing Date of the 1st leg
            if (!this.isCFSMovement) {
                this.confirmItineraryModel.cyClosingDate = this.firstItinerary?.cyClosingDate;
            }
            else {
                this.service.getDefaultCFSClosingDate(this.model.id)
                    .pipe(
                        map((x: string) => x ? new Date(x) : null)
                    )
                    .subscribe(
                        (res: any) => {
                            if (res) {
                                this.confirmItineraryModel.cfsClosingDate = res;
                            }
                        }
                    );
            }
            // open popup to input further info
            this.confirmItineraryPopupOpened = true;
            this.confirmItineraryPopupMode = ShipmentItineraryConfirmPopupMode.Confirm;
        }
        else {
            this.onItineraryConfirm({
                skipConfirmUpdates: true,
                model: this.confirmItineraryModel
            });
        }
    }

    onItineraryConfirm(data: any) : void {
        this.confirmItineraryPopupOpened = false;
        let confirmModel = {
            ...data.model,
            skipUpdates: data.skipConfirmUpdates
        }
        confirmModel = DateHelper.formatDate(confirmModel);
        this.service.confirmItinerary(this.model.id, confirmModel).subscribe(rsp => {
            this.loadInitData(false);
            this.notification.showSuccessPopup('msg.confirmItinerarySuccessfully', 'label.itinerary');
            this._gaService.emitAction('Confirm Itinerary', GAEventCategory.Shipment);
        }, err => {
            this.notification.showErrorPopup('save.failureNotification', 'label.itinerary');
        });
    }

    onItineraryConfirmSave(data: any): void {
        this.confirmItineraryPopupOpened = false;
        let confirmModel = {...data}
        confirmModel = DateHelper.formatDate(confirmModel);
        this.service.updateConfirmItinerary(this.model.id, confirmModel).subscribe(rsp => {
            this.loadInitData(false);
            this.notification.showSuccessPopup('save.sucessNotification', 'label.itinerary');
            this._gaService.emitAction('Update Confirm Itinerary', GAEventCategory.Shipment);
        }, err => {
            this.notification.showErrorPopup('save.failureNotification', 'label.itinerary');
        });
    }

    // Activity
    onAddActivityClick() {
        this.activityFormMode = 'add';
        this.activityFormOpened = true;
        this.heightActivity = 530;
        this.activityDetails = {
            eventName: null,
            activityDate: new Date()
        };
    }

    isExceptionEventType(activityType) {
        return !StringHelper.isNullOrEmpty(activityType) && activityType[activityType.length - 1] === 'E';
    }

    onEditActivityClick(activity) {
        this.activityFormMode = 'edit';
        this.activityFormOpened = true;
        this.activityDetails = Object.assign({}, activity);
        this.activityDetails.eventName = activity.activityCode + ' - ' + activity.activityDescription;
        this.activityDetails.activityTypeDescription = this.allEventOptions.find(x => x.activityCode === activity.activityCode).activityTypeDescription;
        if (this.isExceptionEventType(activity.activityType)) {
            this.heightActivity = 710;
        } else {
            this.heightActivity = 530;
        }
    }

    onDeleteActivityClick(activityId) {
        const confirmDlg = this.notification.showConfirmationDialog('msg.deleteActivityConfirm', 'label.activity');

        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.service.deleteActivity(this.model.id, activityId).subscribe(
                        data => {
                            this.getActivityTab();
                            this.notification.showSuccessPopup('msg.deleteActivitySuccessfully', 'label.activity');
                            this._gaService.emitAction('Delete Activity', GAEventCategory.Shipment);
                        },
                        error => {
                            this.notification.showErrorPopup('save.failureNotification', 'label.activity');
                        });
                }
            });
    }

    openActivityPopup(activity) {
        this.onEditActivityClick(activity);
        this.activityFormMode = 'view';
    }

    onActivityAdded(activity) {
        this.activityFormOpened = false;
        activity.shipmentId = this.model.id;
        activity.createdBy = this.currentUser.username;
        this.service.createActivity(this.model.id, DateHelper.formatDate(activity)).subscribe(
            data => {
                this.notification.showSuccessPopup('save.activityAddedNotification', 'label.activity');
                this.getActivityTab();
                this._gaService.emitAction('Add Activity', GAEventCategory.Shipment);
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.activity');
            });
    }

    onActivityEdited(activity) {
        this.activityFormOpened = false;
        activity.updatedBy = this.currentUser.username;
        this.service.updateActivity(this.model.id, activity.id, DateHelper.formatDate(activity)).subscribe(
            data => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.activity');
                this.getActivityTab();
                this._gaService.emitAction('Edit Activity', GAEventCategory.Shipment);
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.activity');
            });
    }

    onActivityFormClosed() {
        this.activityFormOpened = false;
    }

    customerFormClosedHandler() {
        this.customerFormOpened = false;
    }

    onConfirmItineraryPopupClosed(): void {
        this.confirmItineraryPopupOpened = false;
    }

    onOpenCargoTab() {
        if (!this.isCanClickContainer) {
            return;
        }

        this.allTabs.selectTab(ShipmentTab.CargoDetails);
    }

    onCancelShipmentClick() {
        this.isReadyForCancelShipment = false;
        this.service.trialValidateOnCancelShipment(this.model.id).subscribe(
            success => {
                this.isReadyForCancelShipment = true;
                const confirmDlg = this.notification.showConfirmationDialog('msg.cancelShipment', 'label.shipment');
                confirmDlg.result.subscribe(
                    (result: any) => {
                        if (result.value) {
                            this._cancelShipment();
                        }
                    });
            },
            err => {
                // #region handle business cases based on the error message.
                // must be integrated with the server
                if (err.error) {
                    const errMessage = err.error.errors;
                    if (!StringHelper.isNullOrEmpty(errMessage)) {
                        const errHashTags = errMessage.split('#').filter(x => !StringHelper.isNullOrEmpty(x));
                        switch (errHashTags[0]) {
                            case 'shipmentAlreadyLoaded':
                                this.notification.showInfoDialog('msg.cannotCancelLoadedShipment', 'label.shipment');
                                break;
                            case 'shipmentHasBLLinked':
                                this.notification.showInfoDialog('msg.cannotCancelShipmentHasBLLinked', 'label.shipment');
                                break;
                            default:
                                break;
                        }
                        this.isReadyForCancelShipment = true;
                        return;
                    }
                }
                this.isReadyForCancelShipment = true;
            }
        );
    }

    private _cancelShipment() {
        this.isReadyForCancelShipment = false;
        this.service.cancelShipment(this.model.id).subscribe(
            rsp => {
                this.isReadyForCancelShipment = true;
                this.notification.showSuccessPopup(
                    'confirmation.cancelSuccessfully',
                    'label.shipment'
                );
                this._gaService.emitEvent('cancel', GAEventCategory.Shipment, 'Cancel');
                this.ngOnInit();
            },
            err => {
                this.isReadyForCancelShipment = true;
                this.notification.showErrorPopup(
                    'save.failureNotification',
                    'label.shipment'
                );
            }
        );
    }

    get isHiddenBtnConsolidate(): boolean {
        if (this.model.isFCL) {
            return true;
        }
        if (this.isFullLoadShipment) {
            return true;
        }
        if (this.model.modeOfTransport === ModeOfTransportType.Air) {
            return true;
        }

        return StringHelper.caseIgnoredCompare(this.model.status, ShipmentStatus.Inactive);
    }

    onConsolidateBtnClick() {
        // Not allow to consolidate when Booking is not confirmed yet.
        if (!this.model.isItineraryConfirmed) {
            this.notification.showInfoDialog(
                'msg.confirmItineraryBeforeConsolidating',
                'label.shipment'
            );
            return;
        }

        // Filter on mode of transport Sea/Air
        let seaAirConsignments = Object.assign([], this.consignment || []);
        seaAirConsignments = seaAirConsignments?.filter(c => StringHelper.caseIgnoredCompare(c.modeOfTransport, ModeOfTransportType.Sea) || StringHelper.caseIgnoredCompare(c.modeOfTransport, ModeOfTransportType.Air));

        if (!this.currentUser.isInternal) {
           const consignmentId = seaAirConsignments.find(x => x.executionAgentId === this.currentUser.organizationId)?.id;
            if (!consignmentId) {
                return;
            }
            this.router.navigate(['/consolidations/add/0'], { queryParams: {'selectedconsignment': consignmentId }});
        } else {
            if (seaAirConsignments.length > 1) {
                this.consolidationPopupOpened = true;
            } else {
                this.router.navigate(['/consolidations/add/0'], { queryParams: {'selectedconsignment': seaAirConsignments[0].id }});
            }
        }
    }

    onConsolidationPopupClosed() {
        this.consolidationPopupOpened = false;
    }

    onCreateConsolidation(consignmentId) {
        this.router.navigate(['/consolidations/add/0'], { queryParams: {'selectedconsignment': consignmentId }});
    }

    get isDisableAddNewConsignment() {
        return !this.consignmentFormComponent?.isAllowAddConsignment ?? false;
    }

    /**
     * To render sub-menu for "More actions" button
     * @param dataItem Data of each cruise order item row
     * @returns Array of menu options
     */
     getBLMoreActionMenu(): Array<any> {
        const result = [];
        if (this.currentUser?.permissions?.some(c => c.name === AppPermissions.BillOfLading_HouseBLDetail_Add)) {
            result.push({
                actionName: this.translateService.instant('label.houseBill'),
                click: () => {
                    if (!this.currentUser.isInternal) {
                        const affiliates = JSON.parse(this.currentUser.affiliates);
                        // Filter on mode of transport Sea/Air
                        let seaAirConsignments = Object.assign([], this.consignment || []);
                        seaAirConsignments = seaAirConsignments?.filter(c => StringHelper.caseIgnoredCompare(c.modeOfTransport, ModeOfTransportType.Sea) || StringHelper.caseIgnoredCompare(c.modeOfTransport, ModeOfTransportType.Air));
                        const isUserHasConsignment = seaAirConsignments.some(c => affiliates.some(x => x === c.executionAgentId));
                         if (!isUserHasConsignment) {
                            this.notification.showInfoDialog('msg.createConsignmentHouseBL', 'label.shipment');
                            return;
                        }
                    }

                    this.service.trialValidateOnAssignHouseBL(this.model.id).subscribe(
                        (res) => {
                            this.isOpenHouseBLPopup = true;
                        },
                        (err) => {
                            // #region handle business cases based on the error message.
                            // must be integrated with the server
                            if (err.error) {
                                const errMessage = err.error.errors;
                                if (!StringHelper.isNullOrEmpty(errMessage)) {
                                    const errHashTags = errMessage.split('#').filter(x => !StringHelper.isNullOrEmpty(x));
                                    switch (errHashTags[0]) {
                                        case 'unconfirmedContainer':
                                            this.notification.showInfoDialog('msg.confirmContainerHouseBL', 'label.shipment');
                                            break;
                                        case 'unconfirmedConsolidation':
                                            this.notification.showInfoDialog('msg.confirmConsolidationHouseBL', 'label.shipment');
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            return;
                        }
                    );
                }
            });
        }

        if (this.currentUser?.permissions?.some(c => c.name === AppPermissions.BillOfLading_MasterBLDetail_Add)) {
            result.push( {
                actionName:  this.translateService.instant('label.carrierBill'),
                click: () => {
                    this.onBtnAddMasterBLClick();
                }
            });
        }

        return result;
    }

    onClickHAWBBtn(){
        this.isOpenHouseBLPopup = true;
    }

    houseBLPopupClosed() {
        this.isOpenHouseBLPopup = false;
    }

    houseBLPopupSavedSuccessfully() {
        this._gaService.emitAction('Assign House BL', GAEventCategory.Shipment);
        this.isOpenHouseBLPopup = false;
        this.ngOnInit();
    }

    get standardizationShipmentStatus() {
        return StringHelper.toUpperCaseFirstLetter(this.model.status);
    }

    ngAfterViewChecked(): void {
        this.cdr.detectChanges();
        super.ngAfterViewChecked();
    }

    /* Handler for Add/Assign Master BOL */
    onBtnAddMasterBLClick() {
        // check on itineraries
        let itineraries = Object.assign([], this.itineraries || []);
        // Filter on mode of transport = Sea
        itineraries = itineraries.filter(x => StringHelper.caseIgnoredCompare(x.modeOfTransport, this.modeOfTransportType.Sea));
        if (StringHelper.isNullOrEmpty(itineraries) || itineraries.length === 0) {
            this.notification.showInfoDialog('msg.pleaseScheduleTheShipmentItinerary', 'label.shipment');
            return;
        }

        // Check on execution agents
        let executionAgents = Object.assign([], this.consignment || []);
        // Filter for Mode of transport Sea/Air
        executionAgents = executionAgents?.filter(x => StringHelper.caseIgnoredCompare(x.modeOfTransport, ModeOfTransportType.Sea) || StringHelper.caseIgnoredCompare(x.modeOfTransport, ModeOfTransportType.Air));
        if (this.currentUser.isInternal) {
            // no option if internal, show the message dialog
            if (executionAgents.length === 0) {
                this.notification.showInfoDialog('msg.pleaseCreateConsignment', 'label.shipment');
                return;
            }
        } else {
            // no option matched with current organization id, show the message dialog
            if (!executionAgents.some(x => x.executionAgentId === this.currentUser.organizationId)) {
                this.notification.showInfoDialog('msg.pleaseCreateConsignment', 'label.shipment');
                return;
            }
        }

        this.masterBLPopupMetaData = {
            isFormOpened : true,
            itineraryDataSource : itineraries,
            shipmentId: this.model.id,
            executionAgentDataSource: executionAgents,
            modeOfTransport: this.model.modeOfTransport
        };
    }

    onMasterBOLPopupClosed(masterBLId?: number) {
        this.masterBLPopupMetaData = {...this.masterBLPopupMetaData, isFormOpened : false};
        // call request to assign master bl to current house bl
        if (!StringHelper.isNullOrEmpty(masterBLId) && masterBLId > 0) {
            this.service.assignMasterBillOfLading(this.model.id, masterBLId).subscribe(
                data => {
                    this._gaService.emitAction('Assign Master BL', GAEventCategory.Shipment);
                    this.notification.showSuccessPopup('save.sucessNotification', 'label.shipment');
                    this.ngOnInit();
                },
                error => {
                    this.notification.showErrorPopup('save.failureNotification', 'label.shipment');
                }
            );
        }
    }

    /* End Handler for Add/Assign Master BOL */


    /* Handler for Unlink/Resign Master BOL */
    get isHiddenBtnUnlinkMaster(): boolean {
        const isMasterBLAssigned = this.consignment?.some(x => !StringHelper.isNullOrEmpty(x.masterBillId) && StringHelper.isNullOrEmpty(x.houseBillId)) || false;
        const isShipmentInactive = this.model.status.toLowerCase() === 'inactive';
        const isSeaOrAir = ([this.modeOfTransportType.Sea.toLowerCase(), this.modeOfTransportType.Air.toLowerCase()]).indexOf(this.model.modeOfTransport.toLowerCase()) > -1;

        const result = isMasterBLAssigned && !isShipmentInactive && isSeaOrAir;
        return !result;
    }

    onBtnUnlinkMasterClick(): void {
        const confirmDlg = this.notification.showConfirmationDialog('confirmation.unlinkMaster', 'label.shipment');

        confirmDlg.result.subscribe(
        (result: any) => {
            if (result.value) {
               this.service.unlinkMasterBillOfLading(this.model.id).subscribe(
                    data => {
                        this.notification.showSuccessPopup('save.sucessNotification', 'label.shipment');
                        this.ngOnInit();
                    },
                    error => {
                        this.notification.showErrorPopup('save.failureNotification', 'label.shipment');
                    }
            );
            }
        });
    }

    public canDeleteItinerary(itineraryId: number): boolean {
        const currentItinerary = this.itineraries.find(el => el.id === itineraryId);

        /* Not allow to delete itinerary if it is imported from API
        */
        if (currentItinerary.isImportFromApi) {
            return false;
        }

        /* Not allow to delete Itinerary if Shipment is already linked to Master BL.
        */
        if (this.model.masterBillNos?.length > 0) {
            return false;
        }

        const otherSequences = this.itineraries?.filter(i =>
            i.id !== itineraryId && i.consignmentId === currentItinerary.consignmentId)?.map(i => i.sequence) ?? [];

        return !otherSequences.some(el => el > currentItinerary.sequence);
    }

    /* End Handler for Unlink/Resign Master BOL*/

    seeMoreCargoDescription(description: string) {
        this.openCargoDescriptionDetailPopup = true;
        this.cargoDescriptionDetail = description;
    }
    onCargoDescriptionDetailPopupClosed() {
        this.openCargoDescriptionDetailPopup = false;
    }
}
