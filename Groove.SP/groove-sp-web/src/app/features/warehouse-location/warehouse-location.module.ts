import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { WarehouseLocationRoutingModule } from './warehouse-location-routing.module';
import { WarehouseLocationListComponent } from './warehouse-location-list/warehouse-location-list.component';
import { WarehouseLocationFormComponent } from './warehouse-location-form/warehouse-location-form.component';
import { UiModule } from 'src/app/ui';
import { WarehouseLocationListService } from './warehouse-location-list/warehouse-location-list.service';
import { WarehouseLocationFormService } from './warehouse-location-form/warehouse-location-form.service';


@NgModule({
    providers: [
        WarehouseLocationListService,
        WarehouseLocationFormService
    ],
    declarations: [
        WarehouseLocationListComponent,
        WarehouseLocationFormComponent
    ],
    imports: [
        CommonModule,
        WarehouseLocationRoutingModule,
        UiModule
    ]
})
export class WarehouseLocationModule { }
