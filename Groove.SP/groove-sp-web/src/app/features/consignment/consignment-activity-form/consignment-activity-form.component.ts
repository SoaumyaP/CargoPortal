import { Component, Input, Output, EventEmitter, OnChanges, SimpleChange, SimpleChanges } from '@angular/core';
import { DropDowns, UserContextService, FormComponent, StringHelper } from 'src/app/core';
import { ConsignmentFormService } from '../consignment-form/consignment-form.service';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';

@Component({
    selector: 'app-consignment-activity-form',
    templateUrl: './consignment-activity-form.component.html',
    styleUrls: ['./consignment-activity-form.component.scss']
})
export class ConsignmentActivityFormComponent extends FormComponent implements OnChanges {
    @Input() public activityFormOpened: boolean = false;
    @Input() public model: any;
    @Input() public shipmentNo: string;
    @Input() public consignmentId: any;
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

    @Output() close: EventEmitter<any> = new EventEmitter<any>();
    @Output() add: EventEmitter<any> = new EventEmitter<any>();
    @Output() edit: EventEmitter<any> = new EventEmitter<any>();

    modeOfTransportOptions = DropDowns.ModeOfTransportStringType;
    filteredEventOptions: any[];
    originFilteredEventOptions: any[];

    allLocationOptions: any[];
    filteredLocationOptions: any[];
    validationRules = {
        eventName: {
            required: 'label.eventCode'
        },
        activityDate: {
            required: 'label.activityDates'
        },
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
        public service: ConsignmentFormService,
        public translateService: TranslateService,
        private _userContext: UserContextService) {
        super(route, service, notification, translateService, router);



        this.service.getAllLocations().subscribe(data => {
            this.allLocationOptions = data;
        });
    }

    ngOnChanges(value: SimpleChanges) {
        if (value.allEventOptions && this.allEventOptions) {
            this.filteredEventOptions = [];
            this.originFilteredEventOptions = [];
            this.allEventOptions.forEach(item => {
                item.eventName = item.activityCode + ' - ' + item.activityDescription;
                this.filteredEventOptions.push(item);
                this.originFilteredEventOptions.push(item);
            });
        }
    }

    eventFilterChange(value: string) {
        this.filteredEventOptions = [];
        if (value.length > 0) {
            this.filteredEventOptions = this.allEventOptions.filter((s) =>
                s.activityCode.toLowerCase().indexOf(value.toLowerCase()) !== -1 ||
                s.activityDescription.toLowerCase().indexOf(value.toLowerCase()) !== -1 );
        } else {
            this.filteredEventOptions = this.originFilteredEventOptions;
        }
    }

    onEventChange(value) {
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
            this.setInvalidControl('eventName', 'required');
        } else {
            this.model.activityCode = selectedItem.activityCode;
            this.model.activityDescription = selectedItem.activityDescription;
            this.model.activityType = selectedItem.activityType;
            this.model.activityTypeDescription = selectedItemTypeDescription;
            this.setValidControl('eventName')
            this.heightPopup = this.isExceptionEventType(selectedItem.activityType) ? 710 : 530;
            this.model.resolved = false;
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
