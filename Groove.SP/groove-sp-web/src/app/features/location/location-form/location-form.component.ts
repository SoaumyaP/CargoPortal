import { Component, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges, ViewChild } from '@angular/core';
import { AbstractControl, NgForm } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { StringHelper } from 'src/app/core';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { DefaultDebounceTimeInput } from 'src/app/core/models/constants/app-constants';
import { DropdownListModel } from 'src/app/core/models/dropDowns/dropdown-item-model';
import { FormMode } from 'src/app/core/models/enums/enums';
import { LocationModel } from 'src/app/core/models/location.model';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { ComplianceFormService } from '../../compliance/compliance-form/compliance-form.service';
import { LocationListService } from '../location-list/location-list.service';
import { LocationFormService } from './location-form.service';

@Component({
  selector: 'app-location-form',
  templateUrl: './location-form.component.html',
  styleUrls: ['./location-form.component.scss']
})
export class LocationFormComponent implements OnChanges, OnInit, OnDestroy {
  @ViewChild('mainForm', { static: false }) mainForm: NgForm;
  @Input() formMode: string;
  @Input() isOpen: boolean;
  @Input() model: LocationModel;

  @Output() close: EventEmitter<boolean> = new EventEmitter();

  FormMode = FormMode;
  defaultDropDownItem: { label: string, description: string, value: number } =
    {
      label: 'label.select',
      description: 'select',
      value: null
    };
  defaultDebounceTimeInput = 100;
  countries: DropdownListModel<number>[] = [];
  countriesFiltered: DropdownListModel<number>[] = [];
  typingEdisonCodeEvent$ = new Subject<string>();
  typingLocationCodeEvent$ = new Subject<string>();
  typingLocationnameEvent$ = new Subject<string>();
  firstValuesToCheckDuplication = [];

  private _subscriptions: Array<Subscription> = [];

  constructor(
    private translateService: TranslateService,
    private complianceFormService: ComplianceFormService,
    private locationFormService: LocationFormService,
    private locationListService: LocationListService,
    private notification: NotificationPopup,
  ) { }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.model?.firstChange === false) {
      this.firstValuesToCheckDuplication = [
        {
          ediSonPortCode: this.model.ediSonPortCode
        },
        {
          name: this.model.name
        },
        {
          locationDescription: this.model.locationDescription
        }
      ];
    }
  }

  ngOnInit() {
    this.handleInputEvent();
    const sub = this.complianceFormService.getCountries().subscribe(
      (r: DropdownListModel<number>[]) => {
        this.countries = r;
        this.countriesFiltered = r;
      }
    );
    this._subscriptions.push(sub);
  }

  handleInputEvent() {
    let sub = this.typingEdisonCodeEvent$.pipe(
      debounceTime(this.defaultDebounceTimeInput),
      tap((value: string) => {
        this.checkAlreadyExistLocation('ediSonPortCode', value);
      })).subscribe();
    this._subscriptions.push(sub);

    sub = this.typingLocationCodeEvent$.pipe(
      debounceTime(this.defaultDebounceTimeInput),
      tap((value: string) => {
        this.checkAlreadyExistLocation('name', value);
      })).subscribe();
    this._subscriptions.push(sub);

    sub = this.typingLocationnameEvent$.pipe(
      debounceTime(this.defaultDebounceTimeInput),
      tap((value: string) => {
        this.checkAlreadyExistLocation('locationDescription', value);
      })).subscribe();
    this._subscriptions.push(sub);
  }

  checkAlreadyExistLocation(propertyName: string, value: string) {
    if (StringHelper.isNullOrEmpty(value)) {
      return;
    }

    if (!this.firstValuesToCheckDuplication.some(c => c[propertyName]?.toLowerCase().trim() === value.toLowerCase().trim())) {
      this.locationListService.getLocations().subscribe(
        (r: []) => {
          const isAlreadyExist = r.some((c: any) => c[propertyName].toLowerCase().trim() === value.toLowerCase().trim());
          if (isAlreadyExist) {
            this.getControl(propertyName).setErrors({ isDuplicated: true });
            this.getControl(propertyName).markAsTouched();
          } else {
            this.getControl(propertyName).setErrors(null);
          }
        }
      )
    }
  }

  get formTitle() {
    return this.translateService.instant(this.formMode === FormMode.Add ? 'label.addNewLocation' : 'label.editLocation');
  }

  onCloseForm() {
    this.close.emit(false);
  }

  onSave() {
  FormHelper.ValidateAllFields(this.mainForm);
    if (!this.mainForm.valid) {
      return;
    }

    if (this.formMode === FormMode.Add) {
      this.locationFormService.addNewLocation(this.model).subscribe(
        r => {
          this.notification.showSuccessPopup('save.sucessNotification', 'label.location');
          this.close.emit(true);
        },
        err => {
          this.notification.showErrorPopup('save.failureNotification', 'label.location');
        },
      );
    } else {
      this.locationFormService.editLocation(this.model).subscribe(
        r => {
          this.notification.showSuccessPopup('save.sucessNotification', 'label.location');
          this.close.emit(true);
        },
        err => {
          this.notification.showErrorPopup('save.failureNotification', 'label.location');
        }
      );
    }
  }

  onCountryFilterChanged(value) {
    this.countriesFiltered = this.countries.filter(c => c.label.toLocaleLowerCase().indexOf(value.toLocaleLowerCase()) !== -1);
  }

  getControl(controlName: string): AbstractControl {
    return this.mainForm?.form?.controls[controlName];
  }

  ngOnDestroy(): void {
    this._subscriptions.map(x => x.unsubscribe());
  }
}
