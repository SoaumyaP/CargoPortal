import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { VesselListComponent } from './vessel-list/vessel-list.component';

const routes: Routes = [
    {
        path: '',
        component: VesselListComponent,
        data:
        {
            permission: AppPermissions.Organization_Vessel_List,
            pageName: 'vessels'
        }
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class VesselRoutingModule { }