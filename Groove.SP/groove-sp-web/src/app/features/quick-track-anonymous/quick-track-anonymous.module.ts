import { TranslateModule } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { UiModule } from 'src/app/ui';
import { QuickTrackingAnonymousRoutingModule } from './quick-track-anonymous-routing.module';
import { QuickTrackAnonymousComponent } from './quick-track-anonymous.component';
import { ShipmentAnonymousComponent } from './shipment-anonymous/shipment-anonymous.component';
import { ShipmentAnonymousService } from './shipment-anonymous/shipment-anonymous.service';
import { ContainerAnonymousComponent } from './container-anonymous/container-anonymous.component';
import { ContainerAnonymousService } from './container-anonymous/container-anonymous.service';
import { BillOfLadingAnonymousComponent } from './bill-of-lading-anonymous/bill-of-lading-anonymous.component';
import { BillOfLadingAnonymousService } from './bill-of-lading-anonymous/bill-of-lading-anonymous.service';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';

@NgModule({
    imports: [
        CommonModule,
        UiModule,
        QuickTrackingAnonymousRoutingModule,
        TranslateModule
    ],
    declarations: [
        QuickTrackAnonymousComponent,
        ShipmentAnonymousComponent,
        ContainerAnonymousComponent,
        BillOfLadingAnonymousComponent
    ],
    providers: [
        ShipmentAnonymousService,
        ContainerAnonymousService,
        BillOfLadingAnonymousService,
        GoogleAnalyticsService
    ]
})
export class QuickTrackAnonymousModule { }
