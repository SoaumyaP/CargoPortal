import { NgModule } from '@angular/core';

import { UiModule } from 'src/app/ui';
import { IntegrationLogRoutingModule } from './integration-log-routing.module';
import { IntegrationLogListComponent } from './intergration-log-list/integration-log-list.component';
import { IntegrationLogListService } from './intergration-log-list/integration-log-list.service';
import { IntegrationLogFormComponent } from './intergration-log-form/integration-log-form.component';

@NgModule({
    imports: [
        IntegrationLogRoutingModule,
        UiModule
    ],
    declarations: [
        IntegrationLogListComponent,
        IntegrationLogFormComponent
    ],
    providers: [IntegrationLogListService]
})

export class IntegrationLogModule { }
