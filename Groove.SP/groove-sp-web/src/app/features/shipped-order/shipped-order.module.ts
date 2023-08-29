import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ShippedOrderRoutingModule } from './shipped-order-routing.module';
import { ShippedPurchaseOrderListComponent } from './shipped-purchase-order-list/shipped-purchase-order-list.component';
import { UiModule } from 'src/app/ui';


@NgModule({
  declarations: [ShippedPurchaseOrderListComponent],
  imports: [
    CommonModule,
    ShippedOrderRoutingModule,
    UiModule
  ]
})
export class ShippedOrderModule { }
