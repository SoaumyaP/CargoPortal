import { Component, Input, OnInit } from '@angular/core';
import { ControlContainer, NgForm } from '@angular/forms';
import { faInfoCircle } from '@fortawesome/free-solid-svg-icons';
import { EmailSettingType } from 'src/app/core';
import { MultipleEmailValidationPattern } from 'src/app/core/models/constants/app-constants';

@Component({
  selector: 'app-email-setting',
  templateUrl: './email-setting.component.html',
  styleUrls: ['./email-setting.component.scss'],
  viewProviders: [{ provide: ControlContainer, useExisting: NgForm }]
})
export class EmailSettingComponent implements OnInit {
  @Input() model: Array<EmailSettingModel>;
  @Input() readOnly: boolean;
  @Input() formErrors: any[];
  @Input() validationRules: any[];
  @Input() tabPrefix: string;

  faInfoCircle = faInfoCircle;
  patternEmail = MultipleEmailValidationPattern;

  noSystemEmail = [
    EmailSettingType.BookingImportviaAPI,
    EmailSettingType.BookingRejected,
    EmailSettingType.BookingCargoReceived
  ];

  defaultEmailToOwner = [
    EmailSettingType.BookingImportedFailure,
    EmailSettingType.BookingImportedSuccessfully,
    EmailSettingType.BookingConfirmed
  ];

  constructor() { }

  ngOnInit() {
  }

  onDefaultSendToChanged($event, rowIndex) {
    var dataItem = this.model[rowIndex];
    if (!dataItem.defaultSendTo && !this.noSystemEmail.includes(dataItem.emailType)) {
      this.validationRules[this.tabPrefix + 'sendTo_' + rowIndex] = {
        'pattern': 'label.sendTo',
        'required': 'label.sendTo'
      };
    }
    else {
      this.validationRules[this.tabPrefix + 'sendTo_' + rowIndex] = {
        'pattern': 'label.sendTo'
      };
    }
  }

  public getTooltipData(rowIndex: number): string {
    const dataItem = this.model[rowIndex];
    let rs = '';
    if (this.noSystemEmail.includes(dataItem.emailType)) {
      rs = 'tooltip.noSystemEmailYet';
    }
    else if (this.defaultEmailToOwner.includes(dataItem.emailType)) {
      rs = 'tooltip.defaultSendToOwner';
    }
    else if (dataItem.emailType === EmailSettingType.BookingApproval) {
      rs = 'tooltip.defaultSendToCustomer'
    }
    else if (dataItem.emailType === EmailSettingType.BookingApproved) {
      rs = 'tooltip.defaultSendToWarehouseContact'
    }

    return rs;
  }
}

export interface EmailSettingModel {
  id: number;
  emailType: EmailSettingType;
  emailTypeName: string;
  defaultSendTo: boolean;
  sendTo: string;
  cc: string;
  buyerComplianceId: number;
}