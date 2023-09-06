import { HttpClient } from '@angular/common/http';
import { AfterViewInit, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TelerikReportViewerComponent } from '@progress/telerik-angular-report-viewer';
import { Subscription } from 'rxjs';
import { LocalStorageService, StringHelper, UserContextService } from 'src/app/core';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { ReportName } from 'src/app/core/models/constants/app-constants';
import { TelerikReportSourceModel } from 'src/app/core/models/telerik/telerik-report-source.model';
import { UserProfileModel } from 'src/app/core/models/user/user-profile.model';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { environment } from 'src/environments/environment';
import { ReportOptionModel } from '../../scheduling/models/report.option.model';
import { ReportListService } from '../report-list/report-list.service';
import { TelerikReportService } from './telerik-report.service';

@Component({
    selector: 'app-telerik-report',
    templateUrl: './telerik-report.component.html',
    styleUrls: ['./telerik-report.component.scss']
})
export class TelerikReportComponent implements OnInit, AfterViewInit, OnDestroy {
    @ViewChild('reportViewer', { static: true }) reportViewer: TelerikReportViewerComponent;
    readonly reportNameType = ReportName;

    readonly AppPermissions = AppPermissions;

    /**
     * Data source for level drop-down list
     */
    masterSummaryReportOptions: { label: string, value: string }[] = [
        {
            label: 'PO Level',
            value: 'Master Summary Report (PO Level)',
        },
        {
            label: 'Item Level',
            value: 'Master Summary Report (Item Level)'
        }
    ];

    /**
     * Settings for report viewer control
     */
    viewerContainerStyle = {
        position: 'relative',
        height: '740px',
        ['font-family']: 'ms sans serif'
    };

    reportSource: TelerikReportSourceModel;
    reportName: string;
    selectedCustomerId: number;
    reportId: number;
    currentUser: UserProfileModel;
    reportLevels: Array<ReportOptionModel> = [];
    downTimer: any;

    private _subscriptions: Array<Subscription> = [];

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private _service: TelerikReportService,
        private _notification: NotificationPopup,
        private _userContext: UserContextService,
        private _reportListService: ReportListService) {
        const sub = this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
            }
        });
        this._subscriptions.push(sub);
    }

    ngOnInit() {
        this.reportSource = new TelerikReportSourceModel();
        this.route.queryParams.subscribe(params => {
            this.reportSource.name = decodeURIComponent(params['report_key']);
            this.reportSource.category = decodeURIComponent(params['category']);
            this.reportSource.reportServerUrl = decodeURIComponent(params['report_server_url']);

            this.reportName = decodeURIComponent(params['name'] ?? '');
            this.selectedCustomerId = params['customer'];
            this.reportId = params['report'];
        });
    }

    ngAfterViewInit(): void {
        // call server to get token
        this._service.getReportToken(this.reportId, this.selectedCustomerId).subscribe(token => {
            this.reportSource.token = token;
            this.reportSource.accessToken = token.access_token;
            this.downTimer = setInterval(() => {
                this._service.getReportToken(this.reportId, this.selectedCustomerId).subscribe(newToken => {
                        this.reportViewer.setAuthenticationToken(newToken.access_token);
                    });
            }, 1200000);

            const filterSetModel = {
                selectedCustomerId: this.selectedCustomerId
            };
            const reportSourceObject = {
                report: `${this.reportSource.category}/${this.reportSource.name}.trdp`,
                parameters: filterSetModel
            };
            const reportSource = JSON.parse(JSON.stringify(reportSourceObject));
            // set token to report viewer then refresh data
            this.reportViewer.setAuthenticationToken(this.reportSource.accessToken);
            this.reportViewer.setReportSource(reportSource);
        },
            (error) => {
                this._notification.showErrorPopup('label.titleUnauthorized', 'label.reports', true);
            }
        );

        // If master summary report new, get data for report level
        this._fetchReportLevels();
    }

    ngOnDestroy(): void {
        this._subscriptions.map(x => x.unsubscribe());
        clearInterval(this.downTimer);
    }

    /**
      * Fetch data source for report drop-down list
      */
    private _fetchReportLevels(): void {
        if (this.reportName === this.reportNameType.MasterSummaryReportNew ) {
            const isInternal = this.currentUser.isInternal;
            const roleId = this.currentUser.role.id;
            const affiliates = this.currentUser.affiliates ? JSON.parse(this.currentUser.affiliates) : [0];
            const sub = this._reportListService.$getReportOptions(isInternal, roleId, affiliates).subscribe(
                (value) => {
                    this.reportLevels = value;
                }
            );
            this._subscriptions.push(sub);
        }
    }

    /**
     * Handler on value report level changed
     * @param value
     */
    onMasterSummaryReportLevelChange(value: string) {
        this.router.navigateByUrl('/', {skipLocationChange: true}).then(() =>
        this.router.navigate([`${window.location.pathname.split('?')[0]}`], {
            queryParams: {
                customer: this.selectedCustomerId,
                report: this.reportId,
                name: this.reportName,
                category: this.reportSource.category ?? null,
                report_key: this.reportSource.name ?? null,
                report_server_url: this.reportSource.reportServerUrl ?? null
            }
        }));
    }

    backList() {
        this.router.navigate(['/reports']);
    }

    /**
     * To navigate to scheduling page
     */
    onNavigateToSchedulePage() {

        const selectedPrincipalOrgId = this.selectedCustomerId;
        let selectedReportId = this.reportId;

        // If Master summary report new, must get report in from level drop-down
        if (this.reportName === this.reportNameType.MasterSummaryReportNew) {
            selectedReportId = this.reportLevels.find(x => x.reportName === this.reportSource.name).id;
        }
        const reportParams = this.reportViewer.getReportSource().parameters;
        // Remove null value to shorten
        const reportParamKeys = Object.keys(reportParams);
        const filteredReportParams = {};
        reportParamKeys?.map(x => {
            const value = reportParams[x];
            if (!StringHelper.isNullOrEmpty(value)) {
                if (Array.isArray(value) && value.length === 1 && StringHelper.isNullOrEmpty(value[0])) {
                } else {
                    filteredReportParams[x] = value;
                }
            }
        });
        // store report params to storage, then used later on Schedule form component
        LocalStorageService.write('task-report-params', filteredReportParams);
        window.open(`${environment.spaUrl}/scheduling/Add/0?reportId=${selectedReportId}&selectedCustomerId=${selectedPrincipalOrgId}`, '_blank');
    }
}
