import { Component, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges, ViewChild } from '@angular/core';
import { AbstractControl, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { DATE_FORMAT, ModeOfTransportType, StringHelper, UserContextService } from 'src/app/core';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';
import { LocalDate } from 'src/app/core/models/local-date.model';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { ShipmentModel } from '../../models/shipment.model';
import { BillOfLadingFormService } from '../bill-of-lading-form.service';
import { BillOfLadingModel } from '../models/bill-of-lading.model';

@Component({
  selector: 'app-bl-link-to-shipment-popup',
  templateUrl: './bl-link-to-shipment-popup.component.html',
  styleUrls: ['./bl-link-to-shipment-popup.component.scss']
})
export class BLLinkToShipmentPopupComponent implements OnInit, OnDestroy {
  @Input() isOpenLinkToShipmentPopup: boolean;
  @Input() currentUser: any = {};
  @Input() houseBLModel: BillOfLadingModel;

  @Output() onCloseLinkToShipmentPopup: EventEmitter<any> = new EventEmitter<any>();
  @Output() linkedHouseBLSuccessfully: EventEmitter<any> = new EventEmitter<any>();

  @ViewChild('mainForm', { static: false }) mainForm: NgForm;

  defaultValue: string = DefaultValue2Hyphens;
  dateFormat = DATE_FORMAT;
  shipmentModel: ShipmentModel = new ShipmentModel();
  filteredShipments: Array<ShipmentModel> = [];
  searchShipmentEvent$ = new Subject<string>();
  subscriptions = new Subscription();
  shipmentNo: string;
  StringHelper = StringHelper;
  
  constructor(
    private billOfLadingFormService: BillOfLadingFormService,
    private notification: NotificationPopup,
    private router: Router,
    private userContext: UserContextService,
  ) {
    this.userContext.getCurrentUser().subscribe(user => {
      if (user) {
        this.currentUser = user;
        if (!user.isInternal) {
          this.billOfLadingFormService.affiliateCodes = user.affiliates;
        }
      }
    });
   }

  ngOnInit() {
    this.handleSearchShipmentEvent();
  }

  onSelectShipment() {
    FormHelper.ValidateAllFields(this.mainForm);
    if (!(this.mainForm.valid && this.shipmentModel?.id)) {
      return;
    }

    this.billOfLadingFormService.linkHouseBLToShipment(this.shipmentModel.id, this.houseBLModel.id, this.houseBLModel.executionAgentId).subscribe(
      r => {
        this.notification.showSuccessPopup('save.sucessNotification', 'label.houseBillOfLading');
        this.resetLinkToShipmentPopup();
        this.linkedHouseBLSuccessfully.emit();
      },
      err => {
        this.notification.showErrorPopup('save.failureNotification', 'label.houseBillOfLading')
      }
    )
  }

  onChangeShipmentNo(value: string) {
    if (this.shipmentNo.length === 0) {
      this.shipmentModel = null;
    }
    else {
      const shipment = this.filteredShipments.find(c => c.shipmentNo === this.shipmentNo);
      if (shipment) {
        if (shipment.isConfirmContainer && StringHelper.caseIgnoredCompare(shipment.modeOfTransport,ModeOfTransportType.Sea)) {
          this.getControl('shipmentNo').setErrors({ 'isConfirmContainer': true });
          return;
        }
        if (shipment.isConfirmConsolidation && StringHelper.caseIgnoredCompare(shipment.modeOfTransport,ModeOfTransportType.Sea)) {
          this.getControl('shipmentNo').setErrors({ 'isConfirmConsolidation': true });
          return;
        }

        this.shipmentModel = { ...shipment };
      }
      else {
        this.shipmentModel = null;
      }
      this.getControl('shipmentNo').setErrors(null);
    }
  }

  handleSearchShipmentEvent() {
    const sub = this.searchShipmentEvent$.pipe(
      debounceTime(1000),
      tap((shipmentNo: string) => {
        this.onSearchShipments(shipmentNo);
      }
      )).subscribe();
    this.subscriptions.add(sub);
  }

  onSearchShipments(shipmentNo: string) {
    this.filteredShipments = [];

    if (shipmentNo?.length >= 3) {
      this.billOfLadingFormService.searchShipments(
        this.houseBLModel.id,
        shipmentNo,
        this.houseBLModel.modeOfTransport,
        this.houseBLModel.executionAgentId
      ).subscribe((c: any) => {
        this.filteredShipments = c;
      });
    }
  }

  onCancelLinkToShipmentPopup() {
    this.resetLinkToShipmentPopup();
    this.onCloseLinkToShipmentPopup.emit();
  }

  resetLinkToShipmentPopup() {
    this.shipmentModel = new ShipmentModel();
    this.getControl('shipmentNo').setErrors(null);
    this.shipmentNo = null;
  }

  getControl(controlName: string): AbstractControl {
    return this.mainForm?.form?.controls[controlName];
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
