import { Component, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import moment from 'moment';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { DATE_HOUR_FORMAT, DATE_HOUR_PLACEHOLDER, DropDowns, FormMode, ModeOfTransportType, StringHelper } from 'src/app/core';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { CarrierModel, CarrierSelectionModel } from 'src/app/core/models/carrier.model';
import { GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { LocationModel } from 'src/app/core/models/location.model';
import { CommonService } from 'src/app/core/services/common.service';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { FreightSchedulerService } from '../freight-scheduler.service';
import { FreightSchedulerModel } from '../models/freight-scheduler.model';
@Component({
    selector: 'app-freight-scheduler-form',
    templateUrl: './freight-scheduler-form.component.html',
    styleUrls: ['./freight-scheduler-form.component.scss'],
})
export class FreightSchedulerFormComponent
    implements OnInit, OnChanges, OnDestroy {
    @Input() carriersModel: Array<CarrierModel> = [];
    @Input() locationsModel: Array<LocationModel> = [];
    @Input() popupMode: FormMode = FormMode.View;
    @Input() model: FreightSchedulerModel;
    @ViewChild('mainForm', { static: false }) mainForm: NgForm;

    @Output() close: EventEmitter<any> = new EventEmitter<any>();
    @Output() saveSuccess: EventEmitter<any> = new EventEmitter<any>();

    carriersDataSource: Array<CarrierModel> = [];
    carriersFiltered: Array<CarrierSelectionModel> = [];
    carrierSelected: CarrierModel;
    vesselsFiltered: Array<any> = [];
    locationFromFiltered: Array<LocationModel> = [];
    locationToFiltered: Array<LocationModel> = [];

    modeOfTransportOptions = DropDowns.ModeOfTransportStringType;
    dateHourFormat = DATE_HOUR_FORMAT;
    dateHourPlaceholder = DATE_HOUR_PLACEHOLDER;
    modeOfTransportType = ModeOfTransportType;

    // default values are for Sea
    literalLabels = {
        'carrierName': 'label.carrier',
        'locationFromName': 'label.loadingPort',
        'locationToName': 'label.dischargePort'
    };

    subscriptions: Array<Subscription> = [];
    vesselNameSearchKeyUp$ = new Subject<string>();
    maxDate: Date = moment().endOf('day').toDate();
    stringHelper = StringHelper;

    constructor(
        private _freightSchedulerService: FreightSchedulerService,
        private commonService: CommonService,
        public notification: NotificationPopup,
        private _gaService: GoogleAnalyticsService
    ) {
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes.carriersModel) {
            // Carriers must have mode of transport
            this.carriersDataSource = this.carriersModel.filter(c => !StringHelper.isNullOrEmpty(c.modeOfTransport));
            this.carriersFiltered = this.carriersDataSource
                .map(
                    (c) => new CarrierSelectionModel(c)
                ).sort((a, b) => (a.carrierCode > b.carrierCode) ? 1 : ((b.carrierCode > a.carrierCode) ? -1 : 0));
        }

        if (changes.locationsModel) {
            this.locationFromFiltered = Object.assign([], this.locationsModel);
            this.locationToFiltered = Object.assign([], this.locationsModel);
        }

        this.vesselsFiltered = [];
        if (changes.model.currentValue) {
            this.vesselsFiltered.push({ 'value': changes.model.currentValue.vesselName });
        }
    }

    ngOnInit() {
        this.setDefaultValue();
        this.handleEventInputVesselName();
    }

    setDefaultValue() {
        this.initPopup();
        this.modeOfTransportOptions = this.modeOfTransportOptions.filter(
            (c) =>
                c.value === ModeOfTransportType.Sea ||
                c.value === ModeOfTransportType.Air
        );
    }

    initPopup(mode?: ModeOfTransportType) {
        if (!this.model || this.popupMode === FormMode.Add) {
            this.model = new FreightSchedulerModel();
            this.model.modeOfTransport = mode ?? ModeOfTransportType.Sea;
            this.vesselsFiltered = [];
        }

        // Replace literals by correct values by mode of transport
        switch (this.model.modeOfTransport.toLowerCase()) {
            case ModeOfTransportType.Sea.toLowerCase():
                this.literalLabels.carrierName = 'label.carrier';
                this.literalLabels.locationFromName = 'label.loadingPort';
                this.literalLabels.locationToName = 'label.dischargePort';
                break;

            case ModeOfTransportType.Air.toLowerCase():
                this.literalLabels.carrierName = 'label.airline';
                this.literalLabels.locationFromName = 'label.origin';
                this.literalLabels.locationToName = 'label.destination';
                break;
            default:
                this.literalLabels.carrierName = 'label.carrier';
                this.literalLabels.locationFromName = 'label.loadingPort';
                this.literalLabels.locationToName = 'label.dischargePort';
        }

        this.carriersFiltered = this.carriersDataSource
            .filter(
                (c) => // filter on mode of transport
                    c.modeOfTransport?.toLowerCase() === this.model.modeOfTransport.toLowerCase()
            )
            .map(
                c => new CarrierSelectionModel(c)
            )
            .sort(
                (a, b) => //  sort by code ASC
                    (a.carrierCode > b.carrierCode) ? 1 : ((b.carrierCode > a.carrierCode) ? -1 : 0)
            );

        if (this.popupMode === FormMode.Edit) {
            this.carrierSelected = this.carriersDataSource.find(c => c.carrierCode === this.model.carrierCode);
            setTimeout(() => {
                this.validateMAWB();
                this.validateFlightNo();
            }, 10);
        }
    }

    onChangeCarrierName(carrierName) {
        this.carrierSelected = this.carriersDataSource.find(
            (c) => c.name === carrierName
        );
        this.model.carrierCode = this.carrierSelected.carrierCode;
        this.validateMAWB();
        this.validateFlightNo();
    }

    onChangeVesselName(vesselName) {
        this.model.vesselName = vesselName;
    }

    onChangeLocationFrom(name) {
        const selectedLocationFrom = this.locationFromFiltered.find(
            x => x.locationDescription === name);
        if (selectedLocationFrom) {
            this.model.locationFromName = selectedLocationFrom.locationDescription;
            this.model.locationFromCode = selectedLocationFrom.name;
        } else {
            this.model.locationFromName = null;
            this.model.locationFromCode = null;
        }
    }

    onChangeLocationTo(name) {
        const selectedLocationTo = this.locationToFiltered.find(
            x => x.locationDescription === name);
        if (selectedLocationTo) {
            this.model.locationToName = selectedLocationTo.locationDescription;
            this.model.locationToCode = selectedLocationTo.name;
        } else {
            this.model.locationToName = null;
            this.model.locationToCode = null;
        }
    }

    onChangeModeOfTransports(mode) {
        const currentFormData = { ...this.model };
        this.mainForm.reset();
        this.mainForm.controls['isAllowExternalUpdate'].setValue(currentFormData.isAllowExternalUpdate);
        this.model.modeOfTransport = mode;
        this.initPopup(mode);
    }

    onChangeDateOrDateTime() {
        if (this.model.etaDate && this.model.etdDate && this.model.etaDate < this.model.etdDate) {
            this.mainForm.controls['etaDate'].setErrors({ 'laterThanETD': true });
        } else if (this.mainForm.controls['etaDate'].hasError('laterThanETD')) {
            delete this.mainForm.controls['etaDate'].errors['laterThanETD'];
            this.mainForm.controls['etaDate'].updateValueAndValidity();
        }
    }

    onChangeETD(value): void {
        if (value) {
            if (this.popupMode === FormMode.Update) {
                this.model.isAllowExternalUpdate = false;
            }
        }
    }

    onChangeETA(value): void {
        if (value) {
            if (this.popupMode === FormMode.Update) {
                this.model.isAllowExternalUpdate = false;
            }
        }
    }

    onChangeATD(): void {
        if (this.model.ataDate && this.model.atdDate && this.model.ataDate < this.model.atdDate) {
            this.mainForm.controls['ataDate'].setErrors({ 'laterThanATD': true });
            this.mainForm.controls['ataDate'].markAsTouched();
        } else if (this.mainForm.controls['ataDate'].hasError('laterThanATD')) {
            delete this.mainForm.controls['ataDate'].errors['laterThanATD'];
            this.mainForm.controls['ataDate'].updateValueAndValidity();
        }


        if (this.model.atdDate) {
            if (this.popupMode === FormMode.Update) {
                this.model.isAllowExternalUpdate = false;
            }
        }
    }

    onChangeATA(): void {
        if (this.model.ataDate && !this.model.atdDate) {
            this.mainForm.controls['atdDate'].markAsTouched();
        }

        if (this.model.ataDate && this.model.atdDate && this.model.ataDate < this.model.atdDate) {
            this.mainForm.controls['ataDate'].setErrors({ 'laterThanATD': true });
        } else if (this.mainForm.controls['ataDate'].hasError('laterThanATD')) {
            delete this.mainForm.controls['ataDate'].errors['laterThanATD'];
            this.mainForm.controls['ataDate'].updateValueAndValidity();
        }


        if (this.model.atdDate) {
            if (this.popupMode === FormMode.Update) {
                this.model.isAllowExternalUpdate = false;
            }
        }
    }

    onFilterCarrierName(input) {
        if (input.length >= 3) {
            this.carriersFiltered = this.carriersDataSource
                .map(
                    (c) => new CarrierSelectionModel(c)
                )
                .filter(
                    (c) => // filter on mode of transport
                        c.modeOfTransport?.toLowerCase() === this.model.modeOfTransport.toLowerCase()
                        && c.displayName.toLowerCase().indexOf(input.toLowerCase()) !== -1
                )
                .sort(
                    (a, b) => //  sort by code ASC
                        (a.carrierCode > b.carrierCode) ? 1 : ((b.carrierCode > a.carrierCode) ? -1 : 0)
                );
        } else {
            this.carriersFiltered = this.carriersDataSource
                .map(
                    (c) => new CarrierSelectionModel(c)
                )
                .filter(
                    (c) => // filter on mode of transport
                        c.modeOfTransport?.toLowerCase() === this.model.modeOfTransport.toLowerCase()
                )
                .sort(
                    (a, b) => //  sort by code ASC
                        (a.carrierCode > b.carrierCode) ? 1 : ((b.carrierCode > a.carrierCode) ? -1 : 0)
                );;
        }
    }

    onFilterVesselName(input) {
        if (input.length >= 3) {
            let sub = this.commonService.searchRealActiveVessels(input.toLowerCase()).subscribe(c => {
                this.vesselsFiltered = c;
            });
            this.subscriptions.push(sub);
        } else {
            this.vesselsFiltered = []
        }
    }

    handleEventInputVesselName(): void {
        const sub = this.vesselNameSearchKeyUp$.pipe(
            debounceTime(500),
            tap((vesselsName: string) => {
                this.onFilterVesselName(vesselsName);
            }
            )).subscribe();
        this.subscriptions.push(sub);
    }

    onFilterLocationFrom(input) {
        if (input.length >= 3) {
            this.locationFromFiltered = this.locationFromFiltered.filter(
                (c) =>
                    c.locationDescription
                        .toLowerCase()
                        .indexOf(input.toLowerCase()) !== -1
            );
        } else {
            this.locationFromFiltered = Object.assign([], this.locationsModel);
        }
    }

    onFilterLocationTo(input) {
        if (input.length >= 3) {
            this.locationToFiltered = this.locationToFiltered.filter(
                (c) =>
                    c.locationDescription
                        .toLowerCase()
                        .indexOf(input.toLowerCase()) !== -1
            );
        } else {
            this.locationToFiltered = Object.assign([], this.locationsModel);
        }
    }

    onChangeMAWB() {
        this.validateMAWB();
    }

    onChangeFlightNo() {
        this.validateFlightNo();
    }

    validateMAWB() {
        if (this.carrierSelected && this.model.mawb) {
            if (!this.carrierSelected.carrierNumber) {
                this.mainForm.controls['mawb'].setErrors({ 'invalidCode': true });
                this.mainForm.controls['mawb'].markAsTouched();
                return;
            }
            if (this.model.mawb.length !== 11 || !this.stringHelper.isDigit(this.model.mawb)) {
                this.mainForm.controls['mawb'].setErrors({ 'invalidCode': true });
                this.mainForm.controls['mawb'].markAsTouched();
                return;
            } else {
                const xxx = this.model.mawb.slice(0, 3);
                if (this.carrierSelected.carrierNumber !== +xxx) {
                    this.mainForm.controls['mawb'].setErrors({ 'invalidCode': true });
                    this.mainForm.controls['mawb'].markAsTouched();
                    return;
                } else {
                    const yyyyyyy = +this.model.mawb.slice(xxx.toString().length, 10);
                    const yyyyyyyDerivedBy7 = yyyyyyy / 7;
                    const subtract = yyyyyyyDerivedBy7 % 1;
                    const dividingBy7 = subtract * 7;
                    const validZ = Math.round(dividingBy7);
                    const inputtedZ = +this.model.mawb.substring(this.model.mawb.length - 1);

                    console.log('yyyyyyyDerivedBy7', yyyyyyyDerivedBy7)
                    console.log('subtract', subtract)
                    console.log('dividingBy7', dividingBy7)
                    console.log('validZ', validZ)
                    console.log('inputtedZ', inputtedZ)

                    if (inputtedZ !== validZ) {
                        this.mainForm.controls['mawb'].setErrors({ 'invalidCode': true });
                        this.mainForm.controls['mawb'].markAsTouched();
                        return;
                    }
                }
            }

            this.mainForm.controls['mawb'].updateValueAndValidity();
        }
    }

    validateFlightNo() {
        if (this.carrierSelected && this.model.flightNumber) {
            const lengthOfCarrierCode = this.model.carrierCode.length;
            const xx = this.model.flightNumber.substring(0, lengthOfCarrierCode);

            if (xx !== this.model.carrierCode) {
                this.mainForm.controls['flightNumber'].setErrors({ 'invalidCode': true });
                this.mainForm.controls['flightNumber'].markAsTouched();
                return;
            }
            else {
                const yyy = this.model.flightNumber.substring(lengthOfCarrierCode, this.model.flightNumber.length)
                if (!this.stringHelper.isDigit(yyy) || !(yyy.length === 3 || yyy.length === 4)) {
                    this.mainForm.controls['flightNumber'].setErrors({ 'invalidCode': true });
                    this.mainForm.controls['flightNumber'].markAsTouched();
                    return;
                }
            }
            this.mainForm.controls['flightNumber'].updateValueAndValidity();
        }
    }

    onSave() {
        FormHelper.ValidateAllFields(this.mainForm);
        if (!this.mainForm.valid) {
            return;
        }

        const data = new FreightSchedulerModel(this.model);

        if (this.popupMode === FormMode.Add) {

            this._freightSchedulerService.create(data).subscribe(
                () => {
                    this.notification.showSuccessPopup(
                        'save.sucessNotification',
                        'label.freightSchedulers'
                    );
                    this.saveSuccess.emit();
                    this._gaService.emitAction('Add New', GAEventCategory.FreightSchedule);
                },
                (err) => {
                    this.notification.showErrorPopup(
                        err.error.errors,
                        'label.freightSchedulers',
                    );
                },
                () => {
                    this.close.emit();
                }
            );
        } else if (this.popupMode === FormMode.Update) {

            this._freightSchedulerService
                .update(data.id.toString(), data)
                .subscribe(
                    () => {
                        this.notification.showSuccessPopup(
                            'save.sucessNotification',
                            'label.freightSchedulers'
                        );
                        this.saveSuccess.emit();
                        this._gaService.emitAction('Update', GAEventCategory.FreightSchedule);
                    },
                    (err) => {
                        this.notification.showErrorPopup(
                            err.error.errors,
                            'label.freightSchedulers'
                        );
                    },
                    () => {
                        this.close.emit();
                    }
                );
        } else if (this.popupMode === FormMode.Edit) {
            this._freightSchedulerService
                .edit(data.id.toString(), data)
                .subscribe(
                    () => {
                        this.notification.showSuccessPopup(
                            'save.sucessNotification',
                            'label.freightSchedulers'
                        );
                        this.saveSuccess.emit();
                        this._gaService.emitAction('Edit', GAEventCategory.FreightSchedule);
                    },
                    (err) => {
                        if (err.error) {
                            const errMessage = err.error.errors;
                            if (!StringHelper.isNullOrEmpty(errMessage)) {
                                const errHashTags = errMessage.split('#').filter(x => !StringHelper.isNullOrEmpty(x));
                                switch (errHashTags[0]) {
                                    case 'ScheduleIsInUse':
                                        this.notification.showErrorPopup(
                                            errHashTags[1],
                                            'label.freightSchedulers'
                                        );
                                        return;
                                    default:
                                        this.notification.showErrorPopup(
                                            errHashTags[0],
                                            'label.freightSchedulers'
                                        );
                                        break;
                                }
                            }
                        }
                    },
                    () => {
                        this.close.emit();
                    }
                );
        }
    }

    onCancel() {
        this.close.emit();
    }

    onClose(event) {
        event.preventDefault();
    }

    get isUpdateMode(): boolean {
        return this.popupMode === FormMode.Update;
    }

    get popupTitle(): string {
        let title = '';
        switch (this.popupMode) {
            case FormMode.Add:
                title = 'label.freightSchedulers.add';
                break;
            case FormMode.Edit:
                title = 'label.freightSchedulers.editSchedule';
                break;
            case FormMode.Update:
                title = 'label.freightSchedulers.updateSchedule';
                break;
            default:
                break;
        }
        return title;
    }

    ngOnDestroy(): void {
        this.subscriptions.map(c => c.unsubscribe());
    }
}
