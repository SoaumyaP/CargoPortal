import { Component, ViewChild, ElementRef, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { OrganizationFormService } from './organization-form.service';
import { FormComponent } from 'src/app/core/form/form.component';
import { environment } from 'src/environments/environment';
import { StringHelper, DropDowns, UserContextService } from 'src/app/core';
import { UserStatus, OrganizationStatus, OrganizationType, OrganizationRole, Roles, ConnectionType, MaxLengthValueInput, AgentType, SOFormGenerationFileType } from 'src/app/core/models/enums/enums';
import { faPencilAlt, faPowerOff, faUnlockAlt, faPlus, faEye, faEyeSlash, faTrash } from '@fortawesome/free-solid-svg-icons';
import { TranslateService } from '@ngx-translate/core';
import { DropDownListComponent, AutoCompleteComponent } from '@progress/kendo-angular-dropdowns';
import { ValidateEmailNotTaken } from './customerPrefix.validator';
import { UserAction } from '../organization.constants';
import { Observable, EMPTY, forkJoin } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';
import { Validators } from '@angular/forms';
import { SupplierRelationshipListComponent } from '../supplier-relationship-list/supplier-relationship-list.component';
import { SelectEvent, FileInfo, RemoveEvent } from '@progress/kendo-angular-upload';
import { UserFormService } from '../../user/users/user-form/user-form.service';
import { AssignPOsFormService } from 'src/app/ui/assign-pos-form/assign-pos-form.service';

@Component({
    selector: 'app-organization-form',
    templateUrl: './organization-form.component.html',
    styleUrls: ['./organization-form.component.scss']
})
export class OrganizationFormComponent extends FormComponent implements OnDestroy {
    faPencilAlt = faPencilAlt;
    faPowerOff = faPowerOff;
    faUnlockAlt = faUnlockAlt;
    faPlus = faPlus;
    faEye = faEye;
    faEyeSlash = faEyeSlash;
    faTrash = faTrash;

    @ViewChild('organizationNameElement', { static: false }) public organizationNameElement: ElementRef;
    @ViewChild('organizationAddressElement', { static: false }) public organizationAddressElement: ElementRef;
    @ViewChild('organizationAddressLine2Element', { static: false }) public organizationAddressLine2Element: ElementRef;
    @ViewChild('organizationAddressLine3Element', { static: false }) public organizationAddressLine3Element: ElementRef;
    @ViewChild('organizationAddressLine4Element', { static: false }) public organizationAddressLine4Element: ElementRef;
    @ViewChild('organizationCountryIdElement', { static: false }) public organizationCountryIdElement: DropDownListComponent;
    @ViewChild('organizationContactNameElement', { static: false }) public organizationContactNameElement: ElementRef;
    @ViewChild('organizationContactNumberElement', { static: false }) public organizationContactNumberElement: ElementRef;
    @ViewChild('organizationContactEmailElement', { static: false }) public organizationContactEmailElement: ElementRef;
    @ViewChild('organizationEdisonInstanceIdElement', { static: false }) public organizationEdisonInstanceIdElement: ElementRef;
    @ViewChild('organizationEdisonCompanyCodeIdElement', { static: false }) public organizationEdisonCompanyCodeIdElement: ElementRef;
    @ViewChild('cityAutoComplete', { static: false }) public cityAutoComplete: AutoCompleteComponent;
    @ViewChild(SupplierRelationshipListComponent, { static: false }) supplierRelationshipListComponent: SupplierRelationshipListComponent;
    @ViewChild('organizationTypeElement', { static: false }) public organizationTypeElement: ElementRef;
    @ViewChild('organizationCodeElement', { static: false }) public organizationCodeElement: ElementRef;
    @ViewChild('agentTypeElement', { static: false }) public agentTypeElement: ElementRef;
    @ViewChild('generateSOFormFileTypeElement', { static: false }) public generateSOFormFileTypeElement: ElementRef;

    acpTimeout: any;
    cityLoading = false;

    modelName = 'organizations';
    model: any = {
        location: {
            countryId: 0
        },
        organizationType: OrganizationType.General,
        agentType: AgentType.None,
        soFormGenerationFileType: SOFormGenerationFileType.Pdf
    };

    myRestrictions = {
        allowedExtensions: ['.png', '.jpg'],
        maxFileSize: 153600, // 150Kb
    };
    selectedFile: any;

    organizationTypeName: any;
    countryList: any;
    countryFilter: any;
    countryId: any = null;

    cityList: any;
    cityFilter: any;

    affiliateList: any;
    customerList: Array<any>;

    userList = [];
    organizationCodeOptions: any;
    customerOrganizationCodeOptions: any;
    customerOrganizationCodeOriginOptions: any;
    userStatus = UserStatus;
    organizationStatus = OrganizationStatus;
    organizationType = OrganizationType;
    isAffiliateAdding: boolean;
    isLogoPreviewMode: boolean = false;
    isValidSelectedFile = true;
    initData = [
        { sourceUrl: `${environment.commonApiUrl}/countries/dropDown` }
    ];

    addUserFormOpened: boolean;
    addAffiliateFormOpened: boolean;
    userModel = null;
    currentUser = null;
    backupModel = null;
    roles = Roles;
    organizationTypeList = DropDowns.OrganizationTypeList.filter(c => c.value !== 0);
    agentTypeList = DropDowns.AgentOrganizationType;
    soFormGenerationFileTypeList = DropDowns.SOFormGenerationFileType;
    defaultDropDown = { text: 'label.select', value: null };

    assignPOsPopupArgs = {
        assignPOsPopupOpened: false,
        supplierId: 0,
        customerId: 0,
        popupTitle: 'label.organization',
        hintText: null
    }

    validationRules = {
        'organizationName': {
            'required': 'label.organizationName'
        },
        'organizationContactName': {
            'required': 'label.contactName'
        },
        'organizationEdisonInstanceId': {
            'required': 'label.ediSONInstance'
        },
        'organizationEdisonCompanyCodeId': {
            'required': 'label.ediSONCompanyCode'
        },
        'organizationCountryId': {
            'required': 'label.country'
        },
        'organizationCityId': {
            'required': 'label.city',
            'invalid': 'label.city'
        },
        'customerPrefix': {
            'required': 'label.customerPrefix',
            'minlength': 'label.customerPrefix',
            'pattern': 'label.customerPrefix',
            'customerPrefixTaken': 'label.customerPrefix',
        },
        'organizationContactNumber': {
            'maxLengthInput': MaxLengthValueInput.PhoneNumber
        },
        'organizationType': {
            'required': 'label.organizationType'
        },
        'organizationCode': {
            'required': 'label.organizationCode',
            'invalid': 'label.invalidOrganizationCode',
            'alreadyExists': 'label.organizationCode',
            'invalidChinese': 'validation.organizationCodeInvalidChinese'
        }
    };

    maxLengthInput = MaxLengthValueInput.PhoneNumber;

    constructor(protected route: ActivatedRoute,
        public service: OrganizationFormService,
        public notification: NotificationPopup,
        public router: Router,
        public translateService: TranslateService,
        private _userContext: UserContextService,
        private _assignPOsFormService: AssignPOsFormService) {
        super(route, service, notification, translateService, router);
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
                if (this.router.url.indexOf('/organizations/owner/') >= 0) {
                    this.route.params.subscribe(params => {
                        if (!StringHelper.isNullOrEmpty(params.id)) {
                            if (!(!this.currentUser.isInternal && this.currentUser.organizationId &&
                                this.currentUser.organizationId.toString() === params.id &&
                                (this.currentUser.role.id === Roles.Shipper ||
                                    this.currentUser.role.id === Roles.Factory ||
                                    this.currentUser.role.id === Roles.Agent ||
                                    this.currentUser.role.id === Roles.CruiseAgent ||
                                    this.currentUser.role.id === Roles.Principal ||
                                    this.currentUser.role.id === Roles.CruisePrincipal ||
                                    this.currentUser.role.id === Roles.Warehouse
                                ))) {
                                this.router.navigate(['/error/401']);
                            }
                        }
                    });
                }
            }
        });
    }

    onInitDataLoaded(data): void {
        if (this.isAddMode) {
            this.mappingModelBeforeCreate();
        }

        this.backupModel = Object.assign({}, this.model);
        this.isInitDataLoaded = false;
        this.isAffiliateAdding = false;
        this.countryList = data[0];
        this.countryFilter = this.countryList;
        this.selectedFile = null;
        this.isLogoPreviewMode = false;
        this._userContext.organizationLogo = null;
        this.model.emailNotifications?.forEach(el => {
            el.customerId = el.customerId.toString();
        });

        const organizationType = DropDowns.OrganizationTypeList.find(o => o.value === this.model.organizationType);
        this.organizationTypeName = organizationType != null ? organizationType.text : '';
        if (this.model.location) {
            this.service.getLocationDropDown(this.model.location.countryId).subscribe(cities => {
                this.countryId = this.model.location.countryId;
                this.cityList = cities;
                const locationIdString = this.model.locationId != null ? this.model.locationId.toString() : '';
                const selectedItem = this.cityList.find(x => x.value === locationIdString);
                this.model.locationName = selectedItem != null ? selectedItem.label : '';
                this.isInitDataLoaded = true;
            });
        }
        else {
            this.cityList = [];
            this.model.locationName = '';
            this.countryId = null;
            this.isInitDataLoaded = true;
        }

        // If create new organization, we don't need call apis
        if (!this.isAddMode) {
            this.service.getAffiliates(this.model.id).subscribe(affs => {
                this.affiliateList = affs;
            });

            this.service.getCustomers(this.model.id).subscribe(orgs => {
                this.customerList = orgs.map((x) => {
                    return {
                        ...x,
                        userAction: UserAction.RemoveCustomer
                    };
                });

                this.service.getActiveCodesExcludeIds([this.model.id]).pipe(
                    map((organizations: any[]) => {
                        return organizations.filter(x => x.organizationType === OrganizationType.Principal)
                            .map((i: any) => <{ label: string, value: string }>{
                                value: i.id,
                                label: i.code
                            });
                    })
                ).subscribe(res => {
                    this.customerOrganizationCodeOriginOptions = res;
                    this.updateCustomerRelationList();
                });
            });

            this.organizationCodeOptions = this.service.getOrganizationCodeOptions(this.model.id);

            this.service.getUsers(this.model.id).subscribe((res: any) => {
                this.userList = res;
                const adminUser = StringHelper.isNullOrEmpty(this.model.adminUser) ? '' : this.model.adminUser.toLowerCase();
                this.userList.map(x => {
                    x.isAdmin = x.email.toLowerCase() === adminUser;
                });
            });
        }

        // separate organization roles for model
        // this.separateOrganizationRoles();
        this.model.removeAffiliateIds = [];
        this.model.removeCustomerIds = [];

        if (this.isEditMode) {
            this.service.hasBuyerCompliance(this.model.id).subscribe(
                value => {
                    if (value && this.mainForm) {
                        this.mainForm.controls['customerPrefix'].setValidators([Validators.required,
                        Validators.minLength(5),
                        Validators.maxLength(5),
                        Validators.pattern('^[A-Z0-9]{5}$')]);
                    }
                }
            );
        }
    }

    get checkToShowCustomerRelationship() {
        return this.model.organizationType === OrganizationType.General;
    }

    get checkToShowSupplierRelationship() {
        return this.model.organizationType === OrganizationType.Principal;
    }

    separateOrganizationRoles() {
        this.model.isShipper = this.model.organizationRoleIds.some(x => x === OrganizationRole.Shipper);
        this.model.isConsignee = this.model.organizationRoleIds.some(x => x === OrganizationRole.Consignee);
        this.model.isNotifyParty = this.model.organizationRoleIds.some(x => x === OrganizationRole.NotifyParty);
        this.model.isAlsoNotify = this.model.organizationRoleIds.some(x => x === OrganizationRole.AlsoNotify);
        this.model.isImportBroker = this.model.organizationRoleIds.some(x => x === OrganizationRole.ImportBroker);
        this.model.isExportBroker = this.model.organizationRoleIds.some(x => x === OrganizationRole.ExportBroker);
        this.model.isOriginAgent = this.model.organizationRoleIds.some(x => x === OrganizationRole.OriginAgent);
        this.model.isDestinationAgent = this.model.organizationRoleIds.some(x => x === OrganizationRole.DestinationAgent);
        this.model.isPrincipal = this.model.organizationRoleIds.some(x => x === OrganizationRole.Principal);
    }

    onOrganizationCodeAssignmentChanged(value, rowIndex) {
        this.service.getOrganization(value).subscribe(
            data => {
                this.affiliateList[rowIndex] = data;
                this.affiliateList[rowIndex].isAddLine = true;
            },
            error => {
                console.log(error);
            });
    }

    cancelEditOrganization() {
        const confirmDlg = this.notification.showConfirmationDialog('edit.cancelConfirmation', 'label.organization');

        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    if (this.isAddMode) {
                        this.router.navigate(['/organizations']);
                    } else {
                        if (this.isViewByOwnerAdminOrg) {
                            this.router.navigate([`/organizations/owner/view/${this.model.id}`]);
                        } else {
                            this.router.navigate([`/organizations/view/${this.model.id}`]);
                        }
                    }
                    Object.keys(this.formErrors)
                    .map(x => {
                        delete this.formErrors[x];
                    });
                    this.ngOnInit();
                }
            });
    }

    collectOrganizationRoles() {
        this.model.organizationRoleIds = [];
        if (this.model.isShipper) {
            this.model.organizationRoleIds.push(OrganizationRole.Shipper);
        }
        if (this.model.isConsignee) {
            this.model.organizationRoleIds.push(OrganizationRole.Consignee);
        }
        if (this.model.isNotifyParty) {
            this.model.organizationRoleIds.push(OrganizationRole.NotifyParty);
        }
        if (this.model.isAlsoNotify) {
            this.model.organizationRoleIds.push(OrganizationRole.AlsoNotify);
        }
        if (this.model.isImportBroker) {
            this.model.organizationRoleIds.push(OrganizationRole.ImportBroker);
        }
        if (this.model.isExportBroker) {
            this.model.organizationRoleIds.push(OrganizationRole.ExportBroker);
        }
        if (this.model.isOriginAgent) {
            this.model.organizationRoleIds.push(OrganizationRole.OriginAgent);
        }
        if (this.model.isDestinationAgent) {
            this.model.organizationRoleIds.push(OrganizationRole.DestinationAgent);
        }
        if (this.model.isPrincipal) {
            this.model.organizationRoleIds.push(OrganizationRole.Principal);
        }
    }

    async saveOrganization() {
        this.elements = {
            'organizationName': this.organizationNameElement.nativeElement,
            'organizationAddress': this.organizationAddressElement.nativeElement,
            'organizationAddressLine2': this.organizationAddressLine2Element.nativeElement,
            'organizationAddressLine3': this.organizationAddressLine2Element.nativeElement,
            'organizationAddressLine4': this.organizationAddressLine2Element.nativeElement,
            'organizationCountryId': this.organizationCountryIdElement,
            'organizationCityId': this.cityAutoComplete,
            'organizationContactName': this.organizationContactNameElement.nativeElement,
            'organizationContactNumber': this.organizationContactNumberElement.nativeElement,
            'organizationContactEmail': this.organizationContactEmailElement.nativeElement,
            'organizationEdisonInstanceId': this.organizationEdisonInstanceIdElement.nativeElement,
            'organizationEdisonCompanyCodeId': this.organizationEdisonCompanyCodeIdElement.nativeElement,
            'organizationType': this.organizationTypeElement.nativeElement,
            'organizationCode': this.organizationCodeElement.nativeElement,
            'agentType': this.agentTypeElement?.nativeElement,
            'generateSOFormFileType': this.generateSOFormFileTypeElement?.nativeElement
        };

        // check selected logo.
        if (!this.isValidSelectedFile) {
            this.notification.showErrorPopup('validation.mandatoryFieldsValidation', 'label.organization');
            return;
        }
        if (this.mainForm.valid) {
            // Prompt an alert pop-up if Organization Name is longer than 50 chars
            if (this.model.name?.length > 50) {
                const confirmDlg = this.notification.showConfirmationDialog(
                    'confirmation.organizationNameLongerThan50',
                    'label.organization');
                const isConfirm: any = await confirmDlg.result.toPromise();
                if (!isConfirm?.value) {
                    return;
                }
            }

            if (this.isAddMode) {
                this.createNewOrganization();
                return;
            }

            this.model.pendingCustomerIds = this.customerList.filter(x => x.connectionType === ConnectionType.Pending && x.isConfirmConnectionType).map(x => x.id);
            this.service.update(this.model.id, this.model).subscribe(
                organization => {
                    this.service.updateUserOrganization(this.model.id, this.model).subscribe(user => {
                        this.notification.showSuccessPopup('save.sucessNotification', 'label.organization');
                        this._userContext.queryCurrentUser();
                        if (this.isViewByOwnerAdminOrg) {
                            this.router.navigate([`/organizations/owner/view/${this.model.id}`]);
                        } else {
                            this.router.navigate([`/organizations/view/${this.model.id}`]);
                        }
                        this.ngOnInit();
                    },
                        error => {
                            this.notification.showErrorPopup('save.failureNotification', 'label.organization');
                        });
                },
                httpErrorResponse => {
                    this.notification.showErrorPopup('save.failureNotification', 'label.organization');
                }
            );
        } else {
            this.validateAllFields(false);
        }
    }

    createNewOrganization() {
        this.service.create(this.model).subscribe(
            organization => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.organization');
                this.mappingModelAfterCreated(organization);
                this.router.navigate([`/organizations/view/${this.model.id}`]);
            },
            httpErrorResponse => {
                if (httpErrorResponse.status === 400) {
                    if (httpErrorResponse.error.message) {
                        const errTags = httpErrorResponse.error.message.split('#').filter(x => !StringHelper.isNullOrEmpty(x));
                        switch (errTags[0]) {
                            case 'OrgCodeDuplicated':
                                this.setInvalidControl('organizationCode', 'alreadyExists');
                                this.notification.showErrorPopup('save.failureNotification', 'label.organization');
                                break;
                            case 'OrgCodeInvalidChinese':
                                this.setInvalidControl('organizationCode', 'invalidChinese');
                                this.notification.showErrorPopup('save.failureNotification', 'label.organization');
                                break;
                            default:
                                break;
                        }
                    }
                } else {
                    this.notification.showErrorPopup('save.failureNotification', 'label.organization');
                }
            });
    }

    mappingModelBeforeCreate() {
        this.model.location = null;
        this.model.status = OrganizationStatus.Active;
        this.model.statusName = 'label.active';
    }

    mappingModelAfterCreated(organizationCreated) {
        this.model.id = organizationCreated.id;
        this.model.code = organizationCreated.code;

        this.affiliateList = [];
        this.organizationCodeOptions = this.service.getOrganizationCodeOptions(this.model.id);
    }

    buildParentIdString(model) {
        return (!StringHelper.isNullOrEmpty(model.parentId) ? model.parentId : '') + model.id + '.';
    }

    submitAddAffiliate(dataItem) {
        let request$: Observable<unknown>;
        const affiliate = {
            id: dataItem.id
        };

        request$ = this.service.addAffiliate(this.model.id, affiliate);

        request$.subscribe(
            data => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.organization');
                this.ngOnInit();
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.organization');
            });
    }

    removeAffiliate(dataItem, index) {
        if (this.isViewMode) {
            const confirmDlg = this.notification.showConfirmationDialog('delete.saveConfirmation', 'label.organization');
            confirmDlg.result.subscribe(
                (result: any) => {
                    if (result.value) {
                        this.service.removeAffiliate(this.model.id, dataItem.id).subscribe(
                            data => {
                                this.notification.showSuccessPopup('save.sucessNotification', 'label.organization');
                                this.ngOnInit();
                            },
                            error => {
                                this.notification.showErrorPopup('save.failureNotification', 'label.organization');
                            });
                    }
                });
        } else {
            // data will not save until user clicks on save button
            this.model.removeAffiliateIds.push(dataItem.id);
            this.affiliateList.splice(index, 1);
        }
    }

    activeOrganization() {
        const confirmDlg = this.notification.showConfirmationDialog('edit.activateOrganizationConfirmation', 'label.organization');

        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.model.status = OrganizationStatus.Active;
                    this.service.update(this.model.id, this.model).subscribe(
                        data => {
                            this.service.updateStatusUsers(this.model.id, UserStatus.Active).subscribe(
                                canUpdateUsers => {
                                    if (canUpdateUsers === true) {
                                        this.notification.showSuccessPopup('save.sucessNotification', 'label.organization');
                                        this.ngOnInit();
                                    }
                                },
                                error => {
                                    this.notification.showErrorPopup('save.failureNotification', 'label.organization');
                                });
                        },
                        error => {
                            this.notification.showErrorPopup('save.failureNotification', 'label.organization');
                        });
                }
            });
    }

    suspendOrganization() {
        this.model.status = OrganizationStatus.Inactive;
        this.service.update(this.model.id, this.model).subscribe(
            data => {
                this.service.updateStatusUsers(this.model.id, UserStatus.Inactive).subscribe(
                    canUpdateUsers => {
                        if (canUpdateUsers === true) {
                            this.notification.showSuccessPopup('save.sucessNotification', 'label.organization');
                            this.ngOnInit();
                        }
                    },
                    error => {
                        this.notification.showErrorPopup('save.failureNotification', 'label.organization');
                    });
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.organization');
            });
    }

    backList() {
        this.router.navigate(['/organizations']);
    }

    onCountryValueChanged(value) {
        this.model.locationId = null;
        this.model.locationName = '';
        this.service.getLocationDropDown(value).subscribe(cities => {
            this.cityList = cities;
        });
    }

    onCountryFilterChanged(value) {
        this.countryFilter = this.countryList.filter((s) => s.label.toLowerCase().indexOf(value.toLowerCase()) !== -1);
    }

    editOrganization() {
        if (this.isAffiliateAdding) {
            const confirmDlg = this.notification.showConfirmationDialog('edit.cancelConfirmation', 'label.organization');

            confirmDlg.result.subscribe(
                (result: any) => {
                    if (result.value) {
                        if (this.isViewByOwnerAdminOrg) {
                            this.router.navigate([`/organizations/owner/edit/${this.model.id}`]);
                        } else {
                            this.router.navigate([`/organizations/edit/${this.model.id}`]);
                        }
                        this.ngOnInit();
                    }
                });
        } else {
            if (this.isViewByOwnerAdminOrg) {
                this.router.navigate([`/organizations/owner/edit/${this.model.id}`]);
            } else {
                this.router.navigate([`/organizations/edit/${this.model.id}`]);
            }
            this.ngOnInit();
        }
    }

    cityFilterChange(filterName: string) {
        if (filterName.length >= 3) {
            this.cityAutoComplete.toggle(false);
            this.cityFilter = [];
            clearTimeout(this.acpTimeout);
            this.acpTimeout = setTimeout(() => {
                this.cityLoading = true;
                filterName = filterName.toLowerCase();
                let take = 10;
                for (let i = 0; i < this.cityList.length && take > 0; i++) {
                    if (this.cityList[i].label.toLowerCase().indexOf(filterName) !== -1) {
                        this.cityFilter.push({ value: this.cityList[i].value, label: this.cityList[i].label });
                        take--;
                    }
                }
                this.cityAutoComplete.toggle(true);
                this.cityLoading = false;
            }, 400);
        } else {
            this.cityLoading = false;
            if (this.acpTimeout) {
                clearTimeout(this.acpTimeout);
            }
            this.cityAutoComplete.toggle(false);
        }
    }

    cityValueChange(value) {
        const selectedItem = this.cityList.find(
            (element) => {
                return element.label === value;
            });

        if (!StringHelper.isNullOrEmpty(selectedItem)) {
            this.model.locationId = selectedItem.value;
            this.model.locationName = selectedItem.label;
            this.formErrors['organizationCityId'] = '';
        } else {
            this.model.locationId = null;
            this.model.locationName = '';
            if (!StringHelper.isNullOrEmpty(value)) {
                this.setInvalidControl('organizationCityId');
            }
        }
        this.cityLoading = false;
    }

    customerPrefixValueChange(value) {
        this.mainForm.controls['customerPrefix'].setAsyncValidators(ValidateEmailNotTaken.createValidator(this.service, this.model.id));
    }

    updateCustomerRelationList() {
        this.customerOrganizationCodeOptions = [];
        for (let i = 0; this.customerOrganizationCodeOriginOptions && i < this.customerOrganizationCodeOriginOptions.length; i++) {
            if (this.customerList.findIndex(x => x.id === this.customerOrganizationCodeOriginOptions[i].value) < 0) {
                this.customerOrganizationCodeOptions.push(this.customerOrganizationCodeOriginOptions[i]);
            }
        }
    }

    submitCustomerRelationshipUserAction(customer) {
        let request$: Observable<null>;

        if (customer.userAction === UserAction.RemoveCustomer) {
            if (this.isViewMode) {
                const confirmDlg = this.notification.showConfirmationDialog('delete.saveConfirmation', 'label.organization');
                request$ = confirmDlg.result.pipe(
                    switchMap((result: any) => {
                        if (result.value) {
                            return this.service.removeCustomer(this.modelId, customer.id);
                        }
                        return EMPTY;
                    }));
            } else {
                // data will not save until user clicks on save button
                this.model.removeCustomerIds.push(customer.id);
                this.customerList.splice(this.customerList.indexOf(customer), 1);
                this.updateCustomerRelationList();
            }
        }

        if (customer.userAction === UserAction.AddCustomer) {
            if (this.affiliateList.length > 0) {
                const confirmDlg = this.notification
                    .showConfirmationDialog('msg.addParentCustomersToAllAffiliateConfirmation', 'label.organization');
                request$ = confirmDlg.result.pipe(
                    switchMap((result: any) => {
                        if (result.value) {
                            const observableBatch = [];

                            this.affiliateList.forEach((key) => {
                                observableBatch.push(this.service.addCustomer(key.id, customer.id, customer.connectionType));
                            });

                            forkJoin(observableBatch).subscribe();
                            return this.service.addCustomer(this.modelId, customer.id, customer.connectionType);
                        } else if (result.value === false) {
                            return this.service.addCustomer(this.modelId, customer.id, customer.connectionType);
                        }
                        return EMPTY;
                    }));
            } else {
                request$ = this.service.addCustomer(this.modelId, customer.id, customer.connectionType);
            }
        }

        request$.subscribe(
            data => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.organization');
                this.ngOnInit();
                this.updateCustomerRelationList();
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.organization');
            }
        );
    }

    onAddUserClick() {

        this.userModel = {
            companyName: this.model.name,
            organizationId: this.model.id,
            organizationName: this.model.name,
            organizationCode: this.model.code,
            organizationType: this.model.organizationType,
            userRoles: []
        };

        this.addUserFormOpened = true;
    }

    setUserRoles() {
        if (this.model.organizationType === OrganizationType.Principal) {
            this.userModel.userRoles = [{
                roleId: this.userModel.roleId,
                userId: 0
            }];
        } else if (this.model.organizationType === OrganizationType.General) {
            
            this.userModel.userRoles = [{
                roleId: this.userModel.roleId,
                userId: 0
            }];
        } if (this.userModel.organizationType === OrganizationType.Agent) {
            this.userModel.userRoles = [{
                roleId: this.userModel.roleId,
                userId: 0
            }];
        }
    }

    onUserAdded() {
        this.addUserFormOpened = false;
        this.userModel.username = this.userModel.email;
        this.setUserRoles();
        this.service.addUser(this.userModel).subscribe(
            data => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.organization');
                this.userList.push(data);
                this.updateCustomerRelationList();
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.organization');
            });
    }

    onUserFormClosed() {
        this.addUserFormOpened = false;
    }

    onAdminChange(item) {
        // no -> yes
        if (item.isAdmin) {
            const msg = this.translateService
                .instant('msg.addAdminUser',
                    {
                        emailAddress: item.email
                    });
            const confirmDlg = this.notification.showConfirmationDialog(msg, 'label.organization');

            confirmDlg.result.subscribe(
                (result: any) => {
                    if (result.value) {
                        this.selectAdminUser(item.email);
                    } else {
                        item.isAdmin = false;
                    }
                });
        } else {
            const msg = this.translateService
                .instant('msg.removeAdminUser',
                    {
                        emailAddress: item.email
                    });
            const confirmDlg = this.notification.showConfirmationDialog(msg, 'label.organization');

            confirmDlg.result.subscribe(
                (result: any) => {
                    if (result.value) {
                        this.selectAdminUser('');
                    } else {
                        item.isAdmin = true;
                    }
                });
        }
    }

    selectAdminUser(email) {
        this.userList.map(x => {
            x.isAdmin = x.email === email ? true : false;
        });
        this.model.adminUser = email;
        this.service.updateAdminUser(this.model).subscribe(data => {
            this.notification.showSuccessPopup('save.sucessNotification', 'label.organization');
            if (!this.currentUser.isInternal) {
                window.location.href = `/organizations/owner/view/${this.model.id}`;
            }
        },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.organization');
            });
    }

    get isViewByOwnerAdminOrg() {
        return this.isViewByOwnerUserOrg &&
            this.model.adminUser === this.currentUser.email;
    }

    get isViewByOwnerUserOrg() {
        return !this.currentUser.isInternal &&
            this.currentUser.organizationId === this.model.id;
    }

    get canEditInfor() {
        return this.currentUser.isInternal || this.isViewByOwnerAdminOrg;
    }

    //#region Upload/ Preview logo handler

    public onSelect(ev: SelectEvent): void {
        ev.files.forEach((file: FileInfo) => {
            if (file.rawFile) {
                const reader = new FileReader();
                reader.onloadend = () => {
                    if (!this.validateUploadFile(file)) {
                        this.isValidSelectedFile = false;
                        this.isLogoPreviewMode = false;
                        this.selectedFile = null;
                        this.model.organizationLogo = null;
                        this._userContext.organizationLogo = null;
                        return;
                    };
                    this.isValidSelectedFile = true;
                    this.selectedFile = { ...file, src: <string>reader.result };
                    this.model.organizationLogo = reader.result;
                    this._userContext.organizationLogo = null;
                    this.isLogoPreviewMode = false;
                };

                reader.readAsDataURL(file.rawFile);
            }
        });
    }

    public onRemoveSelectedFile(ev: RemoveEvent): void {
        ev.files.forEach((file: FileInfo) => {
            this.isValidSelectedFile = true;
            this.isLogoPreviewMode = false;
            this.selectedFile = null;
            this.model.organizationLogo = null;
            this._userContext.organizationLogo = null;
        });
    }

    onRemoveLogo() {
        this.isLogoPreviewMode = false;
        this.model.organizationLogo = null;
        this._userContext.organizationLogo = null;
    }

    onToggleLogoPreviewMode() {
        this.isLogoPreviewMode = !this.isLogoPreviewMode;
        if (this.isLogoPreviewMode) {
            this._userContext.organizationLogo = this.selectedFile ? this.selectedFile.src : this.model.organizationLogo;
        } else {
            this._userContext.organizationLogo = null;
        }
    }

    onOrganizationTypeChange() {
        if (this.model.organizationType === OrganizationType.General) {
            this.model.code = null;
        }
    }

    onTypingOrganizationCode() {
        if (this.model.code) {
            // If user typing ORGXXXX (XXXX is number then code invalid)
            if (/^org[0-9]*$/.test(this.model.code.toLowerCase())) {
                this.setInvalidControl('organizationCode');
            }
        }
    }

    validateUploadFile(file: FileInfo) {
        if (StringHelper.isNullOrEmpty(file)) {
            return false;
        }
        if (file.size > this.myRestrictions.maxFileSize) {
            return false;
        }
        if (this.myRestrictions.allowedExtensions.includes(file.extension) === false) {
            return false;
        }
        return true;
    }

    getParentId() {
        const parentIds = this.model.parentId.split('.').filter(c => c);
        const parentId = parentIds.length === 1 ? parentIds[0] : parentIds[parentIds.length - 1];
        return parentId;
    }

    onTypingPhoneNumber(value: string) {
        if (value?.length > MaxLengthValueInput.PhoneNumber) {
            this.setInvalidControl('organizationContactNumber', 'maxLengthInput')

            this.formErrors['organizationContactNumber'] = this.translateService.instant('validation.maxLengthInput',
                {
                    maxValue: MaxLengthValueInput.PhoneNumber
                });
        }
    }

    activateUser(user): void {
        const msg = this.translateService
            .instant('confirmation.activateUser',
                {
                    email: user.email
                });
        const confirmDlg = this.notification.showConfirmationDialog(msg, 'label.organization');

        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.service.updateUserStatus(user.id, UserStatus.Active).subscribe(
                        (result: any) => {
                            user.status = result.status;
                            user.statusName = result.statusName;
                            this.notification.showSuccessPopup('save.sucessNotification', 'label.organization')
                        },
                        error => this.notification.showErrorPopup('save.failureNotification', 'label.organization')
                    );
                }
            }
        )
    }

    deactivateUser(user): void {
        const msg = this.translateService
            .instant('confirmation.deactivateUser',
                {
                    email: user.email
                });
        const confirmDlg = this.notification.showConfirmationDialog(msg, 'label.organization');

        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.service.updateUserStatus(user.id, UserStatus.Inactive).subscribe(
                        (result: any) => {
                            user.status = result.status;
                            user.statusName = result.statusName;
                            this.notification.showSuccessPopup('save.sucessNotification', 'label.organization')
                        },
                        error => this.notification.showErrorPopup('save.failureNotification', 'label.organization')
                    );
                }
            }
        )
    }

    deleteUser(user, rowIndex): void {
        if (user.status === UserStatus.WaitForConfirm) {
            const msg = this.translateService
                .instant('confirmation.deleteUser',
                    {
                        email: user.email
                    });
            const confirmDlg = this.notification.showConfirmationDialog(msg, 'label.organization');
            confirmDlg.result.subscribe(
                (result: any) => {
                    if (result.value) {
                        this.service.deleteUser(user.id).subscribe(
                            r => {
                                this.userList.splice(rowIndex, 1);
                                this.notification.showSuccessPopup('save.sucessNotification', 'label.user');
                            },
                            err => {
                                this.notification.showErrorPopup('save.failureNotification', 'label.user');
                            }
                        );
                    }
                });
        }
    }

    assignPO() {
        if (this.model.organizationType === OrganizationType.General) {
            this.assignPOsPopupArgs.supplierId = this.model.id
        } else {
            return;
        }
        this.assignPOsPopupArgs.assignPOsPopupOpened = true;
    }

    assignPOsPopupClosedHandler(event) {
        this.assignPOsPopupArgs.assignPOsPopupOpened = false;
    }

    addAffiliateFormClosedHandler(event) {
        this.addAffiliateFormOpened = false;
    }

    selectAffiliateHandler(event) {
        this.addAffiliateFormOpened = false;
        this.submitAddAffiliate(event);
    }

    onSupplierRelationshipAdded(supplier) {
        if (!this.currentUser.isInternal && this.currentUser.organizationId === this.model.id) {
            return;
        }

        if (!supplier) {
            return;
        }
        if (this.model.organizationType === OrganizationType.Principal) {
            this.assignPOsPopupArgs.customerId = this.model.id;
        } else {
            return;
        }
        this.assignPOsPopupArgs.hintText = this.translateService.instant('msg.pleaseAssignUnmappedPOToSupplier', {
            supplier: supplier.name
        });
        this.assignPOsPopupArgs.supplierId = supplier.id;

        this._assignPOsFormService.getTotalCount(this.assignPOsPopupArgs.customerId, this.assignPOsPopupArgs.supplierId).subscribe(
            count => {
                if (count && count > 0) {
                    this.assignPOsPopupArgs.assignPOsPopupOpened = true;
                }
            }
        )

    }

    ngOnDestroy(): void {
        this._userContext.organizationLogo = null;
    }

    //#endregion
}
