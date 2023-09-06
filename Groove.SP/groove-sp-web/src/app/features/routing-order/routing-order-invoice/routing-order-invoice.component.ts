import { Component, Input, OnInit } from '@angular/core';
import { RoutingOrderInvoiceModel } from 'src/app/core/models/routing-order.model';

@Component({
  selector: 'app-routing-order-invoice',
  templateUrl: './routing-order-invoice.component.html',
  styleUrls: ['./routing-order-invoice.component.scss']
})
export class RoutingOrderInvoiceComponent implements OnInit {
  @Input() invoices: RoutingOrderInvoiceModel[] = [];
  constructor() { }

  ngOnInit() {
  }

}
