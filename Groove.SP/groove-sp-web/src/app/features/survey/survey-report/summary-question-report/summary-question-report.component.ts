import { Component, Input, OnInit } from '@angular/core';
import { DataStateChangeEvent, GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import { DataSourceRequestState, SortDescriptor, toDataSourceRequestString } from '@progress/kendo-data-query';
import { startWithTap } from 'src/app/core/helpers/operators.helper';
import { SurveyReportService } from '../survey-report.service';

@Component({
  selector: 'app-summary-question-report',
  templateUrl: './summary-question-report.component.html',
  styleUrls: ['./summary-question-report.component.scss']
})
export class SummaryQuestionReportComponent implements OnInit {
  @Input() questionId: number;

  isGridLoading: boolean = false;

  // kendo-grid declarations
  gridSort: SortDescriptor[] = [];

  private defaultGridState: DataSourceRequestState = {
    sort: this.gridSort,
    skip: 0,
    take: 10
  };

  gridState: DataSourceRequestState;

  public gridData: GridDataResult = {
    data: [],
    total: 0
  };

  public pageSizes: Array<number> = [10, 20, 50];
  public pagerType: 'numeric' | 'input' = 'numeric';
  public buttonCount: number = 9;

  constructor(private service: SurveyReportService) { }

  ngOnInit() {
    this.gridState = { ...this.defaultGridState };
    this.fetchGridData();
  }

  public fetchGridData() {
    const requestString = toDataSourceRequestString(this.gridState);
    this.service
      .getSummaryQuestionReport(this.questionId, requestString)
      .pipe(
        startWithTap(() => this.isGridLoading = true)
      )
      .map(({ data, total }: GridDataResult) =>
        (<GridDataResult>{
          data: data,
          total: total
        }))
      .subscribe((res) => {
        this.gridData = res;
        this.isGridLoading = false;
      });
  }

  public gridPageChange(event: PageChangeEvent): void {
    this.gridState.skip = event.skip;
    this.fetchGridData();
  }

  public gridSortChange(sort: SortDescriptor[]): void {
    this.gridState.sort = sort;
    this.fetchGridData();
  }

  gridStateChange(state: DataStateChangeEvent) {
    this.gridState = state;
    this.fetchGridData();
  }

  public pageSizeChange(value: any): void {
    this.gridState.skip = 0;
    this.gridState.take = value;
    this.fetchGridData();
  }
}
