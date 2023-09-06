import { OnInit, ViewChild } from '@angular/core';
import { Location } from '@angular/common';
import { Params, ActivatedRoute } from '@angular/router';

import { GridDataResult, DataStateChangeEvent, GridComponent } from '@progress/kendo-angular-grid';
import { CompositeFilterDescriptor, DataSourceRequestState, FilterDescriptor } from '@progress/kendo-data-query';
import { Observable, from } from 'rxjs';
import { delay } from 'rxjs/operators';

import { ListService } from './list.service';
import { StringHelper, DATE_FORMAT, DATE_HOUR_FORMAT } from '../helpers';
import { LocalStorageService } from '../services/local-storage.service';
import { DynamicView, Separator } from '../models/constants/app-constants';
import { StatisticKey } from '../models/enums/enums';

export interface ColumnSetting {
    field: string;
    title: string;
    format?: string;
    filter: 'text' | 'numeric' | 'boolean' | 'date';
    class?: string;
    width?: string;
    sortable?: boolean;
}
export class ListComponent implements OnInit {
    @ViewChild(GridComponent, { static: false })
    grid: GridComponent;
    public view: Observable<GridDataResult>;
    public isInitDataLoaded: boolean;
    public listName: string;
    /** Module Id of Vew Settings what toggles show/hide columns on grid/list or fields on form. */
    public viewSettingModuleId: string;
    public isFormList = false;
    public quickTrackNumber = '';
    DATE_FORMAT = DATE_FORMAT;
    DATE_HOUR_FORMAT = DATE_HOUR_FORMAT;

    constructor(public service: ListService, protected route: ActivatedRoute, private location: Location) {
        this.allData = this.allData.bind(this);
    }

    ngOnInit() {
        this.route.queryParams.subscribe((params: Params) => {
            if (StringHelper.isNullOrEmpty(this.quickTrackNumber)) {
                this.readStateFromLocalStorage();
                this.replaceState();
                this.view = this.service;

                if (!StringHelper.isNullOrEmpty(this.service.statisticKey)) {
                    this.service.state.skip = 0;
                    this.service.state['filter'] = {
                        filters: [],
                        logic: 'and'
                    };
                    // adjust status filtering is active
                    switch (this.listName.toLowerCase()) {
                        case 'shipments':
                            this.service.state.filter.filters.push({
                                field: 'status',
                                operator: 'eq',
                                value: 'Active'
                            });
                            break;

                        case 'po-fulfillments':
                        case 'buyer-approvals':
                            this.service.state.filter.filters.push({
                                field: 'stage',
                                operator: 'eq',
                                value: 10
                            });
                            break;

                        case 'purchase-orders': {
                            if (this.service.statisticKey.startsWith('categorized')) {
                                const key = this.service.statisticKey;
                                const value = decodeURIComponent(this.service.statisticValue);
                                if (StringHelper.caseIgnoredCompare(key, StatisticKey.CategorizedSupplier)) {
                                    this.service.state.filter.filters.push({
                                        field: 'supplier',
                                        operator: 'contains',
                                        value
                                    });
                                }
                                else if (StringHelper.caseIgnoredCompare(key, StatisticKey.CategorizedStage)) {
                                    this.service.state.filter.filters.push({
                                        field: 'stage',
                                        operator: 'eq',
                                        value: Number.parseInt(value)
                                    });
                                } else if (StringHelper.caseIgnoredCompare(key, StatisticKey.CategorizedStatus)) {
                                    this.service.state.filter.filters.push({
                                        field: 'status',
                                        operator: 'eq',
                                        value: Number.parseInt(value)
                                    });
                                }
                            }
                            else {
                                this.service.state.filter.filters.push({
                                    field: 'status',
                                    operator: 'eq',
                                    value: 1
                                });
                            }

                            break;
                        }

                        case 'shortships': {
                            break;
                        }

                        case 'vessel-arrivals': {
                            break;
                        }

                        default:
                            // adjust status filtering is active
                            this.service.state.filter.filters.push({
                                field: 'status',
                                operator: 'eq',
                                value: 1
                            });
                            break;
                    }
                }

                // Try to add moduleId to grid data request to fetch viewSettings from the server
                if (!StringHelper.isNullOrWhiteSpace(this.viewSettingModuleId)) {
                    const filteredModuleIdStage = this.service?.state?.filter?.filters?.find(c => c['field'] ===  DynamicView.moduleKey);
                    if (!filteredModuleIdStage) {
                        this.service.state.filter.filters.push({
                            field: DynamicView.moduleKey,
                            operator: 'eq',
                            value: this.viewSettingModuleId
                        });
                    }
                }

                this.service.dataStateChange(<DataStateChangeEvent>this.service.state);
            }
        });

        // Every grid changed, handling dynamic columns
        // Handle only if value "viewSettingModuleId" is available on child list component: purchase-order-list.component.ts, pofulfillment.list.component.ts
        this.service.subscribe(gridState => {
        if (gridState && !StringHelper.isNullOrEmpty(this.viewSettingModuleId)) {
            const localGridState = this.service.state;
            const sortingColumnNames = localGridState.sort.map(y => y.field);
            const filteringColumnNames: string[] =  [];
            this.getFilterColumnNames(this.service.state.filter, filteringColumnNames);
            const currentLocalColumns = sortingColumnNames.concat(filteringColumnNames)
                            .filter( y => !StringHelper.caseIgnoredCompare(y, DynamicView.moduleKey))
                            .map(x => x.toLocaleLowerCase());
            const serverColumns = this.service.visibleColumns.map(y => y.toLowerCase());
            // Note: case ignored comparison should be used
            if (currentLocalColumns.some( y => serverColumns.indexOf(y) < 0)) {
                this.service.state = Object.assign({}, this.service.defaultState);
                LocalStorageService.remove(this.queryStateKey);
                // Try to add moduleId to grid data request to fetch viewSettings from the server
                if (!StringHelper.isNullOrWhiteSpace(this.viewSettingModuleId)) {
                    const filteredModuleIdStage = this.service?.state?.filter?.filters?.find(c => c['field'] ===  DynamicView.moduleKey);
                    if (!filteredModuleIdStage) {
                        this.service.state.filter.filters.push({
                            field: DynamicView.moduleKey,
                            operator: 'eq',
                            value: this.viewSettingModuleId
                        });
                    }
                }
                // Call method to ask grid to fetch data from server
                this.service.dataStateChange(<DataStateChangeEvent>this.service.state);
            }
        }
        });
    }

