import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { CarrierListComponent } from './carrier-list/carrier-list.component';

const routes: Routes = [
    {
        path: '',
        component: CarrierListComponent,
        data:
        {
            permission: AppPermissions.Organization_Carrier_List,
            pageName: 'carriers'
        }
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class CarrierRoutingModule { }