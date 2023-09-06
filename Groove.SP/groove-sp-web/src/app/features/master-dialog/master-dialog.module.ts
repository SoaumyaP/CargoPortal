import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MasterDialogRoutingModule } from './master-dialog-routing.module';
import { MasterDialogFormComponent } from './master-dialog-form/master-dialog-form.component';
import { MasterDialogListComponent } from './master-dialog-list/master-dialog-list.component';
import { MasterDialogListService } from './master-dialog-list/master-dialog-list.service';
import { UiModule } from 'src/app/ui';
import { MasterDialogFormService } from './master-dialog-form/master-dialog-form.service';
import { CommonService } from 'src/app/core/services/common.service';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';


@NgModule({
  declarations: [
    MasterDialogFormComponent,
    MasterDialogListComponent
  ],
  imports: [
    CommonModule,
    UiModule,
    MasterDialogRoutingModule
  ],
  providers: [
    CommonService,
    UiModule,
    FormsModule,
    ReactiveFormsModule,

    MasterDialogListService,
    MasterDialogFormService,
  ]
})
export class MasterDialogModule { }