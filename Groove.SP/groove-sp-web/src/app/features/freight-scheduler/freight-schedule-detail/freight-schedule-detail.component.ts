import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { faCalendar, faCalendarAlt, faMinus, faPencilAlt } from '@fortawesome/free-solid-svg-icons';
import { TranslateService } from '@ngx-translate/core';
import { DataStateChangeEvent, GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import { DataSourceRequestState, SortDescriptor, toDataSourceRequestString } from '@progress/kendo-data-query';
import { forkJoin, Observable } from 'rxjs';
import { DATE_FORMAT, DATE_HOUR_FORMAT_12, FormMode, ModeOfTransportType, StringHelper, UserContextService } from 'src/app/core';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { CarrierModel } from 'src/app/core/models/carrier.model';
import { DefaultValue2Hyphens, GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { LocationModel } from 'src/app/core/models/location.model';
import { CommonService } from 'src/app/core/services/common.service';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { environment } from 'src/environments/environment';
import { FreightSchedulerService } from '../freight-scheduler.service';
import { FreightSchedulerModel } from '../models/freight-scheduler.model';

@Component({
  selector: 'app-freight-schedule-detail',
  templateUrl: './freight-schedule-detail.component.html',
  styleUrls: ['./freight-schedule-detail.component.scss']
})
export class FreightScheduleDetailComponent implements OnInit {
  isOpenFreightSchedulerPopup: boolean;
  carriersModel: Array<CarrierModel> = [];
  locationsModel: Array<LocationModel> = [];
  schedulerPopupMode: FormMode = FormMode.View;
  freightSchedulerModel: FreightSchedulerModel;
  DATE_FORMAT = DATE_FORMAT;
  DATE_HOUR_FORMAT_12 = DATE_HOUR_FORMAT_12;
  readonly AppPermissions = AppPermissions;
  stringHelper = StringHelper;
  defaultValue = DefaultValue2Hyphens;
  faPencilAlt = faPencilAlt;
  faMinus = faMinus;
  faCalendar = faCalendarAlt;
  modeOfTransportType = ModeOfTransportType;

  model: FreightSchedulerModel = new FreightSchedulerModel();
  isInitDataLoaded: boolean = false;
  freightSchedulerId: number;

  // default values are for Sea
  literalLabels = {
    'carrierCode': 'label.scacCode',
    'carrierName': 'label.carrierName',
    'vesselFlight': 'label.vesselSlashVoyage',
    'locationFromName': 'label.loadingPort',
    'locationToName': 'label.dischargePort'
  };

  //#region Shipment Grid declarations
  isShipmentGridLoading: boolean = false;
  gridSort: SortDescriptor[] = [
    {
      field: 'shipmentNo',
      dir: 'asc'
    }
  ];

  private defaultGridState: DataSourceRequestState = {
    sort: this.gridSort,
    skip: 0,
    take: 20
  };

  gridState: DataSourceRequestState;

  public shipmentGridData: GridDataResult = {
    data: [],
    total: 0
  };

  public pageSizes: Array<number> = [20, 50, 100];
  public pagerType: 'numeric' | 'input' = 'numeric';
  public buttonCount: number = 9;
  //#endregion

  constructor(private route: ActivatedRoute,
    public translateService: TranslateService,
    private commonService: CommonService,
    private router: Router,
    private freightSchedulerService: FreightSchedulerService,
    private notification: NotificationPopup,
    private _httpService: HttpClient,
    private _gaService: GoogleAnalyticsService) {
  }

  ngOnInit() {
    this.getDataSourcesForPopup();
    this.gridState = { ...this.defaultGridState };
    this.route.params.subscribe(params => {
      this.freightSchedulerId = params['id'];
      const obs1$ = this.freightSchedulerService.getFreightScheduler(this.freightSchedulerId);
      const obs2$ = this.fetchShipmentGridData();
      forkJoin([obs1$, obs2$]).subscribe(
        (results: any) => {
          if (results[0] == null) {
            this.router.navigate(['/error/404']);
          }
          this.model = new FreightSchedulerModel(results[0]);
          this.freightSchedulerModel = new FreightSchedulerModel(results[0]);
          this.updateLiteralLabels(this.model.modeOfTransport);

          this.isInitDataLoaded = true;
        });
    });
  }

  getDataSourcesForPopup() {
    this.commonService.getCarriers().subscribe(response => {
      this.carriersModel = response;
    });

    this.commonService.getAllLocations().subscribe(response => {
      this.locationsModel = response;
    });
  }

  public fetchShipmentGridData(): Observable<any> {
    let url = `${environment.apiUrl}/freightSchedulers/${this.freightSchedulerId}/shipments?${toDataSourceRequestString(this.gridState)}`;
    this.isShipmentGridLoading = true;
    return this._httpService
      .get(`${url}`)
      .map(({ data, total }: GridDataResult) =>
      (<GridDataResult>{
        data: data,
        total: total
      }))
      .map((data) => {
        this.shipmentGridData = data;
        this.isShipmentGridLoading = false;
      });
  }

  public gridPageChange(event: PageChangeEvent): void {
    this.gridState.skip = event.skip;
    this.fetchShipmentGridData().subscribe();
  }

  public gridSortChange(sort: SortDescriptor[]): void {
    this.gridState.sort = sort;
    this.fetchShipmentGridData().subscribe();
  }

  gridStateChange(state: DataStateChangeEvent) {
    this.gridState = state;
    this.fetchShipmentGridData().subscribe();
  }

  public pageSizeChange(value: any): void {
    this.gridState.skip = 0;
    this.gridState.take = value;
    this.fetchShipmentGridData().subscribe();
  }

  onClickUpdateFS() {
    this.schedulerPopupMode = FormMode.Update;
    this.isOpenFreightSchedulerPopup = true;
  }

  onClickEditFS() {
    this.schedulerPopupMode = FormMode.Edit;
    this.isOpenFreightSchedulerPopup = true;
  }
  onCloseFreightSchedulerPopup() {
    this.isOpenFreightSchedulerPopup = false;
    this.freightSchedulerModel = Object.assign({}, this.model);
  }

  onSaveFreightSchedulerSuccess() {
    this.ngOnInit();
  }

  onClickDeleteFS() {
    const confirmDlg = this.notification.showConfirmationDialog(
      "delete.saveConfirmation",
      "label.freightSchedulers",
      false,
      true,
      475
    );
    confirmDlg.result.subscribe((result: any) => {
      if (result.value) {
        this.freightSchedulerService.deleteFreightScheduler(this.model.id).subscribe(
          r => {
            this.backList();
            this.notification.showSuccessPopup(
              'save.sucessNotification',
              'label.freightSchedulers'
            );
            this._gaService.emitAction('Delete', GAEventCategory.FreightSchedule);
          },
          err => {
            this.notification.showErrorPopup('save.failureNotification', 'label.freightSchedulers');
          }
        );
      }
    });
  }

  private updateLiteralLabels(modeOfTransport: string): void {
    if (!modeOfTransport){
      return;
    }
    switch(modeOfTransport.toLowerCase()) {
      case ModeOfTransportType.Sea.toLowerCase():
        this.literalLabels.carrierCode = 'label.scacCode';
        this.literalLabels.carrierName = 'label.carrierName';
        this.literalLabels.vesselFlight = 'label.vesselSlashVoyage';
        this.literalLabels.locationFromName = 'label.loadingPort';
        this.literalLabels.locationToName = 'label.dischargePort';
        break;
      case ModeOfTransportType.Air.toLowerCase():
        this.literalLabels.carrierCode = 'label.airlineCode';
        this.literalLabels.carrierName = 'label.airlineName';
        this.literalLabels.vesselFlight = 'label.flightNo';
        this.literalLabels.locationFromName = 'label.origin';
        this.literalLabels.locationToName = 'label.destination';
        break;
      default:
        this.literalLabels.carrierCode = 'label.scacCode';
        this.literalLabels.carrierName = 'label.carrierName';
        this.literalLabels.vesselFlight = 'label.vesselSlashVoyage';
        this.literalLabels.locationFromName = 'label.loadingPort';
        this.literalLabels.locationToName = 'label.dischargePort';
    }
  }

  get dateTimeFormat() {
    const isAirMode = StringHelper.caseIgnoredCompare(ModeOfTransportType.Air, this.model?.modeOfTransport);
    return isAirMode ? DATE_HOUR_FORMAT_12 : DATE_FORMAT;
  }

  backList() {
    this.router.navigate(['/freight-schedulers']);
  }
}
