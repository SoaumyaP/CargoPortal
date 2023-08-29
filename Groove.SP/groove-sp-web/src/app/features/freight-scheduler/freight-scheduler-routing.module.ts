import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { FreightSchedulerListComponent } from './freight-scheduler-list/freight-scheduler-list.component';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { FreightScheduleDetailComponent } from './freight-schedule-detail/freight-schedule-detail.component';

const routes: Routes = [
    {
        path: '',
        component: FreightSchedulerListComponent,
        data:
        {
            permission: AppPermissions.FreightScheduler_List,
            pageName: 'freightSchedulers'
        }
    },
    {
        path: 'schedule-detail/:id',
        component: FreightScheduleDetailComponent,
        data:
        {
            permission: AppPermissions.FreightScheduler_List,
            pageName: 'freightScheduleDetail'
        }
    },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class FreightSchedulerRoutingModule { }
