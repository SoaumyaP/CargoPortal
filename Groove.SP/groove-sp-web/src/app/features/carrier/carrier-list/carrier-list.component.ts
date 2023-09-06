import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { faPencilAlt, faPlus, faPowerOff } from '@fortawesome/free-solid-svg-icons';
import { CarrierStatus, ColumnSetting, FormMode, ListComponent, ModeOfTransportType } from 'src/app/core';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { CarrierListService } from './carrier-list.service';
import { Location } from '@angular/common';
import { CarrierModel } from 'src/app/core/models/carrier.model';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';

@Component({
  selector: 'app-carrier-list',
  templateUrl: './carrier-list.component.html',
  styleUrls: ['./carrier-list.component.scss']
})
export class CarrierListComponent extends ListComponent implements OnInit {

  listName = 'carriers';
  columns: ColumnSetting[] = [
    {
      field: 'modeOfTransport',
      title: 'label.modeOfTransport',
      filter: 'text',
      width: '14%',
      sortable: true
    },
    {
      field: 'carrierCodeNumber',
      title: 'label.carrierCode',
      filter: 'text',
      width: '20%',
      sortable: true
    },
    {
      field: 'name',
      title: 'label.name',
      filter: 'text',
      width: '38%',
      sortable: true
    },
    {
      field: 'status',
      title: 'label.status',
      filter: 'text',
      width: '14%',
      sortable: true
    },
    {
      field: 'action',
      title: 'label.action',
      filter: 'text',
      width: '14%',
      sortable: false
    }
  ];

  readonly AppPermissions = AppPermissions;
  readonly carrierStatus = CarrierStatus;
  readonly modeOfTransport = ModeOfTransportType;
  faPlus = faPlus;
  faPencilAlt = faPencilAlt;
  faPowerOff = faPowerOff;

  carrierFormOpened = false;
  carrierFormMode = FormMode.Add;
  carrierFormModel: CarrierModel;

  constructor(
    public service: CarrierListService,
    public notification: NotificationPopup,
    route: ActivatedRoute,
    location: Location) {
    super(service, route, location);
  }

  onAddNewCarrier() {
    this.carrierFormMode = FormMode.Add;
    this.carrierFormOpened = true;
    this.carrierFormModel = new CarrierModel();
  }

  onEditCarrier(dataItem) {
    this.carrierFormMode = FormMode.Edit;
    this.carrierFormOpened = true;
    this.carrierFormModel = new CarrierModel();
    this.carrierFormModel.id = dataItem.id;
    this.carrierFormModel.carrierCode = dataItem.carrierCode;
    this.carrierFormModel.name = dataItem.name;
    this.carrierFormModel.modeOfTransport = dataItem.modeOfTransport;
    this.carrierFormModel.carrierNumber = dataItem.carrierNumber;
  }

  carrierAddedHandler(newCarrier: CarrierModel) {
    this.carrierFormOpened = false;
    this.service.createNewCarrier(newCarrier).subscribe(
      rsp => {
        this.notification.showSuccessPopup(
          'save.sucessNotification',
          'label.carriers');
        super.ngOnInit();
      },
      err => {
        this.notification.showErrorPopup(
          'save.failureNotification',
          'label.carriers');
      }
    )
  }

  carrierEditedHandler(model: CarrierModel) {
    this.carrierFormOpened = false;
    this.service.editCarrier(model).subscribe(
      rsp => {
        this.notification.showSuccessPopup(
          'save.sucessNotification',
          'label.carriers');
        super.ngOnInit();
      },
      err => {
        this.notification.showErrorPopup(
          'save.failureNotification',
          'label.carriers');
      }
    )
  }

  carrierFormClosedHandler() {
    this.carrierFormOpened = false;
  }

  ngOnInit() {
    super.ngOnInit();
  }

  onUpdateStatus(dataItem: CarrierModel, status: CarrierStatus) {
    var model = { ...dataItem };
    model.status = status;
    this.service.updateStatus(dataItem.id, model).subscribe(
      r => {
        this.notification.showSuccessPopup('save.sucessNotification', 'label.carrier');
        this.ngOnInit();
      },
      err => {
        this.notification.showErrorPopup('save.failureNotification', 'label.carrier');
      }
    );
  }
}
