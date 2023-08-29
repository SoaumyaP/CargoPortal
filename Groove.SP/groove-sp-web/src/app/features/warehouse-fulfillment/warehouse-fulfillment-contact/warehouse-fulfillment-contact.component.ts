import { Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges } from '@angular/core';
import { ControlContainer, NgForm } from '@angular/forms';
import { faPlus } from '@fortawesome/free-solid-svg-icons';
import { TranslateService } from '@ngx-translate/core';
import { EMPTY, Observable, Subject, Subscription } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { OrganizationNameRole, Roles, RoleSequence, StringHelper, UserContextService, ValidationDataType } from 'src/app/core';
import { DefaultValue2Hyphens, EmailValidationPattern } from 'src/app/core/models/constants/app-constants';
import { ValidationData } from 'src/app/core/models/forms/validation-data';
import { OrganizationReferenceDataModel } from 'src/app/core/models/organization.model';
import { WarehouseFulfillmentFormService } from '../warehouse-fulfillment-form/warehouse-fulfillment-form.service';

@Component({
  selector: 'app-warehouse-fulfillment-contact',
  templateUrl: './warehouse-fulfillment-contact.component.html',
  styleUrls: ['./warehouse-fulfillment-contact.component.scss'],
  viewProviders: [{ provide: ControlContainer, useExisting: NgForm }]
})
export class WarehouseFulfillmentContactComponent implements OnInit, OnChanges, OnDestroy {
  @Input('data')
  contactList: any[];
  @Input()
  isViewMode: boolean;
  @Input()
  isAddMode: boolean;
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

  defaultValue: string = DefaultValue2Hyphens;
  readonly organizationNameRole = OrganizationNameRole;
  originOrganizationRoleOptions: any[];
  contactOptions = [];
  shipperList = [];
  consigneeList = [];
  isForceReset: boolean = false;
  activeOrgList: any[];
  shipperAffiliateIds: any[];
  isInternalUser: any;
  faPlus = faPlus;
  // Store all subscriptions, then should un-subscribe at the end
  private _subscriptions: Array<Subscription> = [];
  // Store all validation results (business, input,...), then should return to validate
  public addressValueKeyUp$ = new Subject<string>();
  public contactNameValueKeyUp$ = new Subject<string>();

  currentUser: any;

