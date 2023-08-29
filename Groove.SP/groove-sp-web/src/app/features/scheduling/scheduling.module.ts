import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SchedulingRoutingModule } from './scheduling-routing.module';
import { SchedulingFormComponent } from './scheduling-form/scheduling-form.component';
import { UiModule } from 'src/app/ui';
import { SchedulingFormService } from './scheduling-form/scheduling-form.service';
import { SchedulerModule } from '@progress/kendo-angular-scheduler';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SchedulingListComponent } from './scheduling-list/scheduling-list.component';
import { SchedulingListService } from './scheduling-list/scheduling-list.service';
import { ColumnOptionsDialogComponent } from './dialogs/column-options-dialog/column-options-dialog.component';

@NgModule({
    declarations: [
        SchedulingFormComponent,
        SchedulingListComponent,
        ColumnOptionsDialogComponent,
    ],
    imports: [
        CommonModule,
        SchedulingRoutingModule,
        UiModule,
        FormsModule,
        ReactiveFormsModule,
        SchedulerModule
    ],
    providers: [
        SchedulingFormService,
        SchedulingListService
    ],
    exports: [
        SchedulerModule,
        FormsModule,
        ReactiveFormsModule,
    ]
})
export class SchedulingModule { }
