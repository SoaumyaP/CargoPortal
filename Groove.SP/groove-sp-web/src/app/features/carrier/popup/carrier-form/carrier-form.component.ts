import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { faPlus, faEllipsisV, faInfoCircle } from '@fortawesome/free-solid-svg-icons';
import { Subscription } from 'rxjs';
import { DropDowns, FormMode, ModeOfTransportType, StringHelper } from 'src/app/core';
import { CarrierModel } from 'src/app/core/models/carrier.model';
import { NgForm } from '@angular/forms';
import { CarrierFormService } from './carrier-form.service';
import { FormHelper } from 'src/app/core/helpers/form.helper';

@Component({
  selector: 'app-carrier-form',
  templateUrl: './carrier-form.component.html',
  styleUrls: ['./carrier-form.component.scss']
})
export class CarrierFormComponent implements OnInit, OnDestroy {
  @Input() public model: CarrierModel;
  @Input() public carrierFormOpened: boolean = false;
  @Input() public formMode: string;
  @Output() close: EventEmitter<any> = new EventEmitter();
  @Output() add: EventEmitter<any> = new EventEmitter<any>();
  @Output() edit: EventEmitter<any> = new EventEmitter<any>();
  @ViewChild('mainForm', { static: false }) mainForm: NgForm;

  faPlus = faPlus;
  faEllipsisV = faEllipsisV;
  faInfoCircle = faInfoCircle;
  modeOfTransportType = ModeOfTransportType;
  formModeType = FormMode;

  modeOfTransportOptions = DropDowns.ModeOfTransportStringType.filter(x => x.value !== ModeOfTransportType.MultiModal);
  defaultModeOfTransportDropDownItem: { label: string, value: string } =
  {
      label: this.translateService.instant('label.select'),
      value: null
  };

  // Store all subscriptions, then should un-subscribe at the end
  private _subscriptions: Array<Subscription> = [];

  constructor(public translateService: TranslateService, public service: CarrierFormService) { }

  ngOnInit() {
  }

  carrierCodeFocusout() {
    if (StringHelper.isNullOrEmpty(this.model.carrierCode)) {
      return;
    }
    this.service.checkDuplicateCarrierCode(this.model).subscribe(
      res => {
        if (res) {
          this.frmControlByName('carrierCode').setErrors({'duplicated': true});
        }
      }
    )
  }

  carrierNameFocusout() {
    if (StringHelper.isNullOrEmpty(this.model.name)) {
      return;
    }
    this.service.checkDuplicateCarrierName(this.model).subscribe(
      res => {
        if (res) {
          this.frmControlByName('carrierName').setErrors({'duplicated': true});
        }
      }
    )
  }

  carrierNumberFocusout() {
    if (StringHelper.isNullOrEmpty(this.model.carrierNumber)) {
      return;
    }
    this.service.checkDuplicateCarrierNumber(this.model).subscribe(
      res => {
        if (res) {
          this.frmControlByName('carrierNumber').setErrors({'duplicated': true});
        }
      }
    )
  }

  onSave() {
    FormHelper.ValidateAllFields(this.mainForm);
    if (this.mainForm.invalid) {
     return; 
    }

    if (this.formMode === FormMode.Add) {
      this.add.emit(this.model);
    } else {
      this.edit.emit(this.model);
    }
  }

  onFormClosed() {
    this.carrierFormOpened = false;
    this.close.emit();
  }

  get formTitle() {
    return this.translateService.instant(this.formMode === FormMode.Add ? 
      'label.addNewCarrier' : 'label.editCarrier');
  }

  // convenience getter for easy access to form fields
  frmControlByName(
    name: string
  ) {
    if (this.mainForm?.controls) {
      return this.mainForm.controls[name];
    }
    return null;
  }

  /** easier to get form label by mode of transport */
  private get label() {
    if (this.model.modeOfTransport === ModeOfTransportType.Air) {
      return {
        carrierCode: 'label.airlineCode',
        carrierName: 'label.airlineName',
        carrierNumber: 'label.airlineNo'
      };
    }
    return {
      carrierCode: 'label.carrierCode',
      carrierName: 'label.carrierName',
      carrierNumber: 'label.carrierNo'
    };
  }

  ngOnDestroy() {
    this._subscriptions.map(x => x.unsubscribe());
  }

}
