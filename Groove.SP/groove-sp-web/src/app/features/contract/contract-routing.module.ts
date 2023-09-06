import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { ContractFormComponent } from './contract-form/contract-form.component';
import { ContractListComponent } from './contract-list/contract-list.component';


const routes: Routes = [
  {
    path: '',
    component: ContractListComponent,
    data:
    {
      permission: AppPermissions.Organization_Contract_List,
      pageName: 'contracts'
    }
  },
  {
    path: ':mode/:id', component: ContractFormComponent,
    data:
    {
      pageName: 'contractDetail',
      permission: {
        'view': AppPermissions.Organization_Contract_Detail,
        'add': AppPermissions.Organization_Contract_Detail_Add,
        'edit': AppPermissions.Organization_Contract_Detail_Edit
      }
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ContractRoutingModule { }
