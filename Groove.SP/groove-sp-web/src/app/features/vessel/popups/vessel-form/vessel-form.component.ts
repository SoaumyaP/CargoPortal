import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ViewChild, SimpleChanges } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { FormComponent, FormMode, StringHelper, VesselStatus } from 'src/app/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { faPlus, faEllipsisV, faInfoCircle } from '@fortawesome/free-solid-svg-icons';
import { Subject, Subscription } from 'rxjs';
import { VesselFormService } from './vessel-form.service';
import { VesselModel } from '../../models/vessel.model';
import { AbstractControl, NgForm } from '@angular/forms';
import { DefaultDebounceTimeInput } from 'src/app/core/models/constants/app-constants';
import { debounceTime, tap } from 'rxjs/operators';
import { FormHelper } from 'src/app/core/helpers/form.helper';

@Component({
  selector: 'app-vessel-form',
  templateUrl: './vessel-form.component.html',
  styleUrls: ['./vessel-form.component.scss']
})
export class VesselFormComponent implements OnInit, OnDestroy {
  @ViewChild('mainForm', { static: false }) mainForm: NgForm;
  @Input() public vesselFormOpened: boolean = false;
  @Input() public formMode: string;
  @Input() model: VesselModel;

  @Output() close: EventEmitter<any> = new EventEmitter();
  @Output() add: EventEmitter<any> = new EventEmitter<any>();
  @Output() edit: EventEmitter<any> = new EventEmitter<any>();

  firstValuesToCheckDuplication = [];
  faPlus = faPlus;
  faEllipsisV = faEllipsisV;
  faInfoCircle = faInfoCircle;
  typingVesselCodeEvent$ = new Subject<string>();
  typingVesselNameEvent$ = new Subject<string>();
  FormMode = FormMode;
  readonly vesselStatus = VesselStatus;
  defaultDebounceTimeInput = 250;

  // Store all subscriptions, then should un-subscribe at the end
  private _subscriptions: Array<Subscription> = [];

  constructor(
    public notification: NotificationPopup,
    public vesselFormService: VesselFormService,
    public translateService: TranslateService) {
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.model?.firstChange === false) {
      this.firstValuesToCheckDuplication = [
        {
          code: this.model.code
        },
        {
          name: this.model.name
        }
      ];
    }
  }

  ngOnInit() {
    this.handleInputEvent();
  }

  handleInputEvent() {
    let sub = this.typingVesselCodeEvent$.pipe(
      debounceTime(this.defaultDebounceTimeInput),
      tap((value: string) => {
        this.checkAlreadyExistVessel('code', value);
      })).subscribe();
    this._subscriptions.push(sub);

    sub = this.typingVesselNameEvent$.pipe(
      debounceTime(this.defaultDebounceTimeInput),
      tap((value: string) => {
        this.checkAlreadyExistVessel('name', value);
      })).subscribe();
    this._subscriptions.push(sub);
  }

  checkAlreadyExistVessel(propertyName: string, value: string) {
    if (StringHelper.isNullOrEmpty(value)) {
      return;
    }

    if (!this.firstValuesToCheckDuplication.some(c => c[propertyName]?.toLowerCase().trim() === value.toLowerCase().trim())) {
      this.vesselFormService.getAllVessels().subscribe(
        (r: []) => {
          const isAlreadyExist = r.some((c: any) => c[propertyName]?.toLowerCase().trim() === value.toLowerCase().trim());
          if (isAlreadyExist) {
            this.getFormControl(propertyName).setErrors({ isDuplicated: true });
            this.getFormControl(propertyName).markAsTouched();
          } else {
            this.getFormControl(propertyName).setErrors(null);
          }
        }
      )
    }
  }


  onFormClosed() {
    this.close.emit();
  }

  get formTitle() {
    return this.translateService.instant(this.formMode === FormMode.Add ?
      'label.addNewVessel' : 'label.editVessel');
  }

  getFormControl(controlName: string): AbstractControl {
    return this.mainForm?.form?.controls[controlName];
  }

  onSave() {
    FormHelper.ValidateAllFields(this.mainForm);
    if (!this.mainForm.valid) {
      return;
    }

    if (this.formMode === FormMode.Add) {
      this.vesselFormService.addNewVessel(this.model).subscribe(
        r => {
          this.notification.showSuccessPopup('save.sucessNotification', 'label.vessel');
          this.close.emit(true);
        },
        err => {
          this.notification.showErrorPopup('save.failureNotification', 'label.vessel');
        },
      );
    } else {
      this.vesselFormService.updateVessel(this.model).subscribe(
        r => {
          this.notification.showSuccessPopup('save.sucessNotification', 'label.vessel');
          this.close.emit(true);
        },
        err => {
          this.notification.showErrorPopup('save.failureNotification', 'label.vessel');
        }
      );
    }
  }

  ngOnDestroy() {
    this._subscriptions.map(x => x.unsubscribe());
  }
}
