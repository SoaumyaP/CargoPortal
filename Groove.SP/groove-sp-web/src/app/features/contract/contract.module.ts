import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ContractRoutingModule } from './contract-routing.module';
import { ContractListComponent } from './contract-list/contract-list.component';
import { UiModule } from 'src/app/ui';
import { ContractListService } from './contract-list/contract-list.service';
import { ContractFormComponent } from './contract-form/contract-form.component';
import { ContractFormService } from './contract-form/contract-form.service';
import { CommonService } from 'src/app/core/services/common.service';


@NgModule({
  declarations: [ContractListComponent, ContractFormComponent],
  imports: [
    CommonModule,
    ContractRoutingModule,
    UiModule
  ],
  providers: [
    ContractListService,
    ContractFormService,
    CommonService
  ]
})
export class ContractModule { }
