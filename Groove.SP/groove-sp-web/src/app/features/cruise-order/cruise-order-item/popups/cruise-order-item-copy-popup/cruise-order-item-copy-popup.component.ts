import { Component, Input, Output, EventEmitter, ViewChildren, QueryList, OnDestroy, AfterViewInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { AutoCompleteComponent } from '@progress/kendo-angular-dropdowns';
import { Subscription } from 'rxjs';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { CommonService } from 'src/app/core/services/common.service';
import { CruiseOrderItemModel } from '../../../models/cruise-order-item.model';

@Component({
    selector: 'app-cruise-order-item-copy-popup',
    templateUrl: './cruise-order-item-copy-popup.component.html',
    styleUrls: ['./cruise-order-item-copy-popup.component.scss']
})
export class CruiseOrderItemCopyPopupComponent implements OnDestroy, AfterViewInit {
    @ViewChild('mainForm', { static: false }) mainForm: NgForm;
    
    @Input() itemCopyPopupOpened: boolean = false;
    @Output() close: EventEmitter<CruiseOrderItemModel> = new EventEmitter();
    @Input() model: CruiseOrderItemModel;

    @Input() itemOptions: Array<string>;

    // Store all subscriptions, then should un-subscribe at the end
    private _subscriptions: Array<Subscription> = [];

    // data sources for elements
    allLocationOptions: Array<any> = [];
    filteredPortOptions: Array<any> = [];

    allCountryOptions: Array<any> = [];
    destinationCountryOptions: Array<any> = [];

    @ViewChildren(AutoCompleteComponent) croAutoCompletes!: QueryList<AutoCompleteComponent>;

    defaultDropDownListOption: { label: string, description: string, value: string } =
    {
        label: 'label.select',
        description: 'select',
        value: null
    };

    constructor( private _commonService: CommonService ) {
        this._fetchDataSources();
    }

    ngAfterViewInit(): void {
        // Inject the kendo-control attributes into its kendo-input
        this.croAutoCompletes.forEach((child: any) => {
            const { searchbar, wrapper } = child;
            searchbar.input.nativeElement.setAttribute('maxlength', wrapper.attributes.maxlength.value);
        });
    }

    private _fetchDataSources(): void {
        let sub = this._commonService.getAllLocations().subscribe(data => {
            this.allLocationOptions = data;
            this.filteredPortOptions = this.allLocationOptions;
        });
        this._subscriptions.push(sub);

        sub = this._commonService.getCountries().subscribe(data => {
            this.allCountryOptions = data;
            this.destinationCountryOptions = this.allCountryOptions;
        });
        this._subscriptions.push(sub);       
    }

    portFilterChange(value) {
        this.filteredPortOptions = [];
        if (value.length >= 3) {
          this.filteredPortOptions = this.allLocationOptions.filter((s) => s.locationDescription.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        }
    }

    destinationValueChange(value) {
        const matchedDestinations = this.allLocationOptions.filter(s => s.locationDescription.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        if (matchedDestinations && matchedDestinations.length === 1) {
            const countryId = matchedDestinations[0].countryId;
            const matchedCountries = this.allCountryOptions.filter((s) => s.id === countryId);
            if (matchedCountries && matchedCountries.length === 1) {
                this.model.destinationCountry = matchedCountries[0].name;
            }
        }
    }

    destinationCountryFilterChange(value) {
        this.destinationCountryOptions = [];
        if (value.length >= 3) {
          this.destinationCountryOptions = this.allCountryOptions.filter((s) => s.name.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        }
    }

    onFormClosed(isSaving?: boolean) {
        if (isSaving) {
            FormHelper.ValidateAllFields(this.mainForm);
            if (!this.mainForm.valid) {
                return;
            }
        }

        this.itemCopyPopupOpened = false;
        if (isSaving) {
            this.close.emit(this.model);
        } else {
            this.close.emit();
        }
    }

    get title() {
        return 'label.copyItem';
    }

    ngOnDestroy(): void {
        this._subscriptions.map(x => x.unsubscribe());
    }

}
