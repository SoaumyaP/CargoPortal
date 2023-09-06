import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { BulkFulfillmentFormComponent } from './bulk-fulfillment-form/bulk-fulfillment-form.component';


const routes: Routes = [
  {
    path: ':mode/:id',
    component: BulkFulfillmentFormComponent,
    data:
    {
        pageName: 'bookingDetail'
    }
}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class BulkFulfillmentRoutingModule { }
