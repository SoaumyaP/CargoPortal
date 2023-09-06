import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from '../../core/auth/auth-constants';
import { WarehouseFulfillmentFormComponent } from './warehouse-fulfillment-form/warehouse-fulfillment-form.component';

const routes: Routes = [
    {
        path: ':mode/:id',
        component: WarehouseFulfillmentFormComponent,
        data:
        {
            permission: {
                'view': AppPermissions.PO_Fulfillment_Detail,
                'add': AppPermissions.PO_Fulfillment_Detail_Add,
                'edit': AppPermissions.PO_Fulfillment_Detail_Edit
            },
            pageName: 'bookingDetail'
        }
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class WarehouseFulfillmentRoutingModule { }
