import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Location } from '@angular/common';
import { ListComponent } from 'src/app/core/list/list.component';
import { ShipmentListService } from './shipment-list.service';
import { DropdownListModel, LocalStorageService, UserContextService } from '../../../core';
import { StringHelper } from 'src/app/core';
import { delay, map, debounceTime, tap } from 'rxjs/operators';
import { OnDestroy } from '@angular/core';
import { CompositeFilterDescriptor, DataSourceRequestState, FilterDescriptor } from '@progress/kendo-data-query';
import { Subject, Subscription } from 'rxjs';
import { DefaultDebounceTimeInput } from 'src/app/core/models/constants/app-constants';

@Component({
    selector: 'app-shipment-list',
    templateUrl: './shipment-list.component.html',
    styleUrls: ['./shipment-list.component.scss']
})

export class ShipmentListComponent extends ListComponent implements OnInit, OnDestroy {

    listName = 'shipments';
    firstLoaded = false;
    public result = [];
    shipmentMilestoneDropdown: Array<DropdownListModel<string>> = [];
    milestoneFilterModel: Array<DropdownListModel<string>> = [];
    milestoneChanged$ = new Subject<string>();
    private _subscriptions: Array<Subscription> = [];
    oldItemSelected: number = 0;
    gridStateChanged$ = new Subject<any>();

