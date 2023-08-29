import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from '../../core/auth/auth-constants';
import { ConsignmentListComponent } from './consignment-list/consignment-list.component';
import { ConsignmentFormComponent } from './consignment-form/consignment-form.component';

const routes: Routes = [
    {
        path: '',
        component: ConsignmentListComponent,
        data:
        {
            permission: AppPermissions.Consignment_List,
            pageName: 'consignments'
        }
    },
    {
        path: ':mode/:id',
        component: ConsignmentFormComponent,
        data:
        {
            permission: {
                'view': AppPermissions.Consignment_Detail,
                'edit': AppPermissions.Consignment_Detail_Edit },
            pageName: 'consignmentDetail'
        }
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class ConsignmentRoutingModule { }
