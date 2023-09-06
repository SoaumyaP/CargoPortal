import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { VesselArrivalRoutingModule } from './vessel-arrival-routing.module';
import { VesselArrivalListComponent } from './vessel-arrival-list/vessel-arrival-list.component';
import { UiModule } from 'src/app/ui';
import { ExcelModule } from '@progress/kendo-angular-grid';


@NgModule({
  declarations: [VesselArrivalListComponent],
  imports: [
    CommonModule,
    VesselArrivalRoutingModule,
    UiModule,
    ExcelModule,
  ]
})
export class VesselArrivalModule { }
