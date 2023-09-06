import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from '../../core/auth/auth-constants';
import { BuyerApprovalListComponent } from './buyer-approval-list/buyer-approval-list.component';
import { BuyerApprovalFormComponent } from './buyer-approval-form/buyer-approval-form.component';

const routes: Routes = [
    {
        path: '',
        component: BuyerApprovalListComponent,
        data:
        {
            permission: AppPermissions.Order_PendingApprovalList,
            pageName: 'pendingApprovals'
        }
    },
    {
        path: ':id',
        component: BuyerApprovalFormComponent,
        data:
        {
            permission: AppPermissions.Order_PendingApproval_Detail,
            pageName: 'approvalDetail'
        }
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class BuyerApprovalRoutingModule { }
