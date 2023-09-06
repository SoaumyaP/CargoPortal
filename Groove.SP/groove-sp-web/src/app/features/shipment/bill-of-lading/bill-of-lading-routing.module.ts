import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { BillOfLadingComponent } from './bill-of-lading.component';
import { AppPermissions } from '../../../core/auth/auth-constants';
import { BillOfLadingListComponent } from './bill-of-lading-list/bill-of-lading-list.component';
import { BillOfLadingFormComponent } from './bill-of-lading-form/bill-of-lading-form.component';

const routes: Routes = [
    {
        path: '',
        component: BillOfLadingListComponent,
        data:
        {
            permission: AppPermissions.BillOfLading_ListOfHouseBL,
            pageName: 'houseBill'
        }
    },
    {
        path: ':mode/:id',
        component: BillOfLadingFormComponent,
        data:
        {
            permission: {
                'edit': AppPermissions.BillOfLading_HouseBLDetail_Edit
            }
        }
    },
    {
        path: ':id',
        component: BillOfLadingComponent,
        data:
        {
            permission: AppPermissions.BillOfLading_HouseBLDetail,
            pageName: 'houseBillDetail'
        }
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class BillOfLadingRoutingModule { }
