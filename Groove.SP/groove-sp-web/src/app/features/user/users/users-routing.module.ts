import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { UsersListComponent } from './users-list/users-list.component';
import { UserFormComponent } from './user-form/user-form.component';
import { AppPermissions } from 'src/app/core/auth/auth-constants';

const routes: Routes = [
    {
        path: '',
        component: UsersListComponent,
        data:
        {
            permission: AppPermissions.User_UserList,
            pageName: 'users'
        }
    },
    {
        path: ':id',
        component: UserFormComponent,
        data:
        {
            permission: AppPermissions.User_UserDetail,
            pageName: 'userDetail'
        }
    },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UsersRoutingModule { }
