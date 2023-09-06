import { AfterViewInit, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { ListComponent } from 'src/app/core/list/list.component';
import { faPlus, faInfoCircle, faCheck, faMinus } from '@fortawesome/free-solid-svg-icons';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { POFulfillmentListService } from './po-fulfillment-list.service';
import { UserContextService, Roles, StringHelper, DropDownListItemModel, DropdownListModel, LocalStorageService } from 'src/app/core';
import { POFulfillmentStatus, POFulfillmentStageType, BuyerApprovalStage, FulfillmentType,OrderFulfillmentPolicy, ViewSettingModuleIdType, FormModeType } from 'src/app/core/models/enums/enums';
import { DomHelper } from 'src/app/core/helpers/dom.helper';
import { TranslateService } from '@ngx-translate/core';
import { debounceTime, map, tap } from 'rxjs/operators';
import { Subject, Subscription } from 'rxjs';
import { CompositeFilterDescriptor, DataSourceRequestState, FilterDescriptor } from '@progress/kendo-data-query';
import { DefaultDebounceTimeInput } from 'src/app/core/models/constants/app-constants';

@Component({
    selector: 'app-po-fulfillment-list',
    templateUrl: './po-fulfillment-list.component.html',
    styleUrls: ['./po-fulfillment-list.component.scss']
})
export class POFulfillmentListComponent extends ListComponent implements OnInit, AfterViewInit, OnDestroy {

    listName = 'po-fulfillments';
    viewSettingModuleId = ViewSettingModuleIdType.BOOKING_LIST;
    faPlus = faPlus;
    readonly AppPermissions = AppPermissions;
    formType = FormModeType;
    isHideBtnAddNew = true;
    poFulfillmentStatus = POFulfillmentStatus;
    poFulfillmentStageType = POFulfillmentStageType;
    fulfillmentType = FulfillmentType;
    orderFulfillmentPolicy = OrderFulfillmentPolicy;
    faInfoCircle = faInfoCircle;
    faCheck = faCheck;
    faMinus = faMinus;
    isHiddenConfirmBtn: boolean;
    isOpenCopyBookingPopup: boolean = false;
    bookingStageFilterModel: Array<DropDownListItemModel<number>> = [];
    bookingStageChanged$ = new Subject<string>();
    bookingStageDropdown: Array<DropdownListModel<string>> = [];
    subscriptions = new Subscription();
    
    constructor(public service: POFulfillmentListService, route: ActivatedRoute, location: Location,
        private _userContext: UserContextService,
        private router: Router,
        public translateService: TranslateService,
    ) {
        super(service, route, location);
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                if (!user.isInternal) {
                    this.service.affiliates = user.affiliates;
                    this.service.state = Object.assign({}, this.service.defaultState);
                    this.service.otherQueryParams.organizationId = user.organizationId;
                    this.isHiddenConfirmBtn = user.role.id !== Roles.Warehouse;

                    // Still hide this button if shipper
                    // if (user.role.id === Roles.Shipper) {
                    //     this.isHideBtnAddNew = false;
                    // }
                }
            }
        });
    }
    

    ngOnInit() {
        this.route.paramMap.pipe(map(() => window.history.state))
            .subscribe(
                (state) => {
                    if (StringHelper.isNullOrEmpty(state.statisticKey)) {
                        this.route.queryParams.subscribe(params => {
                            this.service.otherQueryParams.statisticKey
                                = this.service.statisticKey
                                = params.statistic;

                            this.service.otherQueryParams.userRole
                                = this.service.userRole
                                = params.userRole;
                        });
                    } else {
                        this.service.otherQueryParams.statisticKey
                            = this.service.statisticKey
                            = state.statisticKey;

                        this.service.otherQueryParams.userRole
                            = this.service.userRole
                            = state.userRole;
                    }

                    if (!this.service.statisticKey) {
                        let sub =  this.service.initDataLoaded.subscribe(response => {
                            let state = LocalStorageService.read<DataSourceRequestState>(`GridState_${this.listName}`);
                            if (state?.filter) {
                                const stageFilter: any = state.filter.filters.find((c: any) => c.field === 'stage');
                                if (stageFilter) {
                                    this.bookingStageFilterModel = this.service.populateSelectedStage(stageFilter.value);
                                }
                            }
                            else {
                                const defaultFilterState: any = this.service.defaultState.filter.filters.find((c: any) => c.field === 'stage');
                                if (defaultFilterState) {
                                    this.bookingStageFilterModel = this.service.populateSelectedStage(defaultFilterState.value);
                                }
                            }
                        });
                        
                        this.subscriptions.add(sub);
                    }
                }
            );

        super.ngOnInit();
        
        let sub = this.bookingStageChanged$.pipe(
            debounceTime(DefaultDebounceTimeInput),
            tap((value: any) => {
                this.bookingPOStageChanged();
            }
            )).subscribe();

        this.subscriptions.add(sub);
    }

    bookingPOStageChanged() {
        this.service.state.skip = 0;
        this.service.state.take = 20;
        this.setMilestoneFilterState(this.service.state);
        this.service.query(this.service.state);
    }

    onFilterChanged(filter: CompositeFilterDescriptor) {
        const milestoneFilterDescriptor = {
            field: 'stage',
            operator: 'multiselect',
            value: this.bookingStageFilterModel.map(c => c.value).toString()

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
                milestoneFilter.value = this.bookingStageFilterModel.map(c => c.value).toString();
                milestoneFilter.operator = 'multiselect';
            }
            if (this.bookingStageFilterModel.length === 0) {
                filter.filters = filter.filters.filter((c: any) => c.field !== 'stage');
            }
        }
    }

    setMilestoneFilterState(state: DataSourceRequestState) {
        const milestoneFilterDescriptor = {
            field: 'stage',
            operator: 'multiselect',
            value: this.bookingStageFilterModel.map(c => c.value).toString()

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
                stageFilter.value = this.bookingStageFilterModel.map(c => c.value).toString();
                stageFilter.operator = 'multiselect';
            }
            if (this.bookingStageFilterModel.length === 0) {
                state.filter.filters = state.filter.filters.filter((c: any) => c.field !== 'stage');
            }
        }

        this.saveStateToLocalStorage();
    }

    
    ngAfterViewInit(): void {
        DomHelper.scrollToTop();
    }

    isRejectedStatus(item) {
        return item.isRejected === true &&
            item.status !== POFulfillmentStatus.Inactive;
    }

    onConfirm() {
        this.router.navigate(['warehouse-bookings-confirm']);
    }

    onCopyBookingPopupClosed(event) {
        this.isOpenCopyBookingPopup = false;
    }

    isItemSelected(selectedItem: any) {
        return this.bookingStageFilterModel.some(c => c.value === selectedItem.value);
    }

    get bulkBookingBtnData() {
        const result = [
            {
                actionName: this.translateService.instant('label.addNew'),
                icon: 'plus',
                click: () => {
                    this.router.navigate(['/bulk-fulfillments/add/0'], { queryParams: { isAddNew: true , formType: FormModeType.Add} });
                }
            },
            {
                actionName: this.translateService.instant('label.copyFrom'),
                icon: 'plus',
                click: () => {
                    this.isOpenCopyBookingPopup = true;
                }
            }
        ];
        return result;
    }

    ngOnDestroy(): void {
        this.subscriptions.unsubscribe();
    }
}
