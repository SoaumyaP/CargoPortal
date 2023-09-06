import { NgModule } from '@angular/core';
import { UiModule } from 'src/app/ui';
import { InvoiceRoutingModule } from './invoice-routing.module';
import { InvoiceListComponent } from './invoice-list/invoice-list.component';
import { InvoiceListService } from './invoice-list/invoice-list.service';
import { BillOfLadingFormService } from '../shipment/bill-of-lading/bill-of-lading-form.service';

@NgModule({
  imports: [
    InvoiceRoutingModule,
    UiModule
  ],
  declarations: [InvoiceListComponent],
  providers: [InvoiceListService]
})
export class InvoiceModule { }
