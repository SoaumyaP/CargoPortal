import { Component, OnInit, OnDestroy } from '@angular/core';
import { forkJoin, Observable, of, Subscription } from 'rxjs';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { UserStatus, UserContextService, DATE_FORMAT, StatisticKey, StringHelper, DropdownListModel, Roles, DashboardChartName } from 'src/app/core';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { HomeService } from './home.service';
import { faCaretUp, faCaretDown } from '@fortawesome/free-solid-svg-icons';
import { take, tap } from 'rxjs/operators';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { DefaultEnUsLang, GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { faLightbulb } from '@fortawesome/free-regular-svg-icons';
import { CookieService } from 'src/app/core/services/cookie.service';
import { TodayUpdateModel } from './models/today-update.model';
import { SurveyParticipantModel } from 'src/app/core/models/survey/survey-participant.model';
import { SurveyFormService } from '../survey/survey-form/survey-form.service';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, OnDestroy {
    private _subscriptions: Array<Subscription> = [];
    status = UserStatus;
    name = '';
    email = '';
    isAuthorizedSubscription: Subscription | undefined;
    isAuthorized = false;
    userStatus: number;

    userDataSubscription: Subscription | undefined;
    userData = false;
    isExecute = false;
    isDisableResubmit = false;
    loginType = '';
    currentUser: any;
    readonly AppPermissions = AppPermissions;
    readonly StatisticKey = StatisticKey;

    todayUpdate: TodayUpdateModel = new TodayUpdateModel();
    top5OceanVolumeByOrigin = null;
    isLoadingTop5OceanVolumeByOrigin = null;
    top5OceanVolumeByDestination = null;
    isLoadingTop5OceanVolumeByDestination = null;
    top10ConsigneeThisWeek = null;
    isLoadingTop10ConsigneeThisWeek = false;
    top10ShipperThisWeek = null;
    isLoadingTop10ShipperThisWeek = false;
    top10CarrierThisWeek = null;
    isLoadingTop10CarrierThisWeek = null;
    monthlyOceanVolumeByMovement = null;
    isLoadingMonthlyOceanVolumeByMovement = null;
    monthlyOceanVolumeByServiceType = null;
    isLoadingMonthlyOceanVolumeByServiceType = false;
    faCaretUp = faCaretUp;
    faLightbulb = faLightbulb;
    faCaretDown = faCaretDown;

    weekShipments = null;
    isLoadingWeekShipments = false;
    weekCustomerPO = null;
    isLoadingWeekCustomerPO = false;

    weekOceanVolumeByMovements = null;
    isLoadingWeekOceanVolumeByMovements = false;
    thisWeekOceanVolumeRange: string;
    lastMonthOceanVolumeByMovementRange: string;
    lastMonthOceanVolumeByServiceTypeRange: string;
    DATE_FORMAT = DATE_FORMAT;

    bookedPOStatistics = null;
    isLoadingbookedPOStatistics = false;
    unbookedPOStatistics = null;
    isLoadingUnbookedPOStatistics = false;
    inOriginDC = null;
    isLoadingInOriginDC = false;
    inTransit = null;
    isLoadingInTransit = null;
    customsCleared = null;
    isLoadingCustomsCleared = false;
    shipmentPendingDCDelivery = null;
    pendingDCDelivery = null;
    isLoadingPendingDCDelivery = false;
    // pendingDCDelivery = null;
    // isLoadingPendingDCDelivery = false;
    dcDeliveryConfirmed = null;
    isLoadingDCDeliveryConfirmed = false;
    dcDeliveryPending = null;
    managedToDatePOStatistic = null;
    isLoadingManagedToDatePOStatistic = false;
    isShowManageYTD_PendingDC = true;
    vesselArrival = null;
    isloadingVesselArrival = false;

    unbookedAndBookedDropdown: DropdownListModel<string>[] = [];
    unbookedAndBookedDropdownModel: string;
    shipmentStatusDropdown: DropdownListModel<string>[] = [];
    vesselArrivalDropdown: DropdownListModel<string>[] = [];

    inOriginDCAndInTransitDropdownModel: string;
    vesselArrivalDropdownModel: string;
    customsClearedAndPendingDCDeliveryDropdownModel: string;

    shipmentPendingDCDropdown: DropdownListModel<string>[] = [];
    shipmentPendingDCDropdownModel: string;

    managedYTDDropdown: DropdownListModel<string>[] = [];
    managedYTDDropdownModel: string;

    shipmentDropdown: DropdownListModel<string>[] = [];
    shipmentDropdownModel: string;

    poIssuedDropdown: DropdownListModel<string>[] = [];
    poIssuedDropdownModel: string;

    shipmentVolumeDropdown: DropdownListModel<string>[] = [];
    shipmentVolumeDropdownModel: string;

    top5ShipmentOriginDropdown: DropdownListModel<string>[] = [];
    top5ShipmentOriginDropdownModel: string;

    top5ShipmentDestinationDropdown: DropdownListModel<string>[] = [];
    top5ShipmentDestinationDropdownModel: string;

    shipmentVolumeByMovementDropdown: DropdownListModel<string>[] = [];
    shipmentVolumeByMovementDropdownModel: string;

    shipmentVolumeByServiceTypeDropdown: DropdownListModel<string>[] = [];
    shipmentVolumeByServiceTypeDropdownModel: string;

    top10ConsigneeDropdown: DropdownListModel<string>[] = [];
    top10ConsigneeDropdownModel: string;

    top10ShipperDropdown: DropdownListModel<string>[] = [];
    top10ShipperDropdownModel: string;

    top10CarrierDropdown: DropdownListModel<string>[] = [];
    top10CarrierDropdownModel: string;
    dashboardChartName = DashboardChartName;

    constructor(
        private datePipe: DatePipe,
        public oidcSecurityService: OidcSecurityService,
        private service: HomeService,
        private _userContext: UserContextService,
        private notification: NotificationPopup,
        private _router: Router,
        private _cookieService: CookieService,
        private _surveyFormService: SurveyFormService,
        private _gaService: GoogleAnalyticsService) {
        this.initDashboardFilters();

    }

    initDashboardFilters() {
        this.unbookedAndBookedDropdown = [
            {
                label: 'label.thisWeek',
                value: 'This week'
            },
            {
                label: 'label.next14Days',
                value: 'Next 14 days'
            },
            {
                label: 'label.thisMonth',
                value: 'This month'
            },
            {
                label: 'label.thisYear',
                value: 'This year'
            }
        ];
        this.unbookedAndBookedDropdownModel = this.unbookedAndBookedDropdown[1].value;

        this.shipmentStatusDropdown = [
            {
                label: 'label.all',
                value: 'All'
            },
            {
                label: 'label.thisMonth',
                value: 'This month'
            },
            {
                label: 'label.thisYear',
                value: 'This year'
            }
        ];
        this.customsClearedAndPendingDCDeliveryDropdownModel = this.shipmentStatusDropdown[0].value;
        this.inOriginDCAndInTransitDropdownModel = this.shipmentStatusDropdown[0].value;

        this.vesselArrivalDropdown = [
            {
                label: 'label.next3Days',
                value: 'Next 3 days'
            },
            {
                label: 'label.next4Days',
                value: 'Next 4 days'
            },
            {
                label: 'label.next5Days',
                value: 'Next 5 days'
            },
            {
                label: 'label.next6Days',
                value: 'Next 6 days'
            },
            {
                label: 'label.next7Days',
                value: 'Next 7 days'
            }
        ];
        this.vesselArrivalDropdownModel = this.vesselArrivalDropdown[0].value;

        this.shipmentPendingDCDropdown = [
            {
                label: 'label.all',
                value: 'All'
            },
            {
                label: 'label.thisMonth',
                value: 'This month'
            },
            {
                label: 'label.thisYear',
                value: 'This year'
            }
        ];
        this.shipmentPendingDCDropdownModel = this.shipmentPendingDCDropdown[0].value;

        this.managedYTDDropdown = [
            {
                label: 'label.thisYear',
                value: 'This year'
            },
            {
                label: 'label.lastYear',
                value: 'Last year'
            }
        ];
        this.managedYTDDropdownModel = this.managedYTDDropdown[0].value;

        this.shipmentDropdown = [
            {
                label: 'label.last7Days',
                value: 'Last 7 days'
            },
            {
                label: 'label.last14Days',
                value: 'Last 14 days'
            },
            {
                label: 'label.last30Days',
                value: 'Last 30 days'
            },
            {
                label: 'label.lastMonth',
                value: 'Last month'
            },
            {
                label: 'label.lastYear',
                value: 'Last year'
            }
        ];
        this.shipmentDropdownModel = this.shipmentDropdown[0].value;

        this.poIssuedDropdown = [
            {
                label: 'label.last7Days',
                value: 'Last 7 days'
            },
            {
                label: 'label.last14Days',
                value: 'Last 14 days'
            },
            {
                label: 'label.last30Days',
                value: 'Last 30 days'
            },
            {
                label: 'label.lastMonth',
                value: 'Last month'
            },
            {
                label: 'label.lastYear',
                value: 'Last year'
            }
        ];
        this.poIssuedDropdownModel = this.poIssuedDropdown[0].value;

        this.shipmentVolumeDropdown = [
            {
                label: 'label.last7Days',
                value: 'Last 7 days'
            },
            {
                label: 'label.last14Days',
                value: 'Last 14 days'
            },
            {
                label: 'label.last30Days',
                value: 'Last 30 days'
            },
            {
                label: 'label.lastMonth',
                value: 'Last month'
            },
            {
                label: 'label.lastYear',
                value: 'Last year'
            }
        ];
        this.shipmentVolumeDropdownModel = this.shipmentVolumeDropdown[0].value;

        this.top5ShipmentOriginDropdown = [
            {
                label: 'label.lastMonth',
                value: 'Last month'
            },
            {
                label: 'label.lastYear',
                value: 'Last year'
            }
        ];
        this.top5ShipmentOriginDropdownModel = this.top5ShipmentOriginDropdown[0].value;

        this.top5ShipmentDestinationDropdown = [
            {
                label: 'label.lastMonth',
                value: 'Last month'
            },
            {
                label: 'label.lastYear',
                value: 'Last year'
            }
        ];
        this.top5ShipmentDestinationDropdownModel = this.top5ShipmentDestinationDropdown[0].value;

        this.shipmentVolumeByMovementDropdown = [
            {
                label: 'label.lastMonth',
                value: 'Last month'
            },
            {
                label: 'label.lastYear',
                value: 'Last year'
            }
        ];
        this.shipmentVolumeByMovementDropdownModel = this.shipmentVolumeByMovementDropdown[0].value;

        this.shipmentVolumeByServiceTypeDropdown = [
            {
                label: 'label.lastMonth',
                value: 'Last month'
            },
            {
                label: 'label.lastYear',
                value: 'Last year'
            }
        ];
        this.shipmentVolumeByServiceTypeDropdownModel = this.shipmentVolumeByServiceTypeDropdown[0].value;

        this.top10ConsigneeDropdown = [
            {
                label: 'label.last7Days',
                value: 'Last 7 days'
            },
            {
                label: 'label.lastMonth',
                value: 'Last month'
            },
            {
                label: 'label.lastYear',
                value: 'Last year'
            }
        ];
        this.top10ConsigneeDropdownModel = this.top10ConsigneeDropdown[0].value;

        this.top10ShipperDropdown = [
            {
                label: 'label.last7Days',
                value: 'Last 7 days'
            },
            {
                label: 'label.lastMonth',
                value: 'Last month'
            },
            {
                label: 'label.lastYear',
                value: 'Last year'
            }
        ];
        this.top10ShipperDropdownModel = this.top10ShipperDropdown[0].value;

        this.top10CarrierDropdown = [
            {
                label: 'label.last7Days',
                value: 'Last 7 days'
            },
            {
                label: 'label.lastMonth',
                value: 'Last month'
            },
            {
                label: 'label.lastYear',
                value: 'Last year'
            }
        ];
        this.top10CarrierDropdownModel = this.top10CarrierDropdown[0].value;
    }

    onChangeUnbookedAndBooked() {
        this.isLoadingUnbookedPOStatistics = true;
        let sub = this.service.getUnbookedPOStatistics(this.unbookedAndBookedDropdownModel).subscribe(
            data => {
                this.unbookedPOStatistics = data;
                this.isLoadingUnbookedPOStatistics = false;
            },
            () => {
                this.isLoadingUnbookedPOStatistics = false;
            }
        );

        this._subscriptions.push(sub);

        this.isLoadingbookedPOStatistics = true;
        sub = this.service.getBookedPOStatistics(this.unbookedAndBookedDropdownModel).subscribe(
            data => {
                this.bookedPOStatistics = data;
                this.isLoadingbookedPOStatistics = false;
            },
            () => {
                this.isLoadingbookedPOStatistics = false;
            }
        );
        this._subscriptions.push(sub);
    }

    onChangeInOriginDCAndInTransitDropdown() {
        this.isLoadingInOriginDC = true;
        let sub = this.service.getInOriginDCPOStatistics(this.inOriginDCAndInTransitDropdownModel).subscribe(
            data => {
                this.inOriginDC = data;
                this.isLoadingInOriginDC = false;
            },
            () => {
                this.isLoadingInOriginDC = false;
            }
        ); this._subscriptions.push(sub);

        this.isLoadingInTransit = true;
        sub = this.service.getInTransitPOStatistics(this.inOriginDCAndInTransitDropdownModel).subscribe(
            data => {
                this.inTransit = data;
                this.isLoadingInTransit = false;
            },
            () => {
                this.isLoadingInTransit = false;
            }
        ); this._subscriptions.push(sub);
    }

    onchangeVesselArrivalDropdown() {
        this.isloadingVesselArrival = true;
        let sub = this.service.getVesselArrivalStatistics(this.vesselArrivalDropdownModel).subscribe(
            data => {
                this.vesselArrival = data;
                this.isloadingVesselArrival = false;
            },
            () => {
                this.isloadingVesselArrival = false;
            }
        ); this._subscriptions.push(sub);
    }

    onChangeShipmentStatusDropdown() {
        this.isLoadingCustomsCleared = true;
        let sub = this.service.getCustomsClearedPOStatistics(this.customsClearedAndPendingDCDeliveryDropdownModel).subscribe(
            data => {
                this.customsCleared = data;
                this.isLoadingCustomsCleared = false;
            },
            () => {
                this.isLoadingCustomsCleared = false;
            }
        ); this._subscriptions.push(sub);

        this.isLoadingPendingDCDelivery = true;
        sub = this.service.getPendingDCDeliveryPOStatistics(this.customsClearedAndPendingDCDeliveryDropdownModel).subscribe(
            data => {
                this.shipmentPendingDCDelivery = data;
                this.isLoadingPendingDCDelivery = false;
            },
            () => {
                this.isLoadingPendingDCDelivery = false;
            }
        ); this._subscriptions.push(sub);
    }

    onChangeManagedYTDDropdown() {
        this.isLoadingManagedToDatePOStatistic = true;

        let sub = this.service.getManagedToDatePOStatistics(this.managedYTDDropdownModel).subscribe(
            data => {
                this.managedToDatePOStatistic = data;
                this.isLoadingManagedToDatePOStatistic = false;
            },
            () => {
                this.isLoadingManagedToDatePOStatistic = false;
            }
        ); this._subscriptions.push(sub);
    }

    onChangeShipmentPendingDCDropdown() {
        this.isLoadingDCDeliveryConfirmed = true;
        const $obs1 = this.service.getPendingDCDeliveryPOStatistics(this.shipmentPendingDCDropdownModel).pipe(
            tap(data => {
                this.pendingDCDelivery = data;
            })
        );
        const $obs2 = this.service.getDCDeliveryConfirmedPOStatistics(this.shipmentPendingDCDropdownModel).pipe(
            tap(data => {
                this.dcDeliveryConfirmed = data;
            })
        );
        forkJoin([$obs1, $obs2])
            .pipe(
                tap((res) => {
                    const pendingDCDelivery: any = res[0];
                    const dcDeliveryConfirmed: any = res[1];
                    this.dcDeliveryPending = pendingDCDelivery?.total - dcDeliveryConfirmed?.total;
                })
            ).subscribe(
                data => {
                    this.isLoadingDCDeliveryConfirmed = false;
                },
                () => {
                    this.isLoadingDCDeliveryConfirmed = false;
                }
            );
    }

    onChangeShipmentDropdownModel() {
        this.isLoadingWeekShipments = true;
        let sub = this.service.getWeeklyReportingShipments(this.shipmentDropdownModel).subscribe(
            data => {
                this.weekShipments = {
                    currentValue: data.thisWeekTotal,
                    prevValue: data.lastWeekTotal,
                    thisWeekRange: `${this.datePipe.transform(data.thisWeekStartDate, DATE_FORMAT)} to ${this.datePipe.transform(data.nextWeekStartDate, DATE_FORMAT)}`,
                    lastWeekRange: `${this.datePipe.transform(data.lastWeekStartDate, DATE_FORMAT)} to ${this.datePipe.transform(data.thisWeekStartDate.setDate(data.thisWeekStartDate.getDate() - 1), DATE_FORMAT)}`,
                    delta: {
                        value: this.getDelta(data.thisWeekTotal, data.lastWeekTotal),
                        up: data.thisWeekTotal >= data.lastWeekTotal
                    },
                    hasData: data.thisWeekTotal > 0 || data.lastWeekTotal > 0
                };
                this.isLoadingWeekShipments = false;
            },
            () => {
                this.isLoadingWeekShipments = false;
            }
        ); this._subscriptions.push(sub);
    }

    onChangePOIssuedDropdown() {
        this.isLoadingWeekCustomerPO = true;
        const sub = this.service.getWeeklyReportingPOs(this.poIssuedDropdownModel).subscribe(
            data => {
                this.weekCustomerPO = {
                    currentValue: data.thisWeekTotalPOs,
                    prevValue: data.lastWeekTotalPOs,
                    thisWeekRange: `${this.datePipe.transform(data.thisWeekStartDate, DATE_FORMAT)} to ${this.datePipe.transform(data.nextWeekStartDate, DATE_FORMAT)}`,
                    lastWeekRange: `${this.datePipe.transform(data.lastWeekStartDate, DATE_FORMAT)} to ${this.datePipe.transform(data.thisWeekStartDate.setDate(data.thisWeekStartDate.getDate() - 1), DATE_FORMAT)}`,
                    delta: {
                        value: this.getDelta(data.thisWeekTotalPOs, data.lastWeekTotalPOs),
                        up: data.thisWeekTotalPOs >= data.lastWeekTotalPOs
                    },
                    hasData: data.thisWeekTotalPOs > 0 || data.lastWeekTotalPOs > 0
                };
                this.isLoadingWeekCustomerPO = false;
            },
            () => {
                this.isLoadingWeekCustomerPO = false;
            }
        ); this._subscriptions.push(sub);
    }

    onChangeShipmentVolumeDropdown() {
        this.isLoadingWeekOceanVolumeByMovements = true;
        const sub = this.service.getWeeklyReportingOceanVolume(this.shipmentVolumeDropdownModel).subscribe(
            data => {
                this.weekOceanVolumeByMovements = [];
                this.thisWeekOceanVolumeRange = `${this.datePipe.transform(data.thisWeekStartDate, DATE_FORMAT)} to ${this.datePipe.transform(data.nextWeekStartDate, DATE_FORMAT)}`;
                data?.reportingMetricOceanVolumeByMovement?.forEach(item => {
                    this.weekOceanVolumeByMovements.push(
                        {
                            currentValue: item.thisWeekTotal,
                            prevValue: item.lastWeekTotal,
                            category: item.category,
                            unit: item.unit,
                            delta: {
                                value: this.getDelta(item.thisWeekTotal, item.lastWeekTotal),
                                up: item.thisWeekTotal >= item.lastWeekTotal
                            },
                            hasData: item.thisWeekTotal > 0 || item.lastWeekTotal > 0
                        }
                    );
                });
                this.isLoadingWeekOceanVolumeByMovements = false;
            },
            () => {
                this.isLoadingWeekOceanVolumeByMovements = false;
            }
        ); this._subscriptions.push(sub);
    }

    onChangeTop5ShipmentOriginDropdown() {
        this.isLoadingTop5OceanVolumeByOrigin = true;

        const sub = this.service.getTop5OceanVolumeByOrigin(this.top5ShipmentOriginDropdownModel).subscribe(
            data => {
                this.top5OceanVolumeByOrigin = data;
                this.isLoadingTop5OceanVolumeByOrigin = false;
            },
            () => {
                this.isLoadingTop5OceanVolumeByOrigin = false;
            }
        ); this._subscriptions.push(sub);
    }


    onChangeTop5ShipmentDestinationDropdown() {
        this.isLoadingTop5OceanVolumeByDestination = true;

        const sub = this.service.getTop5OceanVolumeByDestination(this.top5ShipmentDestinationDropdownModel).subscribe(
            data => {
                this.top5OceanVolumeByDestination = data;
                this.isLoadingTop5OceanVolumeByDestination = false;
            },
            () => {
                this.isLoadingTop5OceanVolumeByDestination = false;
            }
        ); this._subscriptions.push(sub);
    }

    onChangeShipmentVolumeByMovementDropdown() {
        this.isLoadingMonthlyOceanVolumeByMovement = true;

        const sub = this.service.getMonthlyOceanVolumeByMovement(this.shipmentVolumeByMovementDropdownModel).subscribe(
            (data: any) => {
                const total = data?.reportingPieCharts.reduce((a, c) => a += c.value, 0);
                this.monthlyOceanVolumeByMovement = data?.reportingPieCharts.map(a => {
                    return {
                        category: a.category,
                        value: a.value,
                        percentage: a.value / total
                    };
                });
                this.lastMonthOceanVolumeByMovementRange = `${this.datePipe.transform(data.lastMonthStartDate, DATE_FORMAT)} to ${this.datePipe.transform(data.thisMonthStartDate, DATE_FORMAT)}`;
                this.isLoadingMonthlyOceanVolumeByMovement = false;
            },
            () => {
                this.isLoadingMonthlyOceanVolumeByMovement = false;
            }
        );
        this._subscriptions.push(sub);
    }

    onChangeShipmentVolumeByServiceTypeDropdown() {
        this.isLoadingMonthlyOceanVolumeByServiceType = true;

        const sub = this.service.getMonthlyOceanVolumeByServiceType(this.shipmentVolumeByServiceTypeDropdownModel).subscribe(
            (data: any) => {
                const total = data?.reportingPieCharts.reduce((a, c) => a += c.value, 0);
                this.monthlyOceanVolumeByServiceType = data?.reportingPieCharts.map(a => {
                    return {
                        category: a.category,
                        value: a.value,
                        percentage: a.value / total
                    };
                });
                this.isLoadingMonthlyOceanVolumeByServiceType = false;
                this.lastMonthOceanVolumeByServiceTypeRange = `${this.datePipe.transform(data.lastMonthStartDate, DATE_FORMAT)} to ${this.datePipe.transform(data.thisMonthStartDate, DATE_FORMAT)}`;
            },
            () => {
                this.isLoadingMonthlyOceanVolumeByServiceType = false;
            }
        );
        this._subscriptions.push(sub);
    }

    onChangeTop10ConsigneeDropdown() {
        this.isLoadingTop10ConsigneeThisWeek = true;

        this.service.getTop10ConsigneeThisWeek(this.top10ConsigneeDropdownModel).subscribe(
            data => {
                this.top10ConsigneeThisWeek = data;
                this.isLoadingTop10ConsigneeThisWeek = false;
            },
            () => {
                this.isLoadingTop10ConsigneeThisWeek = false;
            }
        );
    }

    onChangeTop10ShipperDropdown() {
        this.isLoadingTop10ShipperThisWeek = true;
        this.service.getTop10ShipperThisWeek(this.top10ShipperDropdownModel).subscribe(
            data => {
                this.top10ShipperThisWeek = data;
                this.isLoadingTop10ShipperThisWeek = false;
            },
            () => {
                this.isLoadingTop10ShipperThisWeek = false;
            }
        );
    }

    onChangeTop10CarrierDropdown() {
        this.isLoadingTop10CarrierThisWeek = true;
        this.service.getTop10CarrierThisWeek(this.top10CarrierDropdownModel).subscribe(
            data => {
                this.top10CarrierThisWeek = data;
                this.isLoadingTop10CarrierThisWeek = false;
            },
            () => {
                this.isLoadingTop10CarrierThisWeek = false;
            }
        );
    }

    async loadCharts() {

        // Get data for Updates for Today section then show/hide correctly
        let bookingAwaitingForSubmission$: Observable<number>;
        let bookingPendingForApproval$: Observable<number>;
        let shortship$: Observable<number>;
        let mobileTodayUpdate$: Observable<any>;
        let surveySentToYou$: Observable<SurveyParticipantModel[]>;
        surveySentToYou$ = this._surveyFormService.getSurveyParticipants();

        if (this.currentUser?.role?.id === Roles.CSR || this.currentUser?.role?.id === Roles.Shipper) {
            bookingAwaitingForSubmission$ = this.service.GetBookingAwaitingForSubmission(this.currentUser.organizationId || 0, this.currentUser.role.name);
        } else {
            bookingAwaitingForSubmission$ = of(0);
        }

        if (this.currentUser?.role?.id === Roles.CSR || this.currentUser?.role?.id === Roles.Principal) {
            bookingPendingForApproval$ = this.service.GetBookingPendingForApproval();
        } else {
            bookingPendingForApproval$ = of(0);
        }

        if (this.currentUser?.role?.id === Roles.CSR || this.currentUser?.role?.id === Roles.System_Admin || this.currentUser?.role?.id === Roles.Principal) {
            shortship$ = this.service.getShortships(this.currentUser.organizationId || 0);
        } else {
            shortship$ = of(0);
        }

        // If user is warehouse, call request to server
        if (this.currentUser?.role?.id === Roles.Warehouse) {
            mobileTodayUpdate$ = this.service.GetMobileTodayUpdate();
        } else {
            // Else, set to null
            mobileTodayUpdate$ = of(null);
        }

        // Get data for [Updates for Today] section then show/hide correctly
        const sub = forkJoin([bookingAwaitingForSubmission$, bookingPendingForApproval$, mobileTodayUpdate$, surveySentToYou$, shortship$]).subscribe(
            (data) => {
                // map data for each part
                this.todayUpdate.totalAwaitingForSubmission = data[0];
                this.todayUpdate.totalPendingForApproval = data[1];
                this.todayUpdate.surveyParticipants = data[3];
                this.todayUpdate.shortShip = data[4];

                const cookieSurveyIds = this._cookieService.getCookie(TodayUpdateModel.surveysNotificationDismissCookieName)?.split(',') ?? [];
                if (cookieSurveyIds.length > 0) {
                    this.todayUpdate.surveyParticipants = data[3].filter(c => !cookieSurveyIds.some(s => +s === c.surveyId));
                }

                // If user is not warehouse, hide information
                if (data[2] == null) {
                    this.todayUpdate.mobile_notificationDismissed = true;
                } else {
                    this.todayUpdate.mobile_version = data[2].version;
                    this.todayUpdate.mobile_isNewRelease = data[2].isNewRelease;
                    this.todayUpdate.mobile_packageUrl = data[2].packageUrl;
                    this.todayUpdate.mobile_notificationDismissed = (data[2].version === this._cookieService.getCookie(TodayUpdateModel.mobileNotificationDismissCookieName));
                }
            });
        this._subscriptions.push(sub);

        // PO Statistics
        // End-to-end shipment status
        let isAllowed = await this._userContext.isGranted(AppPermissions.Dashboard_EndToEndShipmentStatus).toPromise();
        if (isAllowed) {
            this.service.getBookedPOStatistics(this.unbookedAndBookedDropdownModel).subscribe(data => {
                this.bookedPOStatistics = data;
            });
            this.service.getUnbookedPOStatistics(this.unbookedAndBookedDropdownModel).subscribe(data => {
                this.unbookedPOStatistics = data;
            });
            this.service.getManagedToDatePOStatistics(this.managedYTDDropdownModel).subscribe(data => {
                this.managedToDatePOStatistic = data;
            });
            this.service.getInOriginDCPOStatistics(this.customsClearedAndPendingDCDeliveryDropdownModel).subscribe(data => {
                this.inOriginDC = data;
            });
            this.service.getInTransitPOStatistics(this.customsClearedAndPendingDCDeliveryDropdownModel).subscribe(data => {
                this.inTransit = data;
            });
            this.service.getVesselArrivalStatistics(this.vesselArrivalDropdownModel).subscribe(data => {
                this.vesselArrival = data;
            });
            this.service.getCustomsClearedPOStatistics(this.customsClearedAndPendingDCDeliveryDropdownModel).subscribe(data => {
                this.customsCleared = data;
            });
            this.service.getPendingDCDeliveryPOStatistics(this.customsClearedAndPendingDCDeliveryDropdownModel).subscribe(data => {
                this.shipmentPendingDCDelivery = data;
            });

            const $obs1 = this.service.getPendingDCDeliveryPOStatistics(this.shipmentPendingDCDropdownModel).pipe(
                tap(data => {
                    this.pendingDCDelivery = data;
                })
            );
            const $obs2 = this.service.getDCDeliveryConfirmedPOStatistics(this.shipmentPendingDCDropdownModel).pipe(
                tap(data => {
                    this.dcDeliveryConfirmed = data;
                })
            );
            forkJoin($obs1, $obs2)
                .pipe(
                    tap((res) => {
                        const pendingDCDelivery: any = res[0];
                        const dcDeliveryConfirmed: any = res[1];
                        this.dcDeliveryPending = pendingDCDelivery?.total - dcDeliveryConfirmed?.total;
                    })
                ).subscribe();
        }

        isAllowed = await this._userContext.isGranted(AppPermissions.Dashboard_ThisWeekShipments).toPromise();
        if (isAllowed) {
            this.service.getWeeklyReportingShipments(this.shipmentDropdownModel).subscribe(
                data => {
                    this.weekShipments = {
                        currentValue: data.thisWeekTotal,
                        prevValue: data.lastWeekTotal,
                        thisWeekRange: `${this.datePipe.transform(data.thisWeekStartDate, DATE_FORMAT)} to ${this.datePipe.transform(data.nextWeekStartDate, DATE_FORMAT)}`,
                        lastWeekRange: `${this.datePipe.transform(data.lastWeekStartDate, DATE_FORMAT)} to ${this.datePipe.transform(data.thisWeekStartDate.setDate(data.thisWeekStartDate.getDate() - 1), DATE_FORMAT)}`,
                        delta: {
                            value: this.getDelta(data.thisWeekTotal, data.lastWeekTotal),
                            up: data.thisWeekTotal >= data.lastWeekTotal
                        },
                        hasData: data.thisWeekTotal > 0 || data.lastWeekTotal > 0
                    };
                }
            );
        }

        isAllowed = await this._userContext.isGranted(AppPermissions.Dashboard_ThisWeekCustomerPO).toPromise();
        if (isAllowed) {
            this.service.getWeeklyReportingPOs(this.poIssuedDropdownModel).subscribe(
                data => {
                    this.weekCustomerPO = {
                        currentValue: data.thisWeekTotalPOs,
                        prevValue: data.lastWeekTotalPOs,
                        thisWeekRange: `${this.datePipe.transform(data.thisWeekStartDate, DATE_FORMAT)} to ${this.datePipe.transform(data.nextWeekStartDate, DATE_FORMAT)}`,
                        lastWeekRange: `${this.datePipe.transform(data.lastWeekStartDate, DATE_FORMAT)} to ${this.datePipe.transform(data.thisWeekStartDate.setDate(data.thisWeekStartDate.getDate() - 1), DATE_FORMAT)}`,
                        delta: {
                            value: this.getDelta(data.thisWeekTotalPOs, data.lastWeekTotalPOs),
                            up: data.thisWeekTotalPOs >= data.lastWeekTotalPOs
                        },
                        hasData: data.thisWeekTotalPOs > 0 || data.lastWeekTotalPOs > 0
                    };
                }
            );
        }

        isAllowed = await this._userContext.isGranted(AppPermissions.Dashboard_ThisWeekOceanVolume).toPromise();
        if (isAllowed) {
            this.service.getWeeklyReportingOceanVolume(this.shipmentVolumeDropdownModel).subscribe(
                data => {
                    this.weekOceanVolumeByMovements = [];
                    this.thisWeekOceanVolumeRange = `${this.datePipe.transform(data.thisWeekStartDate, DATE_FORMAT)} to ${this.datePipe.transform(data.nextWeekStartDate, DATE_FORMAT)}`;
                    data?.reportingMetricOceanVolumeByMovement?.forEach(item => {
                        this.weekOceanVolumeByMovements.push(
                            {
                                currentValue: item.thisWeekTotal,
                                prevValue: item.lastWeekTotal,
                                category: item.category,
                                unit: item.unit,
                                delta: {
                                    value: this.getDelta(item.thisWeekTotal, item.lastWeekTotal),
                                    up: item.thisWeekTotal >= item.lastWeekTotal
                                },
                                hasData: item.thisWeekTotal > 0 || item.lastWeekTotal > 0
                            }
                        );
                    });
                });
        }

        isAllowed = await this._userContext.isGranted(AppPermissions.Dashboard_MonthlyOceanVolumeByMovement).toPromise();
        if (isAllowed) {
            this.service.getMonthlyOceanVolumeByMovement(this.shipmentVolumeByMovementDropdownModel).subscribe(
                (data: any) => {
                    const total = data?.reportingPieCharts.reduce((a, c) => a += c.value, 0);
                    this.monthlyOceanVolumeByMovement = data?.reportingPieCharts.map(a => {
                        return {
                            category: a.category,
                            value: a.value,
                            percentage: a.value / total
                        };
                    });
                    this.lastMonthOceanVolumeByMovementRange = `${this.datePipe.transform(data.lastMonthStartDate, DATE_FORMAT)} to ${this.datePipe.transform(data.thisMonthStartDate, DATE_FORMAT)}`;
                }
            );
        }

        isAllowed = await this._userContext.isGranted(AppPermissions.Dashboard_MonthlyOceanVolumeByServiceType).toPromise();
        if (isAllowed) {
            this.service.getMonthlyOceanVolumeByServiceType(this.shipmentVolumeByServiceTypeDropdownModel).subscribe(
                (data: any) => {
                    const total = data?.reportingPieCharts.reduce((a, c) => a += c.value, 0);
                    this.monthlyOceanVolumeByServiceType = data?.reportingPieCharts.map(a => {
                        return {
                            category: a.category,
                            value: a.value,
                            percentage: a.value / total
                        };
                    });
                    this.lastMonthOceanVolumeByServiceTypeRange = `${this.datePipe.transform(data.lastMonthStartDate, DATE_FORMAT)} to ${this.datePipe.transform(data.thisMonthStartDate, DATE_FORMAT)}`;
                }
            );
        }

        isAllowed = await this._userContext.isGranted(AppPermissions.Dashboard_MonthlyTop5OceanVolumeByOrigin).toPromise();
        if (isAllowed) {
            this.service.getTop5OceanVolumeByOrigin(this.top5ShipmentOriginDropdownModel).subscribe(
                data => {
                    this.top5OceanVolumeByOrigin = data;
                }
            );
        }

        isAllowed = await this._userContext.isGranted(AppPermissions.Dashboard_MonthlyTop5OceanVolumeByDestination).toPromise();
        if (isAllowed) {
            this.service.getTop5OceanVolumeByDestination(this.top5ShipmentDestinationDropdownModel).subscribe(
                data => {
                    this.top5OceanVolumeByDestination = data;
                }
            );
        }

        isAllowed = await this._userContext.isGranted(AppPermissions.Dashboard_Top10ConsigneeThisWeek).toPromise();
        if (isAllowed) {
            this.service.getTop10ConsigneeThisWeek(this.top10ConsigneeDropdownModel).subscribe(
                data => {
                    this.top10ConsigneeThisWeek = data;
                }
            );
        }

        isAllowed = await this._userContext.isGranted(AppPermissions.Dashboard_Top10ShipperThisWeek).toPromise();
        if (isAllowed) {
            this.service.getTop10ShipperThisWeek(this.top10ShipperDropdownModel).subscribe(
                data => {
                    this.top10ShipperThisWeek = data;
                }
            );
        }

        isAllowed = await this._userContext.isGranted(AppPermissions.Dashboard_Top10CarrierThisWeek).toPromise();
        if (isAllowed) {
            this.service.getTop10CarrierThisWeek(this.top10CarrierDropdownModel).subscribe(
                data => {
                    this.top10CarrierThisWeek = data;
                }
            );
        }

    }

    ngOnInit() {
        this.isAuthorizedSubscription = this.oidcSecurityService.getIsAuthorized().pipe(take(1)).subscribe(
            (isAuthorized: boolean) => {
                this.isAuthorized = isAuthorized;
            });

        this._userContext.getCurrentUser().subscribe(currentUser => {
            if (currentUser) {
                this.currentUser = currentUser;
                if (!currentUser.isInternal) {
                    this.service.affiliateCodes = currentUser.affiliates;
                    this.service.delegatedOrgId = currentUser.organizationId || 0;
                    this.service.customerRelationships = this.currentUser.customerRelationships || "";

                    // Just for temporary, the system will check if the user’s OrgCode is 'COBY0002' Manage YTD and Pending DC Delivery will be hidden on Dashboard.
                    if (currentUser.organizationCode === "COBY0002") {
                        this.isShowManageYTD_PendingDC = false;
                    }
                }
                this.loadCharts();
            }
        });
    }

    onClickDismissSurveys() {
        const confirmDlg = this.notification.showConfirmationDialog('label.dismissUpdate', 'label.confirmation');

        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    let surveyIds = this.todayUpdate.surveyParticipants.map(c => c.surveyId);
                    const cookieSurveyIds = this._cookieService.getCookie(TodayUpdateModel.surveysNotificationDismissCookieName)?.split(',').map(c => +c) ?? [];
                    if (cookieSurveyIds.length === 0) {
                        cookieSurveyIds.push(...surveyIds);
                    } else {
                        const newSurveyIds = surveyIds.filter(c => !cookieSurveyIds.some(s => s === +c));
                        cookieSurveyIds.push(...newSurveyIds);
                    }

                    this._cookieService.setCookie(TodayUpdateModel.surveysNotificationDismissCookieName, cookieSurveyIds.join(','));
                    this.todayUpdate.surveyParticipants = [];
                }
            });
    }

    onClickMobileTodayUpdateDismiss() {
        const confirmDlg = this.notification.showConfirmationDialog('label.dismissUpdate', 'label.confirmation');

        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this._cookieService.setCookie(TodayUpdateModel.mobileNotificationDismissCookieName, this.todayUpdate.mobile_version);
                    this.todayUpdate.mobile_notificationDismissed = true;
                }
            });
    }

    public labelTEU = (e: any) => {
        return (e.value).toLocaleString(DefaultEnUsLang, { minimumFractionDigits: 1 });
    }

    public axisTEU = (e: any) => {
        return (e.value).toLocaleString(DefaultEnUsLang, { minimumFractionDigits: 0 }) + ' TEU';
    }

    ngOnDestroy(): void {
        if (this.isAuthorizedSubscription) {
            this.isAuthorizedSubscription.unsubscribe();
        }

        this._subscriptions.map(x => x.unsubscribe());
    }

    resubmit() {
        this.isDisableResubmit = true;
        this.service.reSubmit(this.currentUser.id).subscribe(
            data => {
                this.notification.showSuccessPopup('msg.emailSent', 'label.resubmit');
            },
            error => {
                this.notification.showErrorPopup('msg.emailNotSent', 'label.resubmit');
            });
    }

    getDelta(currentValue: number, prevValue: number) {
        return (prevValue === 0 && currentValue === 0) ? 0 :
            (prevValue === 0 ? 100 : +Math.abs((currentValue - prevValue) / prevValue * 100).toFixed(1));
    }

    onClickShipmentVolumeByOrigin(event) {
        this._gaService.emitEvent('listing_in_top_5_by_origin', GAEventCategory.Dashboard, 'Listing in Top 5 By Origin');
        this._router.navigate(['/shipments'], { state: { statisticKey: `${StatisticKey.ShipmentVolumeByOrigin}-${event.category}`, statisticFilter: this.top5ShipmentOriginDropdownModel } });

    }

    onClickShipmentVolumeByDestination(event) {
        this._gaService.emitEvent('listing_in_top_5_by_destination', GAEventCategory.Dashboard, 'Listing in Top 5 By Destination');
        this._router.navigate(['/shipments'], { state: { statisticKey: `${StatisticKey.ShipmentVolumeByDestination}-${event.category}`, statisticFilter: this.top5ShipmentDestinationDropdownModel } });

    }

    setStatisticKeyTop10Carrier(carrierName) {
        return `${StatisticKey.ShipmentTop10Carrier}-${this.encode(carrierName)}`;
    }


    isShowTop5OceanVolumeByOrigin(items) {
        return items && items.teUs.length > 0 && items.teUs.find(x => x > 0) != null;
    }

    isShowMonthlyOceanVolume(items) {
        return items && items.length > 0 && items.find(x => x.value > 0) != null;
    }

    get hasWeekOceanVolumeData() {
        if (!this.weekOceanVolumeByMovements) {
            return false;
        }
        const n = this.weekOceanVolumeByMovements.length;
        for (let i = 0; i < n; i++) {
            const elmt = this.weekOceanVolumeByMovements[i];
            if (elmt.hasData) {
                return true;
            }
        }

        return false;
    }

    private getWeeklyShipmentVolumeStatisticKey(category: string) {
        switch (category) {
            case 'CY/CY':
                return StatisticKey.CYCYShipmentVolumeInThisWeek;
            case 'CFS/CY':
                return StatisticKey.CFSCYShipmentVolumeInThisWeek;
            case 'CFS/CFS':
                return StatisticKey.CFSCFSShipmentVolumeInThisWeek;
            default:
                return '';
        }
    }

    onMonthlyOceanVolumeClicked(event, chartName: string) {
        let statisticKey = '';
        let selectedCategory = '';
        let statisticFilter = '';
        // transform to lower case to compare
        if (!StringHelper.isNullOrEmpty(event.category)) {
            selectedCategory = event.category.toLowerCase();
        }
        switch (selectedCategory) {
            case 'port-to-door':
                statisticKey = StatisticKey.MonthlyPortToDoorShipmentVolume;
                statisticFilter = this.shipmentVolumeByServiceTypeDropdownModel;
                this._gaService.emitEvent('listing_in_shipment_by_service_type', GAEventCategory.Dashboard, 'Listing in Shipment by Service Type');

                break;
            case 'port-to-port':
                statisticKey = StatisticKey.MonthlyPortToPortShipmentVolume;
                statisticFilter = this.shipmentVolumeByServiceTypeDropdownModel;
                this._gaService.emitEvent('listing_in_shipment_by_service_type', GAEventCategory.Dashboard, 'Listing in Shipment by Service Type');

                break;
            case 'door-to-port':
                statisticKey = StatisticKey.MonthlyDoorToPortShipmentVolume;
                statisticFilter = this.shipmentVolumeByServiceTypeDropdownModel;
                this._gaService.emitEvent('listing_in_shipment_by_service_type', GAEventCategory.Dashboard, 'Listing in Shipment by Service Type');

                break;
            case 'door-to-door':
                statisticKey = StatisticKey.MonthlyDoorToDoorShipmentVolume;
                statisticFilter = this.shipmentVolumeByServiceTypeDropdownModel;
                this._gaService.emitEvent('listing_in_shipment_by_service_type', GAEventCategory.Dashboard, 'Listing in Shipment by Service Type');

                break;
            case 'cfs/cy':
                statisticKey = StatisticKey.MonthlyCFSCYShipmentVolume;
                statisticFilter = this.shipmentVolumeByMovementDropdownModel;
                this._gaService.emitEvent('listing_in_shipment_by_movement', GAEventCategory.Dashboard, 'Listing in Shipment by Movement');

                break;
            case 'cy/cy':
                statisticKey = StatisticKey.MonthlyCYCYShipmentVolume;
                statisticFilter = this.shipmentVolumeByMovementDropdownModel;
                this._gaService.emitEvent('listing_in_shipment_by_movement', GAEventCategory.Dashboard, 'Listing in Shipment by Movement');

                break;
            case 'cfs/cfs':
                statisticKey = StatisticKey.MonthlyCFSCFSShipmentVolume;
                statisticFilter = this.shipmentVolumeByMovementDropdownModel;
                this._gaService.emitEvent('listing_in_shipment_by_movement', GAEventCategory.Dashboard, 'Listing in Shipment by Movement');

                break;
            default:
                if (chartName === this.dashboardChartName.shipmentVolumeByMovement) {
                    statisticKey = `${StatisticKey.ManualInputMovementShipmentVolume}=${selectedCategory}`;
                    statisticFilter = this.shipmentVolumeByMovementDropdownModel;
                    this._gaService.emitEvent('listing_in_shipment_by_movement', GAEventCategory.Dashboard, 'Listing in Shipment by Movement');
                }

                if (chartName === this.dashboardChartName.shipmentVolumeByServiceType) {
                    statisticKey = `${StatisticKey.ManualInputServiceTypeShipmentVolume}=${selectedCategory}`;
                    statisticFilter = this.shipmentVolumeByServiceTypeDropdownModel;
                    this._gaService.emitEvent('listing_in_shipment_by_movement', GAEventCategory.Dashboard, 'Listing in Shipment by Movement');
                }
                break;
        }
        this._router.navigateByUrl('/shipments', { state: { statisticKey: statisticKey, statisticFilter: statisticFilter } });
    }

    public gaEmitEvent(eventAction: string) {
        const eventName = eventAction.replace(/\s\s+/g, ' ').replace(/\s/g, '_').toLocaleLowerCase();
        this._gaService.emitEvent(eventName, GAEventCategory.Dashboard, eventAction);
    }

    private encode(value: string): string {
        return encodeURIComponent(value);
    }
}
