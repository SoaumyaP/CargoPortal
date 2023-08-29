import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from '../../core/auth/auth-constants';
import { PurchaseOrderListComponent } from './purchase-order-list/purchase-order-list.component';
import { PurchaseOrderDetailComponent } from './purchase-order-detail/purchase-order-detail.component';

const routes: Routes = [
    {
        path: '',
        component: PurchaseOrderListComponent,
        data:
        {
            permission: AppPermissions.PO_List,
            pageName: 'purchaseOrders'
        }
    },
    {
        path: 'search/:itemNo',
        component: PurchaseOrderListComponent,
        data:
        {
            permission: AppPermissions.PO_List,
            pageName: 'purchaseOrders'
        }
    },
    {
        path: ':id',
        component: PurchaseOrderDetailComponent,
        data:
        {
            permission: AppPermissions.PO_Detail,
            pageName: 'purchaseOrderDetail'
        }
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class OrderRoutingModule { }
