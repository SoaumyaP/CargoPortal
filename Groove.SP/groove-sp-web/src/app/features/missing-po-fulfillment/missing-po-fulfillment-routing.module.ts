import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from '../../core/auth/auth-constants';
import { MissingPOFulfillmentFormComponent } from './missing-po-fulfillment-form/missing-po-fulfillment-form.component';

const routes: Routes = [
    {
        path: ':mode/:id',
        component: MissingPOFulfillmentFormComponent,
        data:
        {
            permission: {
                'view': AppPermissions.PO_Fulfillment_Detail,
                'add': AppPermissions.PO_Fulfillment_Detail_Add,
                'edit': AppPermissions.PO_Fulfillment_Detail_Edit
            },
            pageName: 'poFulfillmentDetail'
        }
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class MissingPOFulfillmentRoutingModule { }
