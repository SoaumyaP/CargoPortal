import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { BookedStatusReportFormComponent } from './booked-status-report-form/booked-status-report-form.component';
import { MasterSummaryReportFormComponent } from './master-summary-report-form/master-summary-report-form.component';
import { NotBookedStatusReportFormComponent } from './not-booked-status-report-form/not-booked-status-report-form.component';


const routes: Routes = [
    {
        path: 'booked-status-report',
        component: BookedStatusReportFormComponent,
    },
    {
        path: 'not-booked-status-report',
        component: NotBookedStatusReportFormComponent,
    },
    {
        path: 'master-summary-report',
        component: MasterSummaryReportFormComponent,
    }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ReportCriteriaRoutingModule { }
