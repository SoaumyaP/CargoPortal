import { Component, Input, Output, EventEmitter, OnChanges, ViewChild } from '@angular/core';
import { DropDowns, FormComponent, StringHelper, DATE_FORMAT, ModeOfTransportType, DateHelper } from 'src/app/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import moment from 'moment';
import { ConsignmentItineraryFormService } from './consignment-itinerary-form.service';
import { QueryModel } from 'src/app/core/models/forms/query.model';

/**
 * It belongs to Shipment module
 */
@Component({
    selector: 'app-consignment-itinerary-form',
    templateUrl: './consignment-itinerary-form.component.html',
    styleUrls: ['./consignment-itinerary-form.component.scss']
})
export class ConsignmentItineraryFormComponent extends FormComponent implements OnChanges {
    @Input() public formDialogOpened: boolean = false;
    @Input() public model: any;
    @Input() public shipmentNo: string;
    @Input() public consignments: any[];
    @Input() public itineraries: any[];
    @Input() public shipment: any;
    @Input()
    set itineraryFormMode(mode: string) {
        this.isViewModeLocal = mode === this.formMode.view;
        this.isEditModeLocal = mode === this.formMode.edit;
        this.isAddModeLocal = mode === this.formMode.add;
    }

    public isViewModeLocal: boolean;
    public isEditModeLocal: boolean;
    public isAddModeLocal: boolean;
    public isScheduleGridLoading: boolean = false;

    @Output() close: EventEmitter<any> = new EventEmitter<any>();
    @Output() add: EventEmitter<any> = new EventEmitter<any>();
    @Output() edit: EventEmitter<any> = new EventEmitter<any>();

    @ViewChild('loadingPort', {static: false}) locationFromNameInput: any;
    @ViewChild('dischargePort', {static: false}) locationToNameInput: any;
    @ViewChild('carrierName', {static: false}) carrierNameInput: any;

    modeOfTransportType = ModeOfTransportType;
    DATE_FORMAT = DATE_FORMAT;
    modeOfTransportOptions = DropDowns.ModeOfTransportStringTypeForItinerary;
    allCarrierOptions: any[];
    filteredCarrierOptions: any[];
    allPortOptions: any[];
    filteredPortOptions: any[];
    filteredPortSuggestions: any[];
    filteredCarrierSuggestions: any[];
    schedules: any[];
    selectedSchedule = [];
    selectableScheduleSettings = {
        enabled: true,
        checkboxOnly: true,
        mode: 'single'
    };

    readonly stringHelper = StringHelper;

    public value: Date = new Date(2019, 5, 1, 22);
    public format = 'MM/dd/yyyy HH:mm';

    validationRules = {
        sequence: {
            required: 'label.sequence',
            duplicated: 'validation.duplicateSequence'
        },
        modeOfTransport: {
            required: 'label.modeOfTransport'
        },
        carrierName: {
            required: 'label.carrierName'
        },
        carrierSCAC: {
            required: 'label.scacCode'
        },
        loadingPort: {
            required: 'label.loadingPort'
        },
        etdDates: {
            required: 'label.etdDates'
        },
        dischargePort: {
            required: 'label.dischargePort'
        },
        etaDates: {
            required: 'label.etaDates',
            laterThanOrEqualTo: 'label.etdDates'
        },
        vessel: {
            required: 'label.vessel'
        },
        voyage: {
            required: 'label.voyage'
        },
        roadFreightRef: {
            required: 'label.roadFreightRef'
        }
    };

    // default values are for Sea
    literalLabels = {
        'carrier': 'label.carrier',
        'carrierName': 'label.carrierName',
        'locationFromName': 'label.loadingPort',
        'locationToName': 'label.dischargePort',
        'departureDate': 'label.departureDates',
        'vesselFlight': 'label.vesselSlashVoyage'
    };

    constructor(protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: ConsignmentItineraryFormService,
        public translateService: TranslateService) {
        super(route, service, notification, translateService, router);
        this.service.getAllCarriers().subscribe(data => {
            this.allCarrierOptions = data;
        });

        // We use Locations for Port.
        this.service.getAllLocations().subscribe(data => {
            this.allPortOptions = data;
        });
    }

