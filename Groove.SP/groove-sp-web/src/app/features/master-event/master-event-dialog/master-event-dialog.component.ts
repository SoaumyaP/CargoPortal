import { AfterViewInit, Component, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { StringHelper } from 'src/app/core';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { EventOrderType, FormMode } from 'src/app/core/models/enums/enums';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { EventCodeModel } from '../models/master-event.model';
import { MasterEventDialogService } from './master-event-dialog.service';

@Component({
  selector: 'app-master-event-dialog',
  templateUrl: './master-event-dialog.component.html',
  styleUrls: ['./master-event-dialog.component.scss']
})
export class MasterEventDialogComponent implements OnInit, AfterViewInit, OnDestroy {
  @Input() isOpenDialog: boolean;
  @Input() model: EventCodeModel = {} as EventCodeModel;
  @Input() public formMode: string;
  @Output() close: EventEmitter<any> = new EventEmitter<any>();
  @Output() saveDialogSuccessfully: EventEmitter<any> = new EventEmitter<any>();
  @ViewChild('mainForm', { static: false }) mainForm: NgForm;

  eventCodeTypingEvent$ = new Subject<string>();
  FormMode = FormMode;
  eventTypeDataSource: any;
  eventCodeDataSource: any[];
  subscriptions = new Subscription();
  eventOrderType = EventOrderType;

  defaultEventTypeDropDownItem: { text: string, value: string } =
    {
      text: this.translateService.instant('label.select'),
      value: null
    };

  constructor(
    public translateService: TranslateService,
    private masterEventDialogService: MasterEventDialogService,
    private notification: NotificationPopup,
  ) { }

  ngOnInit() {
    this.bindingDataToForm();

    let sub = this.masterEventDialogService.getEventTypeDropdown().subscribe(
      data => {
        this.eventTypeDataSource = data;
      }
    )

    this.subscriptions.add(sub);

    sub = this.masterEventDialogService.getEventCodesDropdown().subscribe(
      data => {
        this.eventCodeDataSource = data;
        if (this.formMode === FormMode.Edit) {
          const eventIndex = this.eventCodeDataSource.findIndex(c => c.value.toLowerCase() === this.model.activityCode.toLowerCase());
          this.model.beforeEvent = eventIndex === this.eventCodeDataSource.length - 1 ? this.eventCodeDataSource[eventIndex].value : this.eventCodeDataSource[eventIndex + 1].value;
          this.model.afterEvent = eventIndex === 0 ? this.eventCodeDataSource[eventIndex].value : this.eventCodeDataSource[eventIndex - 1].value;
        }
      }
    )

    this.subscriptions.add(sub);
  }

  onEventOrderChange(value) {
    const evIndex = this.eventCodeDataSource.findIndex(c => c.value.toLowerCase() === value.toLowerCase());
    if (this.model.eventOrderType === EventOrderType.After) {
      this.model.beforeEvent = evIndex + 1 < this.eventCodeDataSource.length ? this.eventCodeDataSource[evIndex + 1].value : null;
    }
    else if (this.model.eventOrderType === EventOrderType.Before) {
      this.model.afterEvent = evIndex - 1 >= 0 ? this.eventCodeDataSource[evIndex - 1].value : null;
    }
  }

  bindingDataToForm() {
    if (this.formMode === FormMode.Add) {
      this.model = {
        eventOrderType: EventOrderType.Before,
      } as EventCodeModel;
    } else {
      this.model.eventOrderType = EventOrderType.Before;
      this.model.locationRequired = this.model.locationRequired === 'Yes' ? true : false;
      this.model.remarkRequired = this.model.remarkRequired === 'Yes' ? true : false;
    }
  }

  ngAfterViewInit(): void {
    this.handleInputEvent();
  }

  onSave() {
    FormHelper.ValidateAllFields(this.mainForm);
    if (this.mainForm.invalid) {
      return;
    }

    if (this.formMode === FormMode.Add) {
      this.masterEventDialogService.addNewEventCdoe(this.model).subscribe(
        data => {
          this.notification.showSuccessPopup("save.sucessNotification", "label.masterEvent");
          this.saveDialogSuccessfully.emit();
        },
        err => {
          this.notification.showErrorPopup("save.failureNotification", "label.masterEvent");
        }
      )
    } else {
      this.masterEventDialogService.updateEventCode(this.model).subscribe(
        data => {
          this.notification.showSuccessPopup("save.sucessNotification", "label.masterEvent");
          this.saveDialogSuccessfully.emit();
        },
        err => {
          this.notification.showErrorPopup("save.failureNotification", "label.masterEvent");
        }
      )
    }
  }

  onFormClosed() {
    this.close.emit();
  }

  get formTitle() {
    return this.translateService.instant(this.formMode === FormMode.Add ?
      'label.addNewEvent' : 'label.editEvent');
  }

  handleInputEvent() {
    let sub = this.eventCodeTypingEvent$.pipe(
      debounceTime(500),
      tap((value: string) => {
        this.checkEventCodeAlreadyExist(value);
      })).subscribe();
    this.subscriptions.add(sub);
  }

  checkEventCodeAlreadyExist(code: string) {
    if (StringHelper.isNullOrEmpty(code)) {
      return;
    }

    const sub = this.masterEventDialogService.checkEventCodeAlreadyExist(code).subscribe(
      r => {
        if (r) {
          this.getFormControl('activityCode').setErrors({ isAlreadyExists: true });
          this.getFormControl('activityCode').markAsTouched();
        } else {
          this.getFormControl('activityCode').setErrors(null);
        }
      }
    )
    this.subscriptions.add(sub);
  }

  getFormControl(controlName: string) {
    return this.mainForm?.form?.controls[controlName];
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
