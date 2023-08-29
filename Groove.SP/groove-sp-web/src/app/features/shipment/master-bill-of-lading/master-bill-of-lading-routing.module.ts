import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { MasterBillOfLadingListComponent } from './master-bill-of-lading-list/master-bill-of-lading-list.component';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { MasterBillOfLadingFormComponent } from './master-bill-of-lading-form/master-bill-of-lading-form.component';

const routes: Routes = [
    {
        path: '',
        component: MasterBillOfLadingListComponent,
        data:
        {
            permission: AppPermissions.BillOfLading_ListOfMasterBL,
            pageName: 'masterBill'
        }
    },
    {
        path: ':id',
        component: MasterBillOfLadingFormComponent,
        data:
        {
            permission: AppPermissions.BillOfLading_MasterBLDetail,
            pageName: 'masterBillDetail'
        }
    },
    {
        path: ':mode/:id',
        component: MasterBillOfLadingFormComponent,
        data:
        {
            permission: {
                'edit': AppPermissions.BillOfLading_MasterBLDetail_Edit
            },
            pageName: 'masterBillDetail'
        }
    },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class MasterBillOfLadingRoutingModule { }