    ngOnChanges(value: any): void {
        if (this.isAddModeLocal) {
            if (value.formDialogOpened?.currentValue) {
                this.model.consignmentId = this.consignments[0].id;
                this.model.modeOfTransport = this.shipment?.modeOfTransport;
            }
            // set sequence by default
            this.setSequenceByDefault();
            // Set default values for loadingPort, dischargePort
            this.setPortByDefault();
            // Set default values for departure
            this.setDepartureByDefault();
            // get schedule by default
            this.getSchedules();
        }
        if (this.isEditModeLocal) {
            this.model.departureDates = this.model.etdDate;
            this.getSchedules();
        }

        this.updateLiteralLabels(this.model.modeOfTransport);
        this.filteredCarrierOptions = this.allCarrierOptions?.filter(c =>
            c.modeOfTransport.toLowerCase() === this.model.modeOfTransport?.toLowerCase());
        this.filteredPortOptions = this.allPortOptions;
    }

    /** To set the default sequence by selected consignment. */
    setSequenceByDefault() {
        if (this.isEditModeLocal) {
            return;
        }
        const itineraries = this.itinerariesByConsignment;
        if (itineraries?.length > 0) {
            const lastItinerary = itineraries[itineraries.length - 1];
            this.model.sequence = lastItinerary.sequence + 1;
        } else {
            this.model.sequence = 1;
        }
    }

    /** To set the default loading port & discharge port by selected consignment */
    setPortByDefault() {
        if (this.isEditModeLocal) {
            return;
        }
        const itineraries = this.itinerariesByConsignment;
        const shipFromPort = this.allPortOptions.find(x => x.locationDescription.toUpperCase() === this.shipment?.shipFrom?.toUpperCase());
        const shipToPort = this.allPortOptions.find(x => x.locationDescription.toUpperCase() === this.shipment?.shipTo?.toUpperCase());
        if (itineraries?.length > 0) {
            const lastItinerary = itineraries[itineraries.length - 1];
            this.model.loadingPort = lastItinerary.dischargePort;
            this.model.dischargePort = shipToPort?.locationDescription;
        } else {
            this.model.loadingPort = shipFromPort?.locationDescription;
            this.model.dischargePort = shipToPort?.locationDescription;
        }
    }

    /** To set default departure date. It only works for sea/air mode. */
    setDepartureByDefault() {
        if (this.isEditModeLocal || !this.isSeaOrAir) {
            return;
        }
        const itineraries = this.itinerariesByConsignment;
        if (itineraries?.length > 0) {
            const lastItinerary = itineraries[itineraries.length - 1];
            const etaDate = new Date(lastItinerary.etaDate);
            this.model.departureDates = new Date(etaDate.setDate(etaDate.getDate() + 1));
        } else {
            this.model.departureDates = new Date(this.shipment?.shipFromETDDate);
        }
    }

    private get itinerariesByConsignment() {
        return this.itineraries?.filter(
            i => i.consignmentId === this.model.consignmentId
        );
    }

    onSequenceValueChanged() {
        const sequences = this.itinerariesByConsignment?.filter(x => x.id !== this.model.id)
            .map(x => x.sequence);
        if (sequences.includes(this.model.sequence)) {
            this.setInvalidControl('sequence', 'duplicated');
        }
        else {
            if (this.model.sequence && this.model.sequence.length > 0) {
                this.setValidControl('sequence');
            }
        }
    }

    onConsignmentValueChanged(event) {
        this.setSequenceByDefault();
        this.setPortByDefault();
        this.setDepartureByDefault();
        this.getSchedules();
    }

