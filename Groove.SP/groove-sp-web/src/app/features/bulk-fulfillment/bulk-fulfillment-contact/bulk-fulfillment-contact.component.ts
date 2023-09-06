import { Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges } from '@angular/core';
import { ControlContainer, NgForm } from '@angular/forms';
import { faInfoCircle, faPlus } from '@fortawesome/free-solid-svg-icons';
import { TranslateService } from '@ngx-translate/core';
import { EMPTY, Observable, Subject, Subscription } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { AgentType, OrganizationNameRole, OrganizationRole, OrganizationType, Roles, RoleSequence, StringHelper, UserContextService, ValidationDataType, ViewSettingModuleIdType } from 'src/app/core';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { DefaultValue2Hyphens, EmailValidationPattern } from 'src/app/core/models/constants/app-constants';
import { ValidationData } from 'src/app/core/models/forms/validation-data';
import { OrganizationReferenceDataModel } from 'src/app/core/models/organization.model';
import { ViewSettingModel } from 'src/app/core/models/viewSetting.model';
import { OrgContactPreferenceService } from '../../org-contact-preference/org-contact-preference.service';
import { OrgContactPreferenceModel } from '../../org-contact-preference/org-contact.model';
import { BulkFulfillmentFormService } from '../bulk-fulfillment-form/bulk-fulfillment-form.service';

@Component({
  selector: 'app-bulk-fulfillment-contact',
  templateUrl: './bulk-fulfillment-contact.component.html',
  styleUrls: ['./bulk-fulfillment-contact.component.scss'],
  viewProviders: [{ provide: ControlContainer, useExisting: NgForm }]
})
export class BulkFulfillmentContactComponent implements OnInit, OnChanges, OnDestroy {
  @Input('data')
  contactList: any[];
  @Input()
  isContactTabEditable: boolean;
  @Input()
  isAddMode: boolean;
  @Input()
  isEditMode: boolean;
  @Input()
  isCopyMode: boolean;
  @Input()
  saveAsDraft: boolean;
  // It is prefix for formErrors and validationRules
  // Use it to detect what tab contains invalid data
  @Input()
  tabPrefix: string;
  @Input()
  formErrors: any;
  @Input()
  validationRules: any;
  @Input()
  isShipperPickup: boolean;
  @Input()
  isNotifyPartyAsConsignee: boolean;
  @Input() parentForm: any;
  @Input() viewSettings: ViewSettingModel[];
  @Input() isViewMode: boolean;

  defaultValue: string = DefaultValue2Hyphens;
  readonly organizationNameRole = OrganizationNameRole;
  viewSettingModuleIdType = ViewSettingModuleIdType;
  formHelper = FormHelper;
  stringHelper = StringHelper;
  originOrganizationRoleOptions: any[];
  contactOptions = [];
  shipperList = [];
  consigneeList = [];
  originAgentList = [];

  orgContactPreferenceList: Array<OrgContactPreferenceModel> = [];
  orgContactPreferenceOptions = [];

  isForceReset: boolean = false;
  activeOrgList: any[];
  shipperAffiliateIds: any[];
  isInternalUser: any;
  faPlus = faPlus;
  // Store all subscriptions, then should un-subscribe at the end
  private _subscriptions: Array<Subscription> = [];
  public addressValueKeyUp$ = new Subject<string>();
  public contactNameValueKeyUp$ = new Subject<string>();

  currentUser: any;
  faInfoCircle = faInfoCircle;

  constructor(
    private bulkFulfillmentFormService: BulkFulfillmentFormService,
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
    this.isInternalUser = this.userContext?.currentUser?.isInternal;
    this._subscriptions.push(contactNameValueSub);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.saveAsDraft?.currentValue === false) {
      this._triggerGridValidation();
    }

    // reset org-role option whenever the contact list changes.
    if (changes.contactList?.currentValue) {
      this.resetOrganizationRoleOptions();
      this.populateShipperCompany();
    }

