import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { RolesListComponent } from './roles-list/roles-list.component';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { RoleFormComponent } from './role-form/role-form.component';

const routes: Routes = [
    {
        path: '',
        component: RolesListComponent,
        data:
        {
            permission: AppPermissions.User_RoleList,
            pageName: 'roles'
        }
    },
    {
        path: ':id',
        component: RoleFormComponent,
        data:
        {
            permission: AppPermissions.User_RoleDetail,
            pageName: 'roleDetail'
        }
    },
];

@NgModule({
imports: [RouterModule.forChild(routes)],
exports: [RouterModule]
})
export class RolesRoutingModule { }
