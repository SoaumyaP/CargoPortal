import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { MasterEventListComponent } from './master-event-list/master-event-list.component';


const routes: Routes = [
  {
    path: '',
    component: MasterEventListComponent,
    data:
    {
        pageName: 'masterEvents'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class MasterEventRoutingModule { }
