import { Component, Input, OnInit } from '@angular/core';
import { DATE_FORMAT, DATE_HOUR_FORMAT } from 'src/app/core';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';
import { WarehouseFulfillmentModel } from '../models/warehouse-fulfillment.model';

@Component({
  selector: 'app-warehouse-fulfillment-warehouse',
  templateUrl: './warehouse-fulfillment-warehouse.component.html',
  styleUrls: ['./warehouse-fulfillment-warehouse.component.scss']
})
export class WarehouseFulfillmentWarehouseComponent implements OnInit {
  @Input() model: WarehouseFulfillmentModel;
  DATE_FORMAT = DATE_FORMAT;
  DATE_HOUR_FORMAT = DATE_HOUR_FORMAT;
  defaultValue = DefaultValue2Hyphens;
  
  constructor() { }

  ngOnInit() {
  }

}
