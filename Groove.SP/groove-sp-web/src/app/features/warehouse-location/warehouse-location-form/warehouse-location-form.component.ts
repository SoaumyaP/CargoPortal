import { Component, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { faPencilAlt } from '@fortawesome/free-solid-svg-icons';
import { TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { DropDownListItemModel, DropdownListModel, FormComponent, StringHelper, UserContextService } from 'src/app/core';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { OrganizationReferenceDataModel } from 'src/app/core/models/organization.model';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { WarehouseLocationModel } from '../models/warehouse-location.model';
import { WarehouseLocationFormService } from './warehouse-location-form.service';

@Component({
    selector: 'app-warehouse-location-form',
    templateUrl: './warehouse-location-form.component.html',
    styleUrls: ['./warehouse-location-form.component.scss']
})
export class WarehouseLocationFormComponent extends FormComponent implements OnDestroy {

    modelName = 'warehouseLocations';
    model: WarehouseLocationModel;

    readonly faPencilAlt = faPencilAlt;

    countrySource: Array<DropdownListModel<number>> = [];
    countrySourceFilter: Array<DropdownListModel<number>> = [];

    locationSource: Array<DropdownListModel<number>> = [];
    locationSourceFilter: Array<DropdownListModel<number>> = [];

    organizationSource: Array<DropDownListItemModel<number>> = [];
    organizationSourceFilter: Array<DropDownListItemModel<number>> = [];

    customerList: Array<OrganizationReferenceDataModel> = [];
    isOrganizationViewAllow: boolean = false;

    // Form settings: validations....
    validationRules = {
        code: {
            required: 'label.warehouseCode',
            alreadyExists: 'label.warehouseCode'
        },
        name: {
            required: 'label.warehouseName'
        },
        addressLine1: {
            required: 'label.addressLine1'
        },
        countryId: {
            required: 'label.country'
        },
        locationId: {
            required: 'label.city'
        },
        organizationId: {
            required: 'label.provider'
        },
        contactEmail: {
            incorrect: 'validation.emailFormat'
        }
    };

    private _subscriptions: Array<Subscription> = [];

    constructor(
        protected route: ActivatedRoute,
        private _warehouseLocationFormService: WarehouseLocationFormService,
        public notification: NotificationPopup,
        public translateService: TranslateService,
        public router: Router,
        private _userContextService: UserContextService
    ) {
        super(route, _warehouseLocationFormService, notification, translateService, router);

        this._fetchDataSources();

        this._userContextService.isGranted(AppPermissions.Organization_Detail).subscribe(
            result => {
                this.isOrganizationViewAllow = result;
        });
    }

    onInitDataLoaded(): void {
        // Clear errors
        this.formErrors = {};

        // Load stored location and set country
        if (this.model?.locationId > 0) {
            const sub = this._warehouseLocationFormService.getLocationSameCountryDropDown(this.model.locationId).subscribe(
                locations => {
                    this.locationSource = locations;
                    this.locationSourceFilter = locations;
                    const countryId = Number.parseInt(locations[0].description, 10);
                    this.model.countryId = countryId;
                }
            );
            this._subscriptions.push(sub);
        }

        // Set organization/provider
        if (this.model?.organizationId > 0) {
            this.model.organizationName = this.organizationSource.find(x => x.value === this.model.organizationId)?.text;
        }

        // Get data List of Customers
        if (this.model?.id > 0) {
            this._warehouseLocationFormService.getCustomerList(this.model.id).subscribe(
                orgs => {
                    this.customerList = orgs;
                }
            );
        }
      }

    ngOnDestroy(): void {
        this._subscriptions.map(x => x.unsubscribe());
    }

    save(): void {
        this.validateAllFields(false);
        if (!this.mainForm.valid) {
            return;
        }

        const model = this.model;
        if (this.isAddMode) {
            this._warehouseLocationFormService.createWarehouseLocation(model).subscribe(
                r => {
                    this.notification.showSuccessPopup('save.sucessNotification', 'label.warehouseLocation');
                    this.router.navigate([`warehouse-locations/view/${r.id}`]);
                    this.modelId = r.id;
                    this.ngOnInitForAddMode();
                },
                httpErrorResponse => {
                    if (httpErrorResponse.status === 400) {
                        if (httpErrorResponse.error.message) {
                            const errTags = httpErrorResponse.error.message.split('#').filter(x => !StringHelper.isNullOrEmpty(x));
                            if (errTags[0] === 'WarehouseLocationCodeDuplicated') {
                                this.setInvalidControl('code', 'alreadyExists');
                                this.notification.showErrorPopup('save.failureNotification', 'label.warehouseLocation');
                            }
                        }
                    } else {
                        this.notification.showErrorPopup('save.failureNotification', 'label.warehouseLocation');
                    }
                },
            );
        } else {
        this._warehouseLocationFormService.updateWarehouseLocation(model).subscribe(
            r => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.warehouseLocation');
                this.router.navigate([`warehouse-locations/view/${this.model.id}`]);
                this.ngOnInit();
            },
            httpErrorResponse => {
                if (httpErrorResponse.status === 400) {
                    if (httpErrorResponse.error.message) {
                        const errTags = httpErrorResponse.error.message.split('#').filter(x => !StringHelper.isNullOrEmpty(x));
                        if (errTags[0] === 'WarehouseLocationCodeDuplicated') {
                            this.setInvalidControl('code', 'alreadyExists');
                            this.notification.showErrorPopup('save.failureNotification', 'label.warehouseLocation');
                        }
                    }
                } else {
                    this.notification.showErrorPopup('save.failureNotification', 'label.warehouseLocation');
                }
            },
        );
        }
    }

    cancel() {
        const confirmDlg = this.notification.showConfirmationDialog(
            'edit.cancelConfirmation',
            'label.warehouseLocation'
        );

        confirmDlg.result.subscribe((result: any) => {
            if (result.value) {
                this.backToList();
            }
        });
    }

    backToList() {
        this.router.navigate(['warehouse-locations']);
    }

    editWarehouseLocation() {
        this.router.navigate([`warehouse-locations/edit/${this.model.id}`]);
    }

    get isBusinessDataValid(): boolean {
        let isValid = true;
        const formErrorKeys = Object.keys(this.formErrors);
        formErrorKeys.map(
            key => {
                const value = this.formErrors[key];
                if (!StringHelper.isNullOrEmpty(value)) {
                    isValid = false;
                }
            }
        );
        return isValid;
    }

    private _fetchDataSources(): void {

        // Get data for countries
        let sub = this._warehouseLocationFormService.getCountries()
            .subscribe(
                x => {
                    this.countrySource = x;
                    this.countrySourceFilter = x;
                }
            );
        this._subscriptions.push(sub);

        // Get data for organizations
        sub = this._warehouseLocationFormService.getAgentOrganizations()
            .subscribe(
                x => {
                    this.organizationSource = x;
                    this.organizationSourceFilter = x;
                }
            );
        this._subscriptions.push(sub);

    }

    // Handlers for Country

    onCountryValueChanged(value?: number): void {
        if (!StringHelper.isNullOrEmpty(value) && value > 0) {
            const sub = this._warehouseLocationFormService.getLocations(value).subscribe(
                x => {
                    this.locationSource = x;
                    this.locationSourceFilter = x;

                    // clear selected location if it's not in the country.
                    const selectedLocationId = x.findIndex(x => x.value == this.model.locationId);
                    if (selectedLocationId === -1) {
                        this.model.locationId = null;
                    }
                }
            );
            this._subscriptions.push(sub);
        }

    }

    onCountryFilterChanged(value: string): void {
        if (StringHelper.isNullOrEmpty(value)) {
            this.countrySourceFilter = this.countrySource;
        } else {
            this.countrySourceFilter = this.countrySource.filter(x => x.label.toLowerCase().startsWith(value.toLowerCase()));
        }
    }

    // Handlers for Location/City

    onLocationFilterChanged(value: string): void {
        if (StringHelper.isNullOrEmpty(value)) {
            this.locationSourceFilter = this.locationSource;
        } else {
            this.locationSourceFilter = this.locationSource.filter(x => x.label.toLowerCase().startsWith(value.toLowerCase()));
        }
    }

    // Handlers for Organization

    onOrganizationFilterChanged(value: string): void {
        this.model.organizationId = null;
        if (StringHelper.isNullOrEmpty(value) || value.length < 3) {
            this.organizationSourceFilter = [];
        } else {
            this.organizationSourceFilter = this.organizationSource.filter(x => x.text.toLowerCase().startsWith(value.toLowerCase()));
        }
    }

    onOrganizationValueChanged(value: string): void {
        const matchedOrg = this.organizationSource.find(x => x.text.toLowerCase() === (value.toLowerCase()));
        if (matchedOrg) {
            this.model.organizationId = matchedOrg.value;
        } else {
            this.model.organizationId = null;
        }
    }

    // Handler for Contact email

    onContactEmailValueChanged(value: string): void {
        if (StringHelper.isNullOrEmpty(value)) {
            this.mainForm.controls['contactEmail'].setErrors(null);
            delete this.formErrors['contactEmail'];

            return;
        }

        if (StringHelper.validateEmail(value)) {
            this.mainForm.controls['contactEmail'].setErrors(null);
            delete this.formErrors['contactEmail'];

        } else {
            this.mainForm.controls['contactEmail'].setErrors({'incorrect': true});
            this.formErrors['contactEmail'] = this.translateService.instant('validation.emailFormat');

        }
    }

    // Handler for List of Customers

    concatenateAddressLines(address: string, addressLine2?: string, addressLine3?: string, addressLine4?: string) {
        let concatenatedAddress = address;
        if (!StringHelper.isNullOrEmpty(addressLine2)) {
          concatenatedAddress += '<br/>' + addressLine2;
        }
        if (!StringHelper.isNullOrEmpty(addressLine3)) {
          concatenatedAddress += '<br/>' + addressLine3;
        }
        if (!StringHelper.isNullOrEmpty(addressLine4)) {
          concatenatedAddress += '<br/>' + addressLine4;
        }
        return concatenatedAddress;
      }

}
