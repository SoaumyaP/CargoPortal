import { Component, OnInit, Input, Output, EventEmitter, ViewChild, OnDestroy } from '@angular/core';
import { faPlus, faEllipsisV, faPencilAlt, faTrashAlt, faInfoCircle, faTrash, faMinus, faPaperPlane } from '@fortawesome/free-solid-svg-icons';
import { ControlContainer, NgForm } from '@angular/forms';
import { POFulfillmentOrderStatus, DropDowns, VerificationSetting, StringHelper, UserContextService } from 'src/app/core';
import { EnumHelper } from 'src/app/core/helpers/enum.helper';
import { MissingPOFulfillmentFormService } from '../missing-po-fulfillment-form/missing-po-fulfillment-form.service';
import { SelectCustomerPOFormComponent } from '../select-customer-po-form/select-customer-po-form.component';
import { ActivatedRoute, ActivatedRouteSnapshot } from '@angular/router';
import { OrganizationNameRole, LogisticsService, ValidationDataType, Roles, POFulfillmentStageType, ModeOfTransportType, POType, PackageUOMType, AgentAssignmentMode, RoleSequence, FulfillmentType, ViewSettingModuleIdType } from 'src/app/core/models/enums/enums';
import * as cloneDeep from 'lodash/cloneDeep';
import { TranslateService } from '@ngx-translate/core';
import { ValidationData } from 'src/app/core/models/forms/validation-data';
import { delay, filter } from 'rxjs/operators';
import { IntegrationData } from 'src/app/core/models/forms/integration-data';
import { Subject, Subscription } from 'rxjs';
import { MathHelper } from 'src/app/core/helpers/math.helper';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { FormHelper } from 'src/app/core/helpers/form.helper';
export interface AddMissingPOFormModel {
    customerPO: string;
}
@Component({
    selector: 'app-missing-po-fulfillment-customer',
    templateUrl: './missing-po-fulfillment-customer.component.html',
    styleUrls: ['./missing-po-fulfillment-customer.component.scss'],
    viewProviders: [{ provide: ControlContainer, useExisting: NgForm }]
})
export class MissingPOFulfillmentCustomerComponent implements OnInit, OnDestroy {
    @Input() model: any;
    @Input() parentIntegration$: Subject<IntegrationData>;
    @Input() formErrors: any;
    @Input() isEditable: boolean;
    @Input() isViewMode: boolean;
    @Input() isAddMode: boolean;

    @Output() close: EventEmitter<any> = new EventEmitter();
    @Output() add: EventEmitter<any> = new EventEmitter<any>();
    @Output() edit: EventEmitter<any> = new EventEmitter<any>();
    // Please trigger this event to change ('add'/'remove') the booking contacts.
    @Output() contactsChanged: EventEmitter<object> = new EventEmitter();
    // Store all subscriptions, then should un-subscribe at the end
    private _subscriptions: Array<Subscription> = [];
    verificationSetting = VerificationSetting;
    POFulfillmentOrderStatus = POFulfillmentOrderStatus;
    POFulfillmentOrderStatusOptions = DropDowns.POFulfillmentOrderStatus;
    currentUser: any;
    /** Data-source for Customer PO list.
     * Every action to Customer PO line item(adding/editing/deleting) will change it's value.
     * @Notes : Because it is a shadow variable which is cloned deeply this.service.currentCustomerPOs(), make sure both are async-ed together.
    */
    availablePOsList: any[];
    faPlus = faPlus;
    faMinus = faMinus;
    faEllipsisV = faEllipsisV;
    faPencilAlt = faPencilAlt;
    faTrashAlt = faTrashAlt;
    faInfoCircle = faInfoCircle;
    faPaperPlaneRegular = faPaperPlane;

    @ViewChild(SelectCustomerPOFormComponent, { static: false }) public POCustomerFormElement: SelectCustomerPOFormComponent;

    CustomerPOFormModeType = {
        add: 0,
        edit: 1
    };
    customerFormMode = this.CustomerPOFormModeType.add;
    customerFormOpened = false;

    addMissingPOFormOpened = false;
    addMissingPOFormModel: AddMissingPOFormModel;

    /** Model data for customer PO popup. It is details in the right */
    customerPOModel: any;
    /** Flag to open/close customer PO popup. */
    customerDetailPopupOpened = false;
    customerDetailModel: any;
    isSelectedDrag = false;
    // need it to reset selected before on edit mode
    currentSelectedIndex = null;

    // Store all validation results (business, input,...), then should return to validate
    private _validationResults: Array<ValidationData> = [];

    /**Data from 2 sources:
     * 1. Add mode: PO.pOType
     * 2. Edit mode: POFF.fulfilledFromPOType
     * */
    poType: POType;

    buyerCompliance: any = {};
    gridMissingFields = [];

    viewSettingModuleIdType = ViewSettingModuleIdType;
    
    constructor(
        public service: MissingPOFulfillmentFormService,
        activatedRoute: ActivatedRoute,
        public translateService: TranslateService,
        private notification: NotificationPopup,
        private userContext: UserContextService) {
    }

    ngOnInit(): void {
        if (this.model) {
            this.poType = this.model.fulfilledFromPOType;
        }
        if (!this.model.orders) {
            this.model.orders = [];
        }
        const sub = this.service.currentCustomerPOs$.pipe(delay(1)).subscribe(customerPOs => {
            if (customerPOs) {
                this.availablePOsList = cloneDeep(customerPOs);
                this.buildPOsForBooking();
            }
        });
        this._subscriptions.push(sub);

        // Apply business rules for each specific system user roles
        const currentUser = this.userContext.currentUser;
        this.currentUser = currentUser;
        if (currentUser && !currentUser.isInternal && (currentUser.role.id === Roles.Agent ||
            currentUser.role.id === Roles.Principal)) {
            this.model.orders.forEach(o => {
                o.isShowBookedPackageWarning = this.isShowBookedPackageWarning(o);
            });
        }

        this._registerBookingDataLoadedHandler();
        this._registerCustomerPOsRefreshedHandler();
        this._registerPOTypeChangedHandler();
        this._registerOnBuyerComplianceDataLoadedHandler();
    }

