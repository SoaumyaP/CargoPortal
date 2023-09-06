import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { FormComponent, StringHelper, OrderType, ActivityType, EventLevelMapping } from 'src/app/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { ContainerFormService } from '../container-tracking/container-form.service';

@Component({
    selector: 'app-container-activity-form',
    templateUrl: './container-activity-form.component.html',
    styleUrls: ['./container-activity-form.component.scss']
})
export class ContainerActivityFormComponent extends FormComponent implements OnChanges {
    @Input() public activityFormOpened: boolean = false;
    @Input() public model: any;
    @Input() public containerId: any;
    @Input() public heightPopup = 530;
    @Input()
    set activityFormMode(mode: string) {
        this.isViewModeLocal = mode === this.formMode.view;
        this.isEditModeLocal = mode === this.formMode.edit;
        this.isAddModeLocal = mode === this.formMode.add;
    }
    @Input() public allEventOptions: any[];
    public isViewModeLocal: boolean;
    public isEditModeLocal: boolean;
    public isAddModeLocal: boolean;

    @Input() public orderType: OrderType;

    @Output() close: EventEmitter<any> = new EventEmitter<any>();
    @Output() add: EventEmitter<any> = new EventEmitter<any>();
    @Output() edit: EventEmitter<any> = new EventEmitter<any>();

    filteredEventOptions: any[];
    originFilteredEventOptions: any[];

    disableEventSelection: boolean = false;

    allLocationOptions: any[];
    filteredLocationOptions: any[];
    validationRules = {
        eventName: {
            required: 'label.eventCode'
        },
        activityDate: {
            required: 'label.activityDates'
        },
        location: {},
        remark: {},
        resolutionDate: {
            required: 'label.resolveDates'
        },
        resolution: {
            required: 'label.resolution'
        }
    };
    public defaultDropDownEvent: { eventName: string } =
    {
        eventName: 'label.select'
    };

    constructor(protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: ContainerFormService,
        public translateService: TranslateService) {
        super(route, service, notification, translateService, router);

        this.service.getAllLocations().subscribe(data => {
            this.allLocationOptions = data;
        });
    }

    ngOnChanges(value: SimpleChanges) {
        if (value.allEventOptions && this.allEventOptions) {
            this.filteredEventOptions = [];
            this.originFilteredEventOptions = [];

            if (this.orderType) {
                // If shipment is cruise then only take activity dropdown belong cruise
                if (+this.orderType === OrderType.Cruise) {
                    this.allEventOptions =  this.allEventOptions.filter(c => c.activityType === ActivityType.CruiseShipment);
                } else {
                    this.allEventOptions =  this.allEventOptions.filter(c => c.activityType !== ActivityType.CruiseShipment);
                }
            }

            this.allEventOptions.forEach(item => {
                item.eventName = item.activityCode + ' - ' + item.activityDescription;
                this.filteredEventOptions.push(item);
                this.originFilteredEventOptions.push(item);
            });

            if (this.isEditModeLocal && value.model.currentValue?.id) {
                const selectedEvent = this.filteredEventOptions?.find(event => 
                    event.eventName === value.model.currentValue.eventName
                );
                if (selectedEvent) {
                    this.disableEventSelection = selectedEvent.activityTypeLevel !== EventLevelMapping.Container || !selectedEvent.status;
                    this.model.isLocationRequired = selectedEvent.locationRequired;
                    this.model.isRemarkRequired = selectedEvent.remarkRequired;
                    if (this.model.isLocationRequired) {
                        this.validationRules['location']['required'] = 'label.location';
                    }
                    if (this.model.isRemarkRequired) {
                        this.validationRules['remark']['required'] = 'label.remark';
                    }
                }
            }

            if (!this.disableEventSelection) {
                this.originFilteredEventOptions = this.originFilteredEventOptions.filter(event => event.activityTypeLevel === EventLevelMapping.Container && event.status);
                this.filteredEventOptions = [...this.originFilteredEventOptions];
            }
        }
    }

    eventFilterChange(value: string) {
        this.filteredEventOptions = [];
        if (value.length > 0) {
            this.filteredEventOptions = this.originFilteredEventOptions.filter((s) =>
                s.activityCode.toLowerCase().indexOf(value.toLowerCase()) !== -1 ||
                s.activityDescription.toLowerCase().indexOf(value.toLowerCase()) !== -1 );
        } else {
            this.filteredEventOptions = this.originFilteredEventOptions;
        }
    }

    onEventChange(value) {
        if (this.filteredEventOptions) {
            let selectedItemTypeDescription = '';
            const selectedItem = this.filteredEventOptions.find(
                (element) => {
                    const item = element.activityCode + ' - ' + element.activityDescription;
                    if (item === value) {
                        selectedItemTypeDescription = element.activityTypeDescription;
                        return item;
                    }
                });

            if (StringHelper.isNullOrEmpty(selectedItem)) {
                this.model.activityCode = null;
                this.model.activityDescription = '';
                this.model.activityType = '';
                this.model.resolved = null;
                this.heightPopup = 530;
                this.setInvalidControl('eventName','required');
            } else {
                this.model.activityCode = selectedItem.activityCode;
                this.model.activityDescription = selectedItem.activityDescription;
                this.model.activityType = selectedItem.activityType;
                this.model.activityTypeDescription = selectedItemTypeDescription;
                this.setValidControl('eventName');
                this.heightPopup = this.isExceptionEventType(selectedItem.activityType) ? 710 : 530;
                this.model.resolved = false;

                this.model.isLocationRequired = selectedItem.locationRequired;
                this.model.isRemarkRequired = selectedItem.remarkRequired;
                if (this.model.isLocationRequired) {
                    this.validationRules['location']['required'] = 'label.location';
                }
                if (this.model.isRemarkRequired) {
                    this.validationRules['remark']['required'] = 'label.remark';
                }
            }
        }
    }

    locationFilterChange(value) {
        this.filteredLocationOptions = [];
        if (value.length >= 3) {
            this.filteredLocationOptions = this.allLocationOptions.filter((s) => s.locationDescription.toLowerCase().indexOf(value.toLowerCase()) !== -1);
        }
    }

    onResolveDateChange () {
        if (this.model.resolved && StringHelper.isNullOrEmpty(this.model.resolutionDate)) {
            this.model.resolutionDate = new Date();
        }
    }

    onFormClosed() {
        this.activityFormOpened = false;
        this.close.emit();
    }

    onAddClick() {
        this.validateAllFields(false);

        if (!this.mainForm.valid) {
            return;
        }

        this.add.emit(this.model);
    }

    onEditClick() {
        this.validateAllFields(false);

        if (!this.mainForm.valid) {
            return;
        }
        this.edit.emit(this.model);
    }

    get title() {
        if (this.isViewModeLocal) {
            return 'label.activityDetail';
        }

        return this.isAddModeLocal ?  'label.addActivity' : 'label.editActivity';
    }

    isExceptionEventType(activityType) {
        return !StringHelper.isNullOrEmpty(activityType) && activityType[activityType.length - 1] === 'E';
    }
}
