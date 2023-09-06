import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ColumnSetting, FormMode, ListComponent, ModeOfTransportType, UserContextService } from 'src/app/core';
import { FreightSchedulerListService } from './freight-scheduler-list.service';
import { Location } from '@angular/common';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { faPencilAlt, faPlus, faInfo, faBars } from '@fortawesome/free-solid-svg-icons';
import { CommonService } from 'src/app/core/services/common.service';
import { CarrierModel } from 'src/app/core/models/carrier.model';
import { FreightSchedulerModel } from '../models/freight-scheduler.model';
import { LocationModel } from 'src/app/core/models/location.model';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';

@Component({
  selector: 'app-freight-scheduler-list',
  templateUrl: './freight-scheduler-list.component.html',
  styleUrls: ['./freight-scheduler-list.component.scss']
})
export class FreightSchedulerListComponent extends ListComponent implements OnInit {
  isOpenFreightSchedulerPopup: boolean;
  carriersModel: Array<CarrierModel> = [];
  locationsModel: Array<LocationModel> = [];
  schedulerPopupMode: FormMode = FormMode.View;
  freightSchedulerModel: FreightSchedulerModel;

  listName = 'freight-schedulers';
  columns: ColumnSetting[] = [
    {
      field: 'modeOfTransport',
      title: 'label.mode',
      filter: 'text',
      width: '6%',
      sortable: true
    },
    {
      field: 'allowUpdateFromExternal',
      title: 'label.allowApi',
      filter: 'text',
      width: '7%',
      sortable: true
    },
    {
      field: 'carrierName',
      title: 'label.carrier',
      filter: 'text',
      width: '15%',
      sortable: true
    },
    {
      field: 'vesselMAWB',
      title: 'label.vesselMAWB',
      filter: 'text',
      width: '24%',
      sortable: true
    },
    {
      field: 'locationFromName',
      title: 'label.shipFrom',
      filter: 'text',
      width: '11%',
      sortable: true
    },
    {
      field: 'locationToName',
      title: 'label.shipTo',
      filter: 'text',
      width: '11%',
      sortable: true
    },
    {
      field: 'etdDate',
      title: 'label.etdDates',
      filter: 'date',
      width: '11%',
      sortable: true
    },
    {
      field: 'etaDate',
      title: 'label.etaDates',
      filter: 'date',
      width: '11%',
      sortable: true
    },
    {
      field: 'action',
      title: 'label.action',
      filter: 'text',
      width: '5%',
      sortable: false
    }
  ];

  readonly AppPermissions = AppPermissions;
  faPlus = faPlus;
  faPencilAlt = faPencilAlt;
  faInfo = faInfo;
  faBars = faBars;
  ModeOfTransportType = ModeOfTransportType;

  constructor(
    private commonService: CommonService,
    public service: FreightSchedulerListService,
    route: ActivatedRoute,
    private router: Router,
    location: Location,
    private _userContext: UserContextService,
    public notification: NotificationPopup,
    public _translateService: TranslateService,
    private _gaService: GoogleAnalyticsService) {
    super(service, route, location);
    this._userContext.getCurrentUser().subscribe(user => {
      if (user) {
        if (!user.isInternal) {
          this.service.affiliates = user.affiliates;
          this.service.state = Object.assign({}, this.service.defaultState);
        }
      }
    });
  }

  ngOnInit() {
    super.ngOnInit();
    // Applied cache on requests, not problem to call many-times
    this.getDataSourcesForPopup();
  }

  getDataSourcesForPopup() {
    this.commonService.getCarriers().subscribe(response => {
      this.carriersModel = response;
    });

    this.commonService.getAllLocations().subscribe(response => {
      this.locationsModel = response;
    });
  }

  onAddNewFreightScheduler() {
    this.isOpenFreightSchedulerPopup = true;
    this.schedulerPopupMode = FormMode.Add;
  }

  onCloseFreightSchedulerPopup() {
    this.isOpenFreightSchedulerPopup = false;
  }

  onSaveFreightSchedulerSuccess() {
    super.ngOnInit();
  }

  onEditFreightScheduler(value) {
    this.freightSchedulerModel = Object.assign({}, value);
    this.schedulerPopupMode = FormMode.Edit;
    this.isOpenFreightSchedulerPopup = true;
  }

  onUpdateFreightScheduler(value) {
    this.freightSchedulerModel = Object.assign({}, value);
    this.schedulerPopupMode = FormMode.Update;
    this.isOpenFreightSchedulerPopup = true;
  }

  onDeleteFreightScheduler(id) {
    const confirmDlg = this.notification.showConfirmationDialog(
      "delete.saveConfirmation",
      "label.freightSchedulers",
      false,
      true,
      475
    );
    confirmDlg.result.subscribe((result: any) => {
      if (result.value) {
        this.service.deleteFreightScheduler(id).subscribe(
          success => {
            this._gaService.emitAction('Delete', GAEventCategory.FreightSchedule);
            this.notification.showSuccessPopup(
              "save.sucessNotification",
              "label.freightSchedulers");
            super.ngOnInit();
          }
          , err =>
            this.notification.showErrorPopup(
              "save.failureNotification",
              "label.freightSchedulers")
        );
      }
    });
  }

  /**
     * To render sub-menu for "More actions" button
     * @param dataItem Data of each freight scheduler item row
     * @returns Array of menu options
     */
  getMoreActionMenu(dataItem: any) {
    const result = [
      {
        actionName: this._translateService.instant('tooltip.view'),
        icon: 'eye',
        click: () => {
          this.router.navigate([`/freight-schedulers/schedule-detail/${dataItem.id}`]);
        }
      }
    ];

    if (this._userContext.currentUser) {
      const user = this._userContext.currentUser;
      const isEditAllowed = user.permissions.find(
          (s) => s.name === AppPermissions.FreightScheduler_List_Edit
      );
      if (isEditAllowed) {
        if (dataItem.hasLinkedItineraries) {
          result.push({
            actionName: this._translateService.instant('label.update'),
            icon: 'calendar',
            click: () => {
              this.onUpdateFreightScheduler(dataItem)
            }
          });
        } else {
          result.push({
            actionName: this._translateService.instant('label.edit'),
            icon: 'edit',
            click: () => {
              this.onEditFreightScheduler(dataItem);
            }
          });
          result.push({
            actionName: this._translateService.instant('tooltip.delete'),
            icon: 'trash',
            click: () => {
              this.onDeleteFreightScheduler(dataItem.id);
            }
          });
        }
      }
    }
    return result;
  }
}
