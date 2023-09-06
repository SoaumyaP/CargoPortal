import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { POProgressCheckFormComponent } from './po-progress-check-form/po-progress-check-form.component';

const routes: Routes = [
    {
        path: '',
        component: POProgressCheckFormComponent,
        data:
        {
            permission: AppPermissions.PO_ProgressCheckCRD,
            pageName: 'poProgressCheck'
        }
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class POProgressCheckRoutingModule { }
