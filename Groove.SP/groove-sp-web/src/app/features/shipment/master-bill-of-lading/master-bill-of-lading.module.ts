import { NgModule } from '@angular/core';
import { UiModule } from 'src/app/ui';
import { MasterBillOfLadingListComponent } from './master-bill-of-lading-list/master-bill-of-lading-list.component';
import { MasterBillOfLadingListService } from './master-bill-of-lading-list/master-bill-of-lading-list.service';
import { MasterBillOfLadingRoutingModule } from './master-bill-of-lading-routing.module';
import { MasterBillOfLadingFormComponent } from './master-bill-of-lading-form/master-bill-of-lading-form.component';
import { MasterBillOfLadingFormService } from './master-bill-of-lading-form/master-bill-of-lading-form.service';
import { AttachmentUploadPopupService } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.service';
import { MasterBLAddBLPopupComponent } from './master-bl-add-bl-popup/master-bl-add-bl-popup.component';
import { BillOfLadingListService } from '../bill-of-lading/bill-of-lading-list.service';
import { MAWBNumberFormatPipe } from 'src/app/core/pipes/mawb-number-format.pipe';

@NgModule({
  declarations: [MasterBillOfLadingListComponent, MasterBillOfLadingFormComponent, MasterBLAddBLPopupComponent],
    imports: [
        UiModule,
        MasterBillOfLadingRoutingModule
    ],
    providers: [
        MasterBillOfLadingListService,
        MasterBillOfLadingFormService,
        BillOfLadingListService,
        AttachmentUploadPopupService,
        MAWBNumberFormatPipe]
})
export class MasterBillOfLadingModule { }
