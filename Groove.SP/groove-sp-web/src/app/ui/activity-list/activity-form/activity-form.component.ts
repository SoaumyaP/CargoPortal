import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { StringHelper } from 'src/app/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { CommonService } from 'src/app/core/services/common.service';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'app-activity-form',
  templateUrl: './activity-form.component.html',
  styleUrls: ['./activity-form.component.scss']
})
export class ActivityFormComponent implements OnChanges {
  @Input()
  activityFormOpened: boolean = false;
  @Input()
  model: any;
  @Input()
  heightPopup = 530;
  @Input()
  set formMode(mode: string) {
    this.isViewMode = mode === 'view';
    this.isEditMode = mode === 'edit';
    this.isAddMode = mode === 'add';
  }
  @Input()
  allEventOptions: any[];
  @Input()
  isHideEventCode: boolean;

  @Output()
  close: EventEmitter<any> = new EventEmitter<any>();
  @Output()
  add: EventEmitter<any> = new EventEmitter<any>();
  @Output()
  edit: EventEmitter<any> = new EventEmitter<any>();

  @ViewChild('mainForm', { static: false }) currentForm: NgForm;

  isViewMode: boolean;
  isEditMode: boolean;
  isAddMode: boolean;
  filteredEventOptions: any[];
  originFilteredEventOptions: any[];
  allLocationOptions: any[];
  filteredLocationOptions: any[];
  defaultDropDownEvent: { eventName: string } =
    {
      eventName: 'label.select'
    };

  constructor(protected route: ActivatedRoute,
    public notification: NotificationPopup,
    public router: Router,
    private commonsService: CommonService,
    public translateService: TranslateService) {

    this.commonsService.getAllLocations().subscribe(data => {
      this.allLocationOptions = data;
    });
  }

  ngOnChanges(value: SimpleChanges) {
    if (value.allEventOptions && this.allEventOptions) {
      this.filteredEventOptions = [];
      this.originFilteredEventOptions = [];
      this.allEventOptions.forEach(item => {
        item.eventName = this.isHideEventCode ? item.activityDescription : item.activityCode + ' - ' + item.activityDescription;
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
        s.activityDescription.toLowerCase().indexOf(value.toLowerCase()) !== -1);
    } else {
      this.filteredEventOptions = this.originFilteredEventOptions;
    }
  }

  onEventChange(value) {
    if (this.filteredEventOptions) {
      let selectedItemTypeDescription = '';
      const selectedItem = this.filteredEventOptions.find(
        (element) => {
          if (element.eventName === value) {
            selectedItemTypeDescription = element.activityTypeDescription;
            return element.eventName;
          }
          return null;
        });

      if (StringHelper.isNullOrEmpty(selectedItem)) {
        this.model.activityCode = null;
        this.model.activityDescription = '';
        this.model.activityType = '';
        this.model.resolved = null;
        this.heightPopup = 530;
        this.currentForm.controls['eventName'].setErrors({'invalid': true});
      } else {
        this.model.activityCode = selectedItem.activityCode;
        this.model.activityDescription = selectedItem.activityDescription;
        this.model.activityType = selectedItem.activityType;
        this.model.activityTypeDescription = selectedItemTypeDescription;
        this.currentForm.controls['eventName'].setErrors(null);
        this.heightPopup = this.isExceptionEventType(selectedItem.activityType) ? 710 : 530;
        this.model.resolved = false;
      }
    }
  }

  locationFilterChange(value) {
    this.filteredLocationOptions = [];
    if (value.length >= 3) {
      this.filteredLocationOptions = this.allLocationOptions.filter((s) => s.locationDescription.toLowerCase().indexOf(value.toLowerCase()) !== -1);
    }
  }

  onResolveDateChange() {
    if (this.model.resolved && StringHelper.isNullOrEmpty(this.model.resolutionDate)) {
      this.model.resolutionDate = new Date();
    }
  }

  onFormClosed() {
    this.activityFormOpened = false;
    this.close.emit();
  }

  onAddClick() {
    this.add.emit(this.model);
  }

  onEditClick() {
    this.edit.emit(this.model);
  }

  get title() {
    if (this.isViewMode) {
      return 'label.activityDetail';
    }

    return this.isAddMode ? 'label.addActivity' : 'label.editActivity';
  }

  isExceptionEventType(activityType) {
    return !StringHelper.isNullOrEmpty(activityType) && activityType[activityType.length - 1] === 'E';
  }
}
