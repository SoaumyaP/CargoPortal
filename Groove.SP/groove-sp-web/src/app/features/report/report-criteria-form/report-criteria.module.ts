import { NgModule } from '@angular/core';
import { NotBookedStatusReportFormComponent } from './not-booked-status-report-form/not-booked-status-report-form.component';
import { BookedStatusReportFormComponent } from './booked-status-report-form/booked-status-report-form.component';
import { UiModule } from 'src/app/ui';
import { ReportCriteriaRoutingModule } from './report-criteria-routing.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ReportCriteriaFormService } from './report-criteria-form.service';
import { MasterSummaryReportFormComponent } from './master-summary-report-form/master-summary-report-form.component';
import { CommonService } from 'src/app/core/services/common.service';


@NgModule({
  declarations: [
    BookedStatusReportFormComponent,
    NotBookedStatusReportFormComponent,
    MasterSummaryReportFormComponent
  ],
  imports: [
    UiModule,
    ReportCriteriaRoutingModule,
    FormsModule,
    ReactiveFormsModule
  ],
  providers: [ReportCriteriaFormService, CommonService]
})
export class ReportCriteriaModule { }
