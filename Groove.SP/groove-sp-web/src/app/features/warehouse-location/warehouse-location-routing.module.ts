import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { WarehouseLocationFormComponent } from './warehouse-location-form/warehouse-location-form.component';
import { WarehouseLocationListComponent } from './warehouse-location-list/warehouse-location-list.component';


const routes: Routes = [
    {
        path: '',
        component: WarehouseLocationListComponent,
        data:
        {
          permission: AppPermissions.Organization_WarehouseLocation_List,
          pageName: 'warehouseLocations'
        }
    },
    {
        path: ':mode/:id',
        component: WarehouseLocationFormComponent,
        data:
        {
            permission: {
                'view': AppPermissions.Organization_WarehouseLocation_Detail,
                'add': AppPermissions.Organization_WarehouseLocation_Detail_Add,
                'edit': AppPermissions.Organization_WarehouseLocation_Detail_Edit
            },
            pageName: 'warehouseLocationDetail'
        }
    }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class WarehouseLocationRoutingModule { }
