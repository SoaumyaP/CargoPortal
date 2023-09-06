import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { faRedo, faSearch } from '@fortawesome/free-solid-svg-icons';
import { GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import {
  CompositeFilterDescriptor,
  DataSourceRequestState,
  SortDescriptor,
  toDataSourceRequestString
} from '@progress/kendo-data-query';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import {
  DATE_FORMAT,
  FormModeType,
  HttpService,
  StringHelper,
  UserContextService 
} from 'src/app/core';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-po-fulfillment-copy-popup',
  templateUrl: './po-fulfillment-copy-popup.component.html',
  styleUrls: ['./po-fulfillment-copy-popup.component.scss']
})
export class POFulfillmentCopyPopupComponent implements OnInit {
  @Input('open') isOpen: boolean;

  @Output()
  close: EventEmitter<any> = new EventEmitter<any>();

  @ViewChild('mainForm', { static: false }) mainForm: NgForm;

  readonly DATE_FORMAT = DATE_FORMAT;
  faSearch = faSearch;
  faRedo = faRedo;
  model = new CopyBookingPopupModel;
  _currentUser: any;
  formType = FormModeType;
  
  public pageSizes: Array<number> = [5, 10, 20, 50];
  public pagerType: 'numeric' | 'input' = 'numeric';
  public buttonCount: number = 9;
  gridSort: SortDescriptor[] = [
    {
      field: 'number',
      dir: 'desc'
    }
  ];
  gridFilter: CompositeFilterDescriptor = {
      filters: [],
      logic: 'and'
  };

  private defaultGridState: DataSourceRequestState = {
    sort: this.gridSort,
    skip: 0,
    take: 5
  };

  gridState: DataSourceRequestState;

  public gridData: GridDataResult = {
    data: [],
    total: 0
  };
  gridLoading: boolean = false;

  constructor(private _httpService: HttpService, private _userContext: UserContextService) {
    this.gridState = { ...this.defaultGridState };
    this._getCurrentUser$.pipe(
      tap(() => {
        this.fetchGridData()
      })
    ).subscribe();
  }

  ngOnInit() { 
  }

  private get _getCurrentUser$(): Observable<any> {
    if (this._currentUser) {
      return of(this._currentUser);
    } else {
      return this._userContext.getCurrentUser().pipe(
        tap((user: any) => {
          this._currentUser = user;
        })
      );
    }
  }

  /**Click on search button*/
  onSubmit() {
    this.gridState = { ...this.defaultGridState };
    this.buildGridFilter();
    this.fetchGridData();
  }

  resetForm () {
    this.mainForm.reset();
  }

  public fetchGridData() {  
    let url = `${environment.apiUrl}/bulkFulfillments/search?${toDataSourceRequestString(this.gridState)}`;

    let {isInternal, affiliates, organizationId} = this._currentUser;
    if (!isInternal) {
      url += `&affiliates=${affiliates}&organizationId=${organizationId}`
    }
    this.gridLoading = true;
    this._httpService
      .get(`${url}`)
      .map(
        ({ data, total }: GridDataResult) => (
          <GridDataResult>{
            data: data,
            total: total
          }
        )
      ).subscribe(
        (res) => {
          this.gridData = res;
          this.gridLoading = false;
        }
      );
  }

  public gridPageChange(event: PageChangeEvent): void {
    this.gridState.skip = event.skip;
    this.fetchGridData();
  }

  public gridSortChange(sort: SortDescriptor[]): void {
    this.gridState.sort = sort;
    this.fetchGridData();
  }

  public pageSizeChange(value: any): void {
    this.gridState.skip = 0;
    this.gridState.take = value;
    this.fetchGridData();
  }

  buildGridFilter() {
    this.gridFilter.filters = [];
    this.pushGridFilter('number', this.model.poFulfillmentNumber, 'contains');
    this.pushGridFilter('shipFromName', this.model.shipFrom, 'contains');
    this.pushGridFilter('shipToName', this.model.shipTo, 'contains');
    this.pushGridFilter('cargoReadyDate', this.model.cargoReadyDate, 'eq');
    
    this.gridState.filter = this.gridFilter;
  }

  pushGridFilter(field: string, value: any, operator: string) {
    if (StringHelper.isNullOrWhiteSpace(value)) {
      return;
    }

    this.gridFilter.filters.push({
      field,
      operator,
      value
    });
  }

  onClosePopup(): void {
    this.isOpen = false;
    this.close.emit();
  }

}

export class CopyBookingPopupModel {
  poFulfillmentNumber: string;
  cargoReadyDate: Date;
  shipFrom: string;
  shipTo: string;
}