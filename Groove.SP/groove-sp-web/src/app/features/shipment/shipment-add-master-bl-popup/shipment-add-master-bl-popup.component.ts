import { Component, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChange, SimpleChanges, ViewChild } from '@angular/core';
import { AbstractControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { AutoCompleteComponent, ComboBoxComponent } from '@progress/kendo-angular-dropdowns';
import moment from 'moment';
import { EMPTY, of, Subject, Subscription } from 'rxjs';
import { delay, switchMap, tap } from 'rxjs/operators';
import { DateHelper, DropDownListItemModel, DropdownListModel, FormComponent, StringHelper, UserContextService } from 'src/app/core';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { DefaultValue2Hyphens, MasterBillOfLadingTypes, MovementTypes } from 'src/app/core/models/constants/app-constants';
import { ItineraryModel } from 'src/app/core/models/itinerary.model';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { MasterBillOfLadingFormService } from '../master-bill-of-lading/master-bill-of-lading-form/master-bill-of-lading-form.service';
import { MasterBillOfLadingListService } from '../master-bill-of-lading/master-bill-of-lading-list/master-bill-of-lading-list.service';
import { MasterBillOfLadingModel } from '../master-bill-of-lading/models/master-bill-of-lading-model';

@Component({
    selector: 'app-shipment-add-master-bl-popup',
    templateUrl: './shipment-add-master-bl-popup.component.html',
    styleUrls: ['./shipment-add-master-bl-popup.component.scss']
})
export class ShipmentAddMasterBLPopupComponent extends FormComponent implements OnInit, OnChanges, OnDestroy {

    /**
     * Metadata will be inputted into the component
     */
    @Input()
    inputMetaData: ShipmentAddMasterBLPopupComponentMetadata;

    /**
     * As a popup is closing
     */
    @Output()
    close: EventEmitter<number> = new EventEmitter<number>();


    /**
     * To define whether a popup should be opened
     */
    isFormOpened: boolean;

    /**
     * To define mode of transport of the popup: Sea or Air
     */
    modeOfTransport: string;

    /**
     * To define current working shipment id
     */
    shipmentId: number;

    /**
     * To define data source for execution agent drop-down list
     */
    executionAgentDataSource: Array<DropdownListModel<number>>;
    /**
     * To define execution agent
     */
    selectedExecutionAgentId: number;
    isDisabledExecutionAgent: boolean = false;

    /**
     * To produce data source for vessel/voyage drop down list
     */
    itineraryDataSource: Array<any>;

    // Data source for Carrier drop-down list
    carrierDataSource: Array<DropdownListModel<string>>;

    /**
     * Data source for location auto-complete
     */
    locations: Array<string> = [];
    locationDataSource: Array<string> = [];

    /**
     * Data source for carrier contract no
     */
    carrierContractNoDataSource: Array<DropDownListItemModel<string>> = [];
    carrierContractNoKeyUp$ = new Subject<string>();
    // Master BL number
    @ViewChild('carrierContractNoComboBox', { static: false })
    public carrierContractNoComboBox: ComboBoxComponent;
    isCarrierContractNoLoading: boolean = false;


    defaultValue = DefaultValue2Hyphens;

    // Master BL number
    @ViewChild('masterBLAutoComplete', { static: false })
    public masterBLAutoComplete: AutoCompleteComponent;

    selectedMasterBLNumber?: string;
    selectedMasterBL?: MasterBillOfLadingModel;
    masterBLKeyUp$ = new Subject<string>();
    masterBLDataSource: Array<MasterBillOfLadingModel> = [];
    isMasterBLLoading: boolean = false;

    isAddingNewMasterBL: boolean = false;
    model: MasterBillOfLadingModel;

    validationRules = {
        'executionAgentId': {
            'required': 'label.executionAgent'
        },
        'masterBillOfLadingNo': {
            'required': 'label.masterBLNo'
        },
        'placeOfIssue': {
            'required': 'label.placeOfIssue'
        },
        'issueDate': {
            'required': 'label.issueDates',
            'dateWithin90DaysToday': 'validation.dateWithin90DaysToday'
        },
        'onBoardDate': {
            'required': 'label.onBoardDates',
            'dateWithin90DaysToday': 'validation.dateWithin90DaysToday'
        },
        'carrierName': {
            'required': 'label.carrierName'
        },
        'carrierContractNo': {
            'required': 'label.contractNo'
        }
    };

    private _subscriptions: Array<Subscription> = [];

    get isDisabledMasterBL(): boolean {
        return this.isAddingNewMasterBL;
    }

    get isSelectedMasterBLNumberBlank(): boolean {
        return StringHelper.isNullOrEmpty(this.selectedMasterBLNumber);
    }

    get isSelectedMasterBLBlank(): boolean {
        return StringHelper.isNullOrEmpty(this.selectedMasterBL);
    }

    constructor(protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public formService: MasterBillOfLadingFormService,
        public listService: MasterBillOfLadingListService,
        public translateService: TranslateService,
        private _userContext: UserContextService) {
        super(route, formService, notification, translateService, router);
        this._registerEventHandlers();
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
            }
        });
    }
    ngOnDestroy(): void {
        this._subscriptions.map(x => x.unsubscribe());
    }

    ngOnInit() {
    }

    ngOnChanges(changes: SimpleChanges) {
        // to fetch data for attachment type options
        const model: SimpleChange = changes['inputMetaData'];
        const previousValue = model?.previousValue as ShipmentAddMasterBLPopupComponentMetadata;
        const newValue = model?.currentValue as ShipmentAddMasterBLPopupComponentMetadata;
        this.isFormOpened = newValue.isFormOpened;

        // the popup is opening, set values for some properties/variables
        if (newValue?.isFormOpened && !previousValue?.isFormOpened) {
            // Store into internal variables
            this.executionAgentDataSource = newValue.executionAgentDataSource?.map(x => new DropdownListModel<number>(x.executionAgent, x.executionAgentId)) || [];
            const itineraryDataSource = newValue.itineraryDataSource;
            this.itineraryDataSource = itineraryDataSource;
            this.shipmentId = newValue.shipmentId;
            this.modeOfTransport = newValue.modeOfTransport;

            // Set-up data source for drop-down lists
            if (itineraryDataSource) {
                // distinct by scac
                this.carrierDataSource = [];
                this.itineraryDataSource?.map(x => {
                        if (!this.carrierDataSource.some(y => y.value === x.scac)) {
                            this.carrierDataSource.push(new DropdownListModel<string>(x.carrierName, x.scac));
                        }
                    }
                );
            }

            // Set-up drop down list executionAgentDataSource
            if (this.executionAgentDataSource) {
                if (this.currentUser.isInternal) {
                    this.selectedExecutionAgentId = null;
                    this.isDisabledExecutionAgent = false;

                    // no option, then show message dialog and close the popup
                    if (StringHelper.isNullOrEmpty(this.executionAgentDataSource)) {
                        this.notification.showInfoDialog('msg.pleaseCreateConsignment', 'label.masterBOL')
                            .result.subscribe(x => {
                                this.onFormClosed();
                        });

                    } else if (this.executionAgentDataSource.length === 1) {
                        // auto select if there is only one option and disable control
                        this.selectedExecutionAgentId = this.executionAgentDataSource[0].value;
                        this.isDisabledExecutionAgent = true;
                    }
                } else {
                    // auto select a matched option and disable control
                    this.selectedExecutionAgentId = this.executionAgentDataSource.find(x => x.value === this.currentUser.organizationId)?.value;
                    this.isDisabledExecutionAgent = true;

                    // no option matcher current user's organization id, then show message dialog and close the popup
                    if (StringHelper.isNullOrEmpty(this.selectedExecutionAgentId)) {
                        this.notification.showInfoDialog('msg.pleaseCreateConsignment', 'label.masterBOL')
                            .result.subscribe(x => {
                                this.onFormClosed();
                        });
                    }
                }
            }

            const sub = this.formService.getAllLocations().subscribe(data => {
                this.locations = data?.map(x => x.locationDescription) || [];
                this.locationDataSource = this.locations;
            });
            this._subscriptions.push(sub);

            this._resetPopup();
            this._autoSelectOption();
        }

    }

    onExecutionAgentValueChange(value) {
        this.selectedExecutionAgentId = this.executionAgentDataSource.find(x => x.value === value)?.value;
        this.model.executionAgentId = value;
    }

    placeOfIssueFilterChange(value: string) {
        this.locationDataSource = [];
        if (value.length >= 3) {
            this.locationDataSource = this.locations.filter((s) => s.toLowerCase().indexOf(value.toLowerCase()) > -1);
        }
    }

    onCarrierValueChange(value) {
        if (StringHelper.isNullOrEmpty(value)) {
            this.model.carrierName = null;
            this.model.airlineCode = null;
            this.setInvalidControl('carrierName', 'required');

        } else {
            this.model.carrierName = this.itineraryDataSource.find(x => x.scac === value)?.carrierName;
            this.model.airlineCode = value;
            this.setValidControl('carrierName', 'required');

        }
    }

    onMasterBLValueChange(masterBLNumber: string) {
        this.selectedMasterBLNumber = masterBLNumber;
        this.selectedMasterBL = this.masterBLDataSource.find(x => x.masterBillOfLadingNo === masterBLNumber);
        this.model = { ...this.selectedMasterBL, executionAgentId : this.selectedExecutionAgentId };
    }

    issueDateValueChanged(value) {
        const currentDate = new Date().toISOString().split('T')[0];
        const dayDiff = moment(value).diff(moment(currentDate), 'days', true);
        if (dayDiff < -90 || 90 < dayDiff) {
            this.setInvalidControl('issueDate', 'dateWithin90DaysToday');
        } else {
            this.setValidControl('issueDate', 'dateWithin90DaysToday');
        }
    }

    onBoardDateValueChanged(value) {
        const currentDate = new Date().toISOString().split('T')[0];
        const dayDiff = moment(value).diff(moment(currentDate), 'days', true);
        if (dayDiff < -90 || 90 < dayDiff) {
            this.setInvalidControl('onBoardDate', 'dateWithin90DaysToday');
        } else {
            this.setValidControl('onBoardDate', 'dateWithin90DaysToday');
        }
    }

    onBtnResetClick() {
        this.selectedMasterBLNumber = null;
        this.isAddingNewMasterBL = false;
        this.selectedMasterBL = null;
        this.selectedMasterBLNumber = null;

        this._resetModel();
        
        this._autoSelectOption();    
    }

    onBtnAddNewClick() {
        this.isAddingNewMasterBL = true;
        this._resetModel();
        this._autoSelectOption();
    }

    onBtnSelectClick() {
        FormHelper.ValidateAllFields(this.mainForm);
        if (!(this.mainForm.valid && this.selectedMasterBL)) {
            return;
        }
        // Fire event to parent component with id of selected master bl
        this.close.emit(this.model.id);

    }

    onBtnSaveClick() {
        this.validateAllFields(false);
        const isValid = this.mainForm.valid;
        if (!isValid) {
            return;
        }
        // Adding new master bl
        let data = { ...this.model };
        data = DateHelper.formatDate(data);
        this.formService.createMasterBLFromShipment(this.shipmentId, data).subscribe(
            (response: MasterBillOfLadingModel) => {
                // Then fire event to parent component with id of the new master bl
                // Do not show notification here as redundant
                this.close.emit(response.id);
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.masterBOL');
            }
        );
    }

    onFormClosed() {
        this.close.emit();
    }

    /**
     * To handle events
     */
    private _registerEventHandlers(): void {
        let sub = this.masterBLKeyUp$.pipe(
            tap(() => {
                this.masterBLAutoComplete.toggle(false);
            }),
            switchMap((searchTerm: string) => {
                if (!StringHelper.isNullOrEmpty(searchTerm) && searchTerm.length >= 3) {
                    return of(searchTerm).pipe(delay(1000));
                } else {
                    return EMPTY;
                }
            }
        )).subscribe((searchTerm: string) => {
            this._masterBLFilterChange(searchTerm);
        });

        sub = this.carrierContractNoKeyUp$.pipe(
            tap(() => {
                this.carrierContractNoComboBox.toggle(false);
            }),
            switchMap((searchTerm: string) => {
                if (!StringHelper.isNullOrEmpty(searchTerm) && searchTerm.length >= 3) {
                    return of(searchTerm).pipe(delay(1000));
                } else {
                    return EMPTY;
                }
            }
        )).subscribe((searchTerm: string) => {
            this._carrierContractNoFilterChange(searchTerm);
        });
        this._subscriptions.push(sub);
    }

    private _masterBLFilterChange(searchTerm: string) {
        const isInternal = this.currentUser.isInternal;
        const affiliates = this.currentUser.affiliates;
        this.isMasterBLLoading = true;

        this.listService.searchMasterBLsByNumber(searchTerm, true, isInternal, affiliates).subscribe(
            (data) => {
                this.masterBLDataSource = data;
                this.masterBLAutoComplete.toggle(true);
                this.isMasterBLLoading = false;
            }
        );

    }

    private _carrierContractNoFilterChange(searchTerm: string) {

        this.isCarrierContractNoLoading = true;
        this.listService.getContractMasterOptions(searchTerm, this.model.scac, new Date()).subscribe(
            (data) => {
                this.carrierContractNoDataSource = data;
                this.carrierContractNoComboBox.toggle(true);
                this.isCarrierContractNoLoading = false;
            }
        );

    }

    private _resetPopup() {
        this.selectedMasterBLNumber = null;
        this.isAddingNewMasterBL = false;
        this.selectedMasterBL = null;
        this.selectedMasterBLNumber = null;
        this.isMasterBLLoading = false;

        this.formErrors = {};
        this._resetModel();
    }

    private _resetModel() {
        this.model = {
            executionAgentId: this.selectedExecutionAgentId,
            masterBillOfLadingNo: null,
            issueDate: null,
            onBoardDate: null,
            createdDate: (new Date()).toISOString(),
            createdBy: this.currentUser.username,
            movement: MovementTypes.CYSlashCY,
            masterBillOfLadingType: MasterBillOfLadingTypes.OceanBillOfLading,
            isDirectMaster: true,
            modeOfTransport: this.modeOfTransport
        };
    }

    private _autoSelectOption() {
        
        // Auto select if there is only one option on carrier drop-down list
        if (this.carrierDataSource?.length === 1) {
            this.model.scac = this.carrierDataSource[0].value;
            this.model.airlineCode = this.carrierDataSource[0].value;
            this.model.carrierName = this.carrierDataSource[0].label;
        }
    }

    getControl(controlName: string): AbstractControl {
        return this.mainForm?.form?.controls[controlName];
    }

}

/**
 * To define metadata to input component ShipmentAddMasterBLPopupComponent
 */
export interface ShipmentAddMasterBLPopupComponentMetadata {
    /**
     * To define whether a popup should be opened
     */
    isFormOpened: boolean;
    /**
     * Data source of itineraries, to load data for drop-down list vessel/voyage
     */
    itineraryDataSource: Array<any>;
    /**
     * To define current working house bill of lading
     */
    shipmentId: number;
    /**
     * To define mode of transport of the popup: Sea or Air
     */
    modeOfTransport: string;
    /**
     * Data source of execution agent, to load data for drop-down list execution
     */
    executionAgentDataSource: Array<any>;


}
