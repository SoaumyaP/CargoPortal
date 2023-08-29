import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { IntegrationLogListComponent } from './intergration-log-list/integration-log-list.component';

const routes: Routes = [
    {
        path: '', component: IntegrationLogListComponent,
        data:
        {
            pageName: 'integrationLogs',
            permission: AppPermissions.IntegrationLog_List
        }
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule],
})
export class IntegrationLogRoutingModule { }
