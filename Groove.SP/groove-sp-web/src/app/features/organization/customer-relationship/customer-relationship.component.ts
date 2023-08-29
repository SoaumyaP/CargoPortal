import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { OrganizationStatus, StringHelper, UserContextService } from 'src/app/core';
import { UserAction } from '../organization.constants';
import { OrganizationFormService } from '../organization-form/organization-form.service';
import { faPaperPlane } from '@fortawesome/free-solid-svg-icons';
import { ConnectionType } from 'src/app/core/models/enums/enums';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';

@Component({
    selector: 'app-customer-relationship',
    templateUrl: './customer-relationship.component.html',
    styleUrls: ['./customer-relationship.component.scss']
})
export class CustomerRelationshipComponent implements OnInit {
    @Input() isViewMode: boolean;
    @Input() customerList: Array<any>;
    @Input() isAdmin = false;
    @Input() supplierOrg: any;
    @Input() organizationCodeOptions: any[];
    @Output()
    onUserAction = new EventEmitter<any>();

    @Input()
    canEditInfor = false;
    connectionType = ConnectionType;
    faPaperPlane = faPaperPlane;

    UserAction = UserAction;
    OrganizationStatus = OrganizationStatus;
    isAddingNewCustomer = false;
    currentUser: any;

    constructor(public notification: NotificationPopup,
        public service: OrganizationFormService,
        public translateService: TranslateService,
        private _userContextService: UserContextService) {
    }

    ngOnInit(): void {
        this._userContextService.getCurrentUser().subscribe((user) => {
            if (user != null) {
                this.currentUser = user;
            }
        });
    }

    onOrganizationCodeAssignmentChanged(value, rowIndex) {
        this.service.getOrganization(value).subscribe(
            (data: any) => {
                this.customerList[rowIndex] = {
                    id: data.id,
                    code: data.code,
                    name: data.name,
                    organizationType: data.organizationType,
                    organizationTypeName: data.organizationTypeName,
                    countryName: data.location.country.name,
                    contactName: data.contactName,
                    contactEmail: data.contactEmail,
                    connectionType: this.checkHasEdisonCode ? ConnectionType.Active : ConnectionType.Pending,
                    connectionTypeName: this.checkHasEdisonCode ? 'label.active' : 'label.pending',
                    userAction: UserAction.AddCustomer
                };
            },
            error => {
                console.log(error);
            });
    }

    get isEditableCustomerRefId(): boolean {return this.currentUser && this.currentUser.isInternal;}

    get checkHasEdisonCode() {
        return !StringHelper.isNullOrEmpty(this.supplierOrg.edisonInstanceId) &&
                !StringHelper.isNullOrEmpty(this.supplierOrg.edisonCompanyCodeId);
    }

    addBlankCustomerOrganization() {
        if (this.customerList != null) {
            this.customerList.push({
                userAction: UserAction.AddCustomer
            });
        }
        this.isAddingNewCustomer = true;
    }

    resendEmail(customer) {
        const msg = this.translateService.instant('msg.resendConnection',
                    {
                        orgName: customer.contactEmail
                    });
        const confirmDlg = this.notification.showConfirmationDialog(msg, 'label.organization');

        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.service.resendConnectionToCustomer(this.supplierOrg.id, customer.id).subscribe(org => {
                        this.notification.showSuccessPopup('save.sucessNotification', 'label.organization');
                        customer.connectionType = ConnectionType.Pending;
                        customer.connectionTypeName = 'label.pending';
                    },
                    error => {
                        this.notification.showErrorPopup('save.failureNotification', 'label.organization');
                    });
                }
            });
    }

    saveCustomerRefId(customerId, event) {
        this.service.updateCustomerRefId(customerId, this.supplierOrg.id, event).subscribe(data => {
            this.notification.showSuccessPopup('save.sucessNotification', 'label.organization');
        }, error => {
            this.notification.showErrorPopup('save.failureNotification', 'label.organization');
        });
    }
}