    getFilterColumnNames(filter: CompositeFilterDescriptor | FilterDescriptor , result: string[]) {
        const isCompositeFilterDescriptor =  'filters' in filter;

        if (isCompositeFilterDescriptor) {
            (filter as CompositeFilterDescriptor).filters.forEach(element => {
                this.getFilterColumnNames(element, result);
            });
        } else {
            const name: string = (filter as FilterDescriptor).field.toString();
            result.push(name);
        }
    }

    replaceState() {
        if (!this.isFormList) {
            if (!StringHelper.isNullOrEmpty(this.service.parentId)) {
                this.location.replaceState(this.statePath,
                    `state=${encodeURIComponent(JSON.stringify(this.service.state))}&parentid=${this.service.parentId}${!StringHelper.isNullOrEmpty(this.service.statisticKey) ? '&statistic=' + encodeURIComponent(this.service.statisticKey) : ''}${!StringHelper.isNullOrEmpty(this.service.statisticFilter) ? '&statisticFilter=' + encodeURIComponent(this.service.statisticFilter) : ''}${!StringHelper.isNullOrEmpty(this.service.statisticValue) ? '&statisticValue=' + encodeURIComponent(this.service.statisticValue) : ''}${!StringHelper.isNullOrEmpty(this.service.userRole) ? '&userRole=' + this.service.userRole : ''}`);
            } else {
                this.location.replaceState(this.statePath,
                    `state=${encodeURIComponent(JSON.stringify(this.service.state))}${!StringHelper.isNullOrEmpty(this.service.statisticKey) ? '&statistic=' + encodeURIComponent(this.service.statisticKey) : ''}${!StringHelper.isNullOrEmpty(this.service.statisticFilter) ? '&statisticFilter=' + encodeURIComponent(this.service.statisticFilter) : ''}${!StringHelper.isNullOrEmpty(this.service.statisticValue) ? '&statisticValue=' + encodeURIComponent(this.service.statisticValue) : ''}${!StringHelper.isNullOrEmpty(this.service.userRole) ? '&userRole=' + this.service.userRole : ''}`);
            }
        }
    }

    gridStateChange(state: DataStateChangeEvent) {
        from('1')
            .pipe(delay(500))
            .subscribe(() => {
                this.service.dataStateChange(state);
                this.saveStateToLocalStorage();
            });
    }

    gridPageSizeChange(value: any): void {
        this.saveStateToLocalStorage();
    }

    private get queryStateKey(): string {
        return `GridState_${this.listName}`;
    }

    protected readStateFromLocalStorage() {
        if (!this.isFormList && StringHelper.isNullOrEmpty(this.quickTrackNumber)) {
            let state = LocalStorageService.read<DataSourceRequestState>(this.queryStateKey);
            this.fixDateFilterState(state);
            this.service.state = !StringHelper.isNullOrEmpty(state) ? state : Object.assign({}, this.service.defaultState);
        }
        if (this.isFormList) {
            this.service.state = Object.assign({}, this.service.defaultState);
        }
    }

    protected saveStateToLocalStorage() {
        if (!this.isFormList && StringHelper.isNullOrEmpty(this.quickTrackNumber)) {
            LocalStorageService.write(this.queryStateKey, this.service.state);
            this.replaceState();
        }
    }

    private get statePath(): string {
        return `${this.listName}`;
    }

    private fixDateFilterState(state: DataSourceRequestState) {
        if (state == null || state.filter == null) {
            return;
        }
        for (let filter of state.filter.filters) {
            filter = filter as FilterDescriptor;
            const field = filter.field as string;
            if (field.endsWith('Date') ||
                field.endsWith('Time')) {
                filter.value = new Date(filter.value);
            }
        }
    }

    public allData = (): Observable<any> => {
        return this.service.queryToExport();
    }

    exportExcel() {
        this.grid.saveAsExcel();
    }
}
