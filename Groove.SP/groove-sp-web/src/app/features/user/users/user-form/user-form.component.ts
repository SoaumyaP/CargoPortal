import { Component, ViewChild } from '@angular/core';
import { FormComponent } from 'src/app/core/form';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { environment } from 'src/environments/environment';
import { TranslateService } from '@ngx-translate/core';
import { faEnvelope, faPowerOff, faLink, faSave, faPaperPlane } from '@fortawesome/free-solid-svg-icons';
import { UserFormService } from './user-form.service';
import { UsersService } from '../users.service';
import { AutoCompleteComponent } from '@progress/kendo-angular-dropdowns';
import { UserStatus, StringHelper, UserContextService, DATE_FORMAT, OrganizationStatus, DropDowns, DropdownListModel, RoleStatus, DATE_HOUR_FORMAT, ListService, HttpService, MaxLengthValueInput } from 'src/app/core';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { UserProfileModel } from 'src/app/core/models/user/user-profile.model';
import { DataSourceRequestState, SortDescriptor, toDataSourceRequestString } from '@progress/kendo-data-query';
import { DataStateChangeEvent, GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import { Observable } from 'rxjs/Observable';
import { SupplierRelationshipListComponent } from 'src/app/features/organization/supplier-relationship-list/supplier-relationship-list.component';
import { map } from 'rxjs/operators';
import { tap } from 'rxjs/operators';
import { forkJoin } from 'rxjs';

@Component({
    selector: 'app-user-form',
    templateUrl: './user-form.component.html',
    styleUrls: ['./user-form.component.scss']
})
export class UserFormComponent extends FormComponent {
    faEnvelope = faEnvelope;
    faPowerOff = faPowerOff;
    faLink = faLink;
    faSave = faSave;
    faPaperPlane = faPaperPlane;

    modelName = 'user';
    DATE_FORMAT = DATE_FORMAT;
    DATE_HOUR_FORMAT = DATE_HOUR_FORMAT;
    @ViewChild('organizationAutoComplete', { static: false }) public organizationAutoComplete: AutoCompleteComponent;
    organizationList: any;
    backupModel: any;
    currentUser: UserProfileModel;
    userStatus = UserStatus;

    defaultDropDownRole: { name: string, id: number } =
    {
        name: 'label.select',
        id: null
    };

    defaultImageUrl: string = 'assets/images/avatar_default.svg';

    nameString = '';
    internalUserOrganization = '';

    organizationStatus = OrganizationStatus;

    roleOptions: any;
    organizationTypeName: any;

    initData = [
        { sourceUrl: `${environment.commonApiUrl}/countries/dropDown` },
        { sourceUrl: `${environment.apiUrl}/roles/official` },
    ];

    countryList: any;
    countryFilter: any;

    organizationOptions: any[];

    validationRules = {
        'roleId': {
            'required': 'label.role'
        },
        'name': {
            'required': 'label.name'
        },
        'phone': {
            maxLengthInput: MaxLengthValueInput.PhoneNumber
        },
        'companyName': {
            'required': 'label.companyName'
        },
        'organizationId': {
            'required': 'label.organization',
            'invalid': 'label.organization'
        }
    };

    isUserHasEditPermission: boolean;

    // User historical login declarations
    gridSort: SortDescriptor[] = [
        {
            field: 'accessDateTime',
            dir: 'desc'
        }
    ];

    gridState: DataSourceRequestState = {
        sort: this.gridSort,
        skip: 0,
        take: 20
    };

    public gridData: GridDataResult & { totalRowCount: number } = {
        data: [],
        total: 0,
        // Not applied filter
        totalRowCount: 0
    };

    isGridLoading: boolean = false;
    isShowDeleteButton = false;
    isShowResendActivationEmailButton = false;

    constructor(protected route: ActivatedRoute,
        public usersService: UsersService,
        public service: UserFormService,
        public notification: NotificationPopup,
        public translateService: TranslateService,
        public router: Router,
        private userContext: UserContextService,
        private _httpService: HttpService) {
        super(route, service, notification, translateService, router);
    }

    onInitDataLoaded(data) {
        this.backupModel = Object.assign({}, this.model);
        this.backupModel.role = Object.assign({}, this.model.role);
        this.isInitDataLoaded = false;
        this.bindingModel();
        this.countryList = data[0];
        this.roleOptions = data[1].filter(r => {
            return r.isInternal === this.model.isInternal;
        });

        this.countryFilter = this.countryList;

        this.userContext.getCurrentUser().subscribe(user => {
            this.currentUser = user;
            this.isUserHasEditPermission = this.currentUser?.permissions?.some(c => c.name === AppPermissions.User_UserDetail_Edit);
            this.isShowDeleteButton = this.currentUser.isInternal && this.model.status === this.userStatus.WaitForConfirm;
            this.isShowResendActivationEmailButton = this.currentUser.isInternal && this.model.status === this.userStatus.WaitForConfirm
        });

        this.fetchGridData();
    }

    onCountryFilterChanged(value) {
        this.countryFilter = this.countryList.filter((s) => s.label.toLowerCase().indexOf(value.toLowerCase()) !== -1);
    }

    saveUser() {
        if  (this.mainForm.valid) {
            this.saveUserAction(this.model);
        }
    }

    saveUserAction(tempModel, isCheckValid = true) {
        if (!isCheckValid || this.mainForm.valid) {
            this.service.update(tempModel.id, tempModel).subscribe(
                user => {
                    this.notification.showSuccessPopup('save.sucessNotification', 'label.user');
                    if (tempModel.id === this.currentUser.id) {
                        this.userContext.queryCurrentUser();
                    }
                    this.ngOnInit();
                },
                error => {
                    this.notification.showErrorPopup('save.failureNotification', 'label.user');
                });
        } else {
            this.validateAllFields(false);
        }
    }

    bindingModel() {
        this.nameString = this.model.name;

        if (this.model.role.status === RoleStatus.Inactive) {
            this.model.role = { id: null };
        }

        if (this.model.isInternal) {
            this.bindOrganizationTypeName();
            return this.isInitDataLoaded = true;
        }
        this.service.getOrganizationList(this.model.status).subscribe(list => {
            this.organizationList = list;
            const organization = this.organizationList.find(x => x.id === this.model.organizationId);
            this.bindingOrganization(organization);
            this.isInitDataLoaded = true;
        });
    }

    organizationFilterChange(item) {
        if (item.length >= 3) {
            const data = this.usersService.getOrganizationListByInput(this.organizationList, this.model.role.id, item);
            this.organizationOptions = data.map(x => new DropdownListModel<number>(x.orgCodeName, x.id));
        } else {
            this.organizationAutoComplete.toggle(false);
        }
    }

    organizationValueChange(item) {
        if (item) {
            const dataOrg = this.organizationList.find(x => x.orgCodeName === item);
            this.bindingOrganization(dataOrg);
            if (dataOrg != null) {
                this.formErrors['organizationId'] = '';
            } else {
                if (!StringHelper.isNullOrEmpty(item)) {
                    this.setInvalidControl('organizationId');
                }
            }
        }
    }

    bindingOrganization(organization) {
        if (organization != null) {
            this.model.organizationId = organization.id;
            this.model.organizationName = organization.name;
            this.model.organizationCode = organization.code;
            this.model.organizationCodeName = organization.orgCodeName;
            this.model.organizationType = organization.organizationType;
            this.model.organizationStatus = organization.status;
        } else {
            this.model.organizationId = null;
            this.model.organizationName = '';
            this.model.organizationCode = '';
            this.model.organizationCodeName = '';
            this.model.organizationType = 0;
            this.model.organizationStatus = null;
        }
        this.bindOrganizationTypeName();
    }

    backList() {
        this.router.navigate(['/users']);
    }

    roleValueChange(value) {
        const role = this.roleOptions.find(x => x.id === value);
        this.model.role.name = role != null ? role.name : '';

        const requiredOrgType = this.usersService.getRequiredOrgTypeByRole(this.model.role.id);
        if (requiredOrgType && this.model.organizationType !== requiredOrgType) {
            this.organizationAutoComplete.reset();
            this.bindingOrganization(null);
        }
    }

    changeUserStatusConfirm(status) {
        if (this.mainForm.dirty) {
            const confirmDlg = this.notification.showConfirmationDialog('edit.saveConfirmation', 'label.user');

            confirmDlg.result.subscribe(
                (result: any) => {
                    if (!result.value) {
                        this.model = Object.assign({}, this.backupModel);
                        this.model.role = Object.assign({}, this.backupModel.role);

                        if (!this.model.isInternal) {
                            const organization = this.organizationList.find(x => x.id === this.model.organizationId);
                            this.bindingOrganization(organization);
                            if (this.model.status === UserStatus.Inactive &&
                                this.model.organizationStatus === OrganizationStatus.Inactive) {
                                return this.notification.showErrorPopup('msg.cannotActiveOrganization', 'label.user');
                            }
                        }
                        this.changeUserStatusAction(status, false);
                    } else {
                        this.onSubmit();
                        this.changeUserStatusAction(status);
                    }
                });
        } else {
            this.onSubmit();
            this.changeUserStatusAction(status);
        }
    }

    changeUserStatusAction(status, isCheckValid = true) {
        const tmpModel = Object.assign({}, this.model);
        tmpModel.role = Object.assign({}, this.model.role);
        tmpModel.status = status;
        this.saveUserAction(tmpModel, isCheckValid);
    }

    private bindOrganizationTypeName() {
        const organizationType = DropDowns.OrganizationTypeList.find(o => o.value === this.model.organizationType
            && o.isOfficial !== this.model.isInternal);
        this.organizationTypeName = organizationType != null ? organizationType.text : '';
    }

    // User historical login methods
    public gridPageChange(event: PageChangeEvent): void {
        this.gridState.skip = event.skip;
        this.fetchGridData();
    }

    public gridSortChange(sort: SortDescriptor[]): void {
        this.gridState.sort = sort;
        this.fetchGridData();
    }

    gridStateChange(state: DataStateChangeEvent) {
        this.gridState = state;
        this.fetchGridData();
    }

    public fetchGridData() {
        const email = this.model.email;
        let url = `${environment.apiUrl}/users/trace?${toDataSourceRequestString(this.gridState)}&email=${encodeURIComponent(email)}`;

        this.isGridLoading = true;

        const $obs1 = this._httpService
            .get(`${url}`)
            .map(({ data, total }: GridDataResult) =>
                (<GridDataResult>{
                    data: data,
                    total: total
                })
            );

        url = `${environment.apiUrl}/users/trace?action=count&email=${encodeURIComponent(email)}`;
        const $obs2 = this._httpService
            .get(`${url}`)
            .map((totalRowCount: number) => totalRowCount);

        forkJoin($obs1, $obs2)
        .pipe(
            map((res) => {
                this.isGridLoading = false;
                const data = {
                    // Data returned with filter and paging applied
                    data : res[0].data,
                    // Count on rows of returned with filter and paging applied
                    total: res[0].total,
                    // Total row count without filter applied
                    totalRowCount: res[1]
                };
                this.gridData = data;
            })
        ).subscribe();

    }

    onDeleteUser() {
        if (this.model.status === UserStatus.WaitForConfirm) {
            const confirmDlg = this.notification.showConfirmationDialog('msg.removeUser', 'label.user');
            confirmDlg.result.subscribe(
                (result: any) => {
                    if (result.value) {
                        this.service.delete(this.model.id).subscribe(
                            r => {
                                this.notification.showSuccessPopup('save.sucessNotification', 'label.user');
                                this.router.navigate(['/users']);
                            },
                            err => {
                                this.notification.showErrorPopup('save.failureNotification', 'label.user');
                            }
                        );
                    }
                });
        }
    }

    onResendActivationEmail(): void {
        this.service.sendActivationEmail(this.model.id).subscribe(
            (success) => this.notification.showSuccessPopup('msg.sendActivationEmailSuccessfully', 'label.user'),
            (error) => this.notification.showErrorPopup('save.failureNotification', 'label.user')
        )
    }

    onTypingPhone(value: string) {
        if (value?.length > MaxLengthValueInput.PhoneNumber) {
            this.setInvalidControl('phone', 'maxLengthInput');
            this.formErrors['phone'] = this.translateService.instant('validation.maxLengthInput',
                {
                    maxValue: MaxLengthValueInput.PhoneNumber
                });
        }
    }
}
