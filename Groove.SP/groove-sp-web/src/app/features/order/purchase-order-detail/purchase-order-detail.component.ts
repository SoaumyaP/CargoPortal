import { Component, ElementRef, HostListener, OnInit, ViewChild } from '@angular/core';
import { PurchaseOrderDetailService } from './purchase-order-detail.service';
import { Router, ActivatedRoute } from '@angular/router';
import { DATE_FORMAT, FormComponent, UserContextService, StringHelper, DateHelper, DropdownListModel, DropDownListItemModel } from 'src/app/core';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { PurchaseOrderStatus, POStageType, Roles, POFulfillmentStageType, POType, BuyerComplianceServiceType, OrderFulfillmentPolicy, ViewSettingModuleIdType, FormModeType } from 'src/app/core/models/enums/enums';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { faMapMarkerAlt, faBell, faCheck, faExchangeAlt, faPlus, faPencilAlt, faEllipsisV, faTrashAlt, faExclamationCircle, faCheckCircle, faBan } from '@fortawesome/free-solid-svg-icons';
import moment from 'moment';
import { OrganizationNameRole } from 'src/app/core/models/enums/enums';
import { OrderNoteModel } from '../models/order-note.model';
import { forkJoin, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { DefaultValue2Hyphens, EVENT_7001, EVENT_7002, GAEventCategory, MilestoneEventCode } from 'src/app/core/models/constants/app-constants';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { ArrayHelper } from 'src/app/core/helpers/array.helper';
import { FilterActivityModel } from 'src/app/core/models/activity/filter-activity.model';
import { PurchaseOrderTabModel } from '../models/purchase-order-tab.model';
import { ViewSettingModel } from 'src/app/core/models/viewSetting.model';
@Component({
    selector: 'app-purchase-order-detail',
    templateUrl: './purchase-order-detail.component.html',
    styleUrls: ['./purchase-order-detail.component.scss']
})
export class PurchaseOrderDetailComponent extends FormComponent implements OnInit {
    @ViewChild('headerBar', { static: false }) headerBarElement: ElementRef;
    @ViewChild('stickyBar', { static: false }) stickyBarElement: ElementRef;
    @ViewChild('sectionContainer', { static: false }) sectionContainerElement: ElementRef;
    @ViewChild('general', { static: false }) generalElement: ElementRef;
    @ViewChild('product', { static: false }) productElement: ElementRef;
    @ViewChild('termsAndInstructions', { static: false }) termsAndInstructionsElement: ElementRef;
    @ViewChild('contact', { static: false }) contactElement: ElementRef;
    @ViewChild('fulfillment', { static: false }) fulfillmentElement: ElementRef;
    @ViewChild('allocatedPO', { static: false }) allocatedPOElement: ElementRef;
    @ViewChild('activity', { static: false }) activityElement: ElementRef;
    @ViewChild('dialog', { static: false }) dialogElement: ElementRef;

    private isManualScroll: boolean = true;

    modelName = 'purchaseOrders';
    viewSettingModuleId = 'PO.Detail';
    defaultValue: string = DefaultValue2Hyphens;
    readonly milestoneEventCode = MilestoneEventCode;
    ViewSettingModuleIdType = ViewSettingModuleIdType;
    formModeType = FormModeType;
    tabs: Array<PurchaseOrderTabModel> = [];
    isReloadMode: boolean = false;
    purchaseOrderStatus = PurchaseOrderStatus;
    poFulfillmentStageType = POFulfillmentStageType;
    buyerComplianceServiceType = BuyerComplianceServiceType;
    orderFulfillmentPolicy = OrderFulfillmentPolicy;
    poType = POType;
    poStageType = POStageType;
    faExclamationCircle = faExclamationCircle;
    faCheckCircle = faCheckCircle;
    faBan = faBan;
    DATE_FORMAT = DATE_FORMAT;
    carrierName: string;
    gatewayName: string;
    poStages = [
        {
            stage: POStageType.Released,
            title: 'label.released',
            class: 'n-released',
            active: false,
            current: false,
            serviceTypes: [BuyerComplianceServiceType.Freight, BuyerComplianceServiceType.WareHouse]
        },
        {
            stage: POStageType.ForwarderBookingRequest,
            title: 'label.forwarderBookingRequest',
            class: 'n-forwarderBookingRequest',
            active: false,
            current: false,
            serviceTypes: [BuyerComplianceServiceType.Freight, BuyerComplianceServiceType.WareHouse]
        },
        {
            stage: POStageType.ForwarderBookingConfirmed,
            title: 'label.forwarderBookingConfirmed',
            class: 'n-forwarderBookingConfirmed',
            active: false,
            current: false,
            serviceTypes: [BuyerComplianceServiceType.Freight, BuyerComplianceServiceType.WareHouse]
        },
        {
            stage: POStageType.CargoReceived,
            title: 'label.cargoReceived',
            class: 'n-closed',
            active: false,
            current: false,
            serviceTypes: [BuyerComplianceServiceType.WareHouse]
        },
        {
            stage: POStageType.ShipmentDispatch,
            title: 'label.shipmentDispatch',
            class: 'n-shipmentDispatch',
            active: false,
            current: false,
            serviceTypes: [BuyerComplianceServiceType.Freight]
        },
        {
            stage: POStageType.Closed,
            title: 'label.closed',
            class: 'n-closed',
            active: false,
            current: false,
            serviceTypes: [BuyerComplianceServiceType.Freight]
        }
    ];

    filterActivityByOptions: DropdownListModel<string>[] = [];

    filterActivityModel: FilterActivityModel = new FilterActivityModel();
    filterActivityValueDataSource$: Observable<DropDownListItemModel<string>[]>;

    readonly AppPermissions = AppPermissions;
    currentUser: any;

    productPopupOpened = false;
    productDetailModel = null;
    activityModel = null;

    faMapMarkerAlt = faMapMarkerAlt;
    faBell = faBell;
    faExchangeAlt = faExchangeAlt;
    faCheck = faCheck;
    faPlus = faPlus;
    faPencilAlt = faPencilAlt;
    faTrashAlt = faTrashAlt;
    faEllipsisV = faEllipsisV;

    defaultActivityPageSize = 500;
    isActivitySorting: boolean;
    isActivityFiltering: boolean;
    activityPageSetting = {
        pageSize: this.defaultActivityPageSize,
        pageIndex: 0,
        ascending: false,
        filterEventDate: '0001/1/1'
    };
    activityTotal = 0;
    showActivityLoadMoreButton: boolean = true;
    loadedActivityTotal = 0;
    showActivityActionButton: boolean = true;
    showActivityList: boolean = false;
    showActivityNoRecordMsg: boolean = false;
    groupActivityModel = null;
    affiliateIds: number[];
    isHideFulfillmentButton: boolean = true;
    articleMasterData = {};
    activityFormOpened: boolean;
    heightActivity = 530;
    activityFormMode = 'view';
    activityDetails: any = {};
    allEventOptions: any;
    noteList: OrderNoteModel[];
    POStageType = POStageType;
    isCanceling: boolean;
    defaultMilestone: any = [];
    milestoneStatus: any = {
        isExistingBooked: false,
        isExistingBookingConfirmed: false,
        isExistingShipmentDispatch: false,
        isExistingCargoReceived: false,
        isExistingClosed: false
    };
    isCanClickVessel: boolean;
    isCanClickContainerNo: boolean;
    isCanClickShipmentNo: boolean;
    isCanClickBookingNo: boolean;

    constructor(protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: PurchaseOrderDetailService,
        public translateService: TranslateService,
        private _userContext: UserContextService,
        private _gaService: GoogleAnalyticsService) {
        super(route, service, notification, translateService, router);
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
                if (!user.isInternal) {
                    this.service.affiliateCodes = user.affiliates;
                    this.affiliateIds = JSON.parse(user.affiliates);
                }
                this.isCanClickVessel = user.permissions?.some(c => c.name === AppPermissions.FreightScheduler_List);
                this.isCanClickContainerNo = user.permissions?.some(c => c.name === AppPermissions.Shipment_ContainerDetail);
                this.isCanClickShipmentNo = user.permissions?.some(c => c.name === AppPermissions.Shipment_Detail);
                this.isCanClickBookingNo = user.permissions?.some(c => c.name === AppPermissions.PO_Fulfillment_Detail);
            }
        });
        // Fetch list of events for the PO Activity
        this.service.getEvents().subscribe(data => {
            this.allEventOptions = data.filter(c => c.activityCode !== '1005');
        });
    }

    async ngOnInit() {
        if (this.currentUser.role.id === Roles.Shipper) {
            this.service.customerRelationships = await this._userContext.getCustomerRelationships(this.currentUser).toPromise();
            this.service.organizationId = this.currentUser.organizationId;
        }
        super.ngOnInit();
    }

    returnText(field) {
        return this.model[field] ? this.model[field] : '--';
    }

    async onInitDataLoaded(data) {
        this.tabs = this.service.createNavigation(this.model);
        setTimeout(() => {
            this.initTabLink();
        }, 500); // make sure UI has been rendered

        if (this.model.stage >= POStageType.Draft) {
            this.setCurrentStage(this.model.customerServiceType);
            await this.getActivities();
            this.checkToShowActivityList();
            this.checkToShowLoadMoreButton();
            this.checkToShowActionButton();
            this.checkToShowPOFulfillmentButton();
        }

        if (!StringHelper.isNullOrEmpty(this.model.carrierCode)) {
            this.service.getCarrier(this.model.carrierCode).subscribe(
                carrier => {
                    this.carrierName = carrier && carrier.name;
                });
        }

        if (!StringHelper.isNullOrEmpty(this.model.gatewayCode)) {
            this.service.getGateway(this.model.gatewayCode).subscribe(
                port => {
                    this.gatewayName = port && port.name;
                });
        }

        this._userContext.isGranted(AppPermissions.PO_Delegation).subscribe(isAllowed => {
            if (!isAllowed && this.currentUser?.role?.id !== Roles.Factory) {
                this.model.contacts = this.model.contacts.filter(x => x.organizationRole !== OrganizationNameRole.Delegation);
            }
        });

        this.bindingNoteTab();
    }

    setCurrentStage(customerServiceType?: BuyerComplianceServiceType | null) {
        if (customerServiceType) {
            this.poStages = this.poStages.filter(x => x.serviceTypes.includes(customerServiceType))
        }
        for (const item of this.poStages) {
            item.active = item.stage <= this.model.stage;
            item.current = item.stage === this.model.stage;
        }
    }

    productPopupClosedHandler() {
        this.productPopupOpened = false;
    }

    openProductPopup(dataItem) {
        this.productDetailModel = dataItem;
        this.productPopupOpened = true;
        if (!this.articleMasterData[this.productDetailModel.productCode]) {
            this.service.getInformationFromArticleMaster(this.modelId, this.productDetailModel.productCode).subscribe(
                (rs) => {
                    this.articleMasterData[this.productDetailModel.productCode] = rs;
                    this.productDetailModel.outerQuantity = rs.outerQuantity;
                    this.productDetailModel.innerQuantity = rs.innerQuantity;
                    this.productDetailModel.styleName = rs.styleName,
                        this.productDetailModel.colourName = rs.colourName
                }
            );
        }
    }

    backList() {
        const origin = this.route.snapshot.queryParamMap.get('origin');
        if (!StringHelper.isNullOrEmpty(origin) && origin === 'shipped-purchase-orders') {
            this.router.navigate(['/shipped-purchase-orders']);
        } else {
            this.router.navigate(['/purchase-orders']);
        }
    }

    disableBtnPODelegation() {
        if (this.model.stage !== POStageType.Released) {
            return true;
        }

        // All product has no Booked Qty (PO is not booked yet)
        if (this.model.lineItems != null) {
            for (let index = 0; index < this.model.lineItems.length; index++) {
                const item = this.model.lineItems[index];
                if (item.bookedUnitQty > 0) {
                    return true;
                }
            }
        }

        return false;
    }

    isProductBalanceEqualZero() {
        if (this.model.lineItems != null) {
            for (let i = 0; i < this.model.lineItems.length; i++) {
                const item = this.model.lineItems[i];
                if (item.balanceUnitQty > 0) {
                    return false;
                }
            }
        }
        return true;
    }

    hiddenBtnPODelegation() {

        if (this.model.poType === POType.Allocated) {
            return true;
        }

        if (this.model.status === PurchaseOrderStatus.Cancel) {
            return true;
        }

        if (this.currentUser.isInternal) {
            return false;
        }

        const supplierOrganization = this.model.contacts.find(x => x.organizationRole === OrganizationNameRole.Supplier && this.affiliateIds.indexOf(x.organizationId) !== -1);
        if (supplierOrganization) {
            return false;
        }

        return true;
    }

    hiddenBtnFulfillment() {
        if (this.model.poType !== POType.Bulk
            && this.model.poType !== this.model.allowToBookIn) {
            return true;
        }

        if (this.isHideFulfillmentButton) {
            return true;
        }

        if (this.model.customerServiceType === BuyerComplianceServiceType.WareHouse) {
            return true;
        }

        return false;
    }

    isHiddenProgressCheck() {
        if (this.model.stage > POStageType.Released) {
            return true;
        }

        if (this.model.isProgressCargoReadyDates && !this.model.productionStarted) {
            return false;
        } else {
            return true;
        }
    }

    isHiddenByProgressCheck() {
        if (!this.model.isProgressCargoReadyDates) {
            return false;
        }

        if (this.model.isProgressCargoReadyDates && this.model.isCompulsory && !this.model.productionStarted) {
            return true;
        } else {
            return false;
        }
    }

    onPODelegationModalClosed(isDone: boolean) {
        if (isDone) {
            this.loadInitData();
            this.notification.showSuccessPopup('msg.delegatePOSuccessfully', 'label.poDelegation');
            this._gaService.emitEvent('delegate', GAEventCategory.PurchaseOrder, 'Delegate');
        }
    }

    onBtnFulfillmentClick() {
        if (this.model.customerServiceType === BuyerComplianceServiceType.WareHouse) {
            this.router.navigate(['/warehouse-bookings/add/0'], { queryParams: { selectedpos: this.modelId } });
        } else {
            if (this.model.isAllowMissingPO) {
                const customer = this.model.contacts.find(x => x.organizationRole === OrganizationNameRole.Principal);
                const customerId = customer && customer.organizationId;
                this.router.navigate(['/missing-po-fulfillments/add/0'], {
                    queryParams: {
                        selectedpos:
                        this.modelId,
                        selectedCustomer: customerId,
                        formType: FormModeType.Add
                    }
                });
            }
            else {
                this.router.navigate(['/po-fulfillments/add/0'], {
                    queryParams: {
                        selectedpos: this.modelId,
                        formType: FormModeType.Add
                    }
                });
            }

        }
    }

    checkToShowPOFulfillmentButton() {
        const customer = this.model.contacts.find(x => x.organizationRole === OrganizationNameRole.Principal);
        const customerId = customer && customer.organizationId;

        this.service.getOrganization(customerId).subscribe(data => {
            if (data.isBuyer) {
                if (this.currentUser.isInternal && this.model.status !== PurchaseOrderStatus.Cancel) {
                    this.isHideFulfillmentButton = false;
                } else {
                    const delegationOrganization = this.model.contacts.find(x => x.organizationRole ===
                        OrganizationNameRole.Delegation &&
                        x.organizationId === this.currentUser.organizationId);
                    const supplierOrganization = this.model.contacts.find(x => x.organizationRole === OrganizationNameRole.Supplier &&
                        this.affiliateIds && this.affiliateIds.indexOf(x.organizationId) !== -1);
                    const agentOrganization = this.model.contacts.find(x => this.affiliateIds?.indexOf(x.organizationId) !== -1);
                    if ((delegationOrganization || supplierOrganization || agentOrganization) && this.model.status !== PurchaseOrderStatus.Cancel) {
                        this.isHideFulfillmentButton = false;
                    }
                }
            }
        });
    }

    async getActivities() {
        const rs = await this.service.getActivity(this.model.id, {}).toPromise();
        this.activityTotal = rs.result.totalRecord;
        this.activityModel = rs.result.globalIdActivities;

        this.filterActivityByOptions = this.service.getActivityOptionFilter(this.model.customerServiceType);
        this.mappingMilestone();

        this.groupActivity();

        this.groupActivityModel.unshift({
            activities: [{
                activity: { activityCode: 'Released', milestone: 'label.released', matchFilter: true }
            }],
            dateGroup: moment(new Date(this.model.createdDate.toDateString())),
            matchFilter: true
        });

        ArrayHelper.sortBy(this.groupActivityModel, 'dateGroup', 'desc');
    }

    mappingMilestone() {
        this.milestoneStatus.isExistingBooked = false;
        this.milestoneStatus.isExistingBookingConfirmed = false;
        this.milestoneStatus.isExistingShipmentDispatch = false;
        this.milestoneStatus.isExistingCargoReceived = false;
        this.milestoneStatus.isExistingClosed = false;

        this.defaultMilestone = this.service.getDefaultMilestone(this.model.customerServiceType, this.model.modeOfTransport);
        const activityCodes = this.activityModel.map(s => s.activity.activityCode);

        const has7003 = activityCodes.some(x => x === '7003'); // Air
        if (has7003) {
            this.defaultMilestone = this.defaultMilestone.filter(c => c.activityCode !== '7001');
        }
        else {
            this.defaultMilestone = this.defaultMilestone.filter(c => c.activityCode !== '7003');
        }

        this.defaultMilestone = this.defaultMilestone.filter(c => !activityCodes.some(a => a === c.activityCode));

        for (const item of this.activityModel.reverse()) {
            switch (item.activity?.activityCode) {
                case '1051':
                    if (!this.milestoneStatus.isExistingBooked) {
                        item.activity.milestone = 'label.forwarderBookingRequest';
                        this.milestoneStatus.isExistingBooked = true;
                    }
                    break;

                case '1061':
                    if (!this.milestoneStatus.isExistingBookingConfirmed) {
                        item.activity.milestone = 'label.forwarderBookingConfirmed';
                        this.milestoneStatus.isExistingBookingConfirmed = true;
                    }

                    break;
                case '7001': case '7003':
                    if (!this.milestoneStatus.isExistingShipmentDispatch) {
                        item.activity.milestone = 'label.shipmentDispatch';
                        this.milestoneStatus.isExistingShipmentDispatch = true;
                    }
                    break;

                case '1063':
                    if (!this.milestoneStatus.isExistingCargoReceived) {
                        item.activity.milestone = 'label.cargoReceived';
                        this.milestoneStatus.isExistingCargoReceived = true;
                    }
                    break;

                case '1010':
                    if (!this.milestoneStatus.isExistingClosed) {
                        item.activity.milestone = 'label.closed';
                        this.milestoneStatus.isExistingClosed = true;
                    }

                    break;
                default:
                    break;
            }

            this.convertStringToArray('shipmentNos', item);
            this.convertStringToArray('poFulfillmentNos', item);
            this.convertStringToArray('containerNos', item);
            this.convertStringToArray('vesselFlight', item);
        }

        this.activityModel.reverse();
    }

    convertStringToArray(propertyName: string, obj: any) {
        if (obj[propertyName]) {
            obj[`${propertyName}Array`] = [];
            var textArray = obj[propertyName].split(';');
            for (let text of textArray) {
                const info = text.split('~');
                let newObj = {
                    text: info[0],
                    value: info[1]
                }
                if (propertyName === 'poFulfillmentNos') {
                    newObj['stage'] = Number.parseInt(info[2]);
                    newObj['orderFulfillmentPolicy'] = Number.parseInt(info[3]);
                }
                obj[`${propertyName}Array`].push(newObj);
            }
        }
    }

    showAllShipmentNo(dataItem) {
        dataItem.isShowAllShipmentNo = true;
    }

    showAllContainerNo(dataItem) {
        dataItem.isShowAllContainerNo = true;
    }

    showAllBookingNo(dataItem) {
        dataItem.isShowAllBookingNo = true;
    }

    showAllVessel(dataItem) {
        dataItem.isShowAllVessel = true;
    }

    async onLoadMoreClick() {
        this.activityPageSetting.pageIndex++;
        await this.getActivities();
        this.checkToShowLoadMoreButton();
    }

    async onSortClick() {
        this.isActivitySorting = true;
        this.activityPageSetting.ascending = !this.activityPageSetting.ascending;
        if (this.activityPageSetting.pageIndex > 0) {
            this.activityPageSetting.pageSize = this.activityPageSetting.pageSize * (this.activityPageSetting.pageIndex + 1);
            this.activityPageSetting.pageIndex = 0;
        }

        await this.getActivities();
        this.checkToShowLoadMoreButton();

        this.isActivitySorting = false;
    }

    checkToShowLoadMoreButton() {
        const itemCount = this.activityModel.length;

        if (this.activityTotal && itemCount < this.activityTotal && this.activityTotal > 0 && itemCount > 0) {
            this.showActivityLoadMoreButton = true;
        } else {
            this.showActivityLoadMoreButton = false;
        }
    }

    onFromDateFilterChange(value): void {
        if (value) {
            this.filterActivityModel.filterFromDate = `${value.getFullYear()}/${value.getMonth() + 1}/${value.getDate()}`;
        } else {
            this.filterActivityModel.filterFromDate = '0001/1/1';
        }
    }
    onToDateFilterChange(value): void {
        if (value) {
            this.filterActivityModel.filterToDate = `${value.getFullYear()}/${value.getMonth() + 1}/${value.getDate()}`;
        } else {
            this.filterActivityModel.filterToDate = '0001/1/1';
        }
    }

    //#region Activity handlers
    onAddActivityClick() {
        this.activityFormMode = 'add';
        this.activityFormOpened = true;
        this.heightActivity = 530;
        this.activityDetails = {
            eventName: null,
            activityDate: new Date()
        };
    }

    onEditActivityClick(activity) {
        this.activityFormMode = 'edit';
        this.activityFormOpened = true;
        this.activityDetails = Object.assign({}, activity);
        this.activityDetails.eventName = activity.activityCode + ' - ' + activity.activityDescription;
        this.activityDetails.activityTypeDescription = this.allEventOptions.find(x => x.activityCode == activity.activityCode).activityTypeDescription;
        if (this.isExceptionEventType(activity.activityType)) {
            this.heightActivity = 710;
        } else {
            this.heightActivity = 530;
        }
    }

    onActivityFormClosed() {
        this.activityFormOpened = false;
    }

    onActivityAdded(activity) {
        this.activityFormOpened = false;
        activity.purchaseOrderId = this.model.id;
        activity.createdBy = this.currentUser.username;
        this.service.createActivity(this.model.id, DateHelper.formatDate(activity)).subscribe(
            data => {
                this.resetActivityList();
                this.notification.showSuccessPopup('save.activityAddedNotification', 'label.activity');
                this._gaService.emitEvent('add_activity', GAEventCategory.PurchaseOrder, 'Add Activity');
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
                this.resetActivityList();
                this.notification.showSuccessPopup('save.sucessNotification', 'label.activity');
                this._gaService.emitEvent('edit_activity', GAEventCategory.PurchaseOrder, 'Edit Activity');

            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.activity');
            });
    }

    onDeleteActivityClick(activity) {
        const confirmDlg = this.notification.showConfirmationDialog('msg.deleteActivityConfirm', 'label.activity');
        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.service.deleteActivity(this.model.id, activity.id).subscribe(
                        data => {
                            switch (activity.activityCode) {
                                case EVENT_7002:
                                    if (this.model.stage == POStageType.Closed) {
                                        this.model.stage = POStageType.ShipmentDispatch;
                                    }
                                    break;
                                case EVENT_7001:
                                    if (this.model.stage == POStageType.ShipmentDispatch) {
                                        this.model.stage = POStageType.ForwarderBookingConfirmed;
                                    }
                                    break;
                                default:
                                    break;
                            }
                            this.setCurrentStage();
                            this.resetActivityList();
                            this.notification.showSuccessPopup('msg.deleteActivitySuccessfully', 'label.activity');
                            this._gaService.emitEvent('delete_activity', GAEventCategory.PurchaseOrder, 'Delete Activity');

                        },
                        error => {
                            this.notification.showErrorPopup('save.failureNotification', 'label.activity');
                        });
                }
            });
    }

    async resetActivityList() {
        this.isActivityFiltering = true;
        this.isActivitySorting = true;
        this.activityPageSetting.pageIndex = 0;

        await this.getActivities();

        this.checkToShowActivityList();
        this.checkToShowLoadMoreButton();
        this.checkToShowActionButton();

        this.isActivitySorting = false;
        this.isActivityFiltering = false;
    }

    isExceptionEventType(activityType) {
        return !StringHelper.isNullOrEmpty(activityType) && activityType[activityType.length - 1] === 'E';
    }

    isEditableActivity(activity) {
        if (!activity) {
            return false;
        }

        if (activity.actor === 'System') {
            return false;
        }

        if (!this.currentUser.isInternal && !this.isAgentUser) {
            return activity.createdBy === this.currentUser.username;
        }

        if (activity.activity.activityCode === '1005') {
            return false;
        }

        return true;
    }
    //#endregion

    checkToShowActionButton() {
        if ((!this.activityModel || this.activityModel.length <= 0) && this.activityPageSetting.filterEventDate === '0001/1/1') {
            this.showActivityActionButton = false;
        } else {
            this.showActivityActionButton = true;
        }
    }

    checkToShowActivityList() {
        if (this.activityModel && this.activityModel.length > 0) {
            this.showActivityList = true;
        } else {
            this.showActivityList = false;
        }

        this.showActivityNoRecordMsg = !this.showActivityList;
    }

    private groupActivity() {
        let curDate = moment();
        let i = 0;
        let items = [];
        const list = [];
        this.activityModel.forEach(el => {

            el.matchFilter = true;

            const activityDate = moment(el.activityDate);

            if (i === 0) {
                curDate = activityDate;
            }

            if (curDate.isSame(activityDate, 'day')) {
                items.push(el);
            } else {

                list.push({ dateGroup: curDate, activities: items, matchFilter: true });

                items = [];
                items.push(el);
            }

            curDate = activityDate;
            i++;
        });

        if (items.length > 0) {
            list.push({ dateGroup: curDate, activities: items, matchFilter: true });
        }

        this.groupActivityModel = list;
    }

    private get totalBlanketPOQty() {
        // Sum quantity ordered from current PO (blanket)
        let unitQty = 0;
        if (StringHelper.isNullOrEmpty(this.model.lineItems) || this.model.lineItems.length === 0) {
            return unitQty;
        }
        this.model.lineItems.forEach(po => {
            unitQty += po.orderedUnitQty;
        });
        return unitQty;
    }

    private totalAllocatedQty(rowIndex) {
        let unitQty = 0;
        this.model.allocatedPOs[rowIndex].lineItems.forEach(item => {
            unitQty += item.orderedUnitQty;
        });
        return unitQty;
    }

    bindingNoteTab() {
        var noteObs$ = this.service.getNotes(this.model.id)
            .pipe(
                map((res: any) => {
                    return res.map(x => {
                        const newOrderNoteModel = new OrderNoteModel();
                        newOrderNoteModel.MapFrom(x);
                        return newOrderNoteModel;
                    })
                })
            )

        var masterNote$ = this.service.getMasterNotes(this.model.id)
            .pipe(
                map((res: any) => {
                    return res.map(x => {
                        const newOrderNoteModel = new OrderNoteModel();
                        newOrderNoteModel.MapFromMasterNote(x);
                        return newOrderNoteModel;
                    })
                })
            )

        forkJoin([noteObs$, masterNote$]).subscribe(
            (note) => {
                this.noteList = note[0].concat(note[1]);
            });
    }

    progressCheck() {
        const customerId = this.model.contacts.find(x => x.organizationRole === OrganizationNameRole.Principal)?.organizationId;
        const supplierId = this.model.contacts.find(x => x.organizationRole === OrganizationNameRole.Supplier)?.organizationId;
        const cargoReadyDateFrom = moment(this.model.cargoReadyDate).format('MM-DD-YYYY').toLocaleString();
        const queryParams = {
            poNoFrom: this.model.poNumber,
            poNoTo: this.model.poNumber,
            cargoReadyDateFrom: cargoReadyDateFrom,
            cargoReadyDateTo: cargoReadyDateFrom,
            customerId: customerId,
            supplierId: supplierId
        }
        this.router.navigate(['/po-progress-check'], { queryParams });
    }

    checkPendingForProgress(customerPO) {
        var validDate = moment().add(customerPO.progressNotifyDay, 'days');
        return customerPO.cargoReadyDate <= validDate;
    }

    get isShowBookingTab() {
        if (this.model.poType === POType.Bulk) {
            return true;
        }
        return this.model.poType === this.model.allowToBookIn;
    }

    get isAgentUser() {
        return this.currentUser && this.currentUser.userRoles.find(x => x.role.id === Roles.Agent || x.role.id === Roles.CruiseAgent) != null;
    }

    onCancelPOClicked() {
        this.isCanceling = true;
        this.service.close(this.model.id, {}).subscribe(
            success => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.purchaseOrders');
                this.router.navigate([`/purchase-orders/${this.model.id}`]);
                this.resetActivityTimeline();
                super.ngOnInit();
                this.isCanceling = false;
                this._gaService.emitEvent('close', GAEventCategory.PurchaseOrder, 'Close');
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.purchaseOrders');
                this.isCanceling = false;
            }
        );
    }

    filterActivityByValueChange(value): void {
        delete this.filterActivityModel.filterValue;
        this.filterActivityValueDataSource$ = this.service.getFilterActivityValueDropdown(value, this.model?.id);
    }

    clearActivityFilter(): void {
        this.filterActivityModel = new FilterActivityModel();
        this.applyActivityFilter();
    }

    applyActivityFilter(): void {
        if (!this.groupActivityModel || this.groupActivityModel.length === 0) {
            return;
        }
        const hasFilterValue = !StringHelper.isNullOrWhiteSpace(this.filterActivityModel.filterValue);
        const hasDateFromFilter = this.filterActivityModel.filterFromDate && this.filterActivityModel.filterFromDate != '0001/1/1';
        const hasDateToFilter = this.filterActivityModel.filterToDate && this.filterActivityModel.filterToDate != '0001/1/1'

        for (let i = 0; i < this.groupActivityModel.length; i++) {
            const activities = this.groupActivityModel[i].activities;

            for (let j = 0; j < activities.length; j++) {
                let isMatch = true;

                if (hasDateFromFilter &&
                    this.groupActivityModel[i].dateGroup.isBefore(this.filterActivityModel.filterFromDate, 'day')) {
                    isMatch = false;
                }

                if (hasDateToFilter &&
                    this.groupActivityModel[i].dateGroup.isAfter(this.filterActivityModel.filterToDate, 'day')) {
                    isMatch = false;
                }

                if (!isMatch) {
                    activities[j].matchFilter = false;
                    continue;
                }

                if (hasFilterValue) {
                    switch (this.filterActivityModel.filterBy) {
                        case 'BookingNo':
                            isMatch = activities[j].poFulfillmentNosArray &&
                                activities[j].poFulfillmentNosArray.some(x => StringHelper.caseIgnoredCompare(x.text, this.filterActivityModel.filterValue));
                            break;
                        case 'ShipmentNo':
                            isMatch = activities[j].shipmentNosArray &&
                                activities[j].shipmentNosArray.some(x => StringHelper.caseIgnoredCompare(x.text, this.filterActivityModel.filterValue));
                            break;
                        case 'ContainerNo':
                            isMatch = activities[j].containerNosArray &&
                                activities[j].containerNosArray.some(x => StringHelper.caseIgnoredCompare(x.text, this.filterActivityModel.filterValue));
                            break;
                        case 'VesselName':
                            isMatch = activities[j].vesselFlightArray?.some(x => StringHelper.caseIgnoredCompare(x.text, this.filterActivityModel.filterValue)) ?? false;
                            break;
                        default:
                            break;
                    }
                }
                activities[j].matchFilter = isMatch;
            }
            this.groupActivityModel[i].matchFilter = activities.some(x => x.matchFilter || !StringHelper.isNullOrWhiteSpace(x.activity.milestone));
        }
    }

    get enableApplyActivityFilterButton(): boolean {
        return this.filterActivityModel.filterValue?.length > 0 ||
            (this.filterActivityModel.filterFromDate != null && this.filterActivityModel.filterFromDate != '0001/1/1') ||
            (this.filterActivityModel.filterToDate != null && this.filterActivityModel.filterToDate != '0001/1/1');
    }

    /**
    * Get tab details/ settings
    * @param sectionId Id of section
    * @returns
    */
    getTabDetails(sectionId: string): PurchaseOrderTabModel {
        const result = this.tabs.find(x => x.sectionId === sectionId);
        return result;
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
            if (currentYPosition + headerHeight + 110 <= element.sectionElementRef?.nativeElement?.offsetTop + element.sectionElementRef?.nativeElement?.clientHeight) {
                element.selected = true;
                break;
            }
        }
        //#endregion
    }

    onClickStickyBar(event, tab: PurchaseOrderTabModel) {
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
            window.scrollTo({ behavior: 'smooth', top: tab.sectionElementRef?.nativeElement?.offsetTop - headerHeight - 106 });
        }

        // After 1s, reset isManualScroll = true -> it scrolls to target position
        setTimeout(() => {
            this.isManualScroll = true;
        }, 1000);
    }

    /**Link UI element to tabs object
   Must make sure that it is correct order */
    private initTabLink(): void {
        this.tabs.map(tab => {
            switch (tab.sectionId) {
                case 'general':
                    tab.sectionElementRef = this.generalElement;
                    break;
                case 'product':
                    tab.sectionElementRef = this.productElement;
                    break;
                case 'contact':
                    tab.sectionElementRef = this.contactElement;
                    break;
                case 'termsAndInstructions':
                    tab.sectionElementRef = this.termsAndInstructionsElement;
                    break;
                case 'allocatedPO':
                    tab.sectionElementRef = this.allocatedPOElement;
                    break;
                case 'fulfillment':
                    tab.sectionElementRef = this.fulfillmentElement;
                    break;
                case 'activity':
                    tab.sectionElementRef = this.activityElement;
                    break;
                case 'dialog':
                    tab.sectionElementRef = this.dialogElement;
                    break;
            }
        });
    }

    isHiddenColumn(moduleId: ViewSettingModuleIdType, fieldId: string): boolean {
        let result = this.GetVisibleColumns(moduleId, fieldId);
        if (result === null) {
            return false;
        }

        return !result.some(c => StringHelper.caseIgnoredCompare(c.field, fieldId));
    }

    GetVisibleColumns(moduleId: ViewSettingModuleIdType, fieldId?: string): ViewSettingModel[] | null {
        if (!this.model.viewSettings) {
            return null;
        } else {
            let result = this.model.viewSettings.filter(c => StringHelper.caseIgnoredCompare(c.moduleId, moduleId));
            if (!StringHelper.isNullOrWhiteSpace(fieldId)) {
                result = result.filter(c => StringHelper.caseIgnoredCompare(c.field, fieldId));
            }
            return result;
        }
    }

    private resetActivityTimeline(): void {
        this.activityModel = [];
        this.groupActivityModel = null;
        this.isActivitySorting = null;
        this.isActivityFiltering = null;
        this.activityPageSetting = {
            pageSize: this.defaultActivityPageSize,
            pageIndex: 0,
            ascending: false,
            filterEventDate: '0001/1/1'
        };
        this.activityTotal = 0;
        this.showActivityLoadMoreButton = true;
        this.loadedActivityTotal = 0;
        this.showActivityActionButton = true;
        this.showActivityList = false;
        this.showActivityNoRecordMsg = false;
    }

    getLineOrder(dataRow){
        if (!StringHelper.isNullOrEmpty(dataRow.scheduleLineNo)) {
            return `${dataRow.lineOrder}~${dataRow.scheduleLineNo}`;
        }
        return dataRow.lineOrder;
    }
}
