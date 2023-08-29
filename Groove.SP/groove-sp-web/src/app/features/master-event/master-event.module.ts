import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UiModule } from 'src/app/ui';
import { CommonService } from 'src/app/core/services/common.service';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MasterEventRoutingModule } from './master-event-routing.module';
import { MasterEventListComponent } from './master-event-list/master-event-list.component';
import { MasterEventListService } from './master-event-list/master-event-list.service';
import { MasterEventDialogComponent } from './master-event-dialog/master-event-dialog.component';


@NgModule({
  declarations: [
  MasterEventDialogComponent,
  MasterEventListComponent],
  imports: [
    CommonModule,
    UiModule,
    MasterEventRoutingModule
  ],
  providers: [
    CommonService,
    UiModule,
    FormsModule,
    ReactiveFormsModule,
    MasterEventListService
  ]
})
export class MasterEventModule { }