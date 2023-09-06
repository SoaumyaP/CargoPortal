import { Component, Input, OnInit } from '@angular/core';
import { RoutingOrderModel } from 'src/app/core/models/routing-order.model';

@Component({
  selector: 'app-routing-order-additional-info',
  templateUrl: './routing-order-additional-info.component.html',
  styleUrls: ['./routing-order-additional-info.component.scss']
})
export class RoutingOrderAdditionalInfoComponent implements OnInit {
  @Input() model: RoutingOrderModel;
  @Input() tabPrefix: string = '';
  @Input() readonly: boolean = false;

  constructor() { }

  ngOnInit() {
  }

}