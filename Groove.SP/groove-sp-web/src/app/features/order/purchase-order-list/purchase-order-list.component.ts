import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Location } from '@angular/common';
import { ColumnSetting, ListComponent } from 'src/app/core/list/list.component';
import { UserContextService, PurchaseOrderStatus, POStageType, PurchaseOrderTransmissionMethod, Roles, HttpService, POType, POTypeText, StringHelper, DropdownListModel, LocalStorageService, DropDownListItemModel, ViewSettingModuleIdType, FormModeType } from '../../../core';
import { PurchaseOrderListService } from './purchase-order-list.service';
import { faCloudDownloadAlt, faFileImport, faFileDownload, faPlus, faCheck, faExclamationCircle, faCheckCircle } from '@fortawesome/free-solid-svg-icons';
import { environment } from 'src/environments/environment';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { debounceTime, map, tap } from 'rxjs/operators';
import moment from 'moment';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { DefaultDebounceTimeInput, DynamicView, GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { Subject, Subscription } from 'rxjs';
import { CompositeFilterDescriptor, DataSourceRequestState, FilterDescriptor } from '@progress/kendo-data-query';
import * as cloneDeep from 'lodash/cloneDeep';

@Component({
    selector: 'app-purchase-order-list',
    templateUrl: './purchase-order-list.component.html',
    styleUrls: ['./purchase-order-list.component.scss']
})

export class PurchaseOrderListComponent extends ListComponent implements OnInit, OnDestroy {
    @ViewChild('excelexport', { static: false }) excelExportElement: any;
    firstLoaded = false;
    listName = 'purchase-orders';
    viewSettingModuleId = 'PO.List';
    uploadSaveUrl = `${environment.apiUrl}/PurchaseOrders/Import`;
    purchaseOrderStatus = PurchaseOrderStatus;

    stageType = POStageType;
    poType = POType;
    poTypeText = POTypeText;
    faCloudDownloadAlt = faCloudDownloadAlt;
    faFileImport = faFileImport;
    faFileDownload = faFileDownload;
    faPlus = faPlus;
    faCheck = faCheck;
    faExclamationCircle = faExclamationCircle;
    faCheckCircle = faCheckCircle;
    readonly AppPermissions = AppPermissions;
    currentUser: any;
    buyCompliance: any;
    POStageType = POStageType;
    pos: any[] = [];
    isCanExport: boolean;
    parentInCustomerRelationship: boolean = false;
    poStageFilterModel: Array<DropDownListItemModel<number>> = [];
    poStageChanged$ = new Subject<string>();
    poStageDropdown: Array<DropdownListModel<string>> = [];
    subscriptions = new Subscription();

    columns: ColumnSetting[] = [
        {
            field: 'poNumber',
            title: 'label.poNumber',
            filter: 'text',
            class: 'link-code',
            width: '18%'
        },
        {
            field: 'incoterm',
            title: 'label.incoterms',
            filter: 'text',
            width: '11%'
        },
        {
            field: 'createdDate',
            title: 'label.poDateFrom',
            filter: 'date',
            format: this.DATE_FORMAT,
            width: '14%'
        },
        {
            field: 'cargoReadyDate',
            title: 'label.exWorkFrom',
            filter: 'date',
            format: this.DATE_FORMAT,
            class: 'cargo-ready-date-cell',
            width: '14%'
        },
        {
            field: 'supplier',
            title: 'label.supplier',
            filter: 'text',
            width: '18%'
        },
        {
            field: 'stageName',
            title: 'label.milestone',
            filter: 'text',
            width: '15%',
        },
        {
            field: 'statusName',
            title: 'label.status',
            filter: 'text',
            width: '10%',
        },
    ];

    public importFormOpened: boolean = false;
    public selectPOsFormOpened: boolean = false;

    constructor(public service: PurchaseOrderListService, route: ActivatedRoute, location: Location,
        private _userContext: UserContextService, private router: Router, private _gaService: GoogleAnalyticsService,
    ) {
        super(service, route, location);
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
                if (!user.isInternal) {
                    this.service.affiliates = user.affiliates;
                    this.service.state = cloneDeep(this.service.defaultState);

                    service.getActiveBuyerCompliance(user.organizationId).subscribe(compliance => {
                        this.buyCompliance = compliance;
                    });

                    service.checkParentInCustomerRelationship(this.currentUser.organizationId).subscribe(res => this.parentInCustomerRelationship = res);
                }
            }
        });
    }

    async ngOnInit() {
        this.route.paramMap
            .pipe(map(() => window.history.state))
            .subscribe(
                (state) => {
                    if (StringHelper.isNullOrEmpty(state.statisticKey)) {
                        this.route.queryParams.subscribe(params => {
                            this.service.otherQueryParams.statisticKey
                                = this.service.statisticKey
                                = params.statistic;

                            this.service.otherQueryParams.statisticValue
                                = this.service.statisticValue
                                = params.statisticValue;

                            this.service.otherQueryParams.statisticFilter
                                = this.service.statisticFilter
                                = params.statisticFilter;
                        });
                    } else {
                        this.service.otherQueryParams.statisticKey
                            = this.service.statisticKey
                            = state.statisticKey;

                        this.service.otherQueryParams.statisticValue
                            = this.service.statisticValue
                            = state.statisticValue;

                        this.service.otherQueryParams.statisticFilter
                            = this.service.statisticFilter
                            = state.statisticFilter;
                    }

                    if (!this.service.statisticKey) {
                        let state = LocalStorageService.read<DataSourceRequestState>(`GridState_${this.listName}`);
                        if (state?.filter) {
                            const stageFilter: any = state.filter.filters.find((c: any) => c.field === 'stage');
                            if (stageFilter) {
                                this.poStageFilterModel = this.service.populateSelectedStage(stageFilter.value);
                            }
                        }
                        else {
                            const defaultFilterState: any = this.service.defaultState.filter.filters.find((c: any) => c.field === 'stage');
                            if (defaultFilterState) {
                                this.poStageFilterModel = this.service.populateSelectedStage(defaultFilterState.value);
                            }
                        }
                    }
                }
            );

        if (!this.currentUser.isInternal) {
            // apply for shipper/supplier
            if (this.currentUser.customerRelationships) {
                // get current customer-relationships
                const customerRelationshipParams = await this._userContext.getCustomerRelationships(this.currentUser).toPromise();
                this.service.otherQueryParams.customerRelationships = customerRelationshipParams;
            }
            this.service.otherQueryParams.organizationId = this.currentUser.organizationId;
        }

        this.route.queryParams.subscribe((queryParams: Params) => {
            this.firstLoaded = false;
            this.quickTrackNumber = this.route.snapshot.params['itemNo'];
            if (!StringHelper.isNullOrEmpty(this.quickTrackNumber)) {
                this.service.quickTrackNumber = this.quickTrackNumber;
                this.service.state = cloneDeep(this.service.defaultState);
                if (this.service?.state?.filter?.filters?.length > 0) {
                    this.service.state.filter.filters = this.service.state.filter.filters.filter(c =>  c['field'] !== 'status');
                }
                this.viewSettingModuleId = ViewSettingModuleIdType.PO_ITEM_QUICK_SEARCH_LIST;
                const filteredModuleIdStage = this.service?.state?.filter?.filters?.find(c => c['field'] === DynamicView.moduleKey && c['value'] === ViewSettingModuleIdType.PO_ITEM_QUICK_SEARCH_LIST);
                if (!filteredModuleIdStage) {
                    this.service.state.filter.filters.push({
                        field: DynamicView.moduleKey,
                        operator: 'eq',
                        value: ViewSettingModuleIdType.PO_ITEM_QUICK_SEARCH_LIST
                    });
                }

                this.service.queryQuickTracking(this.service.state).subscribe(data => {
                    const stageFilter: any = this.service.defaultState.filter.filters.find((c: any) => c.field === 'stage');
                    this.poStageFilterModel = this.service.populateSelectedStage(stageFilter.value);
                    this.view = this.service;
                    this.service.updateData(data);
                    this.firstLoaded = true;
                    if (data.total === 0) {
                        this.router.navigate(['/search/no-result']);
                    }
                });
            } else {
                this.service.quickTrackNumber = '';
                this.firstLoaded = true;
                super.ngOnInit();
            }
        });

        const sub = this.poStageChanged$.pipe(
            debounceTime(DefaultDebounceTimeInput),
            tap((value: any) => {
                this.onPOStageChanged();
            }
            )).subscribe();

        this.subscriptions.add(sub);
    }

    onPOStageChanged() {
        this.service.state.skip = 0;
        this.service.state.take = 20;
        this.setMilestoneFilterState(this.service.state);

        this.service.query(this.service.state);
    }

    onFilterChanged(filter: CompositeFilterDescriptor) {
        const milestoneFilterDescriptor = {
            field: 'stage',
            operator: 'multiselect',
            value: this.poStageFilterModel.map(c => c.value).toString()

        } as FilterDescriptor;

        if (!filter) {
            filter = {
                filters: [milestoneFilterDescriptor],
                logic: 'and'
            };
        } else {
            const milestoneFilter: any = filter.filters.find((c: any) => c.field === 'stage');

            if (!milestoneFilter) {
                filter.filters.push(milestoneFilterDescriptor);
            } else {
                milestoneFilter.value = this.poStageFilterModel.map(c => c.value).toString();
                milestoneFilter.operator = 'multiselect';
            }
            if (this.poStageFilterModel.length === 0) {
                filter.filters = filter.filters.filter((c: any) => c.field !== 'stage');
            }
        }
    }

    setMilestoneFilterState(state: DataSourceRequestState) {
        const milestoneFilterDescriptor = {
            field: 'stage',
            operator: 'multiselect',
            value: this.poStageFilterModel.map(c => c.value).toString()

        } as FilterDescriptor;

        if (!state.filter) {
            state.filter = {
                filters: [milestoneFilterDescriptor],
                logic: 'and'
            };
        } else {
            const stageFilter: any = state.filter.filters.find((c: any) => c.field === 'stage');

            if (!stageFilter) {
                state.filter.filters.push(milestoneFilterDescriptor);
            } else {
                stageFilter.value = this.poStageFilterModel.map(c => c.value).toString();
                stageFilter.operator = 'multiselect';
            }
            if (this.poStageFilterModel.length === 0) {
                state.filter.filters = state.filter.filters.filter((c: any) => c.field !== 'stage');
            }
        }

        this.saveStateToLocalStorage();
    }

    get isHiddenBookingButton() {
        if (this.service.otherQueryParams.customerRelationships?.length === 1) {
            return !this.parentInCustomerRelationship;
        }
        return false;
    }

    importFormClosedHandler(isImportedSuccessfully: boolean) {
        this.importFormOpened = false;
        this.service.query(this.service.state);
        if (isImportedSuccessfully) {
            this._gaService.emitEvent('import', GAEventCategory.PurchaseOrder, 'Import');
        }
    }

    downloadTemplate() {
        this.service.downloadTemplate(`${environment.apiUrl}/purchaseOrders/downloadExcelTemplate`,
            'PurchaseOrderTemplate.xlsx').subscribe();
        this._gaService.emitEvent('download', GAEventCategory.PurchaseOrder, 'Download');
    }

    hiddenBtnDownloadTemplate() {
        if (this.currentUser.isInternal) {
            return false;
        }

        if (this.currentUser.role.id === Roles.Principal && this.buyCompliance
            && this.buyCompliance.purchaseOrderTransmissionMethods === PurchaseOrderTransmissionMethod.ExcelUpload) {
            return false;
        }

        return true;
    }

    hiddenBtnImportPO() {
        if (this.currentUser.isInternal) {
            return false;
        }

        if (this.currentUser.role.id === Roles.Principal && this.buyCompliance
            && this.buyCompliance.purchaseOrderTransmissionMethods === PurchaseOrderTransmissionMethod.ExcelUpload) {
            return false;
        }

        return true;
    }

    // Select multi-POs

    selectPOsPopupClosedHandler(data: { selectedPOIds: Array<number>, selectedPrincipalId, isAllowMissingPO: boolean }) {
        this.selectPOsFormOpened = false;
        if (data) {
            let queryParams = {};
            let navigateUrl = '/po-fulfillments/add/0';

            queryParams['selectedpos'] = data.selectedPOIds.toString();
            if (data.isAllowMissingPO) {
                queryParams['selectedcustomer'] = data.selectedPrincipalId;
                navigateUrl = '/missing-po-fulfillments/add/0';
            }
            queryParams['formType'] = FormModeType.Add;
            
            this.router.navigate([navigateUrl], { queryParams });
        }
    }

    selectPOsPopupOpeningHandler() {
        this.selectPOsFormOpened = true;
    }

    checkPendingForProgress(customerPO) {
        const new_date = moment().add(customerPO.progressNotifyDay, 'days');
        return customerPO.cargoReadyDate <= new_date;
    }

    progressCheck() {
        this.router.navigate(['/po-progress-check']);
    }

    exportExcel() {
        this.isCanExport = false;
        this.service.queryToExport().subscribe(
            r => {
                this.isCanExport = true;
                this.pos = r.data;
                for (const po of this.pos) {
                    if (new Date(po.createdDate).getTime() === new Date('0001-01-01T00:00:00').getTime()) {
                        po.createdDate = '01/01/0001';
                    }
                }
                setTimeout(() => {
                    this.excelExportElement.save();
                }, 50);
            }
        );
        this._gaService.emitEvent('export', GAEventCategory.PurchaseOrder, 'Export');
    }

    isItemSelected(selectedItem: any) {
        return this.poStageFilterModel.some(c => c.value === selectedItem.value);
    }

    ngOnDestroy(): void {
        this.service.statisticKey = null;
        this.service.otherQueryParams.statisticKey = null;
        this.subscriptions.unsubscribe();
    }

}