    onModeOfTransportChanged(mode) {
        this.model.vesselName = '';
        this.model.voyage = '';
        this.model.vesselFlight = '';
        this.model.roadFreightRef = '';
        this.model.carrierName = null;
        this.model.scac = '';
        if (this.formErrors['etaDates']) {
            this.model.etaDate = null;
        }

        if ([this.modeOfTransportType.Sea.toLowerCase(), this.modeOfTransportType.Air.toLowerCase()].includes(mode.toLowerCase())) {
            const selectedLoadingPortIndex = this.allPortOptions.findIndex(
                (s) => s.locationDescription.toLowerCase() === this.model.loadingPort?.toLowerCase());
            if (selectedLoadingPortIndex < 0) {
                this.model.loadingPort = null;
            }
            const selectedDischargePortIndex = this.allPortOptions.findIndex(
                (s) => s.locationDescription.toLowerCase() === this.model.dischargePort?.toLowerCase());
            if (selectedDischargePortIndex < 0) {
                this.model.dischargePort = null;
            }
        }

        this.locationFromNameInput.control.touched = false;
        this.locationToNameInput.control.touched = false;
        if (this.carrierNameInput) {
            this.carrierNameInput.control.touched = false;
        }

        this.filteredCarrierOptions = this.allCarrierOptions.filter(c =>
            c.modeOfTransport?.toLowerCase() === this.model.modeOfTransport?.toLowerCase());
        this.filteredPortOptions = this.allPortOptions;
        this.updateLiteralLabels(this.model.modeOfTransport);

        // need to reset validation status of Sequence control
        const sequenceControl =  this.mainForm.controls['sequence'];

        // only do if Sequence is null/empty value
        if (StringHelper.isNullOrEmpty(this.model.sequence) || this.model.sequence.length === 0) {
            sequenceControl.markAsPristine();
            sequenceControl.markAsUntouched();
        }

        // need to reset validation status of carrierSCAC control
        const carrierSCACCodeControl =  this.mainForm.controls['carrierSCAC'];
        // only do if carrierSCAC is null/empty value
        if (carrierSCACCodeControl && (StringHelper.isNullOrEmpty(this.model.scac) || this.model.scac.length === 0)) {
            carrierSCACCodeControl.markAsPristine();
            carrierSCACCodeControl.markAsUntouched();
        }

        // need to reset validation status of carrierName control
        const carrierNameCodeControl =  this.mainForm.controls['carrierName'];
        // only do if carrierName is null/empty value
        if (carrierNameCodeControl && (StringHelper.isNullOrEmpty(this.model.carrierName) || this.model.carrierName.length === 0)) {
            carrierNameCodeControl.markAsPristine();
            carrierNameCodeControl.markAsUntouched();
        }

        // need to reset validation status of roadFreightRef control
        const roadFreightRefCodeControl =  this.mainForm.controls['roadFreightRef'];
        // only do if roadFreightRef is null/empty value
        if (roadFreightRefCodeControl && (StringHelper.isNullOrEmpty(this.model.roadFreightRef) || this.model.roadFreightRef.length === 0)) {
            roadFreightRefCodeControl.markAsPristine();
            roadFreightRefCodeControl.markAsUntouched();
        }
        this.validateAllFields(true);
        this.getSchedules();
    }

    updateLiteralLabels(mode) {
        if (!mode) {
            return;
        }
        switch (mode.toLowerCase()) {
            case ModeOfTransportType.Sea.toLowerCase():
                this.literalLabels.carrier = 'label.carrier';
                this.literalLabels.carrierName = 'label.carrierName';
                this.literalLabels.locationFromName = 'label.loadingPort';
                this.literalLabels.locationToName = 'label.dischargePort';
                this.literalLabels.departureDate = 'label.departureDates';
                this.literalLabels.vesselFlight = 'label.vesselSlashVoyage';
                break;
            case ModeOfTransportType.Air.toLowerCase():
                this.literalLabels.carrier = 'label.airline';
                this.literalLabels.carrierName = 'label.airlineName';
                this.literalLabels.locationFromName = 'label.origin';
                this.literalLabels.locationToName = 'label.destination';
                this.literalLabels.departureDate = 'label.flightDates';
                this.literalLabels.vesselFlight = 'label.flightNo';
                break;
            default:
                this.literalLabels.carrier = 'label.carrier';
                this.literalLabels.carrierName = 'label.carrierName';
                this.literalLabels.locationFromName = 'label.shipFrom';
                this.literalLabels.locationToName = 'label.shipTo';
                this.literalLabels.departureDate = 'label.departureDates';
                this.literalLabels.vesselFlight = 'label.vesselSlashVoyage';
                break;
        }
    }

    onLoadingPortValueChange(val) {
        this.getSchedules();
    }

    onDischargePortValueChange(val) {
        this.getSchedules();
    }

