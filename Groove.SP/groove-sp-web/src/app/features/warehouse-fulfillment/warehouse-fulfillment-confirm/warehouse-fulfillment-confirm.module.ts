import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { WarehouseFulfillmentConfirmRoutingModule } from './warehouse-fulfillment-confirm-routing.module';
import { WarehouseFulfillmentConfirmFilterComponent } from './warehouse-fulfillment-confirm-filter/warehouse-fulfillment-confirm-filter.component';
import { WarehouseFulfillmentConfirmFormComponent } from './warehouse-fulfillment-confirm-form/warehouse-fulfillment-confirm-form.component';
import { WarehouseFulfillmentConfirmListComponent } from './warehouse-fulfillment-confirm-list/warehouse-fulfillment-confirm-list.component';
import { UiModule } from 'src/app/ui';
import { CommonService } from 'src/app/core/services/common.service';
import { ComplianceFormService } from '../../compliance/compliance-form/compliance-form.service';
import { ReactiveFormsModule } from '@angular/forms';
import { WarehouseFulfillmentConfirmFormService } from './warehouse-fulfillment-confirm-form/warehouse-fulfillment-confirm-form.service';


@NgModule({
  declarations: [
    WarehouseFulfillmentConfirmFormComponent,
    WarehouseFulfillmentConfirmFilterComponent,
    WarehouseFulfillmentConfirmListComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    WarehouseFulfillmentConfirmRoutingModule,
    UiModule
  ],
  providers:[
    CommonService,
    ComplianceFormService,
    WarehouseFulfillmentConfirmFormService
  ]
})
export class WarehouseFulfillmentConfirmModule { }