    _registerPOTypeChangedHandler() {
         // Handler for value changing on poType
        const sub = this.parentIntegration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-form]poTypeChanged'
            )).subscribe((eventContent: IntegrationData) => {
                this.poType = eventContent.content['poType'];
            });
        this._subscriptions.push(sub);
    }

    _registerBookingDataLoadedHandler() {
        const sub = this.parentIntegration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-form]onInitDataLoaded'
            )).subscribe((eventContent: IntegrationData) => {
                this.model = eventContent.content['bookingModel'];
                this.buildPOsForBooking();
            });
        this._subscriptions.push(sub);
    }

    _registerCustomerPOsRefreshedHandler() {
        const sub = this.parentIntegration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-form]customerPOsRefreshed'
            )).subscribe((eventContent: IntegrationData) => {
                const purchaseOrderAdhocChangedIds = eventContent.content['purchaseOrderIds'];
                const orders = this.model.orders;

                let index = orders.length - 1;
                while (index >= 0) {
                    const item = orders[index];
                    const po = this.availablePOsList.find(x => x.id === item.purchaseOrderId);
                    if (!po) {
                        orders.splice(index, 1);
                    } else {
                        const poLineItem = po.lineItems.find(x => x.id === item.poLineItemId);
                        if (!poLineItem) {
                            // Remove items out of Booking if it's removed from PO
                            orders.splice(index, 1);
                        } else {
                            // Update items from PO
                            const balanceQty = (poLineItem.orderedUnitQty - poLineItem.bookedUnitQty) - item.fulfillmentUnitQty;
                            item.orderedUnitQty = poLineItem.orderedUnitQty;
                            item.balanceUnitQty = balanceQty;
                            poLineItem.balanceUnitQty = balanceQty;

                            item.packageUOM = poLineItem.packageUOM;
                            item.unitUOM = poLineItem.unitUOM;
                        }
                    }
                    index -= 1;
                }

                // Add items into Booking if it's newly added into PO
                const bookedPOIds = [...new Set(orders.map(x => x.purchaseOrderId))]
                    .filter(x => purchaseOrderAdhocChangedIds.includes(x));

                bookedPOIds.forEach(i => {
                    const po = this.availablePOsList.find(x => x.id === i);
                    po.lineItems.forEach(item => {
                        if (!orders.find(x => x.poLineItemId === item.id) && item.balanceUnitQty > 0) {
                            // auto-calculate and populate data based on [article master]
                            const bookedPackage = Math.ceil(!item.outerQuantity || item.outerQuantity <= 0 ? 0 : item.balanceUnitQty / item.outerQuantity);
                            const volume = MathHelper.calculateCBM(item.outerDepth, item.outerWidth, item.outerHeight) * bookedPackage;
                            const grossWeight = item.outerGrossWeight * bookedPackage;
                            orders.push({
                                customerPONumber: po.poNumber,
                                productCode: item.productCode,
                                descriptionOfGoods: item.descriptionOfGoods,
                                commodity: item.commodity,
                                countryCodeOfOrigin: item.countryCodeOfOrigin,
                                currencyCode: item.currencyCode,
                                fulfillmentUnitQty: item.balanceUnitQty,
                                orderedUnitQty: item.orderedUnitQty,
                                balanceUnitQty: 0,
                                outerQuantity: item.outerQuantity,
                                innerQuantity: item.innerQuantity,
                                outerDepth: item.outerDepth,
                                outerHeight: item.outerHeight,
                                outerWidth: item.outerWidth,
                                outerGrossWeight: item.outerGrossWeight,
                                bookedPackage: bookedPackage,
                                volume: isNaN(volume) || volume === 0 ? null : MathHelper.roundToThreeDecimals(volume),
                                grossWeight: isNaN(grossWeight) || grossWeight === 0 ? null : grossWeight,
                                unitPrice: item.unitPrice,
                                unitUOM: item.unitUOM,
                                packageUOM: item.packageUOM,
                                hsCode: item.hsCode,
                                shippingMarks: item.shippingMarks,
                                status: POFulfillmentOrderStatus.Active,
                                purchaseOrderId: po.id,
                                poLineItemId: item.id,
                                poContainerType: po.containerType
                            });

                            item.balanceUnitQty = 0;
                        }
                    });
                });
                this.poffOrderChangeEmitter();
            });
        this._subscriptions.push(sub);
    }

    _registerOnBuyerComplianceDataLoadedHandler() {
        const sub = this.parentIntegration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-general-info]buyerComplianceDataLoaded'
            )).subscribe((eventContent: IntegrationData) => {
                this.buyerCompliance = eventContent.content;
            });
        this._subscriptions.push(sub);
    }

    get addCustomerPOBtnData() {
        const result = [
            {
                actionName: this.translateService.instant('label.addMissingPO'),
                icon: 'plus',
                click: () => {
                    this.addMissingPOFormModel = {
                        customerPO: ''
                    }
                    this.addMissingPOFormOpened = true;
                }
            },
            {
                actionName: this.translateService.instant('label.selectCustomerPO'),
                icon: 'plus',
                disabled: this.isDisableAddCustomerPO,
                click: () => {
                    this.addCustomerPO();
                }
            }
        ];
        return result;
    }

    /**To set-up PO data for current booking (Customer PO tab) */
    buildPOsForBooking() {
        // we don't need to remove line items with balanceQty <= 0 here, it is controlled on the popup via tree data

        // to populate PO data for each booking orders
        // for reuse purposes at the product line-item level, no need query to PO again

        this.model.orders.forEach(o => {
            const purchaseOrder = this.availablePOsList.find(x => x.id === o.purchaseOrderId);
            if (!StringHelper.isNullOrEmpty(purchaseOrder)) {

                o.poContainerType = purchaseOrder.containerType;
                const selectedLineItem = purchaseOrder.lineItems.find(x => x.id === o.poLineItemId);

                if (!StringHelper.isNullOrEmpty(selectedLineItem)) {
                    o.descriptionOfGoods = selectedLineItem.descriptionOfGoods;
                    o.outerQuantity = selectedLineItem.outerQuantity;
                }
            }
            o.isShowBookedPackageWarning = this.isShowBookedPackageWarning(o);
        });
    }

    isShowBookedPackageWarning(lineItem: any): boolean {
        return lineItem.bookedPackage && lineItem.outerQuantity && (lineItem.fulfillmentUnitQty % lineItem.outerQuantity > 0);
    }

    isItemExistsInPoffOrder(poId: number, lineId: number) {
        const bookingOrders = !this.model.orders ? [] : this.model.orders;
        for (let index = 0; index < bookingOrders.length; index++) {
            if (bookingOrders[index].purchaseOrderId === poId && bookingOrders[index].poLineItemId === lineId) {
                return true;
            }
        }
        return false;
    }

    /**To get data of the first PO selected
     * Return undefined if not found
     */
    selectCustomerPOSelected_InOrders_ByIndex(index) {
        if (this.model.orders.length === 0) {
            return null;
        }
        const firstPOCustomer = this.availablePOsList.find(x => x.id === this.model.orders[index].purchaseOrderId);
        return firstPOCustomer;
    }

    /**
     *To get the first index of un-missing order
     */
    get firstOrderSelected_Index(): number {
        if (!this.model.orders) {
            return -1;
        }
        return this.model.orders.findIndex(x => x.purchaseOrderId > 0);
    }

    /**
     *Return the list of un-missing orders
     */
    get filteredOrders() {
        if (!this.model.orders) {
            return [];
        }

        return this.model.orders.filter(x => x.purchaseOrderId > 0);
    }

    private get isReceiptPortInheritShipFrom() {
        return this.model.logisticsService === LogisticsService.PortToPort ||
            this.model.logisticsService === LogisticsService.PortToDoor;
    }

    private get isDeliveryPortInheritShipTo() {
        return this.model.logisticsService === LogisticsService.PortToPort ||
            this.model.logisticsService === LogisticsService.DoorToPort;
    }

    public get isRequireHsCode() {
        return (this.buyerCompliance.hsCodeShipFromIds && this.buyerCompliance.hsCodeShipFromIds.includes(this.model.shipFrom)) ||
            (this.buyerCompliance.hsCodeShipToIds && this.buyerCompliance.hsCodeShipToIds.includes(this.model.shipTo));
    }

    public get isRequirePackageUOM() {
        return true;
    }

    public get allowMixedCarton(): boolean {
        return this.buyerCompliance.shippingCompliance && this.buyerCompliance.shippingCompliance.allowMixedCarton;
    }

    private bindBookingByComplianceSettings() {
        const firstPO = this.selectCustomerPOSelected_InOrders_ByIndex(this.firstOrderSelected_Index);
        const emitValue = {
            name: '[po-fulfillment-customer]add-first-customer-po',
            content: firstPO?.contacts
        };
        this.parentIntegration$.next(emitValue);

            // General tab
            if (this.model.buyerCompliance.purchaseOrderVerificationSetting.shipFromLocationVerification
                === this.verificationSetting.AsPerPO) {
                this.model.shipFrom = firstPO.shipFromId;
                if (this.isReceiptPortInheritShipFrom) {
                    this.model.receiptPortId = this.model.shipFrom;
                }

                this.service.getLocation(this.model.shipFrom).subscribe(location => {
                    this.model.shipFromName = location.locationDescription;
                    if (this.model.agentAssignmentMode === AgentAssignmentMode.Default) {
                        this.shipFromPortValueChangeEmitter();
                    }

                    if (this.isReceiptPortInheritShipFrom) {
                        this.model.receiptPort = this.model.shipFromName;
                    }
                });
            }
            if (this.model.buyerCompliance.purchaseOrderVerificationSetting.shipToLocationVerification
                === this.verificationSetting.AsPerPO) {
                this.model.shipTo = firstPO.shipToId;
                if (this.isDeliveryPortInheritShipTo) {
                    this.model.deliveryPortId = this.model.shipTo;
                }
                this.service.getLocation(this.model.shipTo).subscribe(location => {
                    this.model.shipToName = location.locationDescription;
                    if (this.model.agentAssignmentMode === AgentAssignmentMode.Default) {
                        this.shipToPortValueChangeEmitter();
                    }

                    if (this.isDeliveryPortInheritShipTo) {
                        this.model.deliveryPort = this.model.shipToName;
                    }
                });
            }
            if (this.model.buyerCompliance.purchaseOrderVerificationSetting.modeOfTransportVerification
                === this.verificationSetting.AsPerPO) {
                this.model.modeOfTransport = firstPO.modeOfTransport;
                
                // handle for case create booking with missing PO (Change agent mode)
                const emitValue: IntegrationData = {
                    name: '[po-fulfillment-customer-po]add-first-po'
                };
                this.parentIntegration$.next(emitValue);

            }
            if (this.model.buyerCompliance.purchaseOrderVerificationSetting.incotermVerification
                === this.verificationSetting.AsPerPO) {
                    this.model.incoterm = firstPO.incoterm;
            }

            if (this.model.buyerCompliance.purchaseOrderVerificationSetting.preferredCarrierVerification
                === this.verificationSetting.AsPerPO) {
                this.service.getCarrier(firstPO.carrierCode).subscribe(carrier => {
                    if (carrier && carrier.length > 0) {
                        this.model.preferredCarrier = carrier[0].id;
                        this.model.carrier = carrier[0].name;
                    }
                });
            }

            if (this.model.buyerCompliance.purchaseOrderVerificationSetting.expectedShipDateVerification
                === this.verificationSetting.AsPerPO 
                || this.model.buyerCompliance.purchaseOrderVerificationSetting.expectedShipDateVerification
                === this.verificationSetting.AsPerPODefault) {
                this.model.expectedShipDate = firstPO.expectedShipDate;
            }

            if (this.model.buyerCompliance.purchaseOrderVerificationSetting.expectedDeliveryDateVerification
                === this.verificationSetting.AsPerPO
                || this.model.buyerCompliance.purchaseOrderVerificationSetting.expectedDeliveryDateVerification
                === this.verificationSetting.AsPerPODefault) {
                this.model.expectedDeliveryDate = firstPO.expectedDeliveryDate;
            }

            // Contact tab
            const newContacts = [];
            if (this.model.buyerCompliance.purchaseOrderVerificationSetting.shipperVerification
                === this.verificationSetting.AsPerPO) {
                const shipper = firstPO.contacts.find(c => c.organizationRole === OrganizationNameRole.Shipper);
                if (shipper) {
                    // remove existing Shipper
                    const existingShipperIndex = this.model.contacts.findIndex(c => c.organizationRole === OrganizationNameRole.Shipper);
                    if (existingShipperIndex >= 0) {
                        this.contactsChanged.emit({
                            action: 'remove',
                            rowIndex: existingShipperIndex
                        });
                    }
                    shipper.contactSequence = RoleSequence.Shipper;
                    newContacts.push(shipper);
                }
            }
            if (this.model.buyerCompliance.purchaseOrderVerificationSetting.consigneeVerification
                === this.verificationSetting.AsPerPO) {
                const consignee = firstPO.contacts.find(c => c.organizationRole === OrganizationNameRole.Consignee);
                if (consignee) {
                    // remove existing Consignee
                    const existingConsigneeIndex = this.model.contacts.findIndex(c => c.organizationRole === OrganizationNameRole.Consignee);
                    if (existingConsigneeIndex >= 0) {
                        this.contactsChanged.emit({
                            action: 'remove',
                            rowIndex: existingConsigneeIndex
                        });
                    }
                    consignee.contactSequence = RoleSequence.Consignee;
                    newContacts.push(consignee);

                    if (this.model.isNotifyPartyAsConsignee) {
                        // remove existing Notify-Party
                        const existingNotifyPartyIndex = this.model.contacts.findIndex(c => c.organizationRole === OrganizationNameRole.NotifyParty
                            && (StringHelper.isNullOrEmpty(c.removed) || !c.removed));
                        if (existingNotifyPartyIndex >= 0) {
                            this.contactsChanged.emit({
                                action: 'remove',
                                rowIndex: existingNotifyPartyIndex
                            });
                        }
                        newContacts.push({
                            organizationId: consignee.organizationId,
                            organizationRole: OrganizationNameRole.NotifyParty,
                            companyName: consignee.companyName,
                            contactName: consignee.contactName,
                            contactNumber: consignee.contactNumber,
                            contactEmail: consignee.contactEmail,
                            contactSequence: RoleSequence.NotifyParty,
                            address: consignee.addressLine1
                        });
                    }
                }
            }

            const supplier = firstPO.contacts.find(c => c.organizationRole === OrganizationNameRole.Supplier);
            if (supplier) {
                // remove existing Supplier
                const existingSupplierIndex = this.model.contacts.findIndex(c => c.organizationRole === OrganizationNameRole.Supplier);
                if (existingSupplierIndex >= 0) {
                    this.contactsChanged.emit({
                        action: 'remove',
                        rowIndex: existingSupplierIndex
                    });
                }
                supplier.contactSequence = RoleSequence.Supplier;
                newContacts.push(supplier);
            }

            newContacts.forEach(i => {
                this.model.contacts.push({
                    organizationId: i.organizationId,
                    organizationRole: i.organizationRole,
                    organizationCode: i.organizationCode,
                    companyName: i.companyName,
                    contactName: i.contactName,
                    contactNumber: i.contactNumber,
                    contactEmail: i.contactEmail,
                    contactSequence: i.contactSequence,
                    address: i.addressLine1,
                });
                this.contactsChanged.emit({action: 'add'});
            });
    }

    askForPO() {
        const confirmDlg = this.notification.showConfirmationDialog('msg.missingPO', 'label.fulfillment');

        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.service.askForMissingPO(this.model.id, this.model).subscribe(
                        data => {
                            this.notification.showSuccessPopup(
                                "save.missingPOSuccessfully",
                                "label.fulfillment"
                            );
                        },
                        error => {
                            this.notification.showErrorPopup(
                                "save.failureNotification",
                                "label.fulfillment"
                            );
                        }
                    )
                }
            });
    }

    get isShowAskForPO() {
        if (!this.isAddMode && this.model.stage === POFulfillmentStageType.Draft && this.model.orders?.some(c => c.poLineItemId === 0 && c.purchaseOrderId === 0)) {
            return true;
        }
        return false;
    }

    async addCustomerPO () {
        if (this.model.stage === POFulfillmentStageType.Draft) {
            await this.model.orders.forEach(o => {
                if(o.purchaseOrderId !== 0) {
                    const purchaseOrder = this.availablePOsList.find(x => x.id === o.purchaseOrderId);
                    const selectedLineItem = purchaseOrder.lineItems.find(x => x.id === o.poLineItemId);
                    if (!StringHelper.isNullOrEmpty(purchaseOrder)) {
                        selectedLineItem.balanceUnitQty = (selectedLineItem.orderedUnitQty - selectedLineItem.bookedUnitQty) - o.fulfillmentUnitQty;
                    }
    
                    // Must update value from the service also
                    // Because there is cloneDeep on ngOnInit: this.availablePOsList = cloneDeep(customerPOs);
                    const originPurchaseOrder =  this.service.currentCustomerPOs().find(x => x.id === o.purchaseOrderId);
                    const originSelectedLineItem = originPurchaseOrder.lineItems.find(x => x.id === o.poLineItemId);
                    if (!StringHelper.isNullOrEmpty(originSelectedLineItem)) {
                        originSelectedLineItem.balanceUnitQty = (selectedLineItem.orderedUnitQty - selectedLineItem.bookedUnitQty) - o.fulfillmentUnitQty;
                    }
                }
            });
        }
        const customerPOPopupDataSource = this.setupDataSourceForPOPopup();
        const firstPOSelected = this.selectCustomerPOSelected_InOrders_ByIndex(0);

        const customerOrg = this.model.contacts.find(x => x.organizationRole === OrganizationNameRole.Principal);
        const supplierOrg = this.model.contacts.find(x => x.organizationRole === OrganizationNameRole.Supplier);
        this.POCustomerFormElement.buildTreeForPODataSource(customerPOPopupDataSource, customerOrg, supplierOrg, firstPOSelected);
        this.customerFormMode = this.CustomerPOFormModeType.add;
        this.POCustomerFormElement.model = null;
        this.customerPOModel = null;
        this.customerFormOpened = true;
        this.isSelectedDrag = false;
    }

    customerFormClosedHandler() {
        this.customerFormOpened = false;
        this.currentSelectedIndex = null;
        this.isSelectedDrag = false;
        this.close.emit();
    }

    customerDetailPopupClosedHandler() {
        this.customerDetailPopupOpened = false;
    }

    openCustomerDetailPopup(dataItem) {
        const originCountry = this.POCustomerFormElement.countryOptions
            .find(x => x.description === dataItem.countryCodeOfOrigin);
        dataItem.countryNameOfOrigin = !StringHelper.isNullOrEmpty(originCountry) ? originCountry.label : null;
        this.customerDetailModel = dataItem;
        this.customerDetailPopupOpened = true;
    }

    /**Handle action after new customer po added to the grid */
    customerAddHandler(modelPopup) {
        modelPopup.poContainerType = this.availablePOsList.find(x => x.id === modelPopup.purchaseOrderId).containerType;
        modelPopup.isShowBookedPackageWarning = this.isShowBookedPackageWarning(modelPopup);
        this.model.orders.push(modelPopup);
        this.customerFormMode = this.CustomerPOFormModeType.add;
        this.customerFormOpened = false;

        this.availablePOsList.forEach(el => {
            el.lineItems.forEach(e => {
                if (e.id === modelPopup.poLineItemId) {
                    e.balanceUnitQty = modelPopup.balanceUnitQty;
                }
            });
        });

        // Must update value from the service also
        // Because there is cloneDeep on ngOnInit: this.availablePOsList = cloneDeep(customerPOs);
        this.service.currentCustomerPOs().forEach(el => {
            el.lineItems.forEach(e => {
                if (e.id === modelPopup.poLineItemId) {
                    e.balanceUnitQty = modelPopup.balanceUnitQty;
                }
            });
        });
        
        // If it is the first PO added into the grid
        if (this.filteredOrders.length === 1) {
            let emitValue: IntegrationData;
            this.bindBookingByComplianceSettings();
            if (this.model.modeOfTransport === ModeOfTransportType.Sea) {
                const containerType = this.availablePOsList.find(x => x.id === this.model.orders[this.firstOrderSelected_Index].purchaseOrderId).containerType;

                emitValue = {
                    name: '[po-fulfillment-customer]containerTypeChanged',
                    content: {
                        'containerType': containerType
                    }
                };
                this.parentIntegration$.next(emitValue);
            }
        } else {
            this.poffOrderChangeEmitter();
        }
        this.updateOriginAndDestinationAgent(this.model.orders.length - 1);
    }

    addMissingPO(data: AddMissingPOFormModel) {
        this.addMissingPOFormOpened = false;
        this.model.orders.push({
            customerPONumber: data.customerPO,
            bookedUnitQty: 0,
            fulfillmentUnitQty: 0,
            id: 0,
            innerQuantity: 0,
            isSelected: false,
            isShowBookedPackageWarning: false,
            lineOrder: 20,
            orderedUnitQty: 0,
            balanceUnitQty: 0,
            packageUOM: "Carton",
            poLineItemId: 0,
            poType: 10,
            purchaseOrderId: 0,
            status: 1,
            unitUOM: "Each"
        });
    }

    /**To update Origin agent and Destination agent contacts */
    updateOriginAndDestinationAgent(orderIndex) {
        // Contact tab: origin & destination agent
        if (this.model.orders.length > 0) {
            const newContacts = [];
            const currentPO = this.selectCustomerPOSelected_InOrders_ByIndex(orderIndex);
            const origin = currentPO.contacts.find(c => c.organizationRole === OrganizationNameRole.OriginAgent);
            if (origin) {
                origin.contactSequence = RoleSequence.OriginAgent;
                // If already Origin agent, ignore
                const isExists = this.model.contacts.find(x => x.organizationRole === origin.organizationRole);
                if (origin && !isExists) {
                    newContacts.push(origin);
                }
            }

            const destination = currentPO.contacts.find(c => c.organizationRole === OrganizationNameRole.DestinationAgent);
            if (destination) {
                destination.contactSequence = RoleSequence.DestinationAgent;
                // If already Destination agent, ignore
                const isExists = this.model.contacts.find(x => x.organizationRole === destination.organizationRole);
                if (destination && !isExists) {
                    newContacts.push(destination);
                }
            }

            newContacts.forEach(i => {
                this.model.contacts.push({
                    organizationId: i.organizationId,
                    organizationRole: i.organizationRole,
                    organizationCode: i.organizationCode,
                    companyName: i.companyName,
                    contactName: i.contactName,
                    contactNumber: i.contactNumber,
                    contactEmail: i.contactEmail,
                    contactSequence: i.contactSequence,
                    address: i.addressLine1
                });
                this.contactsChanged.emit({ action: 'add' });
            });
        }
    }

    /** After saving data from popup customer PO */
    customerEditHandler(modelPopup: any) {
        // query by older selected
        const currentIndex = this.model.orders.findIndex(x => x.purchaseOrderId === this.currentSelectedIndex.purchaseOrderId &&
            x.poLineItemId === this.currentSelectedIndex.poLineItemId);
        if (currentIndex >= 0) {
            modelPopup.poContainerType = this.availablePOsList.find(x => x.id === modelPopup.purchaseOrderId).containerType;
            modelPopup.isShowBookedPackageWarning = this.isShowBookedPackageWarning(modelPopup);
            this.model.orders[currentIndex] = modelPopup;
            this.customerFormOpened = false;

            this.availablePOsList.forEach(el => {
                el.lineItems.forEach(e => {
                    if (e.id === modelPopup.poLineItemId) {
                        e.balanceUnitQty = modelPopup.balanceUnitQty;
                    } else if (e.id === this.currentSelectedIndex.poLineItemId) {
                        // if current edited item is replaced by other item,
                        // do not need to reset balanceUnitQty of corresponding PO line item to origin value
                        // e.balanceUnitQty += this.customerPOModel.fulfillmentUnitQty;
                    }
                });
            });

            // Must update value from the service also
            // Because there is cloneDeep on ngOnInit: this.availablePOsList = cloneDeep(customerPOs);
            this.service.currentCustomerPOs().forEach(el => {
                el.lineItems.forEach(e => {
                    if (e.id === modelPopup.poLineItemId) {
                        e.balanceUnitQty = modelPopup.balanceUnitQty;
                    } else if (e.id === this.currentSelectedIndex.poLineItemId) {
                        // if current edited item is replaced by other item,
                        // do not need to reset balanceUnitQty of corresponding PO line item to origin value
                        // e.balanceUnitQty += this.customerPOModel.fulfillmentUnitQty;
                    }
                });
            });

            if (this.filteredOrders.length === 1) {
                this.bindBookingByComplianceSettings();
                if (this.model.modeOfTransport === ModeOfTransportType.Sea) {
                    const emitValue: IntegrationData = {
                        name: '[po-fulfillment-customer]lastCustomerPOEdited'
                    };
                    this.parentIntegration$.next(emitValue);
                }
            } else {
                this.poffOrderChangeEmitter();
            }
            this.customerFormClosedHandler();
            // The updated data is valid so we mark the row as a valid row
            this.model.orders[currentIndex].isValidRow = true;
        }
    }

    /**To set-up data source for customer PO popup */
    setupDataSourceForPOPopup(): any[] {
        const dataSource = this.availablePOsList || [];

        // Make is selected for selected po line item, then it will be disabled on popup
        dataSource.forEach(po => {
            po.lineItems.forEach(poItem => {
                const fulfillmentItem = this.model.orders.find(o => o.purchaseOrderId === po.id &&
                    o.poLineItemId === poItem.id);
                poItem.isSelected = fulfillmentItem ? true : false;
            });
        });

        // Apply the same for the shared variable
        this.service.currentCustomerPOs()?.forEach(po => {
            po.lineItems.forEach(poItem => {
                const fulfillmentItem = this.model.orders.find(o => o.purchaseOrderId === po.id &&
                    o.poLineItemId === poItem.id);
                poItem.isSelected = fulfillmentItem ? true : false;
            });
        });

        return dataSource;
    }

    onDeleteCustomerPO(purchaseOrderId, poLineItemId) {
        const confirmDlg = this.notification.showConfirmationDialog('msg.deleteCustomerPOConfirm', 'label.customerPO');

        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.deleteCustomerPO(purchaseOrderId, poLineItemId);
                }
            });
    }

    deleteCustomerPO(purchaseOrderId, poLineItemId) {
        const curIndex = this.model.orders.findIndex(x => x.purchaseOrderId === purchaseOrderId && x.poLineItemId === poLineItemId);
        if (curIndex >= 0) {
            this.customerFormMode = this.CustomerPOFormModeType.edit;
            const selectedItem = this.model.orders[curIndex];

            this.availablePOsList.forEach(el => {
                el.lineItems.forEach(e => {
                    if (e.id === selectedItem.poLineItemId) {
                        e.balanceUnitQty = e.orderedUnitQty - e.bookedUnitQty;
                        e.isSelected = false;
                    }
                });
            });

            // Must update value from the service also
            // Because there is cloneDeep on ngOnInit: this.availablePOsList = cloneDeep(customerPOs);
            this.service.currentCustomerPOs().forEach(el => {
                el.lineItems.forEach(e => {
                    if (e.id === selectedItem.poLineItemId) {
                        e.balanceUnitQty = e.orderedUnitQty - e.bookedUnitQty;
                    }
                });
            });

            if (selectedItem.purchaseOrderId > 0) {
                this.resetPOFulfillment();
            }
            
            this.model.orders.splice(curIndex, 1);
            this.poffOrderChangeEmitter();
        }
    }

    /** To enforce immutability for orders.
     * Call it every after changed data of this.model.orders.
     * To work with pipe poFulfillmentCustomerOrder*/
    private _enforceImmutableOrders() {
        this.model.orders = Object.assign([], this.model.orders);
    }

    private resetPOFulfillment() {
        if (this.filteredOrders.length === 1) {
            const poVerification = this.model.buyerCompliance.purchaseOrderVerificationSetting;
            if (poVerification.shipperVerification === VerificationSetting.AsPerPO) {
                const existingShipperIndex = this.model.contacts.findIndex(c => c.organizationRole === OrganizationNameRole.Shipper);
                if (existingShipperIndex >= 0) {
                    this.contactsChanged.emit({
                        action: 'remove',
                        rowIndex: existingShipperIndex
                    });
                }
            }

            if (poVerification.consigneeVerification === VerificationSetting.AsPerPO) {
                const existingConsigneeIndex = this.model.contacts.findIndex(c => c.organizationRole === OrganizationNameRole.Consignee);
                if (existingConsigneeIndex >= 0) {
                    this.contactsChanged.emit({
                        action: 'remove',
                        rowIndex: existingConsigneeIndex
                    });
                }
            }

            if (poVerification.modeOfTransportVerification === VerificationSetting.AsPerPO) {
                this.model.modeOfTransport = null;

                // handle for case create booking with missing PO (Change agent mode)
                const emitValue: IntegrationData = {
                    name: '[po-fulfillment-customer-po]delete-last-po'
                };
                this.parentIntegration$.next(emitValue);
            }

            if (poVerification.incotermVerification === VerificationSetting.AsPerPO) {
                this.model.incoterm = null;
            }

            if (poVerification.shipFromLocationVerification === this.verificationSetting.AsPerPO) {
                this.model.shipFrom = null;
                this.model.shipFromName = null;

                if (this.isReceiptPortInheritShipFrom) {
                    this.model.receiptPortId = null;
                    this.model.receiptPort = null;
                }

                // remove existing Origin-Agent
                if (this.model.agentAssignmentMode === AgentAssignmentMode.Default) {
                    const existingOriginAgentIndex = this.model.contacts
                        .findIndex(c => c.organizationRole === OrganizationNameRole.OriginAgent && (StringHelper.isNullOrEmpty(c.removed) || !c.removed));
                    if (existingOriginAgentIndex >= 0) {
                        this.contactsChanged.emit({
                            action: 'remove',
                            rowIndex: existingOriginAgentIndex
                        });
                    }
                }
            }

            if (poVerification.shipToLocationVerification === this.verificationSetting.AsPerPO) {
                this.model.shipTo = null;
                this.model.shipToName = null;

                if (this.isDeliveryPortInheritShipTo) {
                    this.model.deliveryPortId = null;
                    this.model.deliveryPort = null;
                }

                // remove existing Destination-Agent
                if (this.model.agentAssignmentMode === AgentAssignmentMode.Default) {
                    const existingDestinationAgentIndex = this.model.contacts
                        .findIndex(c => c.organizationRole === OrganizationNameRole.DestinationAgent && (StringHelper.isNullOrEmpty(c.removed) || !c.removed));
                    if (existingDestinationAgentIndex >= 0) {
                        this.contactsChanged.emit({
                            action: 'remove',
                            rowIndex: existingDestinationAgentIndex
                        });
                    }
                }
            }

            if (poVerification.preferredCarrierVerification === this.verificationSetting.AsPerPO) {
                this.model.preferredCarrier = null;
                this.model.carrier = null;
            }

            if (poVerification.expectedShipDateVerification === this.verificationSetting.AsPerPO) {
                this.model.expectedShipDate = null;
            }

            if (poVerification.expectedDeliveryDateVerification === this.verificationSetting.AsPerPO) {
                this.model.expectedDeliveryDate = null;
            }
        }
    }

    /** Clicking on Edit action from Customer PO grid */
    async editCustomerPO(purchaseOrderId, poLineItemId) {
        if (this.model.stage === POFulfillmentStageType.Draft) {
            await this.model.orders.forEach(o => {
                if(o.purchaseOrderId !== 0) {
                    const purchaseOrder = this.availablePOsList.find(x => x.id === o.purchaseOrderId);
                    const selectedLineItem = purchaseOrder.lineItems.find(x => x.id === o.poLineItemId);
                    if (!StringHelper.isNullOrEmpty(purchaseOrder)) {
                        selectedLineItem.balanceUnitQty = (selectedLineItem.orderedUnitQty - selectedLineItem.bookedUnitQty) - o.fulfillmentUnitQty;
                    }

                    // Must update value from the service also
                    // Because there is cloneDeep on ngOnInit: this.availablePOsList = cloneDeep(customerPOs);
                    const originPurchaseOrder =  this.service.currentCustomerPOs().find(x => x.id === o.purchaseOrderId);
                    const originSelectedLineItem = originPurchaseOrder.lineItems.find(x => x.id === o.poLineItemId);
                    if (!StringHelper.isNullOrEmpty(originSelectedLineItem)) {
                        originSelectedLineItem.balanceUnitQty = (selectedLineItem.orderedUnitQty - selectedLineItem.bookedUnitQty) - o.fulfillmentUnitQty;
                    }
                }
            });
        }
        const customerPOPopupDataSource = this.setupDataSourceForPOPopup();
        const curIndex = this.model.orders.findIndex(x => x.purchaseOrderId === purchaseOrderId && x.poLineItemId === poLineItemId);
        if (curIndex >= 0) {
            this.customerFormMode = this.CustomerPOFormModeType.edit;
            this.customerPOModel = Object.assign({}, this.model.orders[curIndex]);
            const selectedLineItem = this.availablePOsList.find(x => x.id === this.model.orders[curIndex].purchaseOrderId).lineItems
                .find(x => x.id === this.model.orders[curIndex].poLineItemId);

            // from line item's article master
            this.customerPOModel.outerQuantity = selectedLineItem.outerQuantity;
            this.customerPOModel.innerQuantity = selectedLineItem.innerQuantity;
            this.customerPOModel.outerDepth = selectedLineItem.outerDepth;
            this.customerPOModel.outerHeight = selectedLineItem.outerHeight;
            this.customerPOModel.outerWidth = selectedLineItem.outerWidth;
            this.customerPOModel.outerGrossWeight = selectedLineItem.outerGrossWeight;

            this.currentSelectedIndex = {
                purchaseOrderId: this.customerPOModel.purchaseOrderId,
                poLineItemId: this.customerPOModel.poLineItemId
            };

            const currentPO = this.availablePOsList.find(po => po.id === purchaseOrderId);
            const currentPOItem = currentPO.lineItems.find(item => item.id === poLineItemId);

            this.customerPOModel.balanceUnitQty = currentPOItem.balanceUnitQty;
            this.POCustomerFormElement.originBalance = this.customerPOModel.balanceUnitQty;
            this.POCustomerFormElement.originFulfillmentUnitQty = this.customerPOModel.fulfillmentUnitQty;
            this.isSelectedDrag = true;
            let firstPOSelected = null;

            // If edit item which is only one available in list, not set value for firstPOSelected
            if (this.model.orders.length > 1) {
                firstPOSelected = this.selectCustomerPOSelected_InOrders_ByIndex(0);
            }
            const customerOrg = this.model.contacts.find(x => x.organizationRole === OrganizationNameRole.Principal);
            const supplierOrg = this.model.contacts.find(x => x.organizationRole === OrganizationNameRole.Supplier);
            this.POCustomerFormElement.buildTreeForPODataSource(customerPOPopupDataSource, customerOrg, supplierOrg, firstPOSelected, this.currentSelectedIndex);
            this.customerFormOpened = true;
        }
    }

    labelFromEnum(arr, value) {
        return EnumHelper.convertToLabel(arr, value);
    }

    _validateCustomerPOFromComplianceSettings() {
        const result = new ValidationData(ValidationDataType.Business, true);
        if (!this.model.orders || this.model.orders.length <= 0) {
            this._validationResults.push(result);
            return;
        }

        // Validate Required Fields
        for (let i = 0; i < this.model.orders.length; i++) {
            const order = this.model.orders[i];
            if (order.purchaseOrderId === 0) { // is missing PO, will be skip
                continue;
            }
            // HSCode
            if (this.isRequireHsCode
                && StringHelper.isNullOrWhiteSpace(order.hsCode)) {
                if (!this.gridMissingFields.includes(this.translateService.instant('label.hsCode'))) {
                    this.gridMissingFields.push(this.translateService.instant('label.hsCode'));
                }
                order.isValidRow = false;
            }
            // packageUOM
            if (this.isRequirePackageUOM
                && (StringHelper.isNullOrWhiteSpace(order.packageUOM)
                || !PackageUOMType[order.packageUOM])) {
                if (!this.gridMissingFields.includes(this.translateService.instant('label.packageUOM'))) {
                    this.gridMissingFields.push(this.translateService.instant('label.packageUOM'));
                }
                order.isValidRow = false;
            }
            // grossWeight
            if (this.buyerCompliance.productVerificationSetting.isRequireGrossWeight
                && StringHelper.isNullOrWhiteSpace(order.grossWeight)) {
                if (!this.gridMissingFields.includes(this.translateService.instant('label.grossWeight'))) {
                    this.gridMissingFields.push(this.translateService.instant('label.grossWeight'));
                }
                order.isValidRow = false;
            }
            // volume
            if (this.buyerCompliance.productVerificationSetting.isRequireVolume
                && StringHelper.isNullOrWhiteSpace(order.volume)) {
                if (!this.gridMissingFields.includes(this.translateService.instant('label.volumeCBM'))) {
                    this.gridMissingFields.push(this.translateService.instant('label.volumeCBM'));
                }
                order.isValidRow = false;
            }
        }

        // The total qty of Booked Package must be greater than 0 if the Allow Mixed Carton is enabled.
        if (this.allowMixedCarton) {
            let hasCustomerPO = this.model.orders?.some(x => x.purchaseOrderId > 0);
            if (hasCustomerPO)  {
                let totalBookedPackage = 0;
                this.model.orders?.forEach(o => totalBookedPackage += (o.bookedPackage ?? 0));
                if (totalBookedPackage <= 0)
                {
                    result.status = false;
                    //Total quantity of Booked Package must be greater than 0.
                    result.message = this.translateService.instant('validation.totalBookedPackageMustGreaterThanZero');
                    this._validationResults.push(result);
                    return;
                }
            }
        }
        else {
            this._validateBookedPackage();
        }
        
        const isAllowPartialShipment = this.buyerCompliance.shippingCompliance.allowPartialShipment;
        // Blanket is always no partial shipment allowed
        if (isAllowPartialShipment && this.poType !== POType.Blanket) {
            this._validationResults.push(result);
            return;
        } else {
            const selectedPONumbers = this.model.orders.filter(order => order.purchaseOrderId !== 0).map(item => item.customerPONumber);
            const uniquePONumbers = selectedPONumbers.filter((value, index, self) => self.indexOf(value) === index);
            const customerPOs = this.service.currentCustomerPOs();
            for (let i = 0; i < uniquePONumbers.length; i++) {
                const lineItems = customerPOs.find(x => x.poNumber === uniquePONumbers[i]).lineItems;
                for (let j = 0; j < lineItems.length; j++) {
                    if (!this.model.orders.some(x => x.customerPONumber === uniquePONumbers[i]
                        && x.productCode === lineItems[j].productCode && x.fulfillmentUnitQty > 0)) {
                        result.status = false;
                        result.message = this.translateService.instant('validation.notAllowPartialShipment');
                        this._validationResults.push(result);
                        return;
                    }
                }
            }
            this._validationResults.push(result);
            return;
        }
    }

    _validateBookedPackage() {
        for (let i = 0; i < this.model.orders.length; i++) {
            const order = this.model.orders[i];
            if (this.model.orders[i].purchaseOrderId === 0) { // is missing PO, will be skip
                continue;
            }
            if (!order.bookedPackage || order.bookedPackage <= 0) {
                if (!this.gridMissingFields.includes('Booked Package')) {
                    this.gridMissingFields.push('Booked Package');
                }
                order.isValidRow = false;
            }
        }
    }

    _validateHsCode() {
        for (const order of this.model.orders) {
            if (!StringHelper.isNullOrEmpty(order.hsCode)) {
                if (!this.POCustomerFormElement.validateHsCode(order.hsCode)) {
                    if (!this.gridMissingFields.includes('HS Code')) {
                        this.gridMissingFields.push('HS Code');
                    }
                    order.isValidRow = false;
                }
            }
        }
    }

    validateTab() {
        this._validationResults = [];
        this.gridMissingFields = [];
        this.resetRowErrors();

        // Validate if there is any record in grid first
        const customerPOs = this.model.orders;
        if (StringHelper.isNullOrEmpty(customerPOs) || customerPOs.length === 0) {
            const result = new ValidationData(ValidationDataType.Business, true);
            result.status = false;
            result.message = this.translateService.instant('validation.atLeastOneRecord');
            this._validationResults.push(result);
            return this._validationResults;
        }

        this._validateCustomerPOFromComplianceSettings();
        this._validateHsCode();

        if (this.model.orders.findIndex(c => !c.isValidRow) >= 0) {
            const result = new ValidationData(ValidationDataType.Input, true);
            result.status = false;
            result.message = this.translateService.instant('validation.highlightedRowsWithError',
            {
                fieldList: this.gridMissingFields.join(', ')
            });
            this._validationResults.unshift(result);
        }

        return this._validationResults;
    }

    /**Reset to mark all rows is a valid row */
    resetRowErrors() {
        this.model.orders.forEach(o => {
            o.isValidRow = true;
        });
    }

    /**Using to publish event when the booking order has been changed. */
    poffOrderChangeEmitter() {
        const emitValue: IntegrationData = {
            name: '[po-fulfillment-customer]poffOrderChanged'
        };
        this._enforceImmutableOrders();
        this.parentIntegration$.next(emitValue);
    }

    /**Using to publish event when the ShipFrom Port has been changed. */
    shipFromPortValueChangeEmitter() {
        const emitValue: IntegrationData = {
            name: '[po-fulfillment-customer]shipFromPortValueChanged'
        };
        this.parentIntegration$.next(emitValue);
    }

    /**Using to publish event when the ShipTo Port has been changed. */
    shipToPortValueChangeEmitter() {
        const emitValue: IntegrationData = {
            name: '[po-fulfillment-customer]shipToPortValueChanged'
        };
        this.parentIntegration$.next(emitValue);
    }

    get isDisableAddCustomerPO(): boolean {
        if (!this.availablePOsList) {
            return true;
        }
        if (!this.buyerCompliance?.shippingCompliance?.allowMultiplePOPerFulfillment) {

            if (this.model.orders?.length > 0) {
                let customerPOIds =  [...new Set(this.model.orders.map(x => x.purchaseOrderId))];

                for (let index = 0; index < customerPOIds.length; index++) {
                    const el = customerPOIds[index];
                    // ignore missing po
                    if (el === 0) {
                        continue;
                    }
                    let poLineItemIds = this.model.orders.filter(x => x.purchaseOrderId == el).map(x => x.poLineItemId);
                    let purchaseOrder = this.availablePOsList.find(x => x.id == el);

                    if (purchaseOrder && purchaseOrder?.lineItems) {
                        let isNotFullyProduct = purchaseOrder.lineItems?.filter(x => x.balanceUnitQty > 0).some(x => !poLineItemIds.includes(x.id));
                        if (!isNotFullyProduct) {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public rowCallback(args) {
        return {
            'error-row': args.dataItem.isValidRow === false,
        };
    }

    isVisibleField(moduleId: ViewSettingModuleIdType, fieldId: string): boolean {
        if (!this.isViewMode) { // apply for view mode only
            return true;
        }
        
        return !FormHelper.isHiddenColumn(this.model.viewSettings, moduleId, fieldId);
    }

    ngOnDestroy(): void {
        this._subscriptions.map(x => x.unsubscribe());
    }
}
