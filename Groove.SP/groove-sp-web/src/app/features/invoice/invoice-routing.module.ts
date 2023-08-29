import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { InvoiceListComponent } from './invoice-list/invoice-list.component';
import { AppPermissions } from 'src/app/core/auth/auth-constants';

const routes: Routes = [
    {
        path: '',
        component: InvoiceListComponent,
        data:
        {
            permission: AppPermissions.Invoice_List,
            pageName: 'invoices'
        }
    }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class InvoiceRoutingModule { }
