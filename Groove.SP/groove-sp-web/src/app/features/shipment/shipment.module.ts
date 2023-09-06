import { NgModule } from '@angular/core';
import { UiModule } from 'src/app/ui';
import { ShipmentsRoutingModule } from './shipment-routing.module';
import { ShipmentListService } from './shipment-list/shipment-list.service';
import { ShipmentListComponent } from './shipment-list/shipment-list.component';
import { ShipmentTrackingComponent } from './shipment-tracking/shipment-tracking.component';
import { ShipmentActivityFormComponent } from './shipment-activity-form/shipment-activity-form.component';
import { ShipmentTrackingService } from './shipment-tracking/shipment-tracking.service';
import { AttachmentUploadPopupService } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.service';
import { ConsignmentFormDialogComponent } from './consignment-form-dialog/consignment-form-dialog.component';
import { ConsignmentFormDialogService } from './consignment-form-dialog/consignment-form-dialog.service';
import { ConsignmentItineraryFormService } from './consignment-itinerary-form/consignment-itinerary-form.service';
import { ConsignmentItineraryFormComponent } from './consignment-itinerary-form/consignment-itinerary-form.component';
import { ShipmentNoteListComponent } from './shipment-note-list/shipment-note-list.component';
import { NoteService } from 'src/app/core/notes/note.service';
import { ShipmentNotePopupComponent } from './shipment-note-popup/shipment-note-popup.component';
import { ConsolidationPopupComponent } from './consolidation-popup/consolidation-popup.component';
import { ShipmentAddHouseBLPopupComponent } from './shipment-add-house-bl-popup/shipment-add-house-bl-popup.component';
import { BillOfLadingFormService } from './bill-of-lading/bill-of-lading-form.service';
import { ShipmentAddMasterBLPopupComponent } from './shipment-add-master-bl-popup/shipment-add-master-bl-popup.component';
import { MasterBillOfLadingFormService } from './master-bill-of-lading/master-bill-of-lading-form/master-bill-of-lading-form.service';
import { MasterBillOfLadingListService } from './master-bill-of-lading/master-bill-of-lading-list/master-bill-of-lading-list.service';
import { ShipmentFormComponent } from './shipment-form/shipment-form.component';
import { ShipmentItineraryConfirmPopupComponent } from './shipment-itinerary-confirm-popup/shipment-itinerary-confirm-popup.component';
import { OrderByPipe } from 'src/app/core/pipes/order-by.pipe';
import { ShipmentItineraryConfirmPopupService } from './shipment-itinerary-confirm-popup/shipment-itinerary-confirm.service';
@NgModule({
    imports: [
        ShipmentsRoutingModule,
        UiModule
    ],
    declarations: [
        ShipmentListComponent,
        ShipmentTrackingComponent,
        ConsignmentFormDialogComponent,
        ShipmentActivityFormComponent,
        ConsignmentItineraryFormComponent,
        ShipmentNoteListComponent,
        ShipmentNotePopupComponent,
        ConsolidationPopupComponent,
        ShipmentAddMasterBLPopupComponent,
        ShipmentAddHouseBLPopupComponent,
        ShipmentFormComponent,
        ShipmentItineraryConfirmPopupComponent
    ],
    providers: [
        ShipmentListService,
        ShipmentTrackingService,
        AttachmentUploadPopupService,
        ConsignmentFormDialogService,
        ConsignmentItineraryFormService,
        BillOfLadingFormService,
        MasterBillOfLadingListService,
        MasterBillOfLadingFormService,
        NoteService,
        OrderByPipe,
        ShipmentItineraryConfirmPopupService]
})

export class ShipmentModule { }
