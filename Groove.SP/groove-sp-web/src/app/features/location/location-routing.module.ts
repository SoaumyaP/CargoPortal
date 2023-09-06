import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { LocationListComponent } from './location-list/location-list.component';


const routes: Routes = [
  {
    path: '',
    component: LocationListComponent,
    data:
    {
      permission: AppPermissions.Organization_Location_List,
      pageName: 'locations'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LocationRoutingModule { }
