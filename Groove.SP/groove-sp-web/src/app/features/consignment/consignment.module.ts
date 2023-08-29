import { NgModule } from '@angular/core';
import { UiModule } from 'src/app/ui';
import { ConsignmentRoutingModule } from './consignment-routing.module';
import { ConsignmentListComponent } from './consignment-list/consignment-list.component';
import { ConsignmentFormComponent } from './consignment-form/consignment-form.component';
import { ConsignmentItineraryFormComponent } from './consignment-itinerary-form/consignment-itinerary-form.component';
import { ConsignmentFormService } from './consignment-form/consignment-form.service';
import { ConsignmentListService } from './consignment-list/consignment-list.service';
import { AttachmentUploadPopupService } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.service';
import { ConsignmentActivityFormComponent } from './consignment-activity-form/consignment-activity-form.component';
import { ConsolidationFormService } from '../consolidation/consolidation-form/consolidation-form.service';
import { ConsignmentItineraryFormService } from '../shipment/consignment-itinerary-form/consignment-itinerary-form.service';

@NgModule({
    imports: [
        ConsignmentRoutingModule,
        UiModule
    ],
    exports: [
    ],
    declarations: [
        ConsignmentListComponent,
        ConsignmentFormComponent,
        ConsignmentItineraryFormComponent,
        ConsignmentActivityFormComponent
    ],
    providers: [ConsignmentListService,
        ConsignmentFormService,
        AttachmentUploadPopupService,
        ConsignmentItineraryFormService
    ]
})

export class ConsignmentModule { }
