import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { MasterDialogFormComponent } from './master-dialog-form/master-dialog-form.component';
import { MasterDialogListComponent } from './master-dialog-list/master-dialog-list.component';


const routes: Routes = [
  {
    path: '',
    component: MasterDialogListComponent,
    data:
    {
        permission: AppPermissions.MasterDialog_List,
        pageName: 'masterDialogs'
    }
  },
  {
    path: ':mode/:id',
    component: MasterDialogFormComponent,
    data:
    {
        permission: {
            'add': AppPermissions.MasterDialog_List_Add,
            'edit': AppPermissions.MasterDialog_List_Edit,
            'view': AppPermissions.MasterDialog_List
        },
        pageName: 'masterDialogDetail',
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class MasterDialogRoutingModule { }
