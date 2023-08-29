import { NgModule } from '@angular/core';
import { MissingPOFulfillmentFormComponent } from './missing-po-fulfillment-form/missing-po-fulfillment-form.component';
import { MissingPOFulfillmentFormService } from './missing-po-fulfillment-form/missing-po-fulfillment-form.service';
import { UiModule } from 'src/app/ui';
import { MissingPOFulfillmentGeneralInfoComponent } from './missing-po-fulfillment-general-info/missing-po-fulfillment-general-info.component';
import { MissingPOFulfillmentContactComponent } from './missing-po-fulfillment-contact/missing-po-fulfillment-contact.component';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { MissingPOFulfillmentLoadInfoComponent } from './missing-po-fulfillment-load-info/missing-po-fulfillment-load-info.component';
import { MissingPOFulfillmentActivityComponent } from './missing-po-fulfillment-activity/missing-po-fulfillment-activity.component';
import { AttachmentUploadPopupService } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.service';
import { MissingPOFulfillmentNoteListComponent } from './missing-po-fulfillment-note-list/missing-po-fulfillment-note-list.component';
import { MissingPOFulfillmentNotePopupComponent } from './missing-po-fulfillment-note-popup/missing-po-fulfillment-note-popup.component';
import { NoteService } from 'src/app/core/notes/note.service';
import { OrganizationPreferenceService } from '../organization-preference/Organization-preference.service';
import { ComplianceFormService } from '../compliance/compliance-form/compliance-form.service';
import { ShipmentTrackingService } from '../shipment/shipment-tracking/shipment-tracking.service';
import { MissingPOFulfillmentRoutingModule } from './missing-po-fulfillment-routing.module';
import { SelectCustomerPOFormComponent } from './select-customer-po-form/select-customer-po-form.component';
import { MissingPOFulfillmentCustomerComponent } from './missing-po-fulfillment-customer/missing-po-fulfillment-customer.component';
import { MissingPOFulfillmentCustomerDetailPopupComponent } from './missing-po-fulfillment-customer/missing-po-fulfillment-customer-detail-popup/missing-po-fulfillment-customer-detail-popup.component';
import { AddMissingCustomerPoFormComponent } from './add-missing-customer-po-form/add-missing-customer-po-form.component';

@NgModule({
    imports: [
        MissingPOFulfillmentRoutingModule,
        UiModule,
        DragDropModule
    ],
    exports: [
    ],
    declarations: [
        MissingPOFulfillmentFormComponent,
        MissingPOFulfillmentGeneralInfoComponent,
        MissingPOFulfillmentCustomerComponent,
        MissingPOFulfillmentContactComponent,
        MissingPOFulfillmentLoadInfoComponent,
        MissingPOFulfillmentActivityComponent,
        MissingPOFulfillmentCustomerDetailPopupComponent,
        MissingPOFulfillmentNoteListComponent,
        MissingPOFulfillmentNotePopupComponent,
        SelectCustomerPOFormComponent,
        AddMissingCustomerPoFormComponent
    ],
    providers: [MissingPOFulfillmentFormService, AttachmentUploadPopupService, NoteService, OrganizationPreferenceService, ComplianceFormService, ShipmentTrackingService]
})

export class MissingPOFulfillmentModule { }