  constructor(
    private warehouseFulfillmentFormService: WarehouseFulfillmentFormService,
    private userContext: UserContextService,
    public translateService: TranslateService) {
    this.userContext.getCurrentUser().subscribe(user => {
      if (user) {
        this.currentUser = user;
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
  }

  ngOnInit(): void {
    this.initOrganizations();
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
    }
  }

  private initOrganizations() {
    const sub = this.warehouseFulfillmentFormService.getOrganizations()
      .subscribe((data: Array<OrganizationReferenceDataModel>) => {
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
        }
        const sub3 = this.warehouseFulfillmentFormService.getOrganizationRoles().map((orgs: any[]) => {
          return orgs.filter(x => x.name !== OrganizationNameRole.Supplier
            && x.name !== OrganizationNameRole.Principal
            && x.name !== OrganizationNameRole.Delegation
            && x.name !== OrganizationNameRole.Pickup
            && x.name !== OrganizationNameRole.OriginAgent
            && x.name !== OrganizationNameRole.DestinationAgent
            && x.name !== OrganizationNameRole.Consignee
            && x.name !== OrganizationNameRole.Shipper
            && x.name !== OrganizationNameRole.BillingParty);
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
    const selectedOrg = this.activeOrgList.find(x => x.name.toLowerCase() === value.toLowerCase());

    if (selectedOrg) {
      const concatenatedAddress = this.concatenateAddressLines(selectedOrg.address, selectedOrg.addressLine2, selectedOrg.addressLine3, selectedOrg.addressLine4);
      this.contactList[rowIndex] = {
        organizationId: selectedOrg.id,
        organizationRole: this.contactList[rowIndex].organizationRole,
        companyName: selectedOrg.name,
        contactName: selectedOrg.contactName,
        contactNumber: selectedOrg.contactNumber,
        contactEmail: selectedOrg.contactEmail,
        address: concatenatedAddress,
        isManualInput: this.contactList[rowIndex].isManualInput
      };
      this.validateContactNameInput(rowIndex);
      this.validateAddressInput(rowIndex);
      this.resetOrganizationRoleOptions();
    } else {
      this.contactList[rowIndex].organizationId = 0;
    }
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
      // OrganizationId of Supplier/Delegation should not be reset to 0
      dataRow.organizationId = (dataRow.address === concatenatedAddress
        || dataRow.organizationRole === OrganizationNameRole.Supplier) ? dataRow.organizationId : 0;
    }
  }

  validateAddressInput(rowIndex) {
    const address = this.contactList[rowIndex].address;
    this.formErrors[this.tabPrefix + 'address_' + rowIndex + '_custom'] = null;

    if (!StringHelper.isNullOrEmpty(address)) {
      let addressLines = address.split('\n');
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
          if (addressLines[index].length > 50) {
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
      this.formErrors[this.tabPrefix + 'contactName_' + rowIndex + '_custom'] = this.translateService.instant('validation.contactNameMustNotGreaterThan30Chars');
    }
  }

  roleValueChange(value: any, rowIndex: number) {
    this.contactFilterChange(value, rowIndex);
    const selectedContact = this.contactList[rowIndex];
    const selectedOrgRole = selectedContact.organizationRole;
    if ((!this.isValidOrgRole(value, selectedContact) && selectedContact.organizationId) || this.isForceReset ||
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
    }

    // reset by clear current error.
    this.removeErrorOfRow(rowIndex);

    this.resetOrganizationRoleOptions();
    this.validateGrid(rowIndex);
  }

  onRoleOptionsOpen(rowIndex) {
    const currentOrgRole = this.contactList[rowIndex].organizationRole;
    this.isForceReset = currentOrgRole === OrganizationNameRole.Pickup;
  }

  getAvailableRoleOptions(rowIndex) {
    // Get current selected/used organization roles except from the current row
    const availableOrgRoles = this._filterContactList.map(x => x.organizationRole)
      .filter(x => x != null && x !== this.contactList[rowIndex].organizationRole);

    if (this.originOrganizationRoleOptions) {
      // Get other organization role options
      let orgRoleOptions = this.originOrganizationRoleOptions.filter(x => availableOrgRoles.indexOf(x.name) === -1);
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

      if (selectedOrgRole === OrganizationNameRole.NotifyParty) {
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
    return customerId ? this.warehouseFulfillmentFormService.getAffiliateCodes(customerId) : EMPTY;
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
    const selectedOrgRole = this.contactList[rowIndex].organizationRole;
    return selectedOrgRole === OrganizationNameRole.Pickup;
  }

  validateAllGridInputs(): void {
    for (let index = 0; index < this.contactList.length; index++) {
      const contact = this.contactList[index];
      if (StringHelper.isNullOrEmpty(contact.removed) || !contact.removed) {
        this.validateAddressInput(index);
        this.validateContactNameInput(index);
      }
    }
  }

  removeErrorOfRow(index: number) {
    const formErrorNames = [
      `organizationRole_${index}`,
      `companyName_${index}`,
      `address_${index}`,
      `address_${index}_custom`,
      `contactName_${index}`,
      `contactName_${index}_custom`,
      `contactEmail_${index}`,
      `contactEmail_${index}_custom`
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

    return orgRole === OrganizationNameRole.NotifyParty ||
      orgRole === OrganizationNameRole.AlsoNotify
  }

  canUpdateOrgRole(rowIndex: number): boolean {
    const orgRole = this.contactList[rowIndex].organizationRole;
    if (!orgRole) {
      return true;
    }

    return (orgRole === OrganizationNameRole.NotifyParty) ||
      orgRole === OrganizationNameRole.AlsoNotify
  }

  canUpdateCompanyName(rowIndex: number): boolean {
    const orgRole = this.contactList[rowIndex].organizationRole;

    if (!orgRole) {
      return true;
    }

    return orgRole === OrganizationNameRole.NotifyParty ||
      orgRole === OrganizationNameRole.AlsoNotify ||
      orgRole === OrganizationNameRole.Pickup;
  }

  canUpdateAddress(rowIndex: number): boolean {
    const orgRole = this.contactList[rowIndex].organizationRole;

    if (!orgRole) {
      return true;
    }
    return orgRole !== OrganizationNameRole.OriginAgent && orgRole !== OrganizationNameRole.Principal;
  }

  canUpdateContactName(rowIndex: number): boolean {
    const orgRole = this.contactList[rowIndex].organizationRole;

    if (!orgRole) {
      return true;
    }
    return orgRole !== OrganizationNameRole.OriginAgent && orgRole !== OrganizationNameRole.Principal
  }

  canUpdateContactNumber(rowIndex: number): boolean {
    const orgRole = this.contactList[rowIndex].organizationRole;

    if (!orgRole) {
      return true;
    }
    return orgRole !== OrganizationNameRole.OriginAgent && orgRole !== OrganizationNameRole.Principal
  }

  canUpdateContactEmail(rowIndex: number): boolean {
    const orgRole = this.contactList[rowIndex].organizationRole;

    if (!orgRole) {
      return true;
    }
    return orgRole !== OrganizationNameRole.OriginAgent && orgRole !== OrganizationNameRole.Principal;
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

      if (StringHelper.isNullOrEmpty(contact.companyName) && contact.organizationRole !== OrganizationNameRole.Pickup) {
        isValid = false;
        break;
      }

      if (StringHelper.isNullOrEmpty(contact.contactName) && ![OrganizationNameRole.Pickup].includes(contact.organizationRole)) {
        isValid = false;
        break;
      }

      if (StringHelper.isNullOrEmpty(contact.contactEmail) && ![OrganizationNameRole.Pickup].includes(contact.organizationRole)) {
        isValid = false;
        break;
      }
    }

    if (!isValid) {
      result.push(new ValidationData(ValidationDataType.Input, false));
    }
    return result;
  }

  ngOnDestroy() {
    this._subscriptions.map(x => x.unsubscribe());
  }
}