    this.updateContactGridState();
  }

  ngOnInit(): void {
    if (this.isAddMode && !this.isCopyMode) {
      this.initContacts();
      this.resetOrganizationRoleOptions();
    }
    this.initOrganizations();
  }

  async initContacts() {
    this.contactList = [];
    // Init shipper
    this.addNewContact(OrganizationNameRole.Shipper);

    // Init consignee
    this.addNewContact(OrganizationNameRole.Consignee);

    // Init supplier
    if (!this.currentUser.isInternal && (this.currentUser.role.id === Roles.Shipper || this.currentUser.role.id === Roles.Factory)) {
      const rs = await this.bulkFulfillmentFormService.getOrganizationsByIds([this.currentUser.organizationId]).toPromise();
      if (rs.length > 0) {
        const data = rs[0];
        await this.setOrgToContact(data, OrganizationNameRole.Supplier);
        this.populateShipperContact(data);
      }
    } else {
      this.addNewContact(OrganizationNameRole.Supplier);
    }

    // Init origin agent
    this.addNewContact(OrganizationNameRole.OriginAgent);
  }

  populateShipperContact(org: any) {
    const shipperContact = this.contactList.find(c => c.organizationRole === OrganizationNameRole.Shipper);

    const concatenatedAddress = this.concatenateAddressLines(org.address, org.addressLine2, org.addressLine3, org.addressLine4);
    shipperContact.organizationId = org.id;
    shipperContact.organizationRole = OrganizationNameRole.Shipper;
    shipperContact.companyName = org.name;
    shipperContact.contactName = org.contactName;
    shipperContact.contactNumber = org.contactNumber;
    shipperContact.contactEmail = org.contactEmail;
    shipperContact.weChatOrWhatsApp = org.weChatOrWhatsApp;
    shipperContact.address = concatenatedAddress;
  }

  populateShipperCompany() {
    if (this.currentUser.isInternal && (this.isEditMode || this.isCopyMode)) {
      const shipperContact = this.contactList.find(c => c.organizationRole === OrganizationNameRole.Shipper);
      const indexOfshipperContact = this.contactList.findIndex(c => c.organizationRole === OrganizationNameRole.Shipper);
      if (shipperContact && indexOfshipperContact >= 0) {
        this.contactAutoCompleteFilterChange(shipperContact.companyName, indexOfshipperContact);
      }
    }
  }

  async setOrgToContact(organization: OrganizationReferenceDataModel, orgRoleName: string) {
    const concatenatedAddress = this.concatenateAddressLines(organization.address, organization.addressLine2, organization.addressLine3, organization.addressLine4);
    this.contactList.push({
      organizationId: organization.id,
      organizationRole: orgRoleName,
      organizationCode: organization.code,
      companyName: organization.name,
      contactName: organization.contactName,
      contactNumber: organization.contactNumber,
      contactEmail: organization.contactEmail,
      weChatOrWhatsApp: organization.weChatOrWhatsApp,
      address: concatenatedAddress
    });
  }

  addNewContact(organizationRole: string): void {
    this.contactList.push({
      organizationId: null,
      organizationRole: organizationRole,
      companyName: null,
      contactName: null,
      contactNumber: null,
      contactEmail: null,
      address: null
    });
  }

  isRequirePickupValueChanged(event): void {
    if (this.isShipperPickup) {
      this.contactList.push({
        organizationId: 0,
        organizationRole: OrganizationNameRole.Pickup,
        companyName: '',
        contactName: null,
        contactNumber: null,
        contactEmail: null,
        address: null
      });
      this.resetOrganizationRoleOptions();
      this.validateGrid(this.contactList.length - 1);
    } else {
      const currentPickupPartyIndex = this.contactList.findIndex(x => x.organizationRole === OrganizationNameRole.Pickup);
      this.onDeleteContact(currentPickupPartyIndex);
    }
  }

  public validateGrid(rowIndex: number) {
    if (rowIndex < 0) {
      return;
    }
    const { currentOrgRole, isManualInput } = this.contactList[rowIndex];

    // Default validation rules
    this.validationRules[this.tabPrefix + 'organizationRole_' + rowIndex] = {
      'required': 'label.organizationRole'
    };

    if (!this.saveAsDraft || (isManualInput != null && isManualInput === true)) {
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

  onFocusOutCompany(companyName: string, rowIndex: number) {
    this.validateCompany(companyName, rowIndex);
    this.validateCompanyNameOfNotifyAndAlsoParty(rowIndex);
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
    if (contactEmail?.length > 100) {
      this.formErrors[this.tabPrefix + 'contactEmail_' + rowIndex + '_maxLength'] = this.translateService.instant('validation.fieldLessThan', {
        fieldName: 'Contact Email',
        maxLength: 100
      });
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
    const sub = this.bulkFulfillmentFormService.getOrganizations()
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

        const sub3 = this.bulkFulfillmentFormService.getOrganizationRoles().map((orgs: any[]) => {
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

  contactValueChange(value: any, rowIndex: number) {
    let rowContact = this.contactList[rowIndex];
    let selectedOrg = null;
    const isOriginAgentContact = StringHelper.caseIgnoredCompare(rowContact.organizationRole, OrganizationNameRole.OriginAgent);
    // reset value
    rowContact.organizationId = 0;
    if (this.currentUser.isInternal || isOriginAgentContact) {
      // if the value is organizationId
      if (typeof value == 'number') {
        selectedOrg = this.activeOrgList.find(x => x.id === value);
      }
      else {
        selectedOrg = this.activeOrgList.find(x => x.name.toLowerCase() === value.toLowerCase());
      }

      if (selectedOrg) {
        console.log(selectedOrg)
        const concatenatedAddress = this.concatenateAddressLines(selectedOrg.address, selectedOrg.addressLine2, selectedOrg.addressLine3, selectedOrg.addressLine4);
        this.contactList[rowIndex] = {
          organizationId: selectedOrg.id,
          organizationRole: rowContact.organizationRole,
          companyName: selectedOrg.name,
          contactName: selectedOrg.contactName,
          contactNumber: selectedOrg.contactNumber,
          contactEmail: isOriginAgentContact ? rowContact.contactEmail : selectedOrg.contactEmail,
          address: concatenatedAddress,
          isManualInput: rowContact.isManualInput,
          weChatOrWhatsApp: selectedOrg.weChatOrWhatsApp
        };
        this.validateContactNameInput(rowIndex);
        this.validateAddressInput(rowIndex);
        this.resetOrganizationRoleOptions();
        this.validateCompany(selectedOrg.name, rowIndex);
        this.validateContactNumber(selectedOrg.contactNumber, rowIndex);
        this.validateContactEmail(isOriginAgentContact ? rowContact.contactEmail : selectedOrg.contactEmail, rowIndex);
      }
      this.validateCompany(value, rowIndex);
    } else { // external user => refer to OrgContactPreferences
      selectedOrg = this.orgContactPreferenceList.find(x => x.companyName.toLowerCase() === value.toLowerCase());
      if (selectedOrg) {
        this.contactList[rowIndex] = {
          organizationId: 0,
          organizationRole: rowContact.organizationRole,
          companyName: selectedOrg.companyName,
          contactName: selectedOrg.contactName,
          contactNumber: selectedOrg.contactNumber,
          contactEmail: selectedOrg.contactEmail,
          address: selectedOrg.address,
          isManualInput: rowContact.isManualInput,
          weChatOrWhatsApp: selectedOrg.weChatOrWhatsApp
        };
        this.validateContactNameInput(rowIndex);
        this.validateAddressInput(rowIndex);
        this.resetOrganizationRoleOptions();
        this.validateCompany(selectedOrg.companyName, rowIndex);
        this.validateContactNumber(selectedOrg.contactNumber, rowIndex);
        this.validateContactEmail(selectedOrg.contactEmail, rowIndex);
      } else {
        this.validateCompany(value, rowIndex);
      }
    }
    this.validateCompanyNameOfNotifyAndAlsoParty(rowIndex);
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

  concatenateAddressLines(address: string, addressLine2?: string, addressLine3?: string, addressLine4?: string) {
    let concatenatedAddress = address;
    if (!StringHelper.isNullOrEmpty(addressLine2)) {
      concatenatedAddress += '\n' + addressLine2;
    }
    if (!StringHelper.isNullOrEmpty(addressLine3)) {
      concatenatedAddress += '\n' + addressLine3;
    }
    if (!StringHelper.isNullOrEmpty(addressLine4)) {
      concatenatedAddress += '\n' + addressLine4;
    }
    return concatenatedAddress;
  }

  /**Call when user input address manually */
  addressValueChange(value: any, rowIndex: number) {
    let dataRow = this.contactList[rowIndex];
    // Update organizationId
    const selectedOrg = this.activeOrgList.find(x => x.id === dataRow.organizationId);
    if (selectedOrg) {
      const concatenatedAddress = this.concatenateAddressLines(selectedOrg.address, selectedOrg.addressLine2, selectedOrg.addressLine3, selectedOrg.addressLine4);
      // OrganizationId of Supplier/Shipper should not be reset to 0
      dataRow.organizationId = (dataRow.address === concatenatedAddress
        || dataRow.organizationRole === OrganizationNameRole.Supplier
        || dataRow.organizationRole === OrganizationNameRole.Shipper) ? dataRow.organizationId : 0;
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

  validateAddressInput(rowIndex) {
    const address = this.contactList[rowIndex].address;
    this.formErrors[this.tabPrefix + 'address_' + rowIndex + '_custom'] = null;

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
    const contactName = this.contactList[rowIndex].contactName;
    this.formErrors[this.tabPrefix + 'contactName_' + rowIndex + '_custom'] = null;

    if (!StringHelper.isNullOrEmpty(contactName) && contactName.length > 30) {
      this.formErrors[this.tabPrefix + 'contactName_' + rowIndex + '_custom'] = this.translateService.instant('validation.fieldLessThan', {
        fieldName: 'Contact Name',
        maxLength: 30
      });

    }
  }

  roleValueChange(value: any, rowIndex: number) {
    // this.contactAutoCompleteFilterChange(value, rowIndex);
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
        isManualInput: selectedContact.isManualInput ?? true
      });
      this.validateCompanyNameOfNotifyAndAlsoParty(rowIndex);
    }

    // reset by clear current error.
    this.removeErrorOfRow(rowIndex);

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
    // Get current selected/used organization roles except from the current row
    const availableOrgRoles = this._filterContactList.map(x => x.organizationRole)
      .filter(x => x != null && x !== this.contactList[rowIndex].organizationRole);

    if (this.originOrganizationRoleOptions) {
      // Get other organization role options
      let orgRoleOptions = this.originOrganizationRoleOptions.filter(x => availableOrgRoles.indexOf(x.name) === -1);

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

  contactAutoCompleteFilterChange(value: any, rowIndex: number) {
    let rowContact = this.contactList[rowIndex];

    if (value.length >= 3) {
      this.contactOptions[rowIndex] = this.getContactOptionsByRole(rowContact.organizationRole);
      this.contactOptions[rowIndex] = this.contactOptions[rowIndex].filter(x => x.name.toLowerCase().indexOf(value.toLowerCase()) !== -1);
    } else {
      this.contactOptions[rowIndex] = [];
    }
  }

  contactDropdownFilterChange(value: any, rowIndex: number) {
    let rowContact = this.contactList[rowIndex];
    this.contactOptions[rowIndex] = this.getContactOptionsByRole(rowContact.organizationRole);
    if (value.length >= 3) {
      this.contactOptions[rowIndex] = this.contactOptions[rowIndex].filter(x => x.name.toLowerCase().indexOf(value.toLowerCase()) !== -1);
    }
  }

  contactDropdownOpen($event, rowIndex) {
    const rowContact = this.contactList[rowIndex];
    this.contactOptions[rowIndex] = this.getContactOptionsByRole(rowContact.organizationRole);
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
    return customerId ? this.bulkFulfillmentFormService.getAffiliateCodes(customerId) : EMPTY;
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

    // Delete formErrors and validationRules for this row
    this.removeErrorOfRow(rowIndex);

    // Call other business
    this.resetOrganizationRoleOptions();

    this._clearContactRowData(rowData);
  }

  syncNotifyPartyWithConsignee(event): void {
    const existingConsigneeIndex = this.contactList.findIndex(c => c.organizationRole === OrganizationNameRole.Consignee
      && (StringHelper.isNullOrEmpty(c.removed) || !c.removed));
    const existingNotifyPartyIndex = this.contactList.findIndex(c => c.organizationRole === OrganizationNameRole.NotifyParty);

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
        this.validateGrid(existingNotifyPartyIndex);
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
        this.resetOrganizationRoleOptions();
        this.validateGrid(existingNotifyPartyIndex);
      }
    } else if (existingNotifyPartyIndex !== -1) {
      this.onDeleteContact(existingNotifyPartyIndex);
    }

    // Notify party same as Consignee = yes -> hide Notify party row
    if (event) {
      this.contactList.find(c => c.organizationRole === OrganizationNameRole.NotifyParty).hidden = true;
    } else {
      let notifyPartyIndex = this.contactList.findIndex(c => c.organizationRole === OrganizationNameRole.NotifyParty && !c.removed);
      this.onDeleteContact(notifyPartyIndex);
    }
  }

  private _clearContactRowData(contactData) {
    contactData.organizationRole = -1;
    contactData.companyName = "removed";
    contactData.address = "removed";
    contactData.contactName = "removed";
    contactData.contactEmail = 'removed';
  }

  public rowCallback(args) {
    // Deleted row will be marked with removed property.
    return {
      'hide-row': args.dataItem.removed || args.dataItem.hidden,
      'error-row': args.dataItem.isValidRow === false,
    };
  }

  isAllowInputContact(rowIndex): boolean {
    if (!this.currentUser.isInternal) {
      return false;
    }
    const selectedOrgRole = this.contactList[rowIndex].organizationRole;
    return selectedOrgRole === OrganizationNameRole.BillingParty || selectedOrgRole === OrganizationNameRole.Pickup || selectedOrgRole === OrganizationNameRole.Consignee;
  }

  validateAllGridInputs(): void {
    for (let index = 0; index < this.contactList.length; index++) {
      const contact = this.contactList[index];
      if (StringHelper.isNullOrEmpty(contact.removed) || !contact.removed) {
        this.validateAddressInput(index);
        this.validateContactNameInput(index);
        this.validateContactEmail(contact?.contactEmail, index);
        this.validateContactNumber(contact?.contactNumber, index);
        this.validateCompany(contact?.companyName, index);
      }

      if ((StringHelper.isNullOrEmpty(contact.removed) || !contact.removed)
        && contact.organizationRole === OrganizationNameRole.AlsoNotify) {
        this.validateCompanyNameOfNotifyAndAlsoParty(index);
      }
    }
  }

  removeErrorOfRow(index: number) {
    const formErrorNames = [
      `organizationRole_${index}`,
      `companyName_${index}`,
      `companyName_${index}_maxLength`,
      `companyName_${index}_theSameName`,
      `address_${index}`,
      `address_${index}_custom`,
      `contactName_${index}`,
      `contactName_${index}_maxLength`,
      `contactName_${index}_custom`,
      `contactEmail_${index}`,
      `contactEmail_${index}_custom`,
      `contactEmail_${index}_maxLength`
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

  isShowDeleteBtn(rowIndex: number): boolean {
    const orgRole = this.contactList[rowIndex].organizationRole;
    if (!orgRole) {
      return true;
    }

    return (orgRole === OrganizationNameRole.NotifyParty && !this.isNotifyPartyAsConsignee) ||
      orgRole === OrganizationNameRole.BillingParty ||
      orgRole === OrganizationNameRole.AlsoNotify
  }

  canUpdateOrgRole(rowIndex: number): boolean {
    const orgRole = this.contactList[rowIndex].organizationRole;
    if (!orgRole) {
      return true;
    }

    return (orgRole === OrganizationNameRole.NotifyParty && !this.isNotifyPartyAsConsignee) ||
      orgRole === OrganizationNameRole.BillingParty ||
      orgRole === OrganizationNameRole.AlsoNotify
  }

  canUpdateCompanyName(rowIndex: number): boolean {
    const orgRole = this.contactList[rowIndex].organizationRole;

    if (!orgRole) {
      return true;
    }

    return (orgRole === OrganizationNameRole.Supplier && (this.currentUser.isInternal || this.currentUser.role.id !== Roles.Shipper)) ||
      (orgRole === OrganizationNameRole.NotifyParty && !this.isNotifyPartyAsConsignee) ||
      orgRole === OrganizationNameRole.Shipper ||
      orgRole === OrganizationNameRole.Consignee ||
      orgRole === OrganizationNameRole.BillingParty ||
      orgRole === OrganizationNameRole.AlsoNotify ||
      orgRole === OrganizationNameRole.Pickup ||
      orgRole == OrganizationNameRole.OriginAgent;
  }

  canUpdateAddress(rowIndex: number): boolean {
    const orgRole = this.contactList[rowIndex].organizationRole;

    if (!orgRole) {
      return true;
    }

    if (orgRole === OrganizationNameRole.NotifyParty && this.isNotifyPartyAsConsignee) {
      return false;
    }

    return orgRole !== OrganizationNameRole.OriginAgent && orgRole !== OrganizationNameRole.DestinationAgent;
  }

  canUpdateContactName(rowIndex: number): boolean {
    const orgRole = this.contactList[rowIndex].organizationRole;

    if (!orgRole) {
      return true;
    }

    if (orgRole === OrganizationNameRole.NotifyParty && this.isNotifyPartyAsConsignee) {
      return false;
    }
    return orgRole !== OrganizationNameRole.OriginAgent && orgRole !== OrganizationNameRole.DestinationAgent;
  }

  canUpdateContactNumber(rowIndex: number): boolean {
    const orgRole = this.contactList[rowIndex].organizationRole;

    if (!orgRole) {
      return true;
    }

    if (orgRole === OrganizationNameRole.NotifyParty && this.isNotifyPartyAsConsignee) {
      return false;
    }
    return orgRole !== OrganizationNameRole.OriginAgent && orgRole !== OrganizationNameRole.DestinationAgent;
  }

  canUpdateContactEmail(rowIndex: number): boolean {
    const orgRole = this.contactList[rowIndex].organizationRole;

    if (!orgRole) {
      return true;
    }

    if (orgRole === OrganizationNameRole.NotifyParty && this.isNotifyPartyAsConsignee) {
      return false;
    }
    return orgRole !== OrganizationNameRole.DestinationAgent;
  }

  public validateBeforeSaving(): ValidationData[] {
    let result: ValidationData[] = [];

    // Validate data input (ex: email,...)
    this.validateAllGridInputs();

    // In case there is any error
    const errors = Object.keys(this.formErrors)?.filter(x => x.startsWith(this.tabPrefix));
    for (let index = 0; index < errors.length; index++) {
      const err = Reflect.get(this.formErrors, errors[index]);
      if (err && !StringHelper.isNullOrEmpty(err)) {
        result.push(
          new ValidationData(ValidationDataType.Input, false)
        );
      }
    }
    return result;
  }

  public validateBeforeSubmitting(): ValidationData[] {
    let result: ValidationData[] = [];

    let isValid: boolean = true;
    for (let index = 0; index < this.contactList.length; index++) {
      const contact = this.contactList[index];

      if (StringHelper.isNullOrEmpty(contact.organizationRole)) {
        isValid = false;
        break;
      }

      if (contact.organizationRole === OrganizationNameRole.DestinationAgent) {
        break;
      }

      if (StringHelper.isNullOrEmpty(contact.companyName) && contact.organizationRole !== OrganizationNameRole.Pickup) {
        isValid = false;
        break;
      }

      if (StringHelper.isNullOrEmpty(contact.address) && contact.organizationRole !== OrganizationNameRole.BillingParty) {
        isValid = false;
        break;
      }

      if (StringHelper.isNullOrEmpty(contact.contactName) && ![OrganizationNameRole.Pickup, OrganizationNameRole.BillingParty].includes(contact.organizationRole)) {
        isValid = false;
        break;
      }

      if (StringHelper.isNullOrEmpty(contact.contactEmail) && ![OrganizationNameRole.Pickup, OrganizationNameRole.BillingParty].includes(contact.organizationRole)) {
        isValid = false;
        break;
      }
    }

    if (!isValid) {
      result.push(new ValidationData(ValidationDataType.Input, false));
    }
    return result;
  }

  /**
   * To update state of contact grid: show/hide row
   */
  private updateContactGridState(): void {
    if (this.isNotifyPartyAsConsignee) {
      const existingNotifyParty = this.contactList.find(c => c.organizationRole === OrganizationNameRole.NotifyParty);
      if (existingNotifyParty) {
        existingNotifyParty.hidden = true;
      }
    }
  }

  isDisableCompanyName(orgNameRole: OrganizationNameRole) {
    const isShipperUser = this.currentUser?.role?.id === Roles.Shipper && orgNameRole === this.organizationNameRole.Shipper;
    return isShipperUser;
  }

  ngOnDestroy() {
    this._subscriptions.map(x => x.unsubscribe());
  }
}
