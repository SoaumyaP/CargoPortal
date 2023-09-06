import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from '../../core/auth/auth-constants';
import { CruiseOrderDetailComponent } from './cruise-order-detail/cruise-order-detail.component';
import { CruiseOrderListComponent } from './cruise-order-list/cruise-order-list.component';

const routes: Routes = [
    {
        path: '',
        component: CruiseOrderListComponent,
        data:
        {
            permission: AppPermissions.CruiseOrder_List,
            pageName: 'cruiseOrders'
        }
    },
    {
        path: ':mode/:id',
        component: CruiseOrderDetailComponent,
        data:
        {
            permission: {
                'view': AppPermissions.CruiseOrder_Detail,
                'edit': AppPermissions.CruiseOrder_Detail_Edit },
            pageName: 'cruiseOrderDetail'
        }
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class CruiseOrderRoutingModule { }
