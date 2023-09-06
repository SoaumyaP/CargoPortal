import { Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges } from '@angular/core';
import { ControlContainer, NgForm } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { EMPTY, Observable, Subscription } from 'rxjs';
import { tap } from 'rxjs/operators';
import { AgentType, OrganizationNameRole, OrganizationType, StringHelper, UserContextService } from 'src/app/core';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';
import { OrganizationReferenceDataModel } from 'src/app/core/models/organization.model';
import { RoutingOrderContactModel } from 'src/app/core/models/routing-order.model';
import { OrgContactPreferenceService } from '../../org-contact-preference/org-contact-preference.service';
import { OrgContactPreferenceModel } from '../../org-contact-preference/org-contact.model';
import { RoutingOrderFormService } from '../routing-order-form/routing-order-form.service';

@Component({
  selector: 'app-routing-order-contact',
  templateUrl: './routing-order-contact.component.html',
  styleUrls: ['./routing-order-contact.component.scss'],
  viewProviders: [{ provide: ControlContainer, useExisting: NgForm }]
})
export class RoutingOrderContactComponent implements OnInit, OnChanges, OnDestroy {
  @Input('data')
  contactList: RoutingOrderContactModel[];
  @Input()
  isViewMode: boolean;
  @Input()
  isAddMode: boolean;
  @Input()
  isEditMode: boolean;

  // It is prefix for formErrors and validationRules
  // Use it to detect what tab contains invalid data
  @Input()
  tabPrefix: string;
  @Input()
  formErrors: any;
  @Input()
  validationRules: any;
  @Input()
  parentForm: any;

  defaultValue: string = DefaultValue2Hyphens;
  originOrganizationRoleOptions: any[];
  contactOptions = [];
  shipperList = [];
  consigneeList = [];
  originAgentList = [];

  orgContactPreferenceList: Array<OrgContactPreferenceModel> = [];
  orgContactPreferenceOptions = [];
  
  activeOrgList: any[];
  shipperAffiliateIds: any[];
  isInternalUser: any;
  // Store all subscriptions, then should un-subscribe at the end
  private _subscriptions: Array<Subscription> = [];

  currentUser: any;

  constructor(
    private routingOrderFormService: RoutingOrderFormService,
    private userContext: UserContextService,
    public translateService: TranslateService,
    public orgContactPreferenceService: OrgContactPreferenceService) {
    this.userContext.getCurrentUser().subscribe(user => {
      if (user) {
        this.currentUser = user;
        if (!this.currentUser.isInternal) {
          orgContactPreferenceService.getByOrganization(this.currentUser.organizationId)
            .pipe(tap(
              data => data.map(x => this.orgContactPreferenceOptions.push({ name: x.companyName }))
            )).subscribe(
              (data: Array<OrgContactPreferenceModel>) => {
                this.orgContactPreferenceList = data;
              }
            );
        }
      }
    });
    
    this.isInternalUser = this.userContext?.currentUser?.isInternal;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.saveAsDraft?.currentValue === false) {
      this._triggerGridValidation();
    }

