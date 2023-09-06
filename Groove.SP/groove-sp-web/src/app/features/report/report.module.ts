import { NgModule } from '@angular/core';

import { ReportRoutingModule } from './report-routing.module';
import { ReportListComponent } from './report-list/report-list.component';
import { UiModule } from 'src/app/ui';
import { ReportPermissionFormComponent } from './report-permission-form/report-permission-form.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TelerikReportComponent } from './telerik-report/telerik-report.component';
import { TelerikReportingModule } from '@progress/telerik-angular-report-viewer';
import { SchedulerModule } from '@progress/kendo-angular-scheduler';

@NgModule({
  declarations: [
    ReportListComponent,
    ReportPermissionFormComponent,
    TelerikReportComponent,
  ],
  imports: [
    UiModule,
    ReportRoutingModule,
    FormsModule,
    ReactiveFormsModule,
    TelerikReportingModule,
    SchedulerModule
  ],
  exports: [
    TelerikReportingModule,
    SchedulerModule
  ],
  providers: []
})
export class ReportModule { }
