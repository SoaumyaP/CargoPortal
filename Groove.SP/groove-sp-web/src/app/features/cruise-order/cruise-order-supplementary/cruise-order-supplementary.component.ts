import { Component, Input, OnInit } from '@angular/core';
import { DATE_FORMAT } from 'src/app/core/helpers';
import { CruiseOrderModel } from '../models/cruise-order.model';

@Component({
  selector: 'app-cruise-order-supplementary',
  templateUrl: './cruise-order-supplementary.component.html',
  styleUrls: ['./cruise-order-supplementary.component.scss']
})
export class CruiseOrderSupplementaryComponent implements OnInit {
  @Input() cruiseOrderModel: CruiseOrderModel;
  DATE_FORMAT = DATE_FORMAT;
  defaultValue = '--';

  constructor() { }

  ngOnInit() {
  }

  returnValue(field) {
    return this.cruiseOrderModel[field] ? this.cruiseOrderModel[field] : this.defaultValue;
  }
}
