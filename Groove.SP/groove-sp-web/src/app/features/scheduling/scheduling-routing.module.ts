import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { SchedulingFormComponent } from './scheduling-form/scheduling-form.component';
import { SchedulingListComponent } from './scheduling-list/scheduling-list.component';



const routes: Routes = [
    {
        path: '',
        component: SchedulingListComponent,
        data:
        {
            permission: AppPermissions.Reports_TaskList,
            pageName: 'listOfTasks'
        },
    },
    {
        path: ':mode/:id',
        component: SchedulingFormComponent,
        data:
        {
            permission: {
                'view': AppPermissions.Reports_TaskDetail,
                'add': AppPermissions.Reports_TaskDetail_Add,
                'edit': AppPermissions.Reports_TaskDetail_Edit
            },
            pageName: 'task'
        }
    }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SchedulingRoutingModule { }
