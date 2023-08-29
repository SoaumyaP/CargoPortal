import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { UserRequestListComponent } from './user-request-list/user-request-list.component';
import { UserRequestFormComponent } from './user-request-form/user-request-form.component';
import { AppPermissions } from 'src/app/core/auth/auth-constants';

const routes: Routes = [
    {
        path: '',
        component: UserRequestListComponent,
        data:
        {
            permission: AppPermissions.User_RequestList,
            pageName: 'userRequests'
        }
    },
    {
        path: ':mode/:id',
        component: UserRequestFormComponent,
        data:
        {
            permission: { 'view': AppPermissions.User_RequestDetail, 'edit': AppPermissions.User_RequestDetail_Edit },
            pageName: 'userRequestDetail'
        }
    },
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class UserRequestRoutingModule { }