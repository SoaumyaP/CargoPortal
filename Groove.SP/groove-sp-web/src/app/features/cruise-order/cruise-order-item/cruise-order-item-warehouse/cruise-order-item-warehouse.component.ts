import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { faInfo } from '@fortawesome/free-solid-svg-icons';
import { Subscription } from 'rxjs';
import { delay } from 'rxjs/operators';
import { DATE_FORMAT } from 'src/app/core';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { CruiseOrderWarehouseModel } from '../../models/cruise-order-warehouse.model';
import { CruiseOrderItemService } from '../cruise-order-item.service';

@Component({
  selector: 'app-cruise-order-item-warehouse',
  templateUrl: './cruise-order-item-warehouse.component.html',
  styleUrls: ['./cruise-order-item-warehouse.component.scss']
})
export class CruiseOrderItemWarehouseComponent implements OnInit, OnDestroy {
  @Input() cruiseOrderItemId: number;
  @Input() loadingGrids: {};
  @Input() rowIndex: number;

  warehouseModel: CruiseOrderWarehouseModel;
  defaultValue = '--';
  DATE_FORMAT = DATE_FORMAT;
  subscriptions: Array<Subscription> = [];
  faInfo = faInfo;
  isNotFoundWarehouse: boolean;

  constructor(
    private cruiseOrderItemService: CruiseOrderItemService,
    public notification: NotificationPopup
  ) { }


  ngOnInit() {
    this.loadWarehouse(this.cruiseOrderItemId);
  }

  loadWarehouse(cruiseOrderItemId: number) {
    this.warehouseModel = new CruiseOrderWarehouseModel();
    this.loadingGrids[this.rowIndex] = true;
    const sub = this.cruiseOrderItemService.getWarehouse(cruiseOrderItemId).subscribe(
      response => {
        if (!response) {
          this.warehouseModel = {} as CruiseOrderWarehouseModel;
          this.isNotFoundWarehouse = true;
        } else {
          this.warehouseModel = response;
          this.isNotFoundWarehouse = false;
        }

        this.loadingGrids[this.rowIndex] = false;
      },
      err => {
        this.loadingGrids[this.rowIndex] = false;
        this.isNotFoundWarehouse = true;
      }
    );

    this.subscriptions.push(sub);
  }

  returnText(field) {
    return this.warehouseModel[field] ? this.warehouseModel[field] : this.defaultValue;
  }

  get calculateAvailableDays() {
    const today = new Date();
    const inDate = Date.parse(this.warehouseModel.inDate);
    return inDate ? 
      Math.floor((today.valueOf() - inDate.valueOf()) / (1000 * 60 * 60 * 24)) : null
  }

  ngOnDestroy(): void {
    this.subscriptions.map(x => { x.unsubscribe(); });
  }
}