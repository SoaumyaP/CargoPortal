import { NgModule } from '@angular/core';
import { UiModule } from 'src/app/ui';
import { FreightSchedulerListComponent } from './freight-scheduler-list/freight-scheduler-list.component';
import { FreightSchedulerListService } from './freight-scheduler-list/freight-scheduler-list.service';
import { FreightSchedulerRoutingModule } from './freight-scheduler-routing.module';
import { FreightSchedulerFormComponent } from './freight-scheduler-form/freight-scheduler-form.component';
import { CommonService } from 'src/app/core/services/common.service';
import { FreightSchedulerService } from './freight-scheduler.service';
import { FreightScheduleDetailComponent } from './freight-schedule-detail/freight-schedule-detail.component';
import { IMaskModule } from 'angular-imask';

@NgModule({
    declarations: [FreightSchedulerListComponent, FreightSchedulerFormComponent, FreightScheduleDetailComponent],
    imports: [
        UiModule,
        FreightSchedulerRoutingModule,
        IMaskModule
    ],
    providers: [FreightSchedulerListService, CommonService, FreightSchedulerService]
})
export class FreightSchedulerModule { }
