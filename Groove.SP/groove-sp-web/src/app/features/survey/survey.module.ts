import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SurveyRoutingModule } from './survey-routing.module';
import { SurveyListComponent } from './survey-list/survey-list.component';
import { UiModule } from 'src/app/ui';
import { SurveyFormComponent } from './survey-form/survey-form.component';
import { SurveyFormService } from './survey-form/survey-form.service';
import { SurveyQuestionDialogComponent } from './survey-question-dialog/survey-question-dialog.component';
import { SurveyQuestionAnswerComponent } from './survey-question-answer/survey-question-answer.component';
import { ReactiveFormsModule } from '@angular/forms';
import { SurveyReportComponent } from './survey-report/survey-report.component';
import { SurveyReportService } from './survey-report/survey-report.service';
import { PieChartQuestionReportComponent } from './survey-report/pie-chart-question-report/pie-chart-question-report.component';
import { SummaryQuestionReportComponent } from './survey-report/summary-question-report/summary-question-report.component';
import { BarChartQuestionReportComponent } from './survey-report/bar-chart-question-report/bar-chart-question-report.component';
import { SurveyTemplateComponent } from './survey-template/survey-template.component';
import { ShowLessPipe } from 'src/app/core/pipes/show-less.pipe';

@NgModule({
  declarations: [
    SurveyListComponent,
    SurveyFormComponent,
    SurveyQuestionDialogComponent,
    SurveyQuestionAnswerComponent,
    SurveyReportComponent,
    PieChartQuestionReportComponent,
    SummaryQuestionReportComponent,
    BarChartQuestionReportComponent,
    SurveyTemplateComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    SurveyRoutingModule,
    UiModule
  ],
  providers: [SurveyFormService, SurveyReportService, ShowLessPipe]
})
export class SurveyModule { }
