import { Component, ElementRef, ViewChild } from '@angular/core';
import { UserContextService, StringHelper, DropDowns, MaxLengthValueInput } from 'src/app/core';
import { FormComponent } from 'src/app/core/form';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { environment } from 'src/environments/environment';
import { TranslateService } from '@ngx-translate/core';
import { UserProfileFormService } from './user-profile.service';
import { faEnvelope, faCamera } from '@fortawesome/free-solid-svg-icons';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { GAEventCategory } from 'src/app/core/models/constants/app-constants';

@Component({
    selector: 'app-user-profile',
    templateUrl: './user-profile.component.html',
    styleUrls: ['./user-profile.component.scss']
})
export class UserProfileFormComponent extends FormComponent {
    faEnvelope = faEnvelope;
    faCamera = faCamera;

    @ViewChild('profilePictureElement', { static: false }) public profilePictureElement: ElementRef;

    modelName = '';

    defaultImageUrl: string = 'assets/images/avatar_default.svg';

    nameString = '';

    isEditMode = true;

    initData = [
        { sourceUrl: `${environment.commonApiUrl}/countries/dropDown` }
    ];

    countryList: any;
    countryFilter: any;
    isChangeProfilePicture: boolean;
    organizationTypeName: any;
    validationRules = {
        'name': {
            'required': 'label.name'
        },
        'phone': {
            'maxLengthInput': MaxLengthValueInput.PhoneNumber
        },
        'companyName': {
            'required': 'label.companyName'
        }
    };
    maxLengthInput = MaxLengthValueInput.PhoneNumber;

    constructor(
        public router: Router,
        protected route: ActivatedRoute,
        public service: UserProfileFormService,
        public notification: NotificationPopup,
        public translateService: TranslateService,
        private userContext: UserContextService,
        private _gaService: GoogleAnalyticsService) {
        super(route, service, notification, translateService, router);
    }

    onInitDataLoaded(data) {
        this.isInitDataLoaded = false;
        this.modelName = 'userProfile';
        this.isChangeProfilePicture = false;
        this.userContext.getCurrentUser().subscribe(user => {
            if (user === null) {
                return;
            }
            this.bindingModel(user);
            this.isInitDataLoaded = true;
        });
        this.countryList = data[0];
        this.countryFilter = this.countryList;
        this.formErrors['unvalidFile'] = '';
    }

    onCountryFilterChanged(value) {
        this.countryFilter = this.countryList.filter((s) => s.label.toLowerCase().indexOf(value.toLowerCase()) !== -1);
    }

    saveUserProfile() {
        this.validatePhoneNumber(this.model.phone);
        if (this.mainForm.valid) {
            this.service.updateCurrentUser(this.model).subscribe(
                user => {
                    this.notification.showSuccessPopup('save.sucessNotification', 'label.userProfile');
                    this.userContext.queryCurrentUser();
                    this.isChangeProfilePicture = false;
                    this._gaService.emitEvent('edit', GAEventCategory.UserProfile, 'Edit');
                },
                error => {
                    this.notification.showErrorPopup('save.failureNotification', 'label.userProfile');
                });
        } else {
            this.validateAllFields(false);
        }
    }

    bindingModel(user) {
        this.model = user;
        this.nameString = user.name;

        const organizationType = DropDowns.OrganizationTypeList.find(o => o.value === this.model.organizationType
            && o.isOfficial !== this.model.isInternal);
        this.organizationTypeName = organizationType != null ? organizationType.text : '';
    }

    cancelEditUserProfile() {
        const confirmDlg = this.notification.showConfirmationDialog('edit.cancelConfirmation', 'label.userProfile');

        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.profilePictureElement.nativeElement.value = '';
                    this.modelName = '';
                    this.ngOnInit();
                }
            });
    }

    successUploadProfileEventHandler(e) {
        const validImageFile = !StringHelper.isNullOrEmpty(e.target) &&
            this.validateUploadFile(e.target.files[0]);

        if (validImageFile) {
            this.isChangeProfilePicture = true;
            const reader = new FileReader();
            reader.onload = (event: any) => {
                this.model.profilePicture = event.target.result;
            };

            reader.readAsDataURL(e.target.files[0]);
        }
    }

    validateUploadFile(file: File) {
        if (StringHelper.isNullOrEmpty(file)) {
            return false;
        } else if (file.size > StringHelper.profilePictureRestrictions.maxFileSize) {
            this.formErrors['unvalidFile'] = this.translateService.instant('validation.fileTooLarge');
            return false;
        } else if (/\.(jpg|jpeg|png)$/.test(file.name.toLowerCase()) === false) {
            this.formErrors['unvalidFile'] = this.translateService.instant('validation.invalidFormat',
                {
                    fieldName: this.translateService.instant('label.image')
                });
            return false;
        }

        this.formErrors['unvalidFile'] = '';
        return true;
    }

    selectImage() {
        const profilePicture = this.profilePictureElement.nativeElement;
        profilePicture.click();
    }

    onTypingPhoneNumber(value: string) {
        this.validatePhoneNumber(value);
    }

    validatePhoneNumber(phoneNumber: string) {
        if (phoneNumber?.length > MaxLengthValueInput.PhoneNumber) {
            this.setInvalidControl('phone', 'maxLengthInput');
        }
    }
}
