import { Component, Input, Output, EventEmitter, OnInit, ViewChild } from '@angular/core';
import { UserContextService, FormComponent, StringHelper, OrganizationType, MaxLengthValueInput } from 'src/app/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { OrganizationFormService } from '../organization-form/organization-form.service';
import { AutoCompleteComponent } from '@progress/kendo-angular-dropdowns';
import { SupplierRelationshipListService } from '../supplier-relationship-list/supplier-relationship-list.service';
import { EmailValidationPattern } from 'src/app/core/models/constants/app-constants';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { AbstractControl } from '@angular/forms';

@Component({
    selector: 'app-add-supplier-relationship-form',
    templateUrl: './add-supplier-relationship-form.component.html',
    styleUrls: ['./add-supplier-relationship-form.component.scss']
})
export class AddSupplierRelationshipFormComponent extends FormComponent implements OnInit {
    @Input() public addSupplierFormOpened: boolean = false;
    @Input() public organizationType: any;
    @Input() public selectedSupplierList: any[];
    @Output() close: EventEmitter<any> = new EventEmitter<any>();
    @Output() addSupplierRelationship: EventEmitter<any> = new EventEmitter<any>();
    @Output() addSupplier: EventEmitter<any> = new EventEmitter<any>();

    @ViewChild('supplierAutoComplete', { static: false }) public supplierAutoComplete: AutoCompleteComponent;
    @ViewChild('countryAutoComplete', { static: false }) public countryAutoComplete: AutoCompleteComponent;
    @ViewChild('cityAutoComplete', { static: false }) public cityAutoComplete: AutoCompleteComponent;
    selectedSupplierName: any;
    acpTimeout: any;
    supplierLoading = false;
    supplierList: any[];
    supplierFilter: any[];
    isShowSelectButton = false;
    isSelectedSupplierDuplicated = false;
    isAddNewSupplierMode = false;
    countryLoading = false;
    countryList: any[];
    countryFilter: any[];
    selectedCountryName: string;
    selectedCountry: any;
    cityLoading = false;
    cityList: any[];
    cityFilter: any[];
    selectedCityName: string;
    isSupplierNameNullOrEmpty = true;
    patternEmail = EmailValidationPattern;
    validationRules = {
        'organizationName': {
            'required': 'label.organizationName'
        },
        'contactName': {
            'required': 'label.contactName'
        },
        'contactEmail': {
            'required': 'label.contactEmail',
            'pattern': 'label.contactEmail'
        },
        'countryName': {
            'required': 'label.country',
            'invalid': 'label.country'
        },
        'cityName': {
            'required': 'label.city',
            'invalid': 'label.city'
        },
        'contactNumber': {
            'required': 'label.contactNumber',
            maxLengthInput: MaxLengthValueInput.PhoneNumber
        },
        'supplierAutoComplete': {
            required: 'validation.requiredThisField'
        }
    };


    constructor(protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: OrganizationFormService,
        public supplierListService: SupplierRelationshipListService,
        public translateService: TranslateService,
        private _userContext: UserContextService) {
        super(route, service, notification, translateService, router);
    }

    ngOnInit() {
        this.service.getActiveCodesExcludeIds(0).subscribe(
            (organizations: any[]) => {
                this.supplierList = organizations.filter(x => x.organizationType === OrganizationType.General);
                this.supplierList.forEach(s => s.customValue = s.code.trim() + ' - ' + s.name.trim());
            });

        this.supplierListService.getCountries().subscribe(countries => {
            this.countryList = countries;
        });
    }

    onSupplierFilterChange(value) {
        if (value.length >= 3) {
            this.supplierAutoComplete.toggle(false);
            this.supplierFilter = [];
            clearTimeout(this.acpTimeout);
            this.acpTimeout = setTimeout(() => {
                this.supplierLoading = true;
                value = value.toLowerCase();
                let take = 10;
                for (let i = 0; i < this.supplierList.length && take > 0; i++) {
                    if (this.supplierList[i].customValue.toLowerCase().indexOf(value) !== -1 ||
                        (!StringHelper.isNullOrEmpty(this.supplierList[i].websiteDomain) &&
                            this.supplierList[i].websiteDomain.toLowerCase().indexOf(value) >= 0) ||
                        (!StringHelper.isNullOrEmpty(this.supplierList[i].contactNumber) &&
                            this.supplierList[i].contactNumber.toLowerCase().indexOf(value) >= 0)) {

                        this.supplierFilter.push(this.supplierList[i]);
                        take--;
                    }
                }
                this.supplierAutoComplete.toggle(true);
                this.supplierLoading = false;
            }, 400);
        } else {
            this.supplierLoading = false;
            if (this.acpTimeout) {
                clearTimeout(this.acpTimeout);
            }
            this.supplierAutoComplete.toggle(false);
        }
        this.isSupplierNameNullOrEmpty = (value == null || value === '');
    }

