import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ColumnSetting, ConsolidationStageName, ListComponent, UserContextService } from 'src/app/core';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { ConsolidationListService } from './consolidation-list.service';
import { Location } from '@angular/common';
import { AppPermissions } from 'src/app/core/auth/auth-constants';

@Component({
  selector: 'app-consolidation-list',
  templateUrl: './consolidation-list.component.html',
  styleUrls: ['./consolidation-list.component.scss']
})
export class ConsolidationListComponent extends ListComponent implements OnInit {
  listName = 'consolidations';
  
  ConsolidationStageName = ConsolidationStageName;

  columns: ColumnSetting[] = [
    {
      field: 'consolidationNo',
      title: 'label.loadPlanId',
      filter: 'text',
      width: '13%',
      sortable: true
    },
    {
      field: 'shipmentNo',
      title: 'label.shipmentNo',
      filter: 'text',
      width: '13%',
      sortable: true
    },
    {
      field: 'originCFS',
      title: 'label.originCFS',
      filter: 'text',
      width: '16%',
      sortable: true
    },
    {
      field: 'cfsCutoffDate',
      title: 'label.cfsCutoffDates',
      filter: 'date',
      width: '12%',
      sortable: true
    },
    {
      field: 'containerNo',
      title: 'label.containerNo',
      filter: 'text',
      width: '13%',
      sortable: true
    },
    {
      field: 'loadingDate',
      title: 'label.loadingDates',
      filter: 'date',
      width: '12%',
      sortable: true
    },
    {
      field: 'equipmentType',
      title: 'label.equipmentType',
      filter: 'text',
      width: '12%',
      sortable: true
    },
    {
      field: 'stage',
      title: 'label.stage',
      filter: 'text',
      width: '9%',
      sortable: true
    }
  ];

  constructor(
    public service: ConsolidationListService,
    route: ActivatedRoute,
    location: Location,
    private _userContext: UserContextService,
    public notification: NotificationPopup) {
    super(service, route, location);
    this._userContext.getCurrentUser().subscribe(user => {
      if (user) {
        if (!user.isInternal) {
          this.service.affiliates = user.affiliates;
          this.service.state = Object.assign({}, this.service.defaultState);
        }
      }
    });
  }
  
  readonly AppPermissions = AppPermissions;

  ngOnInit() {
    super.ngOnInit();
  }

  showAllShipmentNo(dataItem) {
    dataItem.isShowAllShipmentNo = true;
  }

}
