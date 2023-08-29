import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from '../../core/auth/auth-constants';
import { ShortshipListComponent } from './shortship-list/shortship-list.component';

const routes: Routes = [
    {
        path: '',
        component: ShortshipListComponent,
        data:
        {
            pageName: 'shortShips',
            permission: AppPermissions.Dashboard_Shortships_List,
        }
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class ShortshipRoutingModule { }
