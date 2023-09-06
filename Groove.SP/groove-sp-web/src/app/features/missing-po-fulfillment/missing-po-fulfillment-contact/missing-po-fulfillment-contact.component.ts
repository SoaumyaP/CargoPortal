import { Component, OnInit, Input, OnDestroy, OnChanges, SimpleChanges, Output, EventEmitter } from '@angular/core';
import { UserContextService, VerificationSetting, StringHelper } from 'src/app/core';
import { ControlContainer, NgForm } from '@angular/forms';
import { MissingPOFulfillmentFormService } from '../missing-po-fulfillment-form/missing-po-fulfillment-form.service';
import { Observable, EMPTY, BehaviorSubject, Subscription, Subject } from 'rxjs';
import { AgentAssignmentMode, AgentType, ModeOfTransportType, OrganizationNameRole, OrganizationRole, OrganizationType, Roles, RoleSequence, ValidationDataType, VerificationSettingType, ViewSettingModuleIdType } from 'src/app/core/models/enums/enums';
import { ValidationData } from 'src/app/core/models/forms/validation-data';
import { IntegrationData } from 'src/app/core/models/forms/integration-data';
import { filter, debounceTime, tap, delay } from 'rxjs/operators';
import { TranslateService } from '@ngx-translate/core';
import { faInfoCircle, faPlus } from '@fortawesome/free-solid-svg-icons';
import { ComplianceFormService } from '../../compliance/compliance-form/compliance-form.service';
import * as cloneDeep from 'lodash/cloneDeep';
import { ArrayHelper } from 'src/app/core/helpers/array.helper';
import { DefaultValue2Hyphens, EmailValidationPattern } from 'src/app/core/models/constants/app-constants';
import { FormHelper } from 'src/app/core/helpers/form.helper';

@Component({
    selector: 'app-missing-po-fulfillment-contact',
    templateUrl: './missing-po-fulfillment-contact.component.html',
    styleUrls: ['./missing-po-fulfillment-contact.component.scss'],
    viewProviders: [{ provide: ControlContainer, useExisting: NgForm }]
})
export class MissingPOFulfillmentContactComponent implements OnChanges, OnInit, OnDestroy {
    @Input('data')
    contactList: any[];
    @Input()
    parentIntegration$: Subject<IntegrationData>;
    // It is prefix for formErrors and validationRules
    // Use it to detect what tab contains invalid data
    @Input()
    tabPrefix: string;
    @Input()
    formErrors: any;
    @Input()
    validationRules: any;
    @Input()
    buyerCompliance: any;
    @Input()
    isNotifyPartyAsConsignee: boolean;
    @Input()
    isEditable: boolean
    @Input()
    isViewMode: boolean;
    @Input()
    set isFulfilledFromPO(value: boolean) {
        if (value !== undefined && value !== null) {
            this._isFulfilledFromPO.next(value);
        }
    }
    @Input() agentAssignmentMode: string;
    @Input() shipFromId: number;
    @Input() parentForm: any;

    @Output() changeAgentMode: EventEmitter<string> = new EventEmitter<string>();
    @Input() bookingModel: any;

    readonly NEW_CONTACT_SEQUENCE: number = 1000;
    faInfoCircle = faInfoCircle;
    defaultValue: string = DefaultValue2Hyphens;
    organizationNameRole = OrganizationNameRole;
    originOrganizationRoleOptions: any[];
    contactOptions = [];
    shipperList = [];
    consigneeList = [];
    isForceReset: boolean = false;
    isRequirePickup: boolean = false;
    activeOrgList: any[];
    shipperAffiliateIds: any[];
    isCanChangeAgent: any;
    faPlus = faPlus;
    isDefaultAgentSelected: boolean;
    // For disable Role and Company selection but can edit contact info
    disabledOrgRoles: string[] = [OrganizationNameRole.Supplier, OrganizationNameRole.Delegation];
    viewOnlyOrgRoles: string[] = [OrganizationNameRole.OriginAgent, OrganizationNameRole.DestinationAgent, OrganizationNameRole.Principal];
    _isFulfilledFromPO = new BehaviorSubject(true);
    // Store all subscriptions, then should un-subscribe at the end
    private _subscriptions: Array<Subscription> = [];
    // Store all validation results (business, input,...), then should return to validate
    private _validationResults: Array<ValidationData> = [];
    public addressValueKeyUp$ = new Subject<string>();
    public contactNameValueKeyUp$ = new Subject<string>();

    agentOrganizations: any = [];
    originAgentAssignmentSource = [];
    originAgentAssignmentFiltered = [];
    destinationAgentAssignmentSource = [];
    destinationAgentAssignmentFiltered = [];
    defaultContacts = [];
    originContactModel: any = {};
    destinationContactModel: any = {};
    isClickedChangeAgentButton = false;

    viewSettingModuleIdType = ViewSettingModuleIdType;
    
    constructor(private service: MissingPOFulfillmentFormService,
        private userContext: UserContextService,
        public complianceFormService: ComplianceFormService,
        public translateService: TranslateService) {

        const addressValueSub = this.addressValueKeyUp$.pipe(
            debounceTime(1000),
            tap((rowIndex) => {
                this.validateAddressInput(rowIndex);
            }
            )).subscribe();

        this._subscriptions.push(addressValueSub);

        const contactNameValueSub = this.contactNameValueKeyUp$.pipe(
            debounceTime(1000),
            tap((rowIndex) => {
                this.validateContactNameInput(rowIndex);
            }
            )).subscribe();
        this.isCanChangeAgent = this.userContext?.currentUser?.isInternal || this.userContext?.currentUser?.role.id === Roles.Agent;
        this._subscriptions.push(contactNameValueSub);
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes?.agentAssignmentMode?.currentValue) {
            this.agentAssignmentMode === AgentAssignmentMode.Default ? this.isDefaultAgentSelected = true : this.isDefaultAgentSelected = false;
        }

