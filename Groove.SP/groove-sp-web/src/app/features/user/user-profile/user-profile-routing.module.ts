import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { UserProfileFormComponent } from './user-profile.component';

const routes: Routes = [
    {
        path: '',
        component: UserProfileFormComponent,
        data:
        {
            pageName: 'userProfile'
        }
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class UserProfileRoutingModule { }