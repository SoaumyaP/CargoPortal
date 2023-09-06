import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ConsolidationDetailsComponent } from './consolidation-details/consolidation-details.component';
import { ConsolidationFormComponent } from './consolidation-form/consolidation-form.component';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { ConsolidationListComponent } from './consolidation-list/consolidation-list.component';

const routes: Routes = [
    {
        path: '',
        component: ConsolidationListComponent,
        data:
        {
            permission: AppPermissions.Shipment_ConsolidationList,
            pageName: 'consolidations'
        }
    },
    {
        path: ':id',
        component: ConsolidationDetailsComponent,
        data:
        {
            permission: AppPermissions.Shipment_ConsolidationDetail,
            pageName: 'consolidationDetail'
        }
    },
    {
        path: ':mode/:id',
        component: ConsolidationFormComponent,
        data:
        {
            permission: {
                'add': AppPermissions.Shipment_ConsolidationDetail_Add,
                'edit': AppPermissions.Shipment_ConsolidationDetail_Edit
            },
            pageName: 'consolidationDetail'
        }
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class ConsolidationRoutingModule { }
