import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ColumnSetting, ListComponent } from 'src/app/core/list';
import { VesselListService } from './vessel-list.service';
import { Location } from '@angular/common';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { faPencilAlt, faPlus, faPowerOff } from '@fortawesome/free-solid-svg-icons';
import { FormMode, VesselStatus } from 'src/app/core';
import { Subscription } from 'rxjs/internal/Subscription';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { VesselModel } from '../models/vessel.model';
@Component({
  selector: 'app-vessel-list',
  templateUrl: './vessel-list.component.html',
  styleUrls: ['./vessel-list.component.scss']
})
export class VesselListComponent extends ListComponent implements OnInit, OnDestroy {

  listName = 'vessels';
  yesNoDropdown = [
    { text: 'label.yes', value: 1 },
    { text: 'label.no', value: 0 },
  ];
  columns: ColumnSetting[] = [
    {
      field: 'code',
      title: 'label.vesselCode',
      filter: 'text',
      width: '29%',
      sortable: true
    },
    {
      field: 'name',
      title: 'label.vesselName',
      filter: 'text',
      width: '29%',
      sortable: true
    },
    {
      field: 'isRealVessel',
      title: 'label.realVessel',
      filter: 'text',
      width: '15%',
      sortable: true
    },
    {
      field: 'status',
      title: 'label.status',
      filter: 'text',
      width: '15%',
      sortable: true
    },
    {
      field: 'action',
      title: 'label.action',
      filter: 'text',
      width: '12%',
      sortable: false
    }
  ];

  readonly AppPermissions = AppPermissions;
  readonly vesselStatus = VesselStatus;
  faPlus = faPlus;
  faPencilAlt = faPencilAlt;
  faPowerOff = faPowerOff;

  vesselFormOpened = false;
  vesselFormMode = FormMode.Add;
  vesselModel: VesselModel;

  private _subscriptions: Array<Subscription> = [];

  constructor(
    public service: VesselListService,
    route: ActivatedRoute,
    private notification: NotificationPopup,
    location: Location) {
    super(service, route, location);
  }

  onAddNewVessel() {
    this.vesselFormMode = FormMode.Add;
    this.vesselFormOpened = true;
    this.vesselModel = new VesselModel();
  }

  onEditVessel(dataItem) {
    this.vesselFormMode = FormMode.Edit;
    this.vesselFormOpened = true;
    this.vesselModel = { ...dataItem };
  }

  vesselFormClosedHandler(isSavedSuccessfully) {
    this.vesselFormOpened = false;
    if (isSavedSuccessfully) {
      this.ngOnInit();
    }
  }


  ngOnInit() {
    super.ngOnInit();
  }

  onUpdateStatus(dataItem: VesselModel, status: VesselStatus) {
    var model = { ...dataItem };
    model.status = status;
    const sub = this.service.updateStatus(dataItem.id, model).subscribe(
      r => {
        this.notification.showSuccessPopup('save.sucessNotification', 'label.vessel');
        this.ngOnInit();
      },
      err => {
        this.notification.showErrorPopup('save.failureNotification', 'label.vessel');
      }
    );
    this._subscriptions.push(sub);
  }

  ngOnDestroy(): void {
    this._subscriptions.map(x => x.unsubscribe());
  }
}