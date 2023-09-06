import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from '../../core/auth/auth-constants';
import { POFulfillmentFormComponent } from './po-fulfillment-form/po-fulfillment-form.component';
import { POFulfillmentListComponent } from './po-fulfillment-list/po-fulfillment-list.component';

const routes: Routes = [
    {
        path: '',
        component: POFulfillmentListComponent,
        data:
        {
            permission: AppPermissions.PO_Fulfillment_List,
            pageName: 'poFulfillments'
        }
    },
    {
        path: ':mode/:id',
        component: POFulfillmentFormComponent,
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
export class POFulfillmentRoutingModule { }
