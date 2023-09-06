import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { faEllipsisV, faPencilAlt, faPlus, faTrashAlt } from '@fortawesome/free-solid-svg-icons';
import { StringHelper } from 'src/app/core';
import { DATE_FORMAT } from 'src/app/core/helpers/date.helper';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';

@Component({
  selector: 'app-activity-list',
  templateUrl: './activity-list.component.html',
  styleUrls: ['./activity-list.component.scss']
})
export class ActivityListComponent implements OnInit {
  @Input()
  disableAllEditButtons: boolean;
  @Input()
  activityList: any[];
  @Input()
  allEventOptions: any[];
  @Input()
  canEditActivity: boolean;
  @Input()
  canAddActivity: boolean;
  @Input()
  isHideEventCode: boolean;
  @Output()
  add: EventEmitter<any> = new EventEmitter<any>();
  @Output()
  edit: EventEmitter<any> = new EventEmitter<any>();
  @Output()
  delete: EventEmitter<any> = new EventEmitter<any>();

  DATE_FORMAT = DATE_FORMAT;
  faPlus = faPlus;
  faEllipsisV = faEllipsisV;
  faPencilAlt = faPencilAlt;
  faTrashAlt = faTrashAlt;
  activityFormMode: string;
  activityFormOpened: boolean;
  activityDetails: any;
  heightActivity: number = 530;

  constructor(public notification: NotificationPopup) { }

  ngOnInit() {
  }

  onAddActivityClick() {
    this.activityFormMode = 'add';
    this.activityFormOpened = true;
    this.heightActivity = 530;
    this.activityDetails = {
      eventName: null,
      activityDate: new Date()
    };
  }

  onEditActivityClick(activity: any) {
    this.activityFormMode = 'edit';
    this.activityFormOpened = true;
    this.activityDetails = Object.assign({}, activity);
    const selectedEvent = this.allEventOptions.find(x => x.activityCode === activity.activityCode);
    this.activityDetails.eventName = this.isHideEventCode ? selectedEvent.activityDescription : activity.activityCode + ' - ' + selectedEvent.activityDescription;
    this.activityDetails.activityTypeDescription = selectedEvent.activityTypeDescription;

    if (this.isExceptionEventType(activity.activityType)) {
      this.heightActivity = 710;
    }
  }

  isExceptionEventType(activityType: string) {
    return !StringHelper.isNullOrEmpty(activityType) && activityType[activityType.length - 1] === 'E';
  }

  onDeleteActivityClick(activityId: number) {
    const confirmDlg = this.notification.showConfirmationDialog('msg.deleteActivityConfirm', 'label.activity');

    confirmDlg.result.subscribe(
      (result: any) => {
        if (result.value) {
          this.delete.emit(activityId);
        }
      });
  }

  openActivityPopup(activity) {
    this.onEditActivityClick(activity);
    this.activityFormMode = 'view';
  }

  onActivityAdded(activity) {
    this.activityFormOpened = false;
    this.add.emit(activity);
  }

  onActivityEdited(activity) {
    this.activityFormOpened = false;
    this.edit.emit(activity);
  }

  onActivityFormClosed() {
    this.activityFormOpened = false;
  }
}