    onCarrierValueChanged(val) {
        if (!val || val.length === 0) {
            return;
        }
        if (this.model.modeOfTransport?.toLowerCase() === ModeOfTransportType.Air.toLowerCase()) {
            const selectedCarrier = this.allCarrierOptions.find(x => x.name.toUpperCase() === val.toUpperCase());
            if (selectedCarrier) {
                this.model.scac = selectedCarrier.carrierCode;
            }
        }
    }

    onPortOptionsOpen() {
        this.filteredPortOptions = this.allPortOptions;
    }

    onCarrierOptionsOpen() {
        this.filteredCarrierOptions = this.allCarrierOptions.filter(c =>
            c.modeOfTransport?.toLowerCase() === this.model.modeOfTransport?.toLowerCase());
    }

    onChangeDateOrDateTime() {
        if (this.model.etaDate && this.model.etdDate && this.model.etaDate < this.model.etdDate) {
            this.setInvalidControl('etaDates', 'laterThanOrEqualTo');
        } else {
            if (this.model.etaDate) {
                this.setValidControl('etaDates');
            }
        }
    }

    /** To get a list of Freight Schedulers.
     *  It only works for sea or air mode. */
    getSchedules() {
        this.schedules = [];
        this.selectedSchedule = [];
        const { modeOfTransport, loadingPort, dischargePort, departureDates } = this.model;
        if (!this.isSeaOrAir) {
            return;
        }
        if (!DateHelper.isValidDate(departureDates) || this.formErrors['loadingPort'] || this.formErrors['dischargePort']) {
            return;
        }
        
        const filterSetObj = new ScheduleFilterSet(
            modeOfTransport,
            loadingPort,
            dischargePort,
            departureDates
        );

        // If Contract No. is inputted and it belongs to Sea Carrier, Sea Itinerary will filter Freight Schedule of the same Carrier with Contract No.
        const selectedCMCarrierCode = this.shipment?.contractMaster?.carrierCode;
        const selectedModeOfTransport = this.allCarrierOptions.find(x => StringHelper.caseIgnoredCompare(x.carrierCode, selectedCMCarrierCode))?.modeOfTransport;
        if (StringHelper.caseIgnoredCompare(selectedModeOfTransport, ModeOfTransportType.Sea)) {
            filterSetObj.CarrierCode = selectedCMCarrierCode;
        }

        this.isScheduleGridLoading = true;
        this.service.getSchedules(filterSetObj.buildToQueryParams).subscribe(
            schedules => {
                this.schedules = schedules;
                if (this.isEditModeLocal && this.isSeaOrAir) {
                    const selectedScheduleIndex = this.schedules.findIndex(x => x.id === this.model?.scheduleId);
                    if (selectedScheduleIndex >= 0) {
                        this.selectedSchedule.push(selectedScheduleIndex);
                    }
                }
                this.isScheduleGridLoading = false;
            }
        );
    }

