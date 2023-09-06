import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { LocationRoutingModule } from './location-routing.module';
import { LocationListComponent } from './location-list/location-list.component';
import { LocationListService } from './location-list/location-list.service';
import { UiModule } from 'src/app/ui/ui.module';
import { LocationFormComponent } from './location-form/location-form.component';
import { ComplianceFormService } from '../compliance/compliance-form/compliance-form.service';
import { LocationFormService } from './location-form/location-form.service';


@NgModule({
  declarations: [LocationListComponent, LocationFormComponent],
  imports: [
    CommonModule,
    LocationRoutingModule,
    UiModule
  ],
  providers:[
    LocationListService,
    LocationFormService,
    ComplianceFormService
  ]
})
export class LocationModule { }
