import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from '../../core/auth/auth-constants';
import { RoutingOrderFormComponent } from './routing-order-form/routing-order-form.component';
import { RoutingOrderListComponent } from './routing-order-list/routing-order-list.component';

const routes: Routes = [
    {
        path: '',
        component: RoutingOrderListComponent,
        data:
        {
            permission: AppPermissions.RoutingOrder_List,
            pageName: 'routingOrders'
        }
    },
    {
        path: ':mode/:id',
        component: RoutingOrderFormComponent,
        data:
        {
            permission: {
                'view': AppPermissions.RoutingOrder_Detail,
                'edit': AppPermissions.RoutingOrder_Detail_Edit
            },
            pageName: 'routingOrderDetail'
        }
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class RoutingOrderRoutingModule { }
