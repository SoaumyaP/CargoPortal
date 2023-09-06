import { NgModule } from '@angular/core';
import { POFulfillmentFormComponent } from './po-fulfillment-form/po-fulfillment-form.component';
import { POFulfillmentListComponent } from './po-fulfillment-list/po-fulfillment-list.component';
import { POFulfillmentListService } from './po-fulfillment-list/po-fulfillment-list.service';
import { POFulfillmentFormService } from './po-fulfillment-form/po-fulfillment-form.service';
import { POFulfillmentRoutingModule } from './po-fulfillment-routing.module';
import { UiModule } from 'src/app/ui';
import { POFulfillmentGeneralInfoComponent } from './po-fulfillment-general-info/po-fulfillment-general-info.component';
import { POFulfillmentContactComponent } from './po-fulfillment-contact/po-fulfillment-contact.component';
import { POFulfillmentCustomerComponent } from './po-fulfillment-customer/po-fulfillment-customer.component';
import { POFulfillmentLoadDetailComponent } from './po-fulfillment-load-detail/po-fulfillment-load-detail.component';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { POFulfillmentLoadInfoComponent } from './po-fulfillment-load-info/po-fulfillment-load-info.component';
import { POFulfillmentLoadDetailFormComponent } from './po-fulfillment-load-detail-form/po-fulfillment-load-detail-form.component';
import { POFulfillmentCargoDetailComponent } from './po-fulfillment-cargo-detail/po-fulfillment-cargo-detail.component';
import { POFulfillmentCargoDetailFormComponent } from './po-fulfillment-cargo-detail-form/po-fulfillment-cargo-detail-form.component';
import { POFulfillmentItineraryComponent } from './po-fulfillment-itinerary/po-fulfillment-itinerary.component';
import { POFulfillmentActivityComponent } from './po-fulfillment-activity/po-fulfillment-activity.component';
import { LoadContainerPopupComponent } from './load-container-popup/load-container-popup.component';
import { POFulfillmentCustomerDetailPopupComponent } from './po-fulfillment-customer/po-fulfillment-customer-detail-popup/po-fulfillment-customer-detail-popup.component';
import { AttachmentUploadPopupService } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.service';
import { PoFulfillmentShipmentComponent } from './po-fulfillment-shipment/po-fulfillment-shipment.component';
import { PoFulfillmentNoteListComponent } from './po-fulfillment-note-list/po-fulfillment-note-list.component';
import { PoFulfillmentNotePopupComponent } from './po-fulfillment-note-popup/po-fulfillment-note-popup.component';
import { NoteService } from 'src/app/core/notes/note.service';
import { OrganizationPreferenceService } from '../organization-preference/Organization-preference.service';
import { ComplianceFormService } from '../compliance/compliance-form/compliance-form.service';
import { LoadCargoPopupComponent } from './load-cargo-popup/load-cargo-popup.component';
import { ShipmentTrackingService } from '../shipment/shipment-tracking/shipment-tracking.service';
import { POFulfillmentCopyPopupComponent } from './po-fulfillment-copy-popup/po-fulfillment-copy-popup.component';

@NgModule({
    imports: [
        POFulfillmentRoutingModule,
        UiModule,
        DragDropModule
    ],
    exports: [
    ],
    declarations: [
        POFulfillmentListComponent,
        POFulfillmentFormComponent,
        POFulfillmentGeneralInfoComponent,
        POFulfillmentCustomerComponent,
        POFulfillmentContactComponent,
        POFulfillmentLoadDetailComponent,
        POFulfillmentLoadInfoComponent,
        POFulfillmentLoadDetailFormComponent,
        POFulfillmentCargoDetailComponent,
        POFulfillmentCargoDetailFormComponent,
        POFulfillmentItineraryComponent,
        POFulfillmentActivityComponent,
        LoadContainerPopupComponent,
        POFulfillmentCustomerDetailPopupComponent,
        PoFulfillmentShipmentComponent,
        PoFulfillmentNoteListComponent,
        PoFulfillmentNotePopupComponent,
        LoadCargoPopupComponent,
        POFulfillmentCopyPopupComponent,
    ],
    providers: [POFulfillmentListService, POFulfillmentFormService, AttachmentUploadPopupService, NoteService, OrganizationPreferenceService, ComplianceFormService, ShipmentTrackingService]
})

export class POFulfillmentModule { }
