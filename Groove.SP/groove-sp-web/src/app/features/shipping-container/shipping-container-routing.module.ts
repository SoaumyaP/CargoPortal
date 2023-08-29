import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { ContainerTrackingComponent } from './container-tracking/container-tracking.component';
import { ShippingContainerListComponent } from './shipping-container-list/shipping-container-list.component';


const routes: Routes = [
  {
    path: "",
    component: ShippingContainerListComponent,
    data:
    {
      permission: AppPermissions.Shipment_Container_List,
      pageName: 'containers'
    }
  },
  {
    path: ':id',
    component: ContainerTrackingComponent,
    data:
    {
        permission: AppPermissions.Shipment_ContainerDetail,
        pageName: 'containerDetail'
    }
  },
  {
    path: ':mode/:id',
    component: ContainerTrackingComponent,
    data:
    {
        permission: {
            'edit': AppPermissions.Shipment_ContainerDetail_Edit
        },
        pageName: 'containerDetail'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ShippingContainerRoutingModule { }