    // reset org-role option whenever the contact list changes.
    if (changes.contactList?.currentValue) {
      this.resetOrganizationRoleOptions();
      this.populateShipperCompany();

      this.contactList.forEach(element => {
        element.address = this.concatenateAddressLines(element.addressLine1, element.addressLine2, element.addressLine3, element.addressLine4);
      });
    }
  }

  ngOnInit(): void {
    this.initOrganizations();
  }

  populateShipperCompany() {
    if (this.currentUser.isInternal && this.isEditMode) {
      const shipperContact = this.contactList.find(c => c.organizationRole === OrganizationNameRole.Shipper);
      const indexOfshipperContact = this.contactList.findIndex(c => c.organizationRole === OrganizationNameRole.Shipper);
      if (shipperContact && indexOfshipperContact >= 0) {
        this.contactAutoCompleteFilterChange(shipperContact.companyName, indexOfshipperContact);
      }
    }
  }

  contactAutoCompleteFilterChange(value: any, rowIndex: number) {
    let rowContact = this.contactList[rowIndex];

    if (value.length >= 3) {
      this.contactOptions[rowIndex] = this.getContactOptionsByRole(rowContact.organizationRole);
      this.contactOptions[rowIndex] = this.contactOptions[rowIndex].filter(x => x.name.toLowerCase().indexOf(value.toLowerCase()) !== -1);
    } else {
      this.contactOptions[rowIndex] = [];
    }
  }

  public validateGrid(rowIndex: number) {
    if (rowIndex < 0) {
      return;
    }
    const { organizationRole, isManualInput } = this.contactList[rowIndex];

    // Default validation rules
    this.validationRules[this.tabPrefix + 'organizationRole_' + rowIndex] = {
      'required': 'label.organizationRole'
    };

    if (isManualInput != null && isManualInput === true) {
      // Apply validation rules based on business rules
      if (organizationRole === OrganizationNameRole.Pickup) {
        this.validationRules[this.tabPrefix + 'address_' + rowIndex] = {
          'required': 'label.address'
        };
      } else if (organizationRole === OrganizationNameRole.BillingParty) {
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
  }

  _triggerGridValidation() {
    for (let i = 0; i < this.contactList.length; i++) {
      this.validateGrid(i);

      // set organizationId 0 => Null to hit validation
      if (this.contactList[i]?.organizationId === 0) {
        this.contactList[i].organizationId = null;
      }
    }
  }

  private initOrganizations() {
    const sub = this.routingOrderFormService.getOrganizations()
      .subscribe((data: Array<OrganizationReferenceDataModel>) => {
        this.activeOrgList = data;
        this.activeOrgList.sort((a, b) => (a.name > b.name) ? 1 : -1);

        if (this.userContext.currentUser.isInternal) {
          this.shipperList = this.activeOrgList;
          this.consigneeList = this.activeOrgList;
          this.populateShipperCompany();
        } else {
          const sub1 = this.consigneeAffiliateIds.subscribe(ids => {
            if (ids) {
              this.consigneeList = this.activeOrgList.filter(x => ids.indexOf(x.id) !== -1);
            }
          });
          this._subscriptions.push(sub1);

          this.shipperAffiliateIds = JSON.parse(this.userContext.currentUser.affiliates);
          this.shipperList = this.activeOrgList.filter(x => this.shipperAffiliateIds.indexOf(x.id) !== -1);
        }

        this.originAgentList = this.activeOrgList.filter(x => x.organizationType === OrganizationType.Agent &&
          (x.agentType === AgentType.Both || x.agentType == AgentType.Export));

        let originAgentContactIndex = this.contactList.findIndex(x => !x.removed &&
          StringHelper.caseIgnoredCompare(x.organizationRole, OrganizationNameRole.OriginAgent));
        if (originAgentContactIndex !== -1) {
          this.contactOptions[originAgentContactIndex] = this.originAgentList;
        }

        const sub3 = this.routingOrderFormService.getOrganizationRoles().map((orgs: any[]) => {
          return orgs.filter(x => x.name !== OrganizationNameRole.Supplier
            && x.name !== OrganizationNameRole.Principal
            && x.name !== OrganizationNameRole.Delegation
            && x.name !== OrganizationNameRole.Pickup
            && x.name !== OrganizationNameRole.OriginAgent
            && x.name !== OrganizationNameRole.DestinationAgent);
        }).subscribe(filteredData => {
          this.originOrganizationRoleOptions = filteredData;
          this.resetOrganizationRoleOptions();
          this._triggerGridValidation();
        });
        this._subscriptions.push(sub3);
      });
    this._subscriptions.push(sub);
  }

  concatenateAddressLines(address: string, addressLine2?: string, addressLine3?: string, addressLine4?: string) {
    let concatenatedAddress = '';
    if (!StringHelper.isNullOrEmpty(address)) {
      concatenatedAddress += address;
    }
    if (!StringHelper.isNullOrEmpty(addressLine2)) {
      concatenatedAddress += (!StringHelper.isNullOrWhiteSpace(concatenatedAddress) ? '\n' : '') + addressLine2;
    }
    if (!StringHelper.isNullOrEmpty(addressLine3)) {
      concatenatedAddress += (!StringHelper.isNullOrWhiteSpace(concatenatedAddress) ? '\n' : '') + addressLine3;
    }
    if (!StringHelper.isNullOrEmpty(addressLine4)) {
      concatenatedAddress += (!StringHelper.isNullOrWhiteSpace(concatenatedAddress) ? '\n' : '') + addressLine4;
    }
    return concatenatedAddress;
  }

  getAvailableRoleOptions(rowIndex) {
    // Get current selected/used organization roles except from the current row
    const availableOrgRoles = this._filterContactList.map(x => x.organizationRole)
      .filter(x => x != null && x !== this.contactList[rowIndex].organizationRole);

    if (this.originOrganizationRoleOptions) {
      // Get other organization role options
      let orgRoleOptions = this.originOrganizationRoleOptions.filter(x => availableOrgRoles.indexOf(x.name) === -1);

      // If not existing notify party -> remove also notify party
      const isExistingNotifyParty = availableOrgRoles.findIndex(orgRole => orgRole === OrganizationNameRole.NotifyParty) !== -1;
      if (!isExistingNotifyParty) {
        orgRoleOptions = orgRoleOptions.filter(x => x.name !== OrganizationNameRole.AlsoNotify);
      }

      // If existing also notify party -> remove also notify party
      const isExistingAlsoNotifyParty = availableOrgRoles.findIndex(orgRole => orgRole === OrganizationNameRole.AlsoNotify) !== -1;
      if (isExistingAlsoNotifyParty) {
        orgRoleOptions = orgRoleOptions.filter(x => x.name !== OrganizationNameRole.AlsoNotify);
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

  private getContactOptionsByRole(role: string): any[] {
    let result = [];
    if (!role) {
      return result;
    }
    if (this.currentUser.isInternal || StringHelper.caseIgnoredCompare(role, OrganizationNameRole.OriginAgent)) {
      switch (role.toLowerCase()) {
        case OrganizationNameRole.Shipper.toLowerCase():
          result = this.shipperList;
          break;
        case OrganizationNameRole.Consignee.toLowerCase():
          result = this.consigneeList;
          break;
        case OrganizationNameRole.NotifyParty.toLowerCase():
          result = this.shipperList.concat(this.consigneeList.filter((item) => this.shipperList.indexOf(item) < 0));
          result.sort((a, b) => (a.name > b.name) ? 1 : -1);
          break;
        case OrganizationNameRole.OriginAgent.toLowerCase():
          result = this.originAgentList;
          break;
        default:
          result = this.activeOrgList;
          break;
      }
    } else { // If external user => refer to OrgContactPreferences
      result = this.orgContactPreferenceOptions;
    }
    return result;
  }

  get consigneeAffiliateIds(): Observable<any[]> {
    const customer = this.contactList.find(x => x.organizationRole === OrganizationNameRole.Principal);
    const customerId = customer && customer.organizationId;
    return customerId ? this.routingOrderFormService.getAffiliateCodes(customerId) : EMPTY;
  }

  private get _filterContactList() {
    // exclude call row removed
    return this.contactList.filter(x => StringHelper.isNullOrEmpty(x.removed) || !x.removed);
  }

  getContactOptions(rowIndex) {
    return this.contactOptions[rowIndex];
  }

  public rowCallback(args) {
    // Deleted row will be marked with removed property.
    return {
      'hide-row': args.dataItem.removed || args.dataItem.hidden,
      'error-row': args.dataItem.isValidRow === false,
    };
  }

  ngOnDestroy() {
    this._subscriptions.map(x => x.unsubscribe());
  }
}
