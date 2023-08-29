import { Component, OnInit, ViewChild, ElementRef, Input, Output, EventEmitter } from '@angular/core';
import { StringHelper, UserContextService } from 'src/app/core';
import { TranslateService } from '@ngx-translate/core';
import * as $ from 'jquery';
import * as bootstrap from 'bootstrap';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { PODelegationService } from './po-delegation-modal.service';
import { faLessThan } from '@fortawesome/free-solid-svg-icons';
import { AutoCompleteComponent } from '@progress/kendo-angular-dropdowns';
import { FormControl, FormGroup, NgForm, Validators } from '@angular/forms';
@Component({
    selector: 'app-po-delegation-modal',
    templateUrl: './po-delegation-modal.component.html',
    styleUrls: ['./po-delegation-modal.component.scss']
})
export class PODelegationModalComponent implements OnInit {
    isDone = false;
    delegatedOrganizationId: number;
    notifyUserId: number;
    organizationList: Array<any> = [];
    form: FormGroup;

    @ViewChild('orgAutoComplete', { static: true }) public orgAutoComplete: AutoCompleteComponent;
    @ViewChild('notifyUserAutocomplete', { static: true }) public notifyUserAutocomplete: AutoCompleteComponent;
    @ViewChild('Cancel', { static: true }) public cancelButton: ElementRef;
    @Input('contactList') contactList: Array<any>;
    @Input('poId') poId: any;
    @Output() close: EventEmitter<any> = new EventEmitter();
    acpTimeout: any;
    orgLoading = false;
    orgFilter: any;
    affiliateIds: any;
    notifyUserLoading = false;
    notifyUserFilter: any;
    notifyUserList: any;
    currentUserOrgId: any;

    constructor(
        public notification: NotificationPopup,
        public service: PODelegationService,
        private userContext: UserContextService
    ) {
        this.userContext.getCurrentUser().subscribe(user => {
            if (user) {
                if (!user.isInternal) {
                    this.service.affiliateCodes = user.affiliates;
                    this.affiliateIds = JSON.parse(user.affiliates);
                    this.currentUserOrgId = user.organizationId;
                }
            }
        });
    }

    ngOnInit() {
        this.form = new FormGroup({
            organizationIdControl: new FormControl('',Validators.required),
            notifyUserAutocomplete: new FormControl('',Validators.required)
        });

        this.service.getOrganizations().subscribe(
            data => {
                if (this.service.affiliateCodes) {
                    this.organizationList = data.filter(x => x.id !== this.currentUserOrgId && this.affiliateIds.indexOf(x.id) !== -1) ;
                }
                else {
                    this.organizationList = data;
                }
            }
        );
    }

    onValueChange(value) {
        this.delegatedOrganizationId = null;
        this.notifyUserList = [];
        this.notifyUserFilter = [];
        this.notifyUserId = null;
        this.notifyUserAutocomplete.reset();
        this.form.controls['notifyUserAutocomplete'].setValue(null);

        let delegatedOrganization = this.organizationList.find(x => x.name.toLowerCase() === value.toLowerCase());
        if (delegatedOrganization) {
            this.delegatedOrganizationId = delegatedOrganization.id;

            this.service.getUsersByOrganizationId(delegatedOrganization.id).subscribe(data =>
                {
                    this.notifyUserList = data;
                }
            );
        }
    }

    onFilterChange(value) {
        if (value.length >= 3) {
            this.orgAutoComplete.toggle(false);
            this.orgFilter = [];
            clearTimeout(this.acpTimeout);
            this.acpTimeout = setTimeout(() => {
                this.orgLoading = true;
                value = value.toLowerCase();
                let take = 10;
                for (let i = 0; i < this.organizationList.length && take > 0; i++) {
                    if (this.organizationList[i].name.toLowerCase().indexOf(value) !== -1) {
                        this.orgFilter.push(this.organizationList[i]);
                        take--;
                    }
                }
                this.orgAutoComplete.toggle(true);
                this.orgLoading = false;
            }, 400);
        } else {
            this.orgLoading = false;
            if (this.acpTimeout) {
                clearTimeout(this.acpTimeout);
            }
            this.orgAutoComplete.toggle(false);
        }
    }

    onNotifyUserValueChange(value) {
        this.notifyUserId = null;
        if (value) {
                let notifyUser = this.notifyUserList.find(x => x.username === value);
                if (notifyUser) {
                    this.notifyUserId = notifyUser.id;
            }
        }
    }

    onNotifyUserFilterChange(value) {
        this.notifyUserFilter = [];
        if (value.length >= 3) {
            this.notifyUserLoading = true;
            this.notifyUserFilter = this.notifyUserList.filter((s) => s.username.toLowerCase().indexOf(value.toLowerCase()) !== -1);
            this.notifyUserLoading = false;
        } else {
            this.notifyUserLoading = false;
        }
    }

    onSubmit() {     
        this.form.markAllAsTouched();
        if (this.isDisableDelegate) {
            return;
        }
        
        let model = {
            id: this.poId,
            organizationId: this.delegatedOrganizationId,
            notifyUserId: this.notifyUserId
        }
        this.service.delegatePO(model).subscribe(
            data => {
                this.isDone = true;
                this.cancelButton.nativeElement.click();
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.poDelegation');
            });
    }

    onClose() {
        this.delegatedOrganizationId = null;
        this.notifyUserId = null;
        this.orgAutoComplete.reset();
        this.notifyUserAutocomplete.reset();
        this.close.emit(this.isDone);
        this.isDone = false;
        this.form.reset();
    }

    get isDisableDelegate() {
        if (this.delegatedOrganizationId && this.notifyUserId) {
            return false;
        }

        return true;
    }
}
