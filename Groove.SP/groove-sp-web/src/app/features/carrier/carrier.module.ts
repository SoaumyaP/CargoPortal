import { NgModule } from '@angular/core';
import { UiModule } from 'src/app/ui';
import { CarrierListComponent } from './carrier-list/carrier-list.component';
import { CarrierListService } from './carrier-list/carrier-list.service';
import { CarrierRoutingModule } from './carrier-routing.module';
import { CarrierFormComponent } from './popup/carrier-form/carrier-form.component';
import { CarrierFormService } from './popup/carrier-form/carrier-form.service';

@NgModule({
    imports: [
        CarrierRoutingModule,
        UiModule
    ],
    exports: [
    ],
    declarations: [
        CarrierListComponent,
        CarrierFormComponent
    ],
    providers: [
        CarrierListService,
        CarrierFormService
    ]
})

export class CarrierModule { }
