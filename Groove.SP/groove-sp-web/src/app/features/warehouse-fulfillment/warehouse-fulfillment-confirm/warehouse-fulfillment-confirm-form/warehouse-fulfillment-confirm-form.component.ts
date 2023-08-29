import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-warehouse-fulfillment-confirm-form',
  templateUrl: './warehouse-fulfillment-confirm-form.component.html',
  styleUrls: ['./warehouse-fulfillment-confirm-form.component.scss']
})
export class WarehouseFulfillmentConfirmFormComponent implements OnInit {
  warehouseBookings: any = [];

  constructor(
    private router: Router
  ) { }


  ngOnInit() {
  }

  onSearchWarehouseBooking(data) {
    this.warehouseBookings = data;
  }

  onResetFilter() {
    this.warehouseBookings = [];
  }

  backToBookingList() {
    this.router.navigate(['po-fulfillments']);
  }
}
