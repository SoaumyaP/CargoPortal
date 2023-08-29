import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DATE_FORMAT, DropDownListItemModel, ListComponent, LocalStorageService, RoutingOrderStatus, StringHelper, UserContextService } from 'src/app/core';
import { RoutingOrderListService } from './routing-order-list.service';
import { Location } from '@angular/common';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { DefaultDebounceTimeInput, GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { TranslateService } from '@ngx-translate/core';
import { faFileDownload } from '@fortawesome/free-solid-svg-icons';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, map, tap } from 'rxjs/operators';
import { DataSourceRequestState, FilterDescriptor } from '@progress/kendo-data-query';

@Component({
  selector: 'app-routing-order-list',
  templateUrl: './routing-order-list.component.html',
  styleUrls: ['./routing-order-list.component.scss']
})
export class RoutingOrderListComponent extends ListComponent implements OnInit {
  @ViewChild('excelexport', { static: false }) excelExportElement: any;

  listName = 'routing-orders';
  readonly DATE_FORMAT = DATE_FORMAT;
  readonly appPermissions = AppPermissions;
  readonly routingOrderStatus = RoutingOrderStatus;

  // Store all subscriptions, then should un-subscribe at the end
  private subscriptions: Array<Subscription> = [];

  stageFilterModel: Array<DropDownListItemModel<number>> = [];
  stageChanged$ = new Subject<string>();

  faFileDownload = faFileDownload;

  rods: any[] = [];
  isCanExport: boolean;

  constructor(
    public _listService: RoutingOrderListService,
    _userContext: UserContextService,
    _route: ActivatedRoute,
    _location: Location,
    private _translateService: TranslateService,
    private _gaService: GoogleAnalyticsService) {
    super(_listService, _route, _location);

    _userContext.getCurrentUser().subscribe(user => {
      if (user) {
        if (!user.isInternal) {
          this.service.affiliates = user.affiliates;
          this.service.state = Object.assign({}, this.service.defaultState);
        }
      }
    });
  }

  onStageChanged() {
    this.service.state.skip = 0;
    this.service.state.take = 20;
    this.setMilestoneFilterState(this.service.state);
    this.service.query(this.service.state);
  }

  setMilestoneFilterState(state: DataSourceRequestState) {
    const milestoneFilterDescriptor = {
      field: 'stage',
      operator: 'multiselect',
      value: this.stageFilterModel.map(c => c.value).toString()

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
        stageFilter.value = this.stageFilterModel.map(c => c.value).toString();
        stageFilter.operator = 'multiselect';
      }
      if (this.stageFilterModel.length === 0) {
        state.filter.filters = state.filter.filters.filter((c: any) => c.field !== 'stage');
      }
    }

    this.saveStateToLocalStorage();
  }

  isItemSelected(selectedItem: any) {
    return this.stageFilterModel.some(c => c.value === selectedItem.value);
  }

  ngOnInit() {
    this.route.paramMap.pipe(map(() => window.history.state))
      .subscribe(
        (state) => {
          if (!this.service.statisticKey) {
            let state = LocalStorageService.read<DataSourceRequestState>(`GridState_${this.listName}`);
            if (state?.filter) {
              const stageFilter: any = state.filter.filters.find((c: any) => c.field === 'stage');
              if (stageFilter) {
                this.stageFilterModel = this._listService.populateSelectedStage(stageFilter.value);
              }
            }
            else {
              const defaultFilterState: any = this.service.defaultState.filter.filters.find((c: any) => c.field === 'stage');
              if (defaultFilterState) {
                this.stageFilterModel = this._listService.populateSelectedStage(defaultFilterState.value);
              }
            }
          }
        }
      );
      
    super.ngOnInit();

    const sub = this.stageChanged$.pipe(
      debounceTime(DefaultDebounceTimeInput),
      tap((value: any) => {
        this.onStageChanged();
      }
      )).subscribe();

    this.subscriptions.push(sub);
  }

  export() {
    this.isCanExport = false;
    this.service.queryToExport().subscribe(
      r => {
        this.isCanExport = true;
        this.rods = r.data;
        for (const item of this.rods) {
          if (new Date(item.createdDate).getTime() === new Date('0001-01-01T00:00:00').getTime()) {
            item.createdDate = '01/01/0001';
          }

          // translate status name
          if (!StringHelper.isNullOrWhiteSpace(item.statusName)) {
            item.statusName = this._translateService.instant(item.statusName);
          }

          // translate stage name
          if (!StringHelper.isNullOrWhiteSpace(item.stageName)) {
            item.stageName = this._translateService.instant(item.stageName);
          }
        }
        setTimeout(() => {
          this.excelExportElement.save();
        }, 50);
      }
    );
    this._gaService.emitEvent('export', GAEventCategory.RoutingOrder, 'Export');
  }

  ngOnDestroy(): void {
    this.subscriptions.map(x => x.unsubscribe());
  }

}