    constructor(public service: ShipmentListService, route: ActivatedRoute, location: Location,
        private _userContext: UserContextService, private router: Router) {
        super(service, route, location);
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                if (!user.isInternal) {
                    this.service.affiliates = user.affiliates;
                    this.service.state = Object.assign({}, this.service.defaultState);
                }
            }
        });

        service.initDataLoaded.subscribe(response => {
            if (response?.data && response?.data?.length > 0) {
                const idList = response.data.map(x => x.id);
                this.service.getExceptionList(idList).subscribe((res: any) => {
                    if (res && res.length > 0) {
                        this.service.serviceData.forEach(item => {
                            const current = res.find(x => x.id === item.id);
                            if (current) {
                                item.isException = current.isException;
                            }
                        });
                    }
                });
            }
        });
        this.shipmentMilestoneDropdown = this.service.initializeShipmentMilestoneDropdown();
    }

    ngOnInit() {
        this.route.paramMap
            .pipe(map(() => window.history.state))
            .subscribe(
                (state) => {
                    if (StringHelper.isNullOrEmpty(state.statisticKey)) {
                        this.route.queryParams.subscribe(params => {
                            this.service.otherQueryParams.statisticKey
                                = this.service.statisticKey
                                = params.statistic;

                            this.service.otherQueryParams.statisticFilter
                                = this.service.statisticFilter
                                = params.statisticFilter;
                        });
                    } else {
                        this.service.otherQueryParams.statisticKey
                            = this.service.statisticKey
                            = state.statisticKey;

                        this.service.otherQueryParams.statisticFilter
                            = this.service.statisticFilter
                            = state.statisticFilter;
                    }

                    if (!this.service.statisticKey) {
                        let state = LocalStorageService.read<DataSourceRequestState>(`GridState_${this.listName}`);
                        if (state?.filter) {
                            const milestoneFilter: any = state.filter.filters.find((c: any) => c.field === 'activityCode');
                            if (milestoneFilter) {
                                this.milestoneFilterModel = this.service.populateSelectedMilestone(milestoneFilter.value);
                            }
                        }
                        else {
                            const defaultFilterState: any = this.service.defaultState.filter.filters.find((c: any) => c.field === 'activityCode');
                            if (defaultFilterState) {
                                this.milestoneFilterModel = this.service.populateSelectedMilestone(defaultFilterState.value);
                            }
                        }

                        const departureAndIntransitItems = this.milestoneFilterModel.filter(c => c.value === '7001,7003' || c.value === '7003,7001');
                        this.oldItemSelected = departureAndIntransitItems?.length;
                    }
                }
            );
        this.route.queryParams.subscribe((queryParams: Params) => {
            this.firstLoaded = false;
            this.quickTrackNumber = this.route.snapshot.params['referenceNo'];
            if (!StringHelper.isNullOrEmpty(this.quickTrackNumber)) {
                this.service.quickTrackNumber = this.quickTrackNumber;
                this.service.state = Object.assign({}, this.service.defaultState);
                this.service.state.filter = null;
                this.milestoneFilterModel = [];
                this.service.queryQuickTracking(this.service.state).subscribe(data => {
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

        const sub = this.milestoneChanged$.pipe(
            tap(() => {
                this.customSelect();
            })
        ).pipe(
            debounceTime(DefaultDebounceTimeInput),
            tap((value: any) => {
                this.onMilestoneChanged();
            }
            )).subscribe();

        this._subscriptions.push(sub);

        const sub1 = this.gridStateChanged$.pipe(
            debounceTime(DefaultDebounceTimeInput),
            tap((event: any) => {
                this.service.dataStateChange(event);
                this.saveStateToLocalStorage();
            })
        ).subscribe();

        this._subscriptions.push(sub1);
    }

    customSelect() {
        const departureItem = this.milestoneFilterModel.find(c => c.value === '7001,7003');
        const intransitItem = this.milestoneFilterModel.find(c => c.value === '7003,7001');

        const departureAndIntransitItems = this.milestoneFilterModel.filter(c => c.value === '7001,7003' || c.value === '7003,7001');

        // unselect both Departure & Intransit items
        if (departureAndIntransitItems.length !== this.oldItemSelected && this.oldItemSelected !== 0) {
            this.milestoneFilterModel = this.milestoneFilterModel.filter(c => c.value !== '7001,7003' && c.value !== '7003,7001');
            this.oldItemSelected = 0;
            return;
        }

        if (departureItem && this.oldItemSelected !== 2) {
            this.oldItemSelected = 2;
            this.milestoneFilterModel.push({
                label: 'label.inTransit',
                value: '7003,7001'
            });
        }

        if (intransitItem && this.oldItemSelected !== 2) {
            this.oldItemSelected = 2;
            this.milestoneFilterModel.push({
                label: 'label.departureFromPort',
                value: '7001,7003'
            });
        }
    }

    get isShowExceptionRemark() {
        return StringHelper.isNullOrEmpty(this.quickTrackNumber);
    }

    onFilterChanged(filter: CompositeFilterDescriptor) {
        const milestoneFilterDescriptor = {
            field: 'activityCode',
            operator: 'multiselect',
            value: this.milestoneFilterModel.map(c => c.value).toString()

        } as FilterDescriptor;

        if (!filter) {
            filter = {
                filters: [milestoneFilterDescriptor],
                logic: 'and'
            };
        } else {
            const milestoneFilter: any = filter.filters.find((c: any) => c.field === 'activityCode');

            if (!milestoneFilter) {
                filter.filters.push(milestoneFilterDescriptor);
            } else {
                milestoneFilter.value = this.milestoneFilterModel.map(c => c.value).toString();
            }
            if (this.milestoneFilterModel.length === 0) {
                filter.filters = filter.filters.filter((c: any) => c.field !== 'activityCode');
            }
        }
    }

    onMilestoneChanged() {
        this.service.state.skip = 0 ;
        this.service.state.take = 20;
        this.setMilestoneFilterState(this.service.state);

        this.service.query(this.service.state);
    }

    setMilestoneFilterState(state: DataSourceRequestState) {
        const milestoneFilterDescriptor = {
            field: 'activityCode',
            operator: 'multiselect',
            value: this.milestoneFilterModel.map(c => c.value).toString()

        } as FilterDescriptor;

        if (!state.filter) {
            state.filter = {
                filters: [milestoneFilterDescriptor],
                logic: 'and'
            };
        } else {
            const milestoneFilter: any = state.filter.filters.find((c: any) => c.field === 'activityCode');

            if (!milestoneFilter) {
                state.filter.filters.push(milestoneFilterDescriptor);
            } else {
                milestoneFilter.value = this.milestoneFilterModel.map(c => c.value).toString();
            }
            if (this.milestoneFilterModel.length === 0) {
                state.filter.filters = state.filter.filters.filter((c: any) => c.field !== 'activityCode');
            }
        }
        this.saveStateToLocalStorage();
    }

    isItemSelected(selectedItem: any) {
        return this.milestoneFilterModel.some(c => c.value === selectedItem.value);
    }

    ngOnDestroy(): void {
        this.service.statisticKey = null;
        this.service.otherQueryParams.statisticKey = null;
        this._subscriptions.map(x => x.unsubscribe());
    }
}
