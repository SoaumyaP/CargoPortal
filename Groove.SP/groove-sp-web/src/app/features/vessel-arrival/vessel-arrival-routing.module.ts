import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { VesselArrivalListComponent } from './vessel-arrival-list/vessel-arrival-list.component';


const routes: Routes = [
  {
    path: '',
    component: VesselArrivalListComponent,
    data:
    {
      permission: AppPermissions.Dashboard_VesselArrival_List,
      pageName: 'vesselArrival'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class VesselArrivalRoutingModule { }