        if (changes?.buyerCompliance?.currentValue) {
            this.setAgentAssignmentDropdown();
        }
    }

    ngOnInit(): void {
        this.initOrganizations();
        this.getAgentOrganization();
        this.checkToShowDelegationContact();
        if (this.buyerCompliance) {
            this.applyPOVerificationSetting();
        }
        //#region Register Event Handlers
        this._registerEventHandlers();
        this._registerBookingDataLoadedHandler();
        this._registerPOContactVerificationSettingsHandler();
        this._registerIsNotifyPartyAsConsigneeChangedHandler();
        this._registerInitializedContactsHandler();
        this._registerShipFromChanged();
        this._registerShipToChanged();
        this._registerModeOfTransportChanged();
        //#endregion
    }

    getAgentOrganization() {
        this.complianceFormService.getOrganizations().subscribe(
            (r: any) => {
                this.agentOrganizations = r.filter(c => c.organizationType === OrganizationType.Agent);
                if (this.buyerCompliance) {
                    this.setAgentAssignmentDropdown();
                }
            }
        );
    }

    setMoreInfoForAgentAssignment() {
        // Add more AgentOrganizationName for AgentAssignments
        for (let agentAssignment of this.buyerCompliance.agentAssignments) {
            const fullAgentInfo = this.agentOrganizations.find(c => c.id === agentAssignment.agentOrganizationId);
            if (fullAgentInfo) {
                agentAssignment.agentOrganizationName = fullAgentInfo.name;
                agentAssignment.address = fullAgentInfo.address;
                agentAssignment.addressLine2 = fullAgentInfo.addressLine2;
                agentAssignment.addressLine3 = fullAgentInfo.addressLine3;
                agentAssignment.addressLine4 = fullAgentInfo.addressLine4;
                agentAssignment.contactName = fullAgentInfo.contactName;
                agentAssignment.contactNumber = fullAgentInfo.contactNumber;
                agentAssignment.contactEmail = fullAgentInfo.contactEmail;
            }
        }
    }

    /**
     * To set values for Agent: Origin and Destination Agents
     * @param resetAgentContacts Yes to reset default values for Origin/Destination Agent contacts.
     *  + It is yes as user changes Mode of Transport --> to set data-sources for Origin and Destination Agents.
     *  + Otherwise, it should be false.
     */
    setAgentAssignmentDropdown(resetAgentContacts = false) {
        this.setMoreInfoForAgentAssignment();
        let originAgentAssignment = this.buyerCompliance?.agentAssignments.filter(c => c.agentType === AgentType.Origin && c.modeOfTransport === this.bookingModel.modeOfTransport);
        originAgentAssignment = ArrayHelper.uniqueBy(originAgentAssignment, 'agentOrganizationId');

        const customerId = this.contactList.find(c => c.organizationRole === OrganizationNameRole.Principal)?.organizationId;
        for (let item of originAgentAssignment) {
            this.service.getEmailNotification(item.agentOrganizationId, customerId, this.shipFromId).subscribe(
                (result: any) => {
                    if (result) {
                        //Get the first email has been separated by comma.
                        //This logic is to make sure the email does not exceed the limit length in the booking.
                        var firstEmail = result.email.split(",")[0];
                        item.contactEmail = firstEmail.trim();
                    }
                }
            );
        }

        let destinationAgentAssignment = this.buyerCompliance?.agentAssignments.filter(c => c.agentType === AgentType.Destination && c.modeOfTransport === this.bookingModel.modeOfTransport);
        destinationAgentAssignment = ArrayHelper.uniqueBy(destinationAgentAssignment, 'agentOrganizationId');

        this.originAgentAssignmentSource = originAgentAssignment;
        this.destinationAgentAssignmentSource = destinationAgentAssignment;

        this.originAgentAssignmentFiltered = originAgentAssignment;
        this.destinationAgentAssignmentFiltered = destinationAgentAssignment;

        if (this.bookingModel.agentAssignmentMode === AgentAssignmentMode.Change) {
            const currentOriginOrgId = this.originContactModel.organizationId;
            const currentDestinationOrgId = this.destinationContactModel.organizationId;
            // to reset to default values for Origin & Destination Agents
            if (resetAgentContacts) {
                this.onSetDefaultAgent();
            }
            setTimeout(() => {
                this.onChangeAgent();
                if (currentOriginOrgId && this.originAgentAssignmentFiltered.some(c => c.organizationId === currentOriginOrgId)) {
                    this.onChangeOriginContactModel(currentOriginOrgId);
                }

                if (currentDestinationOrgId && this.destinationAgentAssignmentFiltered.some(c => c.organizationId === currentDestinationOrgId)) {
                    this.onChangeDestinationContactModel(currentDestinationOrgId);
                }
            }, 200);
        }
    }

    isEnableOrignAgent(dataItem: any) {
        return dataItem.organizationRole === OrganizationNameRole.OriginAgent;
    }

    isEnableDestinationAgent(dataItem: any) {
        return dataItem.organizationRole === OrganizationNameRole.DestinationAgent;
    }

    onFilterOriginCompany(value: string) {
        this.originAgentAssignmentFiltered = this.originAgentAssignmentSource.filter((s) => s.agentOrganizationName.toLowerCase().indexOf(value.toLowerCase()) !== -1);
    }

    onFilterDestinationCompany(value: string) {
        this.destinationAgentAssignmentFiltered = this.destinationAgentAssignmentSource.filter((s) => s.agentOrganizationName.toLowerCase().indexOf(value.toLowerCase()) !== -1);
    }

    onChangeOriginContactModel(value: number) {
        const originAgentAssignment = this.originAgentAssignmentFiltered.find(c => c.agentOrganizationId === value);

        this.originContactModel.companyName = originAgentAssignment?.agentOrganizationName;
        const contactNeedUpdate = this.contactList.find(c => c.organizationRole === OrganizationNameRole.OriginAgent);

        this.updateContactRow(contactNeedUpdate, originAgentAssignment);
        const rowIndex = this.contactList.findIndex(c => c.organizationRole === OrganizationNameRole.OriginAgent);
        this.validateCompany(originAgentAssignment?.agentOrganizationName || originAgentAssignment?.companyName, rowIndex);
        this.validateContactNameInput(rowIndex);
        this.validateContactEmail(originAgentAssignment?.contactEmail, rowIndex);
        this.validateContactNumber(originAgentAssignment?.contactNumber, rowIndex);
        this.validateAddressInput(rowIndex);
    }

    onChangeDestinationContactModel(value: number) {
        const destinationAgentAssigment = this.destinationAgentAssignmentFiltered.find(c => c.agentOrganizationId === value);

        this.destinationContactModel.companyName = destinationAgentAssigment?.agentOrganizationName;
        const contactNeedUpdate = this.contactList.find(c => c.organizationRole === OrganizationNameRole.DestinationAgent);

        this.updateContactRow(contactNeedUpdate, destinationAgentAssigment);
        const rowIndex = this.contactList.findIndex(c => c.organizationRole === OrganizationNameRole.DestinationAgent);
        this.validateCompany(destinationAgentAssigment?.agentOrganizationName || destinationAgentAssigment?.companyName, rowIndex);
        this.validateContactNameInput(rowIndex);
        this.validateContactEmail(destinationAgentAssigment?.contactEmail, rowIndex);
        this.validateContactNumber(destinationAgentAssigment?.contactNumber, rowIndex);
        this.validateAddressInput(rowIndex);
    }

    updateContactRow(contactRow: any, newData: any) {
        const concatenatedAddress = this.service.concatenateAddressLines(newData?.address, newData?.addressLine2, newData?.addressLine3, newData?.addressLine4);
        if (contactRow) {
            contactRow.organizationId = newData?.agentOrganizationId || newData?.organizationId;
            contactRow.organizationCode = newData?.organizationCode || newData?.code;
            contactRow.companyName = newData?.agentOrganizationName || newData?.companyName;
            contactRow.address = concatenatedAddress;
            contactRow.contactName = newData?.contactName;
            contactRow.contactNumber = newData?.contactNumber;
            contactRow.contactEmail = newData?.contactEmail;
        }
    }

    private _registerEventHandlers() {
        // Handler for value changing on Is Require Pickup
        // fired from tab General po-fulfillment-general-info
        const sub = this.parentIntegration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-general-info]isRequirePickupValueChanged'
            )).subscribe((eventContent: IntegrationData) => {
                this.isRequirePickup = eventContent.content['isRequirePickup'];
                this._handleIsRequirePickupChanged();
            });
        this._subscriptions.push(sub);
    }

    _registerBookingDataLoadedHandler() {
        const sub = this.parentIntegration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-form]onInitDataLoaded'
            )).subscribe((eventContent: IntegrationData) => {
                this.contactList = eventContent.content['bookingModel'].contacts;
                this.initOrganizations();
            });
        this._subscriptions.push(sub);
    }

    _handleIsRequirePickupChanged() {
        if (this.isRequirePickup) {
            this.contactList.push({
                organizationId: 0,
                organizationRole: OrganizationNameRole.Pickup,
                companyName: '',
                contactName: null,
                contactNumber: null,
                contactEmail: null,
                contactSequence: RoleSequence.PickupAddress,
                address: null
            });
            this.resetOrganizationRoleOptions();
            this.validateGrid(this.contactList.length - 1);
        } else {
            const currentPickupPartyIndex = this.contactList.findIndex(x => x.organizationRole === OrganizationNameRole.Pickup);
            this.onDeleteContact(currentPickupPartyIndex);
        }
    }

    _registerPOContactVerificationSettingsHandler() {
        const sub = this.parentIntegration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-general-info]applyPOContactVerificationSetting'
            )).subscribe((eventContent: IntegrationData) => {
                const consigneeVerification = eventContent.content['consigneeVerification'];
                const shipperVerification = eventContent.content['shipperVerification'];

                if (consigneeVerification === VerificationSetting.AsPerPO) {
                    this.viewOnlyOrgRoles.push(OrganizationNameRole.Consignee);
                }

                if (shipperVerification === VerificationSetting.AsPerPO) {
                    this.viewOnlyOrgRoles.push(OrganizationNameRole.Shipper);
                }
            });
        this._subscriptions.push(sub);
    }

    _registerIsNotifyPartyAsConsigneeChangedHandler() {
        const sub = this.parentIntegration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-general-info]isNotifyPartyAsConsigneeChanged'
            )).subscribe((eventContent: IntegrationData) => {
                this.isNotifyPartyAsConsignee = eventContent.content['isNotifyPartyAsConsignee'];
                if (this.isNotifyPartyAsConsignee) {
                    this.syncNotifyPartyWithConsignee();
                } else {
                    // reset to turn on the <Notify Party> option
                    this.resetOrganizationRoleOptions();
                }
            });
        this._subscriptions.push(sub);
    }

    _registerShipFromChanged() {
        const sub = this.parentIntegration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-general-info]shipFromPortValueChanged'
            )).subscribe((eventContent: IntegrationData) => {

                // Need setTimeout 1ms for below function to run after both ShipFrom changed and setOrgToContact function in app-po-fulfillment-form has finished
                setTimeout(() => {
                    if (this.agentAssignmentMode === AgentAssignmentMode.Default) {
                        const originDestinationContacts = this.contactList.filter(c => c.organizationRole === OrganizationNameRole.OriginAgent || c.organizationRole === OrganizationNameRole.DestinationAgent);
                        this.defaultContacts = cloneDeep(originDestinationContacts);
                    }

                    if (this.agentAssignmentMode === AgentAssignmentMode.Change) {
                        const newOriginContact = this.contactList.find(c => c.organizationRole === OrganizationNameRole.OriginAgent);
                        this.defaultContacts = this.defaultContacts.filter(c => c.organizationRole === OrganizationNameRole.DestinationAgent);
                        this.defaultContacts.push(cloneDeep(newOriginContact));
                        this.onChangeOriginContactModel(this.originContactModel.organizationId);

                        // Handle for miss highlight rows when change ShipFrom case
                        if (!this.originContactModel.isValidRow && this.isClickedChangeAgentButton) {
                            this.toggleHighlightOriginDestinationRows(true);
                        }
                    }
                }, 100);
            });
        this._subscriptions.push(sub);
    }

    _registerShipToChanged() {
        const sub = this.parentIntegration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-general-info]shipToPortValueChanged'
            )).subscribe((eventContent: IntegrationData) => {

                // Need setTimeout 1ms for below function to run after both ShipTo changed and setOrgToContact function in app-po-fulfillment-form has finished
                setTimeout(() => {
                    if (this.agentAssignmentMode === AgentAssignmentMode.Default) {
                        const originDestinationContacts = this.contactList.filter(c => c.organizationRole === OrganizationNameRole.OriginAgent || c.organizationRole === OrganizationNameRole.DestinationAgent);
                        this.defaultContacts = cloneDeep(originDestinationContacts);
                    }

                    if (this.agentAssignmentMode === AgentAssignmentMode.Change) {
                        const newDestinationContact = this.contactList.find(c => c?.organizationRole === OrganizationNameRole.DestinationAgent);
                        this.defaultContacts = this.defaultContacts.filter(c => c?.organizationRole === OrganizationNameRole.OriginAgent);
                        this.defaultContacts.push(cloneDeep(newDestinationContact));

                        this.onChangeDestinationContactModel(this.destinationContactModel.organizationId);

                        // Handle for miss highlight rows when change ShipTo case
                        if (!this.destinationContactModel.isValidRow && this.isClickedChangeAgentButton) {
                            this.toggleHighlightOriginDestinationRows(true);
                        }
                    }
                }, 100);
            });
        this._subscriptions.push(sub);
    }

    _registerModeOfTransportChanged() {
        let sub = this.parentIntegration$.pipe(
            delay(200),
            filter((eventContent: IntegrationData) =>
                (eventContent.name === '[po-fulfillment-general-info]modeOfTransportValueChanged'
                || eventContent.name === '[po-fulfillment-customer-po]add-first-po'
                || eventContent.name === '[po-fulfillment-customer-po]delete-last-po')
                && this.buyerCompliance
            )).subscribe((eventContent: IntegrationData) => {
                this.setAgentAssignmentDropdown(true);
            });
        this._subscriptions.push(sub);
    }

    /**
     * To handle as changing mode of Agent: default or change mode
     */
    _registerInitializedContactsHandler() {
        const sub = this.parentIntegration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[po-fulfillment-general-info]initializedContacts'
            )).subscribe((eventContent: IntegrationData) => {
                const originDestinationContacts = this.contactList.filter(c => c.organizationRole === OrganizationNameRole.OriginAgent || c.organizationRole === OrganizationNameRole.DestinationAgent);
                const originContact = this.contactList.find(c => c.organizationRole === OrganizationNameRole.OriginAgent);
                const destinationContact = this.contactList.find(c => c.organizationRole === OrganizationNameRole.DestinationAgent);

                this.defaultContacts = cloneDeep(originDestinationContacts);
                this.originContactModel = cloneDeep(originContact ? originContact : {}); // Handle for case ShipFrom is ManualInput/AllowOverride, will create new object
                this.destinationContactModel = cloneDeep(destinationContact ? destinationContact : {}); // Handle for case ShipTo ManualInput/AllowOverride, will create new object

            });
        this._subscriptions.push(sub);
    }

    public validateGrid(rowIndex) {
        const currentOrgRole = this.contactList[rowIndex].organizationRole;
        // Default validation rules
        this.validationRules[this.tabPrefix + 'organizationRole_' + rowIndex] = {
            'required': 'label.organizationRole'
        };
        // Apply validation rules based on business rules
        if (currentOrgRole === OrganizationNameRole.Pickup) {
            this.validationRules[this.tabPrefix + 'address_' + rowIndex] = {
                'required': 'label.address'
            };
        } else if (currentOrgRole === OrganizationNameRole.BillingParty) {
            this.validationRules[this.tabPrefix + 'companyName_' + rowIndex] = {
                'required': 'label.company'
            };
        } else {
            this.validationRules[this.tabPrefix + 'contactEmail_' + rowIndex] = {
                'required': 'label.contactEmail',
            };
            this.validationRules[this.tabPrefix + 'contactName_' + rowIndex] = {
                'required': 'label.contactName',
            };
            this.validationRules[this.tabPrefix + 'address_' + rowIndex] = {
                'required': 'label.address'
            };
            this.validationRules[this.tabPrefix + 'companyName_' + rowIndex] = {
                'required': 'label.company'
            };
        }
    }

    _validateAllGridInputs() {
        for (let i = 0; i < this.contactList.length; i++) {
            this.validateGrid(i);
        }
    }

    private initOrganizations() {
        const sub = this.service.getOrganizations()
            .subscribe(data => {
                this.activeOrgList = data;
                this.activeOrgList.sort((a, b) => (a.name > b.name) ? 1 : -1);

                if (this.userContext.currentUser.isInternal) {
                    this.shipperList = this.activeOrgList;
                    this.consigneeList = this.activeOrgList;
                } else {
                    const sub1 = this.consigneeAffiliateIds.subscribe(ids => {
                        if (ids) {
                            this.consigneeList = this.activeOrgList.filter(x => ids.indexOf(x.id) !== -1);
                        }
                    });
                    this._subscriptions.push(sub1);

                    this.shipperAffiliateIds = JSON.parse(this.userContext.currentUser.affiliates);
                    this.shipperList = this.activeOrgList.filter(x => this.shipperAffiliateIds.indexOf(x.id) !== -1);
                    const sub2 = this._isFulfilledFromPO.subscribe(val => {
                        if (val) {
                            const addMoreOrgIds = this.contactList.filter(x => x.organizationRole === OrganizationNameRole.Supplier ||
                                x.organizationRole === OrganizationNameRole.Delegation)
                                .map(x => x.organizationId);
                            const addMoreOrgs = this.activeOrgList.filter(x => addMoreOrgIds.indexOf(x.id) !== -1);
                            this.shipperList = this.shipperList.concat(addMoreOrgs.filter((item) => this.shipperList.indexOf(item) < 0));
                            this.shipperList.sort((a, b) => (a.name > b.name) ? 1 : -1);
                        }
                    });
                    this._subscriptions.push(sub2);
                }
                const sub3 = this.service.getOrganizationRoles().map(orgs => {
                    return orgs.filter(x => x.name !== OrganizationNameRole.Supplier
                        && x.name !== OrganizationNameRole.Principal
                        && x.name !== OrganizationNameRole.Delegation
                        && x.name !== OrganizationNameRole.Pickup
                        && x.name !== OrganizationNameRole.OriginAgent
                        && x.name !== OrganizationNameRole.DestinationAgent);
                }).subscribe(filteredData => {
                    this.originOrganizationRoleOptions = filteredData;
                    this.resetOrganizationRoleOptions();
                    this._validateAllGridInputs();
                });
                this._subscriptions.push(sub3);
            });
        this._subscriptions.push(sub);
    }

    private checkToShowDelegationContact() {
        if (this.userContext.currentUser && this.userContext.currentUser.organizationId) {
            const principalContact = this.contactList.find(x => x.organizationRole === OrganizationNameRole.Principal);
            if (principalContact && principalContact.organizationId === this.userContext.currentUser.organizationId) {
                this.contactList = this.contactList.filter(x => x.organizationRole !== OrganizationNameRole.Delegation);
            }
        }
    }

    private applyPOVerificationSetting() {
        if (this.buyerCompliance.purchaseOrderVerificationSetting) {
            const poVerification = this.buyerCompliance.purchaseOrderVerificationSetting;

            if (poVerification.shipperVerification === VerificationSetting.AsPerPO) {
                this.disabledOrgRoles.push(OrganizationNameRole.Shipper);
            }

            if (poVerification.consigneeVerification === VerificationSetting.AsPerPO) {
                this.disabledOrgRoles.push(OrganizationNameRole.Consignee);
            }
        }
    }

    contactValueChange(value: any, rowIndex: number) {
        const selectedOrg = this.activeOrgList.find(x => x.name.toLowerCase() === value.toLowerCase());

        if (selectedOrg) {
            const concatenatedAddress = this.service.concatenateAddressLines(selectedOrg.address, selectedOrg.addressLine2, selectedOrg.addressLine3, selectedOrg.addressLine4);
            this.contactList[rowIndex] = {
                organizationId: selectedOrg.id,
                organizationRole: this.contactList[rowIndex].organizationRole,
                companyName: selectedOrg.name,
                contactName: selectedOrg.contactName,
                contactNumber: selectedOrg.contactNumber,
                contactEmail: selectedOrg.contactEmail,
                weChatOrWhatsApp: selectedOrg.weChatOrWhatsApp,
                address: concatenatedAddress,
                isManualInput: this.contactList[rowIndex].isManualInput
            };
            this.validateContactNameInput(rowIndex);
            this.validateAddressInput(rowIndex);
            this.resetOrganizationRoleOptions();
            this.validateCompany(selectedOrg.name, rowIndex);
            this.validateContactNumber(selectedOrg.contactNumber, rowIndex);
            this.validateContactEmail(selectedOrg.contactEmail, rowIndex);
        } else {
            this.contactList[rowIndex].organizationId = 0;
            this.validateCompany(value, rowIndex);
        }
        this.validateCompanyNameOfNotifyAndAlsoParty(rowIndex);
    }

    validateCompany(companyName: string, rowIndex: number) {
        this.formErrors[this.tabPrefix + 'companyName_' + rowIndex + '_maxLength'] = null;
        const form = this.parentForm.form;
        if (companyName?.length > 0) {
            form.get(this.tabPrefix + 'companyName_' + rowIndex)?.setErrors(null);
            this.formErrors[this.tabPrefix + 'companyName_' + rowIndex] = null;
        }
        if (companyName?.length > 50) {
            this.formErrors[this.tabPrefix + 'companyName_' + rowIndex + '_maxLength'] = this.translateService.instant('validation.fieldLessThan', {
                fieldName: 'Company Name',
                maxLength: 50
            });
        }
    }

    validateCompanyNameOfNotifyAndAlsoParty(rowIndex: number) {
        const alsoNotifyRowIndex = this.contactList.findIndex(c => c.organizationRole === OrganizationNameRole.AlsoNotify);
        const form = this.parentForm.form;

        if (alsoNotifyRowIndex !== -1) {
            this.formErrors[this.tabPrefix + 'companyName_' + alsoNotifyRowIndex + '_theSameName'] = null;
        } else {
            this.formErrors[this.tabPrefix + 'companyName_' + rowIndex + '_theSameName'] = null;
        }

        const notifyParty = this.contactList.find(c => c.organizationRole === OrganizationNameRole.NotifyParty);
        const alsoNotify = this.contactList.find(c => c.organizationRole === OrganizationNameRole.AlsoNotify);
        const orgRole = this.contactList[rowIndex]?.organizationRole;

        if ((orgRole === this.organizationNameRole.NotifyParty || orgRole === this.organizationNameRole.AlsoNotify) && notifyParty?.companyName && alsoNotify?.companyName) {
            if (notifyParty.companyName.toLowerCase() === alsoNotify.companyName.toLowerCase()) {
                this.formErrors[this.tabPrefix + 'companyName_' + alsoNotifyRowIndex + '_theSameName'] = this.translateService.instant('validation.notTheSameNotifyParty', {
                    fieldName: 'Company Name',
                    theSameName: true
                });
            }
        }

        form.get(this.tabPrefix + 'companyName_' + alsoNotifyRowIndex)?.updateValueAndValidity();
    }

    validateContactNumber(contactNumber: any, rowIndex: number) {
        this.formErrors[this.tabPrefix + 'contactNumber_' + rowIndex + '_maxLength'] = null;
        if (contactNumber?.length > 30) {
            this.formErrors[this.tabPrefix + 'contactNumber_' + rowIndex + '_maxLength'] = this.translateService.instant('validation.fieldLessThan', {
                fieldName: 'Contact Number',
                maxLength: 30
            });
        }
    }

    validateContactEmail(contactEmail: any, rowIndex: number) {
        this.formErrors[this.tabPrefix + 'contactEmail_' + rowIndex + '_maxLength'] = null;
        const form = this.parentForm.form;
        if (contactEmail?.length > 0) {
            form.get(this.tabPrefix + 'contactEmail_' + rowIndex)?.setErrors(null);
            this.formErrors[this.tabPrefix + 'contactEmail_' + rowIndex] = null;
        }

        if (contactEmail?.length > 100) {
            this.formErrors[this.tabPrefix + 'contactEmail_' + rowIndex + '_maxLength'] = this.translateService.instant('validation.fieldLessThan', {
                fieldName: 'Contact Email',
                maxLength: 100
            });
        }
    }

    sortBySequence() {
        this.contactList.sort((a, b) => {
            if (a.contactSequence < b.contactSequence) {
                return -1;
            }
            if (a.contactSequence > b.contactSequence) {
                return 1;
            }
            return 0;
        })
    }

    /**Call when user input address manually */
    addressValueChange(value: any, rowIndex: number) {
        let dataRow = this.contactList[rowIndex];
        // Update organizationId
        const selectedOrg = this.activeOrgList.find(x => x.id === dataRow.organizationId);
        if (selectedOrg) {
            const concatenatedAddress = this.service.concatenateAddressLines(selectedOrg.address, selectedOrg.addressLine2, selectedOrg.addressLine3, selectedOrg.addressLine4);
            // OrganizationId of Supplier/Delegation should not be reset to 0
            dataRow.organizationId = (dataRow.address === concatenatedAddress
                || this.disabledOrgRoles.includes(dataRow.organizationRole)) ? dataRow.organizationId : 0;
        }

        this.handleAddressLines(dataRow, rowIndex);
    }

    handleAddressLines(dataRow, rowIndex) {
        const validLength = 50;
        let addressLines = dataRow.address.split('\n').filter(c => c);
        const addressLineEL = <HTMLInputElement>document.getElementById(`${this.tabPrefix}address_${rowIndex}`);
        if (addressLines.length > 0) {
            switch (addressLines.length) {
                case 1:
                    if (addressLines[0].length > validLength) {
                        const addressLine1 = addressLines[0].substring(0, addressLines[0].lastIndexOf(" "));
                        if (!addressLine1) break;
                        const newAddressLine2 = addressLines[0].substring(addressLines[0].lastIndexOf(" "), addressLines[0].length);

                        dataRow.address = addressLine1 + '\n' + newAddressLine2.trimStart();
                    }
                    break;

                case 2:
                    if (addressLines[1].length > validLength) {
                        const addressLine2 = addressLines[1].substring(0, addressLines[1].lastIndexOf(" "));
                        if (!addressLine2) break;
                        const addressLine3 = addressLines[1].substring(addressLines[1].lastIndexOf(" "), addressLines[1].length);
                        dataRow.address = addressLines[0] + '\n' + addressLine2.trimStart() + '\n' + addressLine3.trimStart();
                    }

                    const addressLine1 = addressLines[0].substring(0, addressLines[0].lastIndexOf(" "));
                    if (!addressLine1) break;
                    if (addressLines[0].length > validLength) {
                        const addressLine2 = addressLines[0].substring(addressLines[0].lastIndexOf(" "), addressLines[0].length);
                        dataRow.address = addressLine1 + '\n' + addressLine2.trimStart() + '\n' + addressLines[1];

                        setTimeout(() => {
                            let start = addressLineEL.selectionStart;
                            addressLineEL.setSelectionRange(start, start - 1 - (addressLines[1]?.length));
                        });
                    }
                    break;

                case 3:
                    if (addressLines[2].length > validLength) {
                        const addressLine3 = addressLines[2].substring(0, addressLines[2].lastIndexOf(" "));
                        if (!addressLine3) break;
                        const addressLine4 = addressLines[2].substring(addressLines[2].lastIndexOf(" "), addressLines[2].length);
                        dataRow.address = addressLines[0] + '\n' + addressLines[1] + '\n' + addressLine3.trimStart() + '\n' + addressLine4.trimStart();
                        break;
                    }

                    if (addressLines[1].length > validLength) {
                        const addressLine2 = addressLines[1].substring(0, addressLines[1].lastIndexOf(" "));
                        if (!addressLine2) break;
                        const addressLine3 = addressLines[1].substring(addressLines[1].lastIndexOf(" "), addressLines[1].length);
                        dataRow.address = addressLines[0] + '\n' + addressLine2.trimStart() + '\n' + addressLine3.trimStart() + '\n' + addressLines[2];

                        setTimeout(() => {
                            let start = addressLineEL.selectionStart;
                            addressLineEL.setSelectionRange(start, start - 1 - (addressLines[2]?.length));
                        });
                        break;
                    }

                    const firstAddressLine = addressLines[0].substring(0, addressLines[0].lastIndexOf(" "));
                    if (!firstAddressLine) break;
                    if (addressLines[0].length > validLength) {
                        const addressLine2 = addressLines[0].substring(addressLines[0].lastIndexOf(" "), addressLines[0].length);
                        dataRow.address = firstAddressLine.trimStart() + '\n' + addressLine2.trimStart() + '\n' + addressLines[1] + '\n' + addressLines[2];

                        setTimeout(() => {
                            let start = addressLineEL.selectionStart;
                            addressLineEL.setSelectionRange(start, start - 2 - (addressLines[1]?.length + addressLines[2]?.length));
                        });
                        break;
                    }

                case 4:
                    if (addressLines[3].length > validLength) {
                        const addressLine4 = addressLines[3].substring(0, addressLines[3].lastIndexOf(" "));
                        if (!addressLine4) break;
                        const addressLine5 = addressLines[3].substring(addressLines[3].lastIndexOf(" "), addressLines[3].length);
                        dataRow.address = addressLines[0] + '\n' + addressLines[1] + '\n' + addressLines[2] + '\n' + addressLine4.trimStart() + '\n' + addressLine5.trimStart();
                        break;
                    }

                    if (addressLines[2].length > validLength) {
                        const addressLine3 = addressLines[2].substring(0, addressLines[2].lastIndexOf(" "));

                        if (!addressLine3) break;

                        const addressLine4 = addressLines[2].substring(addressLines[2].lastIndexOf(" "), addressLines[2].length);
                        dataRow.address = addressLines[0] + '\n' + addressLines[1] + '\n' + addressLine3.trimStart() + '\n' + addressLine4.trimStart() + '\n' + addressLines[3];

                        setTimeout(() => {
                            let start = addressLineEL.selectionStart;
                            addressLineEL.setSelectionRange(start, start - addressLines[3]?.length - 1);
                        });
                        break;
                    }

                    if (addressLines[1].length > validLength) {
                        const addressLine2 = addressLines[1].substring(0, addressLines[1].lastIndexOf(" "));
                        if (!addressLine2) break;
                        const addressLine3 = addressLines[1].substring(addressLines[1].lastIndexOf(" "), addressLines[1].length);
                        dataRow.address = addressLines[0] + '\n' + addressLine2.trimStart() + '\n' + addressLine3.trimStart() + '\n' + addressLines[2] + '\n' + addressLines[3];

                        setTimeout(() => {
                            let start = addressLineEL.selectionStart;
                            addressLineEL.setSelectionRange(start, start - 2 - (addressLines[3].trim()?.length + addressLines[2].trim()?.length));
                        });
                        break;
                    }

                    if (addressLines[0].length > validLength) {
                        const addressLine1 = addressLines[0].substring(0, addressLines[0].lastIndexOf(" "));
                        if (!addressLine1) break;
                        const addressLine2 = addressLines[0].substring(addressLines[0].lastIndexOf(" "), addressLines[0].length);
                        dataRow.address = addressLine1.trimStart() + '\n' + addressLine2.trimStart() + '\n' + addressLines[1] + '\n' + addressLines[2] + '\n' + addressLines[3];

                        setTimeout(() => {
                            let start = addressLineEL.selectionStart;
                            addressLineEL.setSelectionRange(start, start - 3 - (addressLines[1]?.length + addressLines[3]?.length + addressLines[2]?.length));
                        });
                        break;
                    }
                default:
                    break;
            }
        }
    }

    setSelectionRange(input, selectionStart, selectionEnd) {
        if (input.setSelectionRange) {
            input.focus();
            input.setSelectionRange(selectionStart, selectionEnd);
        }
        else if (input.createTextRange) {
            var range = input.createTextRange();
            range.collapse(true);
            range.moveEnd('character', selectionEnd);
            range.moveStart('character', selectionStart);
            range.select();
        }
    }

    validateAddressInput(rowIndex) {
        const address = this.contactList[rowIndex]?.address;
        this.formErrors[this.tabPrefix + 'address_' + rowIndex + '_custom'] = null;
        this.parentForm.form.get(this.tabPrefix + 'address_' + rowIndex)?.setErrors(null);
        if (!StringHelper.isNullOrEmpty(address)) {
            let addressLines = address.split('\n');
            const oldLengthOfAddressLines = address.split('\n')?.filter(c => c)?.length;

            for (let index = 0; index < addressLines.length; index++) {
                if (StringHelper.isNullOrEmpty(addressLines[index])) {
                    addressLines.splice(index, 1);
                    index--;
                } else {
                    if (index === 3 && addressLines.length > 4) {
                        for (let i = index + 1; i < addressLines.length; i++) {
                            if (!StringHelper.isNullOrEmpty(addressLines[i])) {
                                addressLines[index] += addressLines[i];
                            }
                            addressLines.splice(i, 1);
                            i--;
                        }
                    }
                    const validLength = index !== 3 ? 50 : 50 - (oldLengthOfAddressLines - 4);
                    if (addressLines[index].length > validLength) {
                        this.formErrors[this.tabPrefix + 'address_' + rowIndex + '_custom'] = this.translateService.instant('validation.addressLineMustNotGreaterThan50Chars',
                            {
                                lineNumber: this.translateService.instant(`${index + 1}`)
                            });
                        break;
                    }
                }
            }
        }
    }

    validateContactNameInput(rowIndex) {
        const contactName = this.contactList[rowIndex]?.contactName;
        this.formErrors[this.tabPrefix + 'contactName_' + rowIndex + '_custom'] = null;
        const form = this.parentForm.form;
        if (contactName?.length > 0) {
            form.get(this.tabPrefix + 'contactName_' + rowIndex)?.setErrors(null);
            this.formErrors[this.tabPrefix + 'contactName_' + rowIndex] = null;
        }

        if (!StringHelper.isNullOrEmpty(contactName) && contactName.length > 30) {
            this.formErrors[this.tabPrefix + 'contactName_' + rowIndex + '_custom'] = this.translateService.instant('validation.fieldLessThan', {
                fieldName: 'Contact Name',
                maxLength: 30
            });
        }
    }

    roleValueChange(value: any, rowIndex: number) {
        this.contactFilterChange(value, rowIndex);
        const selectedContact = this.contactList[rowIndex];
        const selectedOrgRole = selectedContact.organizationRole;

        if ((!this.isValidOrgRole(value, selectedContact) && selectedContact.organizationId) || this.isForceReset ||
            selectedOrgRole === OrganizationNameRole.BillingParty ||
            selectedOrgRole === OrganizationNameRole.Pickup) {
            this.contactList.splice(rowIndex, 1, {
                organizationId: null,
                organizationRole: value,
                companyName: null,
                contactName: null,
                contactNumber: null,
                contactEmail: null,
                contactSequence: selectedContact.contactSequence,
                address: null,
                isManualInput: true
            });
            this.validateCompany(null, rowIndex);
            this.validateCompanyNameOfNotifyAndAlsoParty(rowIndex);
            this.validateAddressInput(rowIndex);
            this.validateContactNameInput(rowIndex);
            this.validateContactNumber(null, rowIndex);
            this.validateContactEmail(null, rowIndex);
            this.validateMultiEmailAddresses(null, this.tabPrefix + 'contactEmail_' + rowIndex);
        }

        if (selectedOrgRole === OrganizationNameRole.AlsoNotify) {
            this.validateCompanyNameOfNotifyAndAlsoParty(rowIndex);
        }

        this.resetOrganizationRoleOptions();
        this.validateGrid(rowIndex);
    }

    onRoleOptionsOpen(rowIndex) {
        const currentOrgRole = this.contactList[rowIndex].organizationRole;
        this.isForceReset = (currentOrgRole === OrganizationNameRole.BillingParty ||
            currentOrgRole === OrganizationNameRole.Pickup);
    }

    getAvailableRoleOptions(rowIndex) {
        let availableOrgRoles = this._filterContactList.map(x => x.organizationRole)
            .filter(x => x != null && x !== this.contactList[rowIndex].organizationRole);

        if (this.originOrganizationRoleOptions) {
            let orgRoleOptions = this.originOrganizationRoleOptions.filter(x => availableOrgRoles.indexOf(x.name) === -1);
            if (this.isNotifyPartyAsConsignee) {
                orgRoleOptions = orgRoleOptions.filter(x => x.name !== this.organizationNameRole.NotifyParty);
            }

            // If not existing notify party -> remove also notify party
            const isExistingNotifyParty = availableOrgRoles.findIndex(orgRole => orgRole === OrganizationNameRole.NotifyParty) !== -1;
            if (!isExistingNotifyParty) {
                orgRoleOptions = orgRoleOptions.filter(x => x.name !== this.organizationNameRole.AlsoNotify);
            }

            // If existing also notify party -> remove also notify party
            const isExistingAlsoNotifyParty = availableOrgRoles.findIndex(orgRole => orgRole === OrganizationNameRole.AlsoNotify) !== -1;
            if (isExistingAlsoNotifyParty) {
                orgRoleOptions = orgRoleOptions.filter(x => x.name !== this.organizationNameRole.AlsoNotify);
            }
            return orgRoleOptions;
        } else {
            return [];
        }
    }

    resetOrganizationRoleOptions() {
        for (let index = 0; index < this.contactList.length; index++) {
            if (StringHelper.isNullOrEmpty(this.contactList[index].removed) || !this.contactList[index].removed) {
                this.contactList[index].organizationRoleOptions = this.getAvailableRoleOptions(index);
                if (this.contactList[index].organizationRole === OrganizationNameRole.AlsoNotify) {
                    const notifyPartyIndex = this._filterContactList.findIndex(x => x.organizationRole === OrganizationNameRole.NotifyParty);
                    if (notifyPartyIndex === -1) {
                        this.contactList[index].organizationRole = null;
                    }
                }
            }
        }
    }

    isValidOrgRole(role: string, orgItem: any) {
        switch (role) {
            case OrganizationNameRole.Shipper:
                return this.shipperList.some(item => item.id === orgItem.organizationId);
            case OrganizationNameRole.Consignee:
                return this.consigneeList.some(item => item.id === orgItem.organizationId);
            case OrganizationNameRole.NotifyParty:
                return this.shipperList.concat(this.consigneeList.filter((item) => this.shipperList.indexOf(item) < 0)).some(item => item.id === orgItem.organizationId);
            default:
                return this.activeOrgList.some(item => item.id === orgItem.organizationId);
        }
    }

    contactFilterChange(value: any, rowIndex: number) {
        this.contactOptions[rowIndex] = [];
        if (value.length >= 3) {
            const selectedOrgRole = this.contactList[rowIndex].organizationRole;

            if (selectedOrgRole === OrganizationNameRole.Shipper) {
                this.contactOptions[rowIndex] = this.shipperList;
            } else if (selectedOrgRole === OrganizationNameRole.Consignee) {
                this.contactOptions[rowIndex] = this.consigneeList;
            } else if (selectedOrgRole === OrganizationNameRole.NotifyParty) {
                this.contactOptions[rowIndex] = this.shipperList.concat(this.consigneeList.filter((item) => this.shipperList.indexOf(item) < 0));
                this.contactOptions[rowIndex].sort((a, b) => (a.name > b.name) ? 1 : -1);
            } else {
                this.contactOptions[rowIndex] = this.activeOrgList;
            }

            this.contactOptions[rowIndex] = this.contactOptions[rowIndex].filter(x => x.name.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        }
    }

    get consigneeAffiliateIds(): Observable<any[]> {
        const customer = this.contactList.find(x => x.organizationRole === OrganizationNameRole.Principal);
        const customerId = customer && customer.organizationId;
        return customerId ? this.service.getAffiliateCodes(customerId) : EMPTY;
    }

    private get _filterContactList() {
        // exclude call row removed
        return this.contactList.filter(x => StringHelper.isNullOrEmpty(x.removed) || !x.removed);
    }

    getContactOptions(rowIndex) {
        return this.contactOptions[rowIndex];
    }

    addBlankContact() {
        if (this.contactList != null) {
            this.contactList.push({
                organizationId: null,
                organizationRole: null,
                companyName: null,
                contactName: null,
                contactNumber: null,
                contactEmail: null,
                contactSequence: this.NEW_CONTACT_SEQUENCE,
                address: null,
                isManualInput: true
            });
            this.resetOrganizationRoleOptions();
            this.validateGrid(this.contactList.length - 1);
        }
    }

    onDeleteContact(rowIndex: any) {
        // Update properties for current contact row
        const rowData = this.contactList[rowIndex];
        rowData.removed = true;

        // Reset formErrors and validationRules for this row
        const formErrorNames = [
            `organizationRole_${rowIndex}`,
            `companyName_${rowIndex}`,
            `companyName_${rowIndex}_maxLength`,
            `address_${rowIndex}`,
            `address_${rowIndex}_custom`,
            `contactName_${rowIndex}`,
            `contactName_${rowIndex}_maxLength`,
            `contactName_${rowIndex}_custom`,
            `contactEmail_${rowIndex}`,
            `contactEmail_${rowIndex}_custom`,
            `contactEmail_${rowIndex}_maxLength`
        ];

        // Clear current formErrors for current row
        Object.keys(this.formErrors)
            .filter(x => formErrorNames.some(y => x.startsWith(this.tabPrefix + y)))
            .map(x => {
                delete this.formErrors[x];
            });

        // Clear also validationRules for current row
        Object.keys(this.validationRules)
            .filter(x => formErrorNames.some(y => x.startsWith(this.tabPrefix + y)))
            .map(x => {
                delete this.validationRules[x];
            });

        // Call other business
        this.resetOrganizationRoleOptions();

        if (rowData.organizationRole === this.organizationNameRole.Consignee && this.isNotifyPartyAsConsignee) {
            const existingNotifyPartyIndex = this.contactList.findIndex(c => c.organizationRole === OrganizationNameRole.NotifyParty
                && (StringHelper.isNullOrEmpty(c.removed) || !c.removed));

            // callback to delete Notify Party
            this.onDeleteContact(existingNotifyPartyIndex);
        }

        this._clearContactRowData(rowData);
    }

    syncNotifyPartyWithConsignee() {
        const existingConsigneeIndex = this.contactList.findIndex(c => c.organizationRole === OrganizationNameRole.Consignee
            && (StringHelper.isNullOrEmpty(c.removed) || !c.removed));
        const existingNotifyPartyIndex = this.contactList.findIndex(c => c.organizationRole === OrganizationNameRole.NotifyParty
            && (StringHelper.isNullOrEmpty(c.removed) || !c.removed));

        if (existingConsigneeIndex !== -1) {
            const consignee = this.contactList[existingConsigneeIndex];
            if (existingNotifyPartyIndex !== -1) {
                this.contactList[existingNotifyPartyIndex].organizationId = consignee.organizationId;
                this.contactList[existingNotifyPartyIndex].companyName = consignee.companyName;
                this.contactList[existingNotifyPartyIndex].contactName = consignee.contactName;
                this.contactList[existingNotifyPartyIndex].contactNumber = consignee.contactNumber;
                this.contactList[existingNotifyPartyIndex].contactEmail = consignee.contactEmail;
                this.contactList[existingNotifyPartyIndex].address = consignee.address;
                this.resetOrganizationRoleOptions();
            } else {
                this.contactList.push({
                    organizationId: consignee.organizationId,
                    organizationRole: OrganizationNameRole.NotifyParty,
                    companyName: consignee.companyName,
                    contactName: consignee.contactName,
                    contactNumber: consignee.contactNumber,
                    contactEmail: consignee.contactEmail,
                    contactSequence: RoleSequence.NotifyParty,
                    address: consignee.address
                });
            }
        } else if (existingNotifyPartyIndex !== -1) {
            this.onDeleteContact(existingNotifyPartyIndex);
        }
    }

    private _clearContactRowData(contactData) {
        contactData.organizationRole = -1;
    }

    public rowCallback(args) {
        // Deleted row will be marked with removed property.
        return {
            'hide-row': args.dataItem.removed,
            'error-row': args.dataItem.isValidRow === false,
        };
    }

    isDisabledRow(rowIndex) {
        const selectedOrgRole = this.contactList[rowIndex].organizationRole;
        const isManualInput = this.contactList[rowIndex].isManualInput;

        if (this.isNotifyPartyAsConsignee &&
            selectedOrgRole === this.organizationNameRole.NotifyParty) {
            return true;
        }

        if (isManualInput) {
            return false;
        }

        if (this.viewOnlyOrgRoles.indexOf(selectedOrgRole) !== -1) {
            return true;
        }

        if (this.disabledOrgRoles.indexOf(selectedOrgRole) !== -1) {
            return true;
        }

        return false;
    }

    isViewOnlyRow(rowIndex): boolean {
        const selectedOrgRole = this.contactList[rowIndex].organizationRole;
        if (!this.isEditable || (this.isNotifyPartyAsConsignee
            && this.contactList[rowIndex].organizationRole === this.organizationNameRole.NotifyParty)) {
            return true;
        }
        if (this.viewOnlyOrgRoles.indexOf(selectedOrgRole) !== -1) {
            return true;
        }
        return false;
    }

    _validateContactsFromComplianceSettings() {
        const result = new ValidationData(ValidationDataType.Business, true);
        if (this.buyerCompliance) {
            const poVerification = this.buyerCompliance
                .purchaseOrderVerificationSetting;
            const hasShipperPerPO =
                poVerification.shipperVerification ===
                VerificationSetting.AsPerPO;
            const hasConsigneePerPO =
                poVerification.consigneeVerification ===
                VerificationSetting.AsPerPO;

            for (const contact of this.contactList) {
                if (
                    contact.isManualInput &&
                    contact.organizationRole === OrganizationNameRole.Shipper &&
                    hasShipperPerPO
                ) {
                    result.status = false;
                    this._validationResults.push(result);
                    return;
                }

                if (
                    contact.isManualInput &&
                    contact.organizationRole ===
                    OrganizationNameRole.Consignee &&
                    hasConsigneePerPO
                ) {
                    result.status = false;
                    this._validationResults.push(result);
                    return;
                }
            }
            this._validationResults.push(result);
            return;
        }
        this._validationResults.push(result);
        return;
    }

    isAllowInputContact(rowIndex): boolean {
        const selectedOrgRole = this.contactList[rowIndex].organizationRole;
        return selectedOrgRole === OrganizationNameRole.BillingParty || selectedOrgRole === OrganizationNameRole.Pickup;
    }

    _validateAllGridAddressInput() {
        let isValid = true;
        for (let index = 0; index < this.contactList.length; index++) {
            const contact = this.contactList[index];
            if (StringHelper.isNullOrEmpty(contact.removed) || !contact.removed) {
                this.validateAddressInput(index);

                if (isValid) {
                    const formErrorNames = [
                        `address_${index}_custom`
                    ];
                    Object.keys(this.formErrors)
                        .filter(x => formErrorNames.some(y => x.startsWith(this.tabPrefix + y)))
                        .map(x => {
                            if (!StringHelper.isNullOrEmpty(this.formErrors[x])) {
                                const result = new ValidationData(ValidationDataType.Business, true);
                                result.status = false;
                                result.message = this.translateService.instant('validation.mandatoryFieldsValidation');
                                this._validationResults.push(result);
                                isValid = false;
                            }
                        });
                }
            }
        }
    }

    _validateAllGridContactNameInput() {
        let isValid = true;

        for (let index = 0; index < this.contactList.length; index++) {
            const contact = this.contactList[index];
            if (StringHelper.isNullOrEmpty(contact.removed) || !contact.removed) {
                this.validateContactNameInput(index);

                if (isValid) {
                    const formErrorNames = [
                        `contactName_${index}_custom`
                    ];
                    Object.keys(this.formErrors)
                        .filter(x => formErrorNames.some(y => x.startsWith(this.tabPrefix + y)))
                        .map(x => {
                            if (!StringHelper.isNullOrEmpty(this.formErrors[x])) {
                                const result = new ValidationData(ValidationDataType.Business, true);
                                result.status = false;
                                result.message = this.translateService.instant('validation.mandatoryFieldsValidation');
                                this._validationResults.push(result);
                                isValid = false;
                            }
                        });
                }
            }
        }
    }

    _validateAllGridContactNumberInput() {
        let isValid = true;

        for (let index = 0; index < this.contactList.length; index++) {
            const contact = this.contactList[index];
            if (StringHelper.isNullOrEmpty(contact.removed) || !contact.removed) {
                this.validateContactNumber(contact.contactNumber, index);

                if (isValid) {
                    const formErrorNames = [
                        `contactNumber_${index}`
                    ];
                    Object.keys(this.formErrors)
                        .filter(x => formErrorNames.some(y => x.startsWith(this.tabPrefix + y)))
                        .map(x => {
                            if (!StringHelper.isNullOrEmpty(this.formErrors[x])) {
                                const result = new ValidationData(ValidationDataType.Business, true);
                                result.status = false;
                                result.message = this.translateService.instant('validation.mandatoryFieldsValidation');
                                this._validationResults.push(result);
                                isValid = false;
                            }
                        });
                }
            }
        }
    }

    _validateAllGridContactEmailInput() {
        let isValid = true;

        for (let index = 0; index < this.contactList.length; index++) {
            const contact = this.contactList[index];
            if (StringHelper.isNullOrEmpty(contact.removed) || !contact.removed) {
                this.validateContactEmail(contact.contactEmail, index);

                if (isValid) {
                    const formErrorNames = [
                        `contactEmail_${index}`
                    ];
                    Object.keys(this.formErrors)
                        .filter(x => formErrorNames.some(y => x.startsWith(this.tabPrefix + y)))
                        .map(x => {
                            if (!StringHelper.isNullOrEmpty(this.formErrors[x])) {
                                const result = new ValidationData(ValidationDataType.Business, true);
                                result.status = false;
                                result.message = this.translateService.instant('validation.mandatoryFieldsValidation');
                                this._validationResults.push(result);
                                isValid = false;
                            }
                        });
                }
            }
        }
    }

    _validateAllGridContactCompanyInput() {
        let isValid = true;

        for (let index = 0; index < this.contactList.length; index++) {
            const contact = this.contactList[index];
            if (StringHelper.isNullOrEmpty(contact.removed) || !contact.removed) {
                this.validateCompany(contact.companyName, index);
                if (isValid) {
                    const formErrorNames = [
                        `companyName_${index}_maxLength`
                    ];
                    Object.keys(this.formErrors)
                        .filter(x => formErrorNames.some(y => x.startsWith(this.tabPrefix + y)))
                        .map(x => {
                            if (!StringHelper.isNullOrEmpty(this.formErrors[x])) {
                                const result = new ValidationData(ValidationDataType.Business, true);
                                result.status = false;
                                result.message = this.translateService.instant('validation.mandatoryFieldsValidation');
                                this._validationResults.push(result);
                                isValid = false;
                            }
                        });
                }
            }
        }

        for (let index = 0; index < this.contactList.length; index++) {
            const contact = this.contactList[index];

            if ((StringHelper.isNullOrEmpty(contact.removed) || !contact.removed) && contact.organizationRole === OrganizationNameRole.AlsoNotify) {
                this.validateCompanyNameOfNotifyAndAlsoParty(index);
                if (isValid) {
                    const formErrorNames = [
                        `companyName_${index}_theSameName`
                    ];
                    Object.keys(this.formErrors)
                        .filter(x => formErrorNames.some(y => x.startsWith(this.tabPrefix + y)))
                        .map(x => {
                            if (!StringHelper.isNullOrEmpty(this.formErrors[x])) {
                                const result = new ValidationData(ValidationDataType.Business, true);
                                result.status = false;
                                result.message = this.translateService.instant('validation.mandatoryFieldsValidation');
                                this._validationResults.push(result);
                                isValid = false;
                            }
                        });
                }
            }
        }
    }

    validateTab(): Array<ValidationData> {
        this._validationResults = [];
        this._validateAllGridAddressInput();
        this._validateAllGridContactNameInput();
        this._validateAllGridContactNumberInput();
        this._validateAllGridContactEmailInput();
        this._validateAllGridContactCompanyInput();
        return this._validationResults;
    }

    onSetDefaultAgent() {
        this.isDefaultAgentSelected = true;
        this.changeAgentMode.emit(AgentAssignmentMode.Default);

        this.shipFromPortValueChangeEmitter();
        this.shipToPortValueChangeEmitter();

        this.removeOriginDestinationRows();
        this.toggleHighlightOriginDestinationRows(false);
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

    removeErrorOfRow(index: number) {
        const formErrorNames = [
            `companyName_${index}`,
        ];

        // Clear current formErrors for current row
        Object.keys(this.formErrors)
            .filter(x => formErrorNames.some(y => x.startsWith(this.tabPrefix + y)))
            .map(x => {
                delete this.formErrors[x];
            });

        // Clear also validationRules for current row
        Object.keys(this.validationRules)
            .filter(x => formErrorNames.some(y => x.startsWith(this.tabPrefix + y)))
            .map(x => {
                delete this.validationRules[x];
            });
    }

    removeOriginDestinationRows() {
        // Either ShipFrom or ShipTo has inputted
        if (this.defaultContacts.length === 1) {
            if (this.defaultContacts[0]?.organizationRole === OrganizationNameRole.OriginAgent) {
                for (let index = 0; index < this.contactList.length; index++) {
                    if (this.contactList[index].organizationRole === OrganizationNameRole.DestinationAgent) {
                        this.onDeleteContact(index);
                        return;
                    }
                }
            } else {
                for (let index = 0; index < this.contactList.length; index++) {
                    if (this.contactList[index].organizationRole === OrganizationNameRole.OriginAgent) {
                        this.onDeleteContact(index);
                        return;
                    }
                }
            }
            return;
        }

        // ShipFrom and ShipTo have not inputted yet
        if (this.defaultContacts.length === 0) {
            for (let index = 0; index < this.contactList.length; index++) {
                if (this.contactList[index].organizationRole === OrganizationNameRole.OriginAgent || this.contactList[index].organizationRole === OrganizationNameRole.DestinationAgent) {
                    this.removeErrorOfRow(!errorRowIndex ? index : errorRowIndex + 1);
                    this.contactList.splice(index, 1);
                    var errorRowIndex = index;
                    index--;
                }
            }
        }

        // ShipFrom and Shipto have inputted, just clear errors of change mode
        if (this.defaultContacts.length === 2) {
            for (let index = 0; index < this.contactList.length; index++) {
                if (this.contactList[index].organizationRole === OrganizationNameRole.OriginAgent || this.contactList[index].organizationRole === OrganizationNameRole.DestinationAgent) {
                    this.removeErrorOfRow(index);
                }
            }
        }
    }

    onChangeAgent() {
        this.isClickedChangeAgentButton = true;
        if (this.isDefaultAgentSelected) {
            this.addOriginOrDestinationRows();
            this.setValueForOriginDestinationModel();
            this.changeAgentMode.emit(AgentAssignmentMode.Change);
        }

        this.isDefaultAgentSelected = false;
        this.toggleHighlightOriginDestinationRows(true);

        for (let index = 0; index < this.contactList.length; index++) {
            if (this.contactList[index].organizationRole === OrganizationNameRole.OriginAgent) {
                this.validateCompany(this.contactList[index].companyName, index);
                this.validateAddressInput(index);
                this.validateContactNameInput(index);
                this.validateContactNumber(this.contactList[index].contactNumber, index);
                this.validateContactEmail(this.contactList[index].contactEmail, index);
            }

            if (this.contactList[index].organizationRole === OrganizationNameRole.DestinationAgent) {
                this.validateCompany(this.contactList[index].companyName, index);
                this.validateAddressInput(index);
                this.validateContactNameInput(index);
                this.validateContactNumber(this.contactList[index].contactNumber, index);
                this.validateContactEmail(this.contactList[index].contactEmail, index);
            }
        }
    }

    /**
     * when change from default to change mode, AgentAssignment selection will binding data by defaultContacts
     */
    setValueForOriginDestinationModel() {
        const originDefaultContactPopulated = this.contactList.find(c => c.organizationRole === OrganizationNameRole.OriginAgent);
        const destinationDefaultContactPopulated = this.contactList.find(c => c.organizationRole === OrganizationNameRole.DestinationAgent);

        // Handle for case populate ShipFrom but auto create shipment is No, therefore there will be no Agent in dropdown
        const originDefaultContact = this.originAgentAssignmentSource.find(c => c.agentOrganizationId === originDefaultContactPopulated?.organizationId);
        if (!originDefaultContact) {
            const contactNeedUpdate = this.contactList.find(c => c.organizationRole === OrganizationNameRole.OriginAgent);
            this.updateContactRow(contactNeedUpdate, null);
            this.originContactModel.organizationId = originDefaultContact?.agentOrganizationId;
            this.originContactModel.companyName = originDefaultContact?.agentOrganizationName;
        } else {
            this.originContactModel.organizationId = originDefaultContactPopulated?.organizationId;
            this.originContactModel.companyName = originDefaultContactPopulated?.companyName;
        }
        this.originContactModel.isValidRow = false;

        this.destinationContactModel.organizationId = destinationDefaultContactPopulated?.organizationId;
        this.destinationContactModel.companyName = destinationDefaultContactPopulated?.companyName;
        this.destinationContactModel.isValidRow = false;
    }

    toggleHighlightOriginDestinationRows(isHighlight: boolean) {
        for (const contact of this.contactList) {
            if (contact.organizationRole === OrganizationNameRole.OriginAgent || contact.organizationRole === OrganizationNameRole.DestinationAgent) {
                contact.isValidRow = !isHighlight;
            }
        }
    }

    addOriginOrDestinationRows() {
        const isAlreadyExistOriginAgent = this.contactList.some(c => c.organizationRole === OrganizationNameRole.OriginAgent);
        if (!isAlreadyExistOriginAgent) {
            this.contactList.push(
                {
                    organizationRole: OrganizationNameRole.OriginAgent,
                    companyName: null,
                    organizationId: null,
                    contactSequence: RoleSequence.OriginAgent,
                    isValidRow: false,
                }
            );
            this.validateGrid(this.contactList.length - 1);
        }

        const isAlreadyExistDestinationAgent = this.contactList.some(c => c.organizationRole === OrganizationNameRole.DestinationAgent);
        if (!isAlreadyExistDestinationAgent) {
            this.contactList.push(
                {
                    organizationRole: OrganizationNameRole.DestinationAgent,
                    companyName: null,
                    organizationId: null,
                    contactSequence: RoleSequence.DestinationAgent,
                    isValidRow: false,
                }
            );
            this.validateGrid(this.contactList.length - 1);
        }
    }

    validateMultiEmailAddresses(inputValue: string, inputName: string) {
        const errorName = inputName + '_custom';
        let isValid = true;
        if (!StringHelper.isNullOrEmpty(inputValue)) {
            const patternEmail = EmailValidationPattern;
            const emails = inputValue.split(',');
            emails?.map(email => {
                if (!patternEmail.test(email)) {
                    isValid = false;
                }
            });
        }
        if (isValid) {
            delete this.formErrors[errorName];
        } else {
            this.formErrors[errorName] = this.translateService.instant('validation.invalidMultipleEmails');
        }

    }

    onFocusOutCompany(companyName, rowIndex) {
        this.validateCompany(companyName, rowIndex);
        this.validateCompanyNameOfNotifyAndAlsoParty(rowIndex);
    }

    onFocusOutAddress(rowIndex) {
        this.validateAddressInput(rowIndex)
    }

    onFocusOutContactName(rowIndex) {
        this.validateContactNameInput(rowIndex);
    }

    onFocusOutContactNumber(contactNumber, rowIndex) {
        this.validateContactNumber(contactNumber, rowIndex);
    }

    onFocusOutContactEmail(contactEmail, rowIndex) {
        this.validateContactEmail(contactEmail, rowIndex);
    }

    isVisibleField(moduleId: ViewSettingModuleIdType, fieldId: string): boolean {
        if (!this.isViewMode) { // apply for view mode only
            return true;
        }

        return !FormHelper.isHiddenColumn(this.bookingModel.viewSettings, moduleId, fieldId);
    }

    ngOnDestroy() {
        this._subscriptions.map(x => x.unsubscribe());
    }
}