    carrierFilterChange(value: string) {
        this.filteredCarrierOptions = [];
        if (value.length > 0) {
            this.filteredCarrierSuggestions = this.filteredCarrierOptions = this.allCarrierOptions.filter((c) =>
                c.modeOfTransport?.toLowerCase() === this.model.modeOfTransport?.toLowerCase()
                && c.name.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        } else {
            this.filteredCarrierSuggestions = this.filteredCarrierOptions = this.allCarrierOptions.filter((c) =>
                c.modeOfTransport?.toLowerCase() === this.model.modeOfTransport?.toLowerCase());
        }
    }

    portFilterChange(value) {
        this.filteredPortOptions = [];
        this.filteredPortSuggestions = [];
        if (value.length > 0) {
            this.filteredPortSuggestions = this.filteredPortOptions = this.allPortOptions.filter((s) => s.locationDescription.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        }
    }

    onFormClosed() {
        this.formDialogOpened = false;
        this.close.emit();
        this.resetSchedule();
    }

    resetSchedule() {
        this.selectedSchedule = [];
        this.model.departureDates = null;
        this.schedules = null;
    }

    onAddClick() {
        this.validateAllFields(false);
        if (this.isDisableFormButton) {
            return;
        }

        if (!this.mainForm.valid) {
            return;
        }

        // Set a flag to mark the itinerary has been created from UI
        this.model.isCalledFromApp = true;

        // Bind selected schedule into Itinerary
        if (this.isSeaOrAir) {
            const selectedSchedule = this.schedules[this.selectedSchedule[0]];
            this.model.scheduleId = selectedSchedule?.id;
        } else {
            this.model.scheduleId = null;
        }
        this.add.emit(this.model);
        this.resetSchedule();
    }

    onEditClick() {
        this.validateAllFields(false);
        if (this.isDisableFormButton) {
            return;
        }
        if (!this.mainForm.valid) {
            return;
        }

        // Set a flag to mark the itinerary has been updated from UI
        this.model.isCalledFromApp = true;

        // Bind selected schedule into Itinerary
        if (this.isSeaOrAir) {
            const selectedSchedule = this.schedules[this.selectedSchedule[0]];
            this.model.carrierName = selectedSchedule.carrierName;
            this.model.scac = selectedSchedule.carrierCode;
            this.model.scheduleId = selectedSchedule?.id;
            this.model.vesselName = selectedSchedule.vesselName;
            this.model.vesselFlight = `${selectedSchedule.vesselName ?? ''}/${selectedSchedule.voyage}`;
            this.model.etdDate = selectedSchedule.etdDate;
            this.model.etaDate = selectedSchedule.etaDate;
        } else {
            this.model.scheduleId = null;
        }
        this.edit.emit(this.model);
        this.resetSchedule();
    }

    onVesselChange() {
        this.generateVesselFlight();
    }

    onVoyageChange() {
        this.generateVesselFlight();
    }

    onRoadFreightRefChange() {
        this.generateVesselFlight();
    }

    onETDChange() {
        this.generateVesselFlight();
    }

    private generateVesselFlight() {
        this.model.vesselFlight = '';

        if (this.model.modeOfTransport.toLowerCase() === ModeOfTransportType.Sea.toLowerCase()) {
            if (StringHelper.isNullOrWhiteSpace(this.model.vesselName)) {
                return;
            }

            if (StringHelper.isNullOrWhiteSpace(this.model.voyage)) {
                return;
            }

            this.model.vesselFlight = this.model.vesselName + '/' + this.model.voyage;
        }

        if (this.model.modeOfTransport.toLowerCase() === ModeOfTransportType.Air.toLowerCase()) {
            if (StringHelper.isNullOrWhiteSpace(this.model.flightNo)) {
                return;
            }

            this.model.vesselFlight = this.model.flightNo;
        }

        if (this.model.modeOfTransport.toLowerCase() === ModeOfTransportType.Road.toLowerCase()
            || this.model.modeOfTransport.toLowerCase() === ModeOfTransportType.Railway.toLowerCase()
            || this.model.modeOfTransport.toLowerCase() === ModeOfTransportType.Courier.toLowerCase()) {
            if (StringHelper.isNullOrWhiteSpace(this.model.roadFreightRef)) {
                return;
            }

            this.model.vesselFlight = this.model.roadFreightRef;
        }
    }

    get title() {
        if (this.isViewModeLocal) {
            return 'label.itineraryDetails';
        }

        return this.isAddModeLocal ? 'label.addItinerary' : 'label.editItinerary';
    }

    get isDisableFormButton() {
        this.validateAllFields(false);
        if (!this.mainForm?.valid) {
            return true;
        }
        if (this.isSeaOrAir) {
            return this.selectedSchedule.length === 0;
        }
        return false;
    }

    get isSeaOrAir() {
        return StringHelper.caseIgnoredCompare(this.model?.modeOfTransport, ModeOfTransportType.Air) ||
            StringHelper.caseIgnoredCompare(this.model?.modeOfTransport, ModeOfTransportType.Sea);
    }
}

export class ScheduleFilterSet extends QueryModel {
    private ModeOfTransport: string;
    private LoadingPort: string;
    private DischargePort: string;
    private Departure: string;
    public CarrierCode?: string;


    /**
     *
     */
    constructor(modeOfTransport: string = ModeOfTransportType.Sea.toLowerCase(),
        loadingPort: string,
        dischargePort: string,
        departure: Date) {
        super();
        this.ModeOfTransport = modeOfTransport;
        this.LoadingPort = loadingPort;
        this.DischargePort = dischargePort;
        this.Departure = this.convertToQueryDateString(departure);
    }
}
