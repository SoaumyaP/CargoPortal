import { Component, OnInit, EventEmitter, OnDestroy } from '@angular/core';
import { Location } from '@angular/common';
import { ListComponent } from 'src/app/core/list/list.component';
import { ReportListService } from './report-list.service';
import { ActivatedRoute, Router, Params } from '@angular/router';
import {
    UserContextService,
    StringHelper,
    Roles,
    DATE_HOUR_FORMAT,
    DropDownListItemModel
} from 'src/app/core';
import {Observable, forkJoin, Subscription } from 'rxjs';
import { ReportPermissionModel } from '../models/report-permission.model';
import { FormGroup } from '@angular/forms';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';

@Component({
    selector: 'app-report-list',
    templateUrl: './report-list.component.html',
    styleUrls: ['./report-list.component.scss']
})
export class ReportListComponent extends ListComponent implements OnInit, OnDestroy {
    listName = 'reports';
    firstLoaded: boolean = false;
    currentUser: any;
    canGrantPermission: boolean = false;
    DATE_HOUR_FORMAT = DATE_HOUR_FORMAT;

    public principalDataSource: Array<{ text: string; value: number }> = [];
    public selectedPrincipal: number = 0;
    public organizationIds: Array<number> = [];
    public reportPermissionOpening: EventEmitter<ReportPermissionModel> = new EventEmitter<ReportPermissionModel>();
    public currentRoleId: number = 0;
    public viewPermissionOnly: boolean = false;

    private _subscriptions: Array<Subscription> = [];

    constructor(
        public service: ReportListService,
        route: ActivatedRoute,
        location: Location,
        private _userContext: UserContextService,
        private router: Router,
        private _notification: NotificationPopup,
    ) {
        super(service, route, location);
        const sub = this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
                this.selectedPrincipal = this.currentUser.organizationId || 0;
                if (this.currentUser.affiliates) {
                    this.organizationIds = JSON.parse(this.currentUser.affiliates);
                } else {
                    this.organizationIds = [this.selectedPrincipal];
                }
                this.currentRoleId = this.currentUser.role
                    ? this.currentUser.role.id
                    : 0;
                // CSR and Admin are able to grant permission for report
                this.canGrantPermission = [Roles.CSR, Roles.System_Admin].indexOf(this.currentRoleId) > -1;
                this.viewPermissionOnly = [Roles.CSR].indexOf(this.currentRoleId) > -1;
                this.service.affiliates = user.affiliates;
                this.service.otherQueryParams.roleId = this.currentRoleId;
                this.service.otherQueryParams.selectedOrganizationId = this.selectedPrincipal;
                // Note: allowed query length is about 5484 chars
                // Internal user don't need to this parameter with all Organization Ids in system.
                this.service.otherQueryParams.organizationIds = !this.currentUser.isInternal ? this.organizationIds : 0;
                this.service.state = Object.assign({}, this.service.defaultState);
            }
        });
        this._subscriptions.push(sub);
    }

    ngOnInit() {
        const sub = this.route.queryParams.subscribe((queryParams: Params) => {
            this.firstLoaded = true;
            const sub1 = this.initializeData().subscribe(x => {
                // select principal by default after initial data was completed
                this.selectedPrincipal = !this.principalDataSource ? 0 : this.principalDataSource[0].value;
                this.service.otherQueryParams.selectedOrganizationId = this.selectedPrincipal;
                // Note: allowed query length is about 5484 chars
                // Internal user don't need to this parameter with all Organization Ids in system.
                this.service.otherQueryParams.organizationIds = !this.currentUser.isInternal ? this.organizationIds : 0;

                super.ngOnInit();
            });
            this._subscriptions.push(sub1);
        });
        this._subscriptions.push(sub);
    }

    pushPrincipalSelectionChanged(selectedValue) {
        if (selectedValue) {
            this.selectedPrincipal = parseInt(selectedValue, 0);
            this.service.otherQueryParams.selectedOrganizationId = selectedValue;
            // Note: allowed query length is about 5484 chars
            // Internal user don't need to this parameter with all Organization Ids in system.
            this.service.otherQueryParams.organizationIds = !this.currentUser.isInternal ? this.organizationIds : 0;
            this.service.query(this.service.state);
        }
    }

    preGrantReportPermissionOpened(reportId) {
        if (reportId > 0) {
            const sub = this.service.getReportPermission(reportId).subscribe(x => {
                this.reportPermissionOpening.emit(x);
            });
            this._subscriptions.push(sub);
        }
    }

    pushReportPermissionFormClosed(form: FormGroup) {

        // If null, do nothing as view mode
        if (StringHelper.isNullOrEmpty(form)) {
            return;
        }
        // else, save data
        const formData = form.value;
        // transfer organizations into list of id: 1,2,3
        if (!StringHelper.isNullOrEmpty(formData.organizations)) {
            const ids = formData.organizations.map(x => x.value).join(',');
            formData.organizationIds = ids;
        }
        const sub = this.service.grantReportPermission(formData)
            .subscribe(
                (data: any) => {
                    this._notification.showSuccessPopup('save.sucessNotification', 'label.reports');
                },
                error => {
                    this._notification.showErrorPopup('save.failureNotification', 'label.reports');
                });
        this._subscriptions.push(sub);
    }

    private initializeData(): Observable<void> {
        const organizationId = this.currentUser.organizationId;
        const obs1 = this.service.getPrincipalDataSource(this.currentUser.role.id, organizationId, this.currentUser.affiliates)
                    .map(data => {
                        const dataSource: Array<DropDownListItemModel<number>> = [];
                        let selectedPrincipalFound: boolean = false;
                        data.forEach(element => {
                            if (element) {
                                const elementValue = element.value;
                                if (elementValue === this.selectedPrincipal) {
                                    selectedPrincipalFound = true;
                                }
                                const newOption = new DropDownListItemModel<number>(element.text, elementValue);
                                if (element.disabled === false) {
                                    dataSource.push(newOption);
                                }
                            }
                        });
                        this.principalDataSource = dataSource;
                        if (!selectedPrincipalFound) {
                            this.selectedPrincipal = 0;
                            this.service.otherQueryParams.selectedOrganizationId = this.selectedPrincipal;
                        }
                    });
        const obs2 = this.service.getAccessibleOrganizationIds(this.currentUser.role.id, organizationId, this.currentUser.affiliates)
                    .map(data => {
                        this.organizationIds = data;
                    });

        return forkJoin(obs1, obs2).map((val1, val2) => { });
    }

    public goToExportPage(reportUrl, reportId, reportName) {
        const queryPart = reportUrl.split('?')[1];
        const queryParams = queryPart?.split('&');
        this.router.navigate([`/reports/${reportUrl.split('?')[0]}`], {
            queryParams: {
                customer: this.selectedPrincipal,
                report: reportId,
                name: reportName,
                category: queryParams?.find(x => x.split('=')[0] === 'category')?.split('=')[1] ?? null,
                report_key: queryParams?.find(x => x.split('=')[0] === 'reportkey')?.split('=')[1] ?? null,
                report_server_url: queryParams?.find(x => x.split('=')[0] === 'reportserverurl')?.split('=')[1] ?? null
            }
        });
    }


    ngOnDestroy(): void {
        this._subscriptions.map(x => x.unsubscribe());
    }
}
