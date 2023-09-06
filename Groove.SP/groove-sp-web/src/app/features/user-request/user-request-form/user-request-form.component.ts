import { Component, ViewChild, ElementRef } from '@angular/core';
import { UserStatus, OrganizationStatus, StringHelper, DropdownListModel, OrganizationType, Roles, MaxLengthValueInput } from 'src/app/core';
import { FormComponent } from 'src/app/core/form';
import { ActivatedRoute, Router } from '@angular/router';
import { UserRequestFormService } from './user-request-form.service';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { environment } from 'src/environments/environment';
import { TranslateService } from '@ngx-translate/core';
import { faCheck, faBan } from '@fortawesome/free-solid-svg-icons';
import { AutoCompleteComponent } from '@progress/kendo-angular-dropdowns';
import { UsersService } from '../../user/users/users.service';
import { CommonService } from 'src/app/core/services/common.service';
@Component({
    selector: 'app-user-request-form',
    templateUrl: './user-request-form.component.html',
    styleUrls: ['./user-request-form.component.scss']
})
export class UserRequestFormComponent extends FormComponent {

    modelName = 'userRequests';
    model: any;
    faCheck = faCheck;
    faBan = faBan;

    initData = [
        { sourceUrl: `${environment.apiUrl}/roles` },
        { sourceUrl: `${environment.commonApiUrl}/organizations/orgReferenceData` },
    ];

    isDisabledOrganizationRole = true;
    userStatus = UserStatus;
    organizationOptions: any[];
    organizations: any;
    roleOptions: any;
    selectedRole: any;
    maxLengthInput = MaxLengthValueInput.PhoneNumber;
    countryDropdownDataSource: any[] = [];
    locationDataSource: any[] = [];

    @ViewChild('userPhoneElement', { static: false }) public userPhoneElement: ElementRef;
    @ViewChild('organizationUserElement', { static: false }) public organizationUserElement: ElementRef;
    @ViewChild('organizationUserElement', { static: false }) public autocomplete: AutoCompleteComponent;

    validationRules = {
        'userPhone': {
            'maxLengthInput': MaxLengthValueInput.PhoneNumber
        },
        'organizationUser': {
            'required': 'label.organization',
            'invalid': 'label.organization'
        },
        'organizationRoleId': {
            'required': 'label.organizationRole'
        },
        'companyName': {
            'required': 'label.companyName'
        },
        'companyAddress': {
            'required': 'label.companyAddress'
        }
    };

    constructor(
        public router: Router,
        protected route: ActivatedRoute,
        public service: UserRequestFormService,
        public notification: NotificationPopup,
        public translateService: TranslateService,
        public usersService: UsersService,
        public commonService: CommonService) {
        super(route, service, notification, translateService, router);
        commonService.getCountryDropdown().subscribe(
            (data) => {
                    this.countryDropdownDataSource = data ? data : [];
            }
        )
    }

    onInitDataLoaded([roles, organizations]): void {
        this.roleOptions = roles.filter(r => {
            return r.isInternal === this.model.isInternal;
        });
        this.organizations = organizations;

        this.model.username = this.model.email;
    }

    private bindOrganization(dataOrg: any) {
        this.model.organizationId = dataOrg != null ? dataOrg.id : null;
        this.model.organizationCode = dataOrg != null ? dataOrg.code : null;
        this.model.organizationName = dataOrg != null ? dataOrg.name : null;
        this.model.organizationType = dataOrg != null ? dataOrg.organizationType : null;
    }

    roleValueChange(item) {
        const requiredOrgType = this.usersService.getRequiredOrgTypeByRole(this.model.roleId);

        if (requiredOrgType && this.model.organizationType !== requiredOrgType) {
            this.autocomplete.reset();
            this.bindOrganization(null);
        }
    }

    organizationFilterChange(item) {
        if (item.length >= 3) {
            const data = this.usersService.getOrganizationListByInput(this.organizations, this.model.roleId, item);
            this.organizationOptions = data.map(x => new DropdownListModel<number>(x.orgCodeName, x.id));
        } else {
            this.autocomplete.toggle(false);
        }
    }

    organizationValueChange(item) {
        if (item) {
            const dataOrg = this.organizations.find(x => x.orgCodeName === item);
            this.bindOrganization(dataOrg);

            if (dataOrg != null) {
                this.formErrors['organizationUser'] = '';
            } else {
                if (!StringHelper.isNullOrEmpty(item)) {
                    this.setInvalidControl('organizationUser');
                }
            }
        }
    }

    approveUser() {
        this.validatePhoneNumber(this.model.phone);
        if (this.mainForm.valid) {
            if (this.model.organizationId !== null || this.model.isInternal) {
                const orgStatus = !this.model.isInternal ? (this.organizations.find(x => x.id === this.model.organizationId).status)
                    : null;

                const selectedRole = this.roleOptions.find(x => x.id === this.model.roleId);
                if (selectedRole != null && selectedRole.isOfficial) {
                    if (!this.model.isInternal && orgStatus === OrganizationStatus.Inactive) {
                        const confirmDlg = this.notification.showConfirmationDialog('edit.inactiveConfirmation', 'label.userRequest');
                        confirmDlg.result.subscribe(
                            (result: any) => {
                                if (result.value) {
                                    this.handleApproveRequest();
                                }
                            });
                    } else {
                        this.handleApproveRequest();
                    }
                } else {
                    this.notification.showErrorPopup(`${this.model.isInternal ? 'save.failureInternalApproveGuest'
                        : 'save.failureExternalApproveGuest'}`, 'label.userRequest');
                }
            } else {
                this.notification.showErrorPopup('save.failureOrganizationId', 'label.userRequest');
            }
        } else {
            this.validateAllFields(false);
        }
    }

    rejectUser() {
        this.handleRejectRequest();
    }

    handleApproveRequest() {
        this.service.approveUserRequest(this.model).subscribe(
            res => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.userRequest');
                this.router.navigate(['/user-requests']);
            },
            err => {
                this.notification.showErrorPopup('save.failureNotification', 'label.userRequest');
            }
        );
    }

    handleRejectRequest() {
        this.service.rejectUserRequest(this.model).subscribe(
            res => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.userRequest');
                this.router.navigate(['/user-requests']);
            },
            err => {
                this.notification.showErrorPopup('save.failureNotification', 'label.userRequest');
            }
        );
    }

    backToList() {
        if (this.mainForm.dirty) {
            const confirmDlg = this.notification.showConfirmationDialog('edit.cancelConfirmation', 'label.userRequest');
            confirmDlg.result.subscribe(
                (result: any) => {
                    if (result.value) {
                        this.router.navigate(['/user-requests']);
                    }
                });
        } else {
            this.router.navigate(['/user-requests']);
        }

    }

    onTypingPhoneNumber(value: string) {
        this.validatePhoneNumber(value);
    }

    validatePhoneNumber(phoneNumber: string) {
        if (phoneNumber?.length > MaxLengthValueInput.PhoneNumber) {
            this.setInvalidControl('userPhone', 'maxLengthInput');
        }
    }
}
