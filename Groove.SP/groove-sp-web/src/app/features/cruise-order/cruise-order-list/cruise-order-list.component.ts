import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ColumnSetting, ListComponent } from 'src/app/core';
import { Location } from '@angular/common';
import { CruiseOrderListService } from './cruise-order-list.service';
@Component({
  selector: 'app-cruise-order-list',
  templateUrl: './cruise-order-list.component.html',
  styleUrls: ['./cruise-order-list.component.scss']
})
export class CruiseOrderListComponent extends ListComponent implements OnInit {
  listName = "cruise-orders";

  columns: ColumnSetting[] = [
    {
        field: 'poNumber',
        title: 'label.poNumber',
        filter: 'text',
        class: 'link-code',
        width: '20%'
    },
    {
        field: 'poDate',
        title: 'label.poDates',
        filter: 'date',
        format: this.DATE_FORMAT,
        width: '15%'
    },
    {
        field: 'consignee',
        title: 'label.consignee',
        filter: 'text',
        width: '20%'
    },
    {
        field: 'supplier',
        title: 'label.supplier',
        filter: 'text',
        width: '20%'
    },
    // {
    //     field: 'milestone',
    //     title: 'Milestone',
    //     filter: 'text',
    //     width: '15%',
    // },
    {
        field: 'poStatus',
        title: 'label.status',
        filter: 'text',
        width: '10%',
    }
  ];

  constructor(service: CruiseOrderListService, route: ActivatedRoute, location: Location) {
    super(service, route, location);
  }

}
