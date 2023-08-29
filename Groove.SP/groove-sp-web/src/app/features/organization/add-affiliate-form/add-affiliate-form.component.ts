import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges, ViewChild } from '@angular/core';
import { AbstractControl, NgForm } from '@angular/forms';
import { AutoCompleteComponent } from '@progress/kendo-angular-dropdowns';
import { StringHelper } from 'src/app/core';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';
import { OrganizationFormService } from '../organization-form/organization-form.service';

@Component({
  selector: 'app-add-affiliate-form',
  templateUrl: './add-affiliate-form.component.html',
  styleUrls: ['./add-affiliate-form.component.scss']
})
export class AddAffiliateFormComponent implements OnInit {
  @ViewChild('mainForm', { static: false }) mainForm: NgForm;

  @Input() public formOpen: boolean = false;

  /**Current organization id which will be added affiliate to */
  @Input() public organizationId: number;

  @Input() public affiliateList: any[];

  @Output() onClose: EventEmitter<any> = new EventEmitter<any>();
  @Output() onSelect: EventEmitter<any> = new EventEmitter<any>();

  @ViewChild('mainForm', { static: false }) currentForm: NgForm;
  @ViewChild('affiliateAutoComplete', { static: false }) public affiliateAutoComplete: AutoCompleteComponent;

  defaultValue = DefaultValue2Hyphens;

  acpTimeout: any;
  affiliateLoading = false;

  selectedAffiliateName: string;
  selectedCountryName: string;
  selectedCityName: string;

  affiliateDataSource: any[]
  affiliateDataSourceFilter: any[];
  model: any;

  constructor(private service: OrganizationFormService) {
  }
  
  ngOnInit(): void {
    this.service.getAffiliateOrganizationDropdown(this.organizationId).subscribe((
      x: any[]) => this.affiliateDataSource = x);
  }

  onFormClosed() {
    this.reset();
    this.onClose.emit();
  }

  onAffiliateFilterChange(value) {
    if (value.length >= 3) {
      this.affiliateAutoComplete.toggle(false);
      this.affiliateDataSourceFilter = [];
      clearTimeout(this.acpTimeout);
      this.acpTimeout = setTimeout(() => {
        this.affiliateLoading = true;
        value = value.toLowerCase();
        let take = 10;
        for (let i = 0; i < this.affiliateDataSource.length && take > 0; i++) {
          if (this.affiliateDataSource[i].label.toLowerCase().indexOf(value) !== -1) {

            this.affiliateDataSourceFilter.push(this.affiliateDataSource[i]);
            take--;
          }
        }
        this.affiliateAutoComplete.toggle(true);
        this.affiliateLoading = false;
      }, 400);
    } else {
      this.affiliateLoading = false;
      if (this.acpTimeout) {
        clearTimeout(this.acpTimeout);
      }
      this.affiliateAutoComplete.toggle(false);
    }
  }

  onAffiliateValueChange(value) {
    this.model = {};
    const selectedAffiliate = this.affiliateDataSource.find(x => x.label.toLowerCase() === value.toLowerCase());

    if (selectedAffiliate) {
      this.service.getOrganization(selectedAffiliate.value).subscribe((organization: any) => {
        this.model = organization;
        this.model.address = this.concatenateAddressLines(this.model.address, this.model.addressLine2, this.model.addressLine3, this.model.addressLine4);
        if (organization.location) {
          this.selectedCountryName = organization.location.country.name;
          this.selectedCityName = organization.location.locationDescription;
          this.model.locationId = organization.location.id;
        }
      });
    }
  }

  public concatenateAddressLines(address: string, addressLine2?: string, addressLine3?: string, addressLine4?: string) : string {
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

  onSelectClick() {
    FormHelper.ValidateAllFields(this.mainForm);
    if (!this.mainForm.valid || !this.model?.id) {
      return;
    }

    this.onSelect.emit(this.model);
    this.reset();
  }

  public reset(): void {
    this.selectedAffiliateName = null;
    this.selectedCountryName = null;
    this.selectedCityName = null;
    this.model = {};
  }

  getControl(controlName: string): AbstractControl {
    return this.mainForm?.form?.controls[controlName];
  }
}