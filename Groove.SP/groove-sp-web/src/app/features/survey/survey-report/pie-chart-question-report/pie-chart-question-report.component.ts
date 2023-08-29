import { Component, Input, OnInit } from '@angular/core';
import { LegendLabelsContentArgs } from '@progress/kendo-angular-charts';
import { IntlService } from '@progress/kendo-angular-intl';
import { startWithTap } from 'src/app/core/helpers/operators.helper';
import { ShowLessPipe } from 'src/app/core/pipes/show-less.pipe';
import { SurveyReportService } from '../survey-report.service';

@Component({
  selector: 'app-pie-chart-question-report',
  templateUrl: './pie-chart-question-report.component.html',
  styleUrls: ['./pie-chart-question-report.component.scss']
})
export class PieChartQuestionReportComponent implements OnInit {
  @Input() questionId: number;

  pieChartData: any;
  hasOtherAnswer: boolean = false;
  otherAnswers: any[] = [];
  isInitDataLoaded: boolean = false;

  pieChartColors = [
    {
      seq: 0,
      color: '#ff6358'
    },
    {
      seq: 1,
      color: '#ffd246'
    },
    {
      seq: 2,
      color: '#78d237'
    },
    {
      seq: 3,
      color: '#28b4c8'
    },
    {
      seq: 4,
      color: '#2d73f5'
    },
    {
      seq: 5,
      color: '#aa46be'
    },
    {
      seq: 6,
      color: '#f4891d'
    },
    {
      seq: 7,
      color: '#d90b91'
    },
    {
      seq: 8,
      color: '#feaa91'
    },
    {
      seq: 9,
      color: '#775549'
    },
  ];

  constructor(private service: SurveyReportService, private intl: IntlService, private showLessPipe: ShowLessPipe) {
    this.labelContent = this.labelContent.bind(this);
    this.pointColor = this.pointColor.bind(this);
  }

  ngOnInit() {

    this.service.getPieChartQuestionReport(this.questionId)
      .pipe(
        startWithTap(() => this.isInitDataLoaded = false)
      )
      .subscribe(
        (res: any) => {
          this.pieChartData = res.categories;

          let c = 0;
          for (let i = 0; i < this.pieChartData.length; i++) {
            let el = this.pieChartData[i];
            el.fullCategoryContent = el.category;
            el.category = this.showLessPipe.transform(el.category, 50);
            el.colorSeq = c++;
            
            // reset to the first color.
            if (c > this.pieChartColors.length) {
              c = 0;
            }
          }

          this.hasOtherAnswer = this.pieChartData.findIndex(x => x.fullCategoryContent === 'Other') !== -1;

          if (this.hasOtherAnswer) {
            this.otherAnswers = res.otherAnswers;
          }

          const total = this.sumResponseTotal;
          this.pieChartData.forEach(el => {
            el.percentage = (el.value / total);
          });

          this.isInitDataLoaded = true;
        }
      )
  }

  get sumResponseTotal() {
    let sum = 0;
    for (let i = 0; i < this.pieChartData.length; i++) {
      sum += this.pieChartData[i].value;
    }

    return sum;
  }

  public rowCallback(args) {
    return {
      'expandable': args.dataItem.category === 'Other'
    };
  }

  public labelContent(args: LegendLabelsContentArgs): string {
    const a = this.intl.formatNumber(
      args.dataItem.value / this.sumResponseTotal,
      "p2"
    );
    if (a === '0.00%') {
      return null;
    }
    return `${args.dataItem.category}: ${a}`;
  }

  public pointColor(point): string {
    return this.pieChartColors.find(c => c.seq === point.dataItem.colorSeq).color;
  }
}