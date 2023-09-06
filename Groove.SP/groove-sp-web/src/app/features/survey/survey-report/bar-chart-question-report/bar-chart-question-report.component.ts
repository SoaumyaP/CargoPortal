import { Component, Input, OnInit } from '@angular/core';
import { LegendLabelsContentArgs, SeriesLabels } from '@progress/kendo-angular-charts';
import { startWithTap } from 'src/app/core/helpers/operators.helper';
import { SurveyReportService } from '../survey-report.service';

@Component({
  selector: 'app-bar-chart-question-report',
  templateUrl: './bar-chart-question-report.component.html',
  styleUrls: ['./bar-chart-question-report.component.scss']
})
export class BarChartQuestionReportComponent implements OnInit {
  @Input() questionId: number;
  @Input() completedTotal: number = 0;
  
  categories: string[];
  data: number[];
  totalData: number[];
  barChartData: any;

  hasOtherAnswer: boolean = false;
  otherAnswers: any[] = [];
  isInitDataLoaded: boolean = false;

  seriesLabelsSetting: SeriesLabels = {
    visible: true,
    background: 'none',
    color: '#fff',
    position: 'insideEnd', // left
    content: this.labelContent
  }

  constructor(private service: SurveyReportService) {
  }

  ngOnInit() {
    this.service.getBarChartQuestionReport(this.questionId)
      .pipe(
        startWithTap(() => this.isInitDataLoaded = false)
      )
      .subscribe(
        (res: any) => {
          this.barChartData = res.categories;
          this.categories = res.categories.map(x => x.category);
          this.data = res.categories.map(x => x.value);
          this.totalData = res.categories.map(x => this.completedTotal);

          this.hasOtherAnswer = this.categories.findIndex(x => x === 'Other') !== -1;
          if (this.hasOtherAnswer) {
            this.otherAnswers = res.otherAnswers;
          }

          this.isInitDataLoaded = true;
        }
      )
  }

  get sumResponseTotal() {
    let sum = 0;
    for (let i = 0; i < this.barChartData.length; i++) {
      sum += this.barChartData[i].value;
    }

    return sum;
  }
  
  public rowCallback(args) {
    return {
      'expandable': args.dataItem.category === 'Other'
    };
  }

  public labelContent(args: LegendLabelsContentArgs): string {
    return args.value === 0 ? '' : args.value;
  }
}