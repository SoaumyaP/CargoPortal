import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UiModule } from 'src/app/ui';
import { CommonService } from 'src/app/core/services/common.service';
import { WarehouseFulfillmentRoutingModule } from './warehouse-fulfillment-routing.module';
import { WarehouseFulfillmentFormComponent } from './warehouse-fulfillment-form/warehouse-fulfillment-form.component';
import { WarehouseFulfillmentFormService } from './warehouse-fulfillment-form/warehouse-fulfillment-form.service';
import { WarehouseFulfillmentMilestoneComponent } from './warehouse-fulfillment-milestone/warehouse-fulfillment-milestone.component';
import { WarehouseFulfillmentGeneralInfoComponent } from './warehouse-fulfillment-general-info/warehouse-fulfillment-general-info.component';
import { WarehouseFulfillmentContactComponent } from './warehouse-fulfillment-contact/warehouse-fulfillment-contact.component';
import { WarehouseFulfillmentActivityComponent } from './warehouse-fulfillment-activity/warehouse-fulfillment-activity.component';
import { WarehouseFulfillmentAttachmentComponent } from './warehouse-fulfillment-attachment/warehouse-fulfillment-attachment.component';
import { AttachmentUploadPopupService } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.service';
import { WarehouseFulfillmentCustomerPoComponent } from './warehouse-fulfillment-customer-po/warehouse-fulfillment-customer-po.component';
import { WarehouseFulfillmentCustomerPoDetailDialogComponent } from './warehouse-fulfillment-customer-po-detail-dialog/warehouse-fulfillment-customer-po-detail-dialog.component';
import { WarehouseFulfillmentWarehouseComponent } from './warehouse-fulfillment-warehouse/warehouse-fulfillment-warehouse.component';
import { WarehouseFulfillmentCargoReceiveComponent } from './warehouse-fulfillment-cargo-receive/warehouse-fulfillment-cargo-receive.component';

@NgModule({
    declarations: [
        WarehouseFulfillmentFormComponent,
        WarehouseFulfillmentMilestoneComponent,
        WarehouseFulfillmentGeneralInfoComponent,
        WarehouseFulfillmentContactComponent,
        WarehouseFulfillmentActivityComponent,
        WarehouseFulfillmentAttachmentComponent,
        WarehouseFulfillmentCustomerPoComponent,
        WarehouseFulfillmentCustomerPoDetailDialogComponent,
        WarehouseFulfillmentWarehouseComponent,
        WarehouseFulfillmentCargoReceiveComponent
    ],
    imports: [
        CommonModule,
        WarehouseFulfillmentRoutingModule,
        UiModule
    ],
    providers: [
        CommonService,
        WarehouseFulfillmentFormService,
        AttachmentUploadPopupService
    ]
})
export class WarehouseFulfillmentModule { }