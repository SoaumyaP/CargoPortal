import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { WarehouseFulfillmentConfirmFormComponent } from './warehouse-fulfillment-confirm-form/warehouse-fulfillment-confirm-form.component';


const routes: Routes = [
  {
    path: '',
    component: WarehouseFulfillmentConfirmFormComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class WarehouseFulfillmentConfirmRoutingModule { }
