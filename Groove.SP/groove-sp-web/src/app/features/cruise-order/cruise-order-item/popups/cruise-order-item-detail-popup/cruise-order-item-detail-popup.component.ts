import { Component, Input, Output, EventEmitter, OnInit, ViewChildren, QueryList, OnDestroy, AfterViewInit, OnChanges, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { AutoCompleteComponent } from '@progress/kendo-angular-dropdowns';
import { Subject, Subscription } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { tap } from 'rxjs/operators';
import { DropdownListModel, DropDownListItemModel, StringHelper } from 'src/app/core';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { CommonService } from 'src/app/core/services/common.service';
import { CruiseOrderItemModel, ReviseCruiseOrderItemModel } from '../../../models/cruise-order-item.model';
import { CruiseOrderItemService } from '../../cruise-order-item.service';

@Component({
    selector: 'app-cruise-order-item-detail-popup',
    templateUrl: './cruise-order-item-detail-popup.component.html',
    styleUrls: ['./cruise-order-item-detail-popup.component.scss']
})
export class CruiseOrderItemDetailPopupComponent implements OnInit, OnDestroy, AfterViewInit, OnChanges {
    @ViewChild('mainForm', { static: false }) mainForm: NgForm;
    
    @Input() itemDetailPopupOpened: boolean = false;
    @Output() close: EventEmitter<ReviseCruiseOrderItemModel> = new EventEmitter();
    @Input() data: CruiseOrderItemModel;
    model: ReviseCruiseOrderItemModel;

    @Input() itemOptions: Array<string>;


    // Store all subscriptions, then should un-subscribe at the end
    private _subscriptions: Array<Subscription> = [];

    // data sources for elements
    allLocationOptions: Array<any> = [];
    filteredPortOptions: Array<any> = [];

    allCountryOptions: Array<any> = [];
    destinationCountryOptions: Array<any> = [];

    currencyOptions: Array<DropdownListModel<string>> = [];
    priorityOptions: Array<DropdownListModel<string>> = [
        { label : 'A', value: 'A'},
        { label : 'B', value: 'B'},
        { label : 'C', value: 'C'}
    ];

    /** Data-source for cruise order item auto-complete */
    itemOptionsSource: Array<string>;

    /** Data-source for cruise order item auto-complete after filtered */
    filteredItemOptionsSource: Array<string>;

    /** Data-source for shipment auto-complete  */
    shipmentOptionsSource: Array<DropDownListItemModel<number>> = [];
    selectedShipment: DropDownListItemModel<number>;
    shipmentSearchTermKeyUp$ = new Subject<string>();
    isShipmentSearching: boolean = false;

    @ViewChildren(AutoCompleteComponent) croAutoCompletes!: QueryList<AutoCompleteComponent>;

    defaultDropDownListOption: { label: string, description: string, value: string } =
    {
        label: 'label.select',
        description: 'select',
        value: null
    };

    constructor( private _commonService: CommonService,
                private _cruiseOrderItemService: CruiseOrderItemService) {
        this._fetchDataSources();
    }

    ngOnChanges(): void {
        // Have to map cruise order model to cruise order revise model as current popup need
        if (this.data) {
            this.model = ReviseCruiseOrderItemModel.mapToReviseModel(this.data);

            // initialize data source for selected shipment auto-complete
            this._initializeShipmentDataSource();
            this._initializeItemOptionDataSource();
        }
    }

    ngOnInit() {
        this._registerEventHandlers();
        this._fetchDataSources();
    }

    ngAfterViewInit(): void {
        // Inject the kendo-control attributes into its kendo-input
        this.croAutoCompletes.forEach((child: any) => {
            const { searchbar, wrapper } = child;
            searchbar.input.nativeElement.setAttribute('maxlength', wrapper.attributes.maxlength.value);
        });
    }

    private _initializeShipmentDataSource(): void {
        this.shipmentOptionsSource = [];
        this.selectedShipment = null;
        // Add option for current select shipment
        if (this.data?.shipment) {
            const option = {
                text: this.data.shipment.shipmentNo,
                value: this.data.shipment.id
            };
            this.shipmentOptionsSource.push(
                option
            );
            this.selectedShipment = option;
        }

    }

    /**Initialize data-source for cruise order Item auto-complete
    // clone object */
    private _initializeItemOptionDataSource() {
        this.itemOptionsSource = Object.assign([], this.itemOptions);
        this.filteredItemOptionsSource = this.itemOptionsSource;
    }

    private _registerEventHandlers(): void {
        const sub = this.shipmentSearchTermKeyUp$.pipe(
            debounceTime(1000),
            tap((searchTerm: string) => {
                if (StringHelper.isNullOrEmpty(searchTerm) || searchTerm.length === 0 || searchTerm.length >= 3) {
                    this._shipmentFilterChange(searchTerm);
                }
            }
        )).subscribe();
        this._subscriptions.push(sub);
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

        // Currency options
        sub = this._commonService.getCurrencies()
            .subscribe(
                (data) => {
                    this.currencyOptions = data.map(item => {
                        return new DropdownListModel(item.code, item.code);
                    });
                }
            );
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

        this.itemDetailPopupOpened = false;
        if (isSaving) {
            // need to update some additional information
            this.model.shipmentNumber = this.selectedShipment?.text;
            this.model.selectedItemPOLines = this.model.selectedItems.map(x => Number.parseInt(x.split(' - ')[0]));
            this.close.emit(this.model);
        } else {
            this.close.emit();
        }
    }

    /** Handler for PO Item auto-complete filter changed */
    poItemsFilterChange(value: string) {
        if (value.length >= 1) {
            this.filteredItemOptionsSource = this.itemOptionsSource.filter((s) =>
                    s.toLowerCase().indexOf(value.toLowerCase()) !== -1
                    || this.model.selectedItems.map(x => x).indexOf(s) !== -1);
        } else {
            this.filteredItemOptionsSource = this.itemOptionsSource;
        }
    }

    /** Handler for Shipment combo box filter changed */
    private _shipmentFilterChange(value: string) {

        // Only call to server after user input >= 3 characters
        if (value.length >= 3) {
            this.isShipmentSearching = true;
            const sub = this._cruiseOrderItemService.searchShipmentSelectionOptions(value, this.model.orderId)
                .subscribe(
                (data) => {
                    this.shipmentOptionsSource = data;
                },
                (error) => {

                },
                () => {
                    this.isShipmentSearching = false;
                }
            );
            this._subscriptions.push(sub);
        }
    }

     /** Handler for Shipment combo box value changed */
    shipmentValueChange(selectedItem?: number) {
        this.selectedShipment =  this.shipmentOptionsSource.find(x => x.value === selectedItem);
    }

    /** Check whether current cruise order Item option is selected */
    isItemSelected(value) {
        return this.model.selectedItems.some(item => item === value);
    }

    get title() {
        return 'label.editItem';
    }

    ngOnDestroy(): void {
        this._subscriptions.map(x => x.unsubscribe());
    }

}