    onSupplierValueChange(value) {
        this.isSelectedSupplierDuplicated = false;
        this.isShowSelectButton = false;
        this.model = {};
        const selectedOrganization = this.supplierList.find(x => x.customValue.toLowerCase() === value.toLowerCase());

        if (selectedOrganization) {
            if (this.selectedSupplierList && this.selectedSupplierList.find(s => s.id === selectedOrganization.id)) {
                this.isSelectedSupplierDuplicated = true;
            }

            this.service.getOrganization(selectedOrganization.id).subscribe((organization: any) => {
                this.model = organization;
                if (organization.location) {
                    this.selectedCountryName = organization.location.country.name;
                    this.selectedCountry = organization.location.country;
                    this.selectedCityName = organization.location.locationDescription;
                    this.model.locationId = organization.location.id;
                    this.supplierListService.getLocations(organization.location.countryId).subscribe(cities => {
                        this.cityList = cities;
                    });
                } else {
                    this.selectedCountry = null;
                }
                this.isShowSelectButton = this.isSelectedSupplierDuplicated ? false : true;
            });
        }
    }

    onCountryFilterChange(value) {
        this.selectedCityName = null;
        if (value.length >= 3) {
            this.countryAutoComplete.toggle(false);
            this.countryFilter = [];
            this.cityFilter = [];
            this.selectedCountry = null;
            this.model.locationId = null;
            clearTimeout(this.acpTimeout);
            this.acpTimeout = setTimeout(() => {
                this.countryLoading = true;
                value = value.toLowerCase();
                let take = 10;
                for (let i = 0; i < this.countryList.length && take > 0; i++) {
                    if (this.countryList[i].label.toLowerCase().indexOf(value) !== -1) {
                        this.countryFilter.push(this.countryList[i]);
                        take--;
                    }
                }
                this.countryAutoComplete.toggle(true);
                this.countryLoading = false;
            }, 400);
        } else {
            this.countryLoading = false;
            if (this.acpTimeout) {
                clearTimeout(this.acpTimeout);
            }
            this.countryAutoComplete.toggle(false);
        }
    }

    onCountryValueChange(value) {
        this.selectedCountry = null;
        if (StringHelper.isNullOrEmpty(value)) {
            value = this.selectedCountryName;
        }
        const validationRs = this.onValidateCountry(value);
        if (validationRs) {
            this.supplierListService.getLocations(this.selectedCountry.value).subscribe(data => {
                this.cityList = data;
            });
        }
    }

    onValidateCountry(value) {
        if (!StringHelper.isNullOrEmpty(value)) {
            this.selectedCountry = this.countryList.find(x => x.label.toLowerCase() === value.toLowerCase());
        }
        if (this.selectedCountry) {
            this.setValidControl('countryName');
            return true;
        } else {
            if (!StringHelper.isNullOrEmpty(value)) {
                this.setInvalidControl('countryName');
            } else {
                this.setInvalidControl('countryName', 'required');
            }
            return false;
        }
    }

    onCityFilterChange(value) {
        if (value.length >= 3) {
            this.cityAutoComplete.toggle(false);
            this.cityFilter = [];
            clearTimeout(this.acpTimeout);
            this.acpTimeout = setTimeout(() => {
                this.cityLoading = true;
                value = value.toLowerCase();
                let take = 10;
                for (let i = 0; i < this.cityList.length && take > 0; i++) {
                    if (this.cityList[i].label.toLowerCase().indexOf(value) !== -1) {
                        this.cityFilter.push(this.cityList[i]);
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

    onCityValueChange(value) {
        const selectedCity = this.cityList.find(x => x.label.toLowerCase() === value.toLowerCase());

        if (selectedCity) {
            this.model.locationId = selectedCity.value;
            this.setValidControl('cityName');
        } else {
            this.model.locationId = null;
            if (!StringHelper.isNullOrEmpty(value)) {
                this.setInvalidControl('cityName');
            }
        }
    }

    onFormClosed() {
        this.reset();
        this.close.emit();
    }

    onSelectClick() {
        this.validateAllFields(false);
        if (!this.isShowSelectButton || !this.currentForm.valid) {
            return;
        }

        this.addSupplierRelationship.emit(this.model);
        this.reset();
    }

    onSaveClick() {
        this.validateAllFields(false);
        
        if (!this.mainForm.valid) {
            return;
        }

        this.addSupplier.emit(this.model);
        this.reset();
    }

    onBtnResetClick() {
        this.selectedSupplierName = null;
        this.isShowSelectButton = false;
        this.isSelectedSupplierDuplicated = false;

        this.isAddNewSupplierMode = false;
        this.selectedCountryName = null;
        this.selectedCityName = null;
        this.model = {};
        this.isSupplierNameNullOrEmpty = true;
        this.selectedCountry = null;
        this.resetCurrentForm();
    }

    onBtnAddNewClick() {
        this.resetCurrentForm();
        this.isAddNewSupplierMode = true;
    }

    reset() {
        this.addSupplierFormOpened = false;

        this.selectedSupplierName = null;
        this.isSelectedSupplierDuplicated = false;
        this.isShowSelectButton = false;

        this.isAddNewSupplierMode = false;
        this.selectedCountry = null;
        this.selectedCountryName = null;
        this.selectedCityName = null;
        this.model = {};
    }

    setSelectedSupplier(supplier) {
        const supplierCustom = supplier.code.trim() + ' - ' + supplier.name.trim();
        // For show selected supplier form
        this.selectedSupplierName = supplierCustom;

        // For enable reset button
        this.isSupplierNameNullOrEmpty = false;

        this.onSupplierValueChange(supplierCustom);
    }

    onTypingContactNumber(value: string) {
        if (value?.length > MaxLengthValueInput.PhoneNumber) {
            this.setInvalidControl('contactNumber', 'maxLengthInput');
        }
    }
}
