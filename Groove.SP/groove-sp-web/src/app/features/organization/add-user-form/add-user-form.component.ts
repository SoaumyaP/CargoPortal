import { Component, Input, Output, EventEmitter, OnChanges, SimpleChange, SimpleChanges, OnInit } from '@angular/core';
import { DropDowns, UserContextService, FormComponent, StringHelper, OrganizationType, Roles, MaxLengthValueInput } from 'src/app/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { OrganizationFormService } from '../organization-form/organization-form.service';
import { EmailValidationPattern } from 'src/app/core/models/constants/app-constants';

@Component({
    selector: 'app-add-user-form',
    templateUrl: './add-user-form.component.html',
    styleUrls: ['./add-user-form.component.scss']
})
export class AddUserFormComponent extends FormComponent implements OnInit {
    @Input() public addUserFormOpened: boolean = false;
    @Input() public model: any;
    @Input() public organizationType: any;
    @Output() close: EventEmitter<any> = new EventEmitter<any>();
    @Output() add: EventEmitter<any> = new EventEmitter<any>();
    isDisabledRoleSelection: boolean;
    patternEmail = EmailValidationPattern;

    validationRules = {
        email: {
            'required': 'label.email',
            'pattern': 'label.email',
            'duplicateUser': 'validation.duplicateUser'
        },
        contactName: {
            'required': 'label.contactName'
        },
        phone: {
            'maxLengthInput': MaxLengthValueInput.PhoneNumber
        },
        roleId: {
            'required': 'label.userRole'
        },
    };

    roleOptions = [];

    constructor(protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: OrganizationFormService,
        public translateService: TranslateService,
        private _userContext: UserContextService) {
        super(route, service, notification, translateService, router);
    }

    ngOnInit() {
        this.isDisabledRoleSelection = this.organizationType === OrganizationType.General;
        this.service.getUserRoles().subscribe((data: any) => {
            this.roleOptions = data;
            this.filterRoleOptionByOrgType();
        });
    }

    filterRoleOptionByOrgType() {
        switch (this.organizationType) {
            case OrganizationType.Agent:
                this.roleOptions = this.roleOptions.filter(c => c.id === Roles.Agent || c.id === Roles.CruiseAgent || c.id === Roles.Warehouse);
                break;
            case OrganizationType.Principal:
                this.roleOptions = this.roleOptions.filter(c => c.id === Roles.Principal || c.id === Roles.CruisePrincipal);
                break;

            case OrganizationType.General:
                this.roleOptions = this.roleOptions.filter(c => c.id === Roles.Shipper || c.id === Roles.Factory);

                break;
            default:
                break;
        }
    }

    onFormClosed() {
        this.addUserFormOpened = false;
        this.close.emit();
        this.resetCurrentForm();
    }

    onAddClick() {
        this.validateAllFields(false);

        if (!this.mainForm.valid) {
            return;
        }

        this.add.emit(this.model);
    }

    checkUserExists() {
        this.service.checkUserExists(this.model.email).subscribe(res => {
            if (res === true) {
                this.setInvalidControl('email', 'duplicateUser');
            }
        });
    }

    onTypingPhoneNumber(value: string) {
        if (value?.length > MaxLengthValueInput.PhoneNumber) {
            this.setInvalidControl('phone', 'maxLengthInput');
            this.formErrors['phone'] = this.translateService.instant('validation.maxLengthInput',
                {
                    maxValue: MaxLengthValueInput.PhoneNumber
                });
        }
    }
}
