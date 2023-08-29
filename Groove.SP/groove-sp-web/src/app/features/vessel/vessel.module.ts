import { NgModule } from '@angular/core';
import { UiModule } from 'src/app/ui';
import { VesselRoutingModule } from './vessel-routing.module';
import { VesselListComponent } from './vessel-list/vessel-list.component';
import { VesselListService } from './vessel-list/vessel-list.service';
import { VesselFormComponent } from './popups/vessel-form/vessel-form.component';
import { VesselFormService } from './popups/vessel-form/vessel-form.service';

@NgModule({
    imports: [
        VesselRoutingModule,
        UiModule
    ],
    exports: [
    ],
    declarations: [
        VesselListComponent,
        VesselFormComponent
    ],
    providers: [VesselListService, VesselFormService]
})

export class VesselModule { }
