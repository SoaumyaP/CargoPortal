import { NgModule } from '@angular/core';
import { UiModule } from 'src/app/ui';
import { BuyerApprovalRoutingModule } from './buyer-approval-routing.module';
import { BuyerApprovalListComponent } from './buyer-approval-list/buyer-approval-list.component';
import { BuyerApprovalListService } from './buyer-approval-list/buyer-approval-list.service';
import { BuyerApprovalFormComponent } from './buyer-approval-form/buyer-approval-form.component';
import { BuyerApprovalFormService } from './buyer-approval-form/buyer-approval-form.service';
import { WarehouseFulfillmentFormService } from '../warehouse-fulfillment/warehouse-fulfillment-form/warehouse-fulfillment-form.service';

@NgModule({
    imports: [
        BuyerApprovalRoutingModule,
        UiModule
    ],
    exports: [
    ],
    declarations: [
        BuyerApprovalListComponent,
        BuyerApprovalFormComponent
    ],
    providers: [BuyerApprovalListService, BuyerApprovalFormService, WarehouseFulfillmentFormService]
})

export class BuyerApprovalModule { }
