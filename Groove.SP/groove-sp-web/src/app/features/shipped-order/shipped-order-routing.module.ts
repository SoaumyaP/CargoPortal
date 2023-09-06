import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { ShippedPurchaseOrderListComponent } from './shipped-purchase-order-list/shipped-purchase-order-list.component';


const routes: Routes = [
    {
        path: '',
        component: ShippedPurchaseOrderListComponent,
        data:
        {
            permission: AppPermissions.Shipment_ShippedPO_List,
            pageName: 'shippedPurchaseOrders'
        }
    }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ShippedOrderRoutingModule { }
