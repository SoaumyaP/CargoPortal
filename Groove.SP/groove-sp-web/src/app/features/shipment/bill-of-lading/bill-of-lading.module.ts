import { NgModule } from '@angular/core';
import { UiModule } from 'src/app/ui';
import { BillOfLadingRoutingModule } from './bill-of-lading-routing.module';
import { BillOfLadingComponent } from './bill-of-lading.component';
import { BillOfLadingFormService } from './bill-of-lading-form.service';
import { AttachmentUploadPopupService } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.service';
import { BLAddMasterBLPopupComponent } from './bl-add-master-bl-popup/bl-add-master-bl-popup.component';
import { MasterBillOfLadingFormService } from '../master-bill-of-lading/master-bill-of-lading-form/master-bill-of-lading-form.service';
import { MasterBillOfLadingListService } from '../master-bill-of-lading/master-bill-of-lading-list/master-bill-of-lading-list.service';
import { ShipmentTrackingService } from '../shipment-tracking/shipment-tracking.service';
import { BLLinkToShipmentPopupComponent } from './bl-link-to-shipment-popup/bl-link-to-shipment-popup.component';
import { BillOfLadingListComponent } from './bill-of-lading-list/bill-of-lading-list.component';
import { BillOfLadingListService } from './bill-of-lading-list/bill-of-lading-list.service';
import { BillOfLadingFormComponent } from './bill-of-lading-form/bill-of-lading-form.component';


@NgModule({
    imports: [
        BillOfLadingRoutingModule,
        UiModule
    ],
    declarations: [
    BillOfLadingComponent,
    BLAddMasterBLPopupComponent,
    BLLinkToShipmentPopupComponent,
    BillOfLadingListComponent,
    BillOfLadingFormComponent
    ],
    providers: [BillOfLadingFormService,
        AttachmentUploadPopupService,
        MasterBillOfLadingFormService,
        MasterBillOfLadingListService,
        BillOfLadingListService,
        ShipmentTrackingService
    ]
})

export class BillOfLadingModule { }
