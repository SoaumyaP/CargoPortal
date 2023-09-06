import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ReportListComponent } from './report-list/report-list.component';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { TelerikReportComponent } from './telerik-report/telerik-report.component';


const routes: Routes = [
    {
        path: '',
        component: ReportListComponent,
        data:
        {
            permission: AppPermissions.Reports_List,
            pageName: 'reports'
        }
    },
    {
        path: 'telerik-report',
        component: TelerikReportComponent,
        data:
        {
            permission: AppPermissions.Reports_List,
            pageName: 'reports'
        }
    },
    {
        path: '',
        data:
        {
            permission: AppPermissions.Reports_List,
            pageName: 'export'
        },
        loadChildren: () => import('./report-criteria-form/report-criteria.module').then(m => m.ReportCriteriaModule),
    }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ReportRoutingModule { }
