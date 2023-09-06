import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ColumnSetting, ListComponent, UserContextService } from 'src/app/core';
import { ShippingContainerListService } from './shipping-container-list.service';
import { Location } from '@angular/common';

@Component({
  selector: 'app-shipping-container-list',
  templateUrl: './shipping-container-list.component.html',
  styleUrls: ['./shipping-container-list.component.scss']
})
export class ShippingContainerListComponent extends ListComponent implements OnInit {

  constructor(
    service: ShippingContainerListService,
    userContext: UserContextService,
    route: ActivatedRoute,
    location: Location) {
    super(service, route, location);
    userContext.getCurrentUser().subscribe(user => {
      if (user) {
        if (!user.isInternal) {
          this.service.affiliates = user.affiliates;
          this.service.state = Object.assign({}, this.service.defaultState);
        }
      }
    });
  }

  listName = "containers";
  columns: ColumnSetting[] = [
    {
      field: 'containerNo',
      title: 'label.containerNo',
      filter: 'text',
      width: '15%'
    },
    {
      field: 'shipFrom',
      title: 'label.shipFrom',
      filter: 'text',
      width: '21%'
    },
    {
      field: 'shipTo',
      title: 'label.shipTo',
      filter: 'text',
      width: '21%'
    },
    {
      field: 'shipFromETDDate',
      title: 'label.etdDates',
      format: this.DATE_FORMAT,
      filter: 'date',
      width: '16%'
    },
    {
      field: 'shipToETADate',
      title: 'label.etaDates',
      format: this.DATE_FORMAT,
      filter: 'date',
      width: '16%',
    },
    {
      field: 'movement',
      title: 'label.movement',
      filter: 'text',
      width: '11%',
    }
  ];
}
