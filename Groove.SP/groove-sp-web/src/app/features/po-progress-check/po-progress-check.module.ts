import { NgModule } from '@angular/core';
import { UiModule } from 'src/app/ui';
import { POProgressCheckFormComponent } from './po-progress-check-form/po-progress-check-form.component';
import { POProgressCheckRoutingModule } from './po-progress-check-routing.module';
import { POProgressCheckListFormComponent } from './po-progress-check-list-form/po-progress-check-list-form.component';
import { PurchaseOrderListService } from '../order/purchase-order-list/purchase-order-list.service';
import { POProgressCheckFilterFormComponent } from './po-progress-check-filter-form/po-progress-check-filter-form.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { POProgressCheckFilterFormService } from './po-progress-check-filter-form/po-progress-check-filter-form.service';
import { CommonService } from 'src/app/core/services/common.service';
import { POProgressCheckFormService } from './po-progress-check-form/po-progress-check-form.service';
@NgModule({
    imports: [
        POProgressCheckRoutingModule,
        UiModule,
        FormsModule,
        ReactiveFormsModule
    ],
    exports: [
    ],
    declarations: [
        POProgressCheckListFormComponent,
        POProgressCheckFormComponent,
        POProgressCheckFilterFormComponent
    ],
    providers: [
        CommonService,
        PurchaseOrderListService,
        POProgressCheckFormService,
        POProgressCheckFilterFormService
    ]
})

export class POProgressCheckModule { }