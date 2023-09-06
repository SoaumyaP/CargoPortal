import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { OrganizationListComponent } from './organization-list/organization-list.component';
import { OrganizationFormComponent } from './organization-form/organization-form.component';
import { AppPermissions } from 'src/app/core/auth/auth-constants';

const routes: Routes = [
    {
        path: '', component: OrganizationListComponent,
        data:
        {
            pageName: 'organizations',
            permission: AppPermissions.Organization_List
        }
    },
    {
        path: ':mode/:id', component: OrganizationFormComponent,
        data:
        {
            pageName: 'organizationDetail',
            permission: {
                'view': AppPermissions.Organization_Detail,
                'edit': AppPermissions.Organization_Detail_Edit,
                'add': AppPermissions.Organization_Detail_Add
            }
        }
    },
    {
        path: 'owner/:mode/:id', component: OrganizationFormComponent,
        data:
        {
            pageName: 'organizationDetail'
        }
    },
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class OrganizationsRoutingModule { }