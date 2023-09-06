import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UiModule } from 'src/app/ui';
import { BulkFulfillmentMilestoneComponent } from './bulk-fulfillment-milestone/bulk-fulfillment-milestone.component';
import { BulkFulfillmentFormComponent } from './bulk-fulfillment-form/bulk-fulfillment-form.component';
import { BulkFulfillmentRoutingModule } from './bulk-fulfillment-routing.module';
import { BulkFulfillmentFormService } from './bulk-fulfillment-form/bulk-fulfillment-form.service';
import { CommonService } from 'src/app/core/services/common.service';
import { BulkFulfillmentGeneralComponent } from './bulk-fulfillment-general/bulk-fulfillment-general.component';
import { BulkFulfillmentContactComponent } from './bulk-fulfillment-contact/bulk-fulfillment-contact.component';
import { POFulfillmentFormService } from '../po-fulfillment/po-fulfillment-form/po-fulfillment-form.service';
import { BulkFulfillmentAttachmentComponent } from './bulk-fulfillment-attachment/bulk-fulfillment-attachment.component';
import { AttachmentUploadPopupService } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.service';
import { BulkFulfillmentCargoDetailComponent } from './bulk-fulfillment-cargo-detail/bulk-fulfillment-cargo-detail.component';
import { BulkFulfillmentItemFormDialogComponent } from './bulk-fulfillment-item-form-dialog/bulk-fulfillment-item-form-dialog.component';
import { BulkFulfillmentItemDetailDialogComponent } from './bulk-fulfillment-item-detail-dialog/bulk-fulfillment-item-detail-dialog.component';
import { BulkFulfillmentActivityComponent } from './bulk-fulfillment-activity/bulk-fulfillment-activity.component';
import { PurchaseOrderDetailService } from '../order/purchase-order-detail/purchase-order-detail.service';
import { BulkFulfillmentDialogComponent } from './bulk-fulfillment-dialog/bulk-fulfillment-dialog.component';
import { BulkFulfillmentDialogPopupComponent } from './bulk-fulfillment-dialog/bulk-fulfillment-dialog-popup/bulk-fulfillment-dialog-popup.component';
import { NoteService } from 'src/app/core/notes/note.service';
import { BulkFulfillmentLoadDetailComponent } from './bulk-fulfillment-load-detail/bulk-fulfillment-load-detail.component';
import { LoadCargoDialogComponent } from './bulk-fulfillment-load-detail/load-cargo-dialog/load-cargo-dialog.component';
import { LoadContainerDialogComponent } from './bulk-fulfillment-load-detail/load-container-dialog/load-container-dialog.component';
import { LoadDetailFormDialogComponent } from './bulk-fulfillment-load-detail/load-detail-form-dialog/load-detail-form-dialog.component';
import { BulkFulfillmentPlannedScheduleComponent } from './bulk-fulfillment-planned-schedule/bulk-fulfillment-planned-schedule.component';
import { OrganizationPreferenceService } from '../organization-preference/Organization-preference.service';
import { OrgContactPreferenceService } from '../org-contact-preference/org-contact-preference.service';
import { OrganizationFormService } from '../organization/organization-form/organization-form.service';
import { BulkFulfillmentDuplicatedCompanyDialogComponent } from './bulk-fulfillment-duplicated-company-dialog/bulk-fulfillment-duplicated-company-dialog.component';


@NgModule({
    declarations: [
        BulkFulfillmentAttachmentComponent,
        BulkFulfillmentFormComponent,
        BulkFulfillmentGeneralComponent,
        BulkFulfillmentContactComponent,
        BulkFulfillmentMilestoneComponent,
        BulkFulfillmentCargoDetailComponent,
        BulkFulfillmentItemFormDialogComponent,
        BulkFulfillmentItemDetailDialogComponent,
        BulkFulfillmentActivityComponent,
        BulkFulfillmentDialogComponent,
        BulkFulfillmentDialogPopupComponent,
        BulkFulfillmentLoadDetailComponent,
        LoadCargoDialogComponent,
        LoadContainerDialogComponent,
        LoadDetailFormDialogComponent,
        BulkFulfillmentPlannedScheduleComponent,
        BulkFulfillmentDuplicatedCompanyDialogComponent],
    imports: [
        CommonModule,
        BulkFulfillmentRoutingModule,
        UiModule
    ],
    providers: [
        PurchaseOrderDetailService,
        NoteService,
        AttachmentUploadPopupService,
        BulkFulfillmentFormService,
        POFulfillmentFormService,
        CommonService,
        OrganizationPreferenceService,
        OrgContactPreferenceService,
        OrganizationFormService
    ]
})
export class BulkFulfillmentModule { }