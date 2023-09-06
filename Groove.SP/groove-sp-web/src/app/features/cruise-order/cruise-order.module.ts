import { NgModule } from '@angular/core';
import { UiModule } from 'src/app/ui';
import { CruiseOrderRoutingModule } from './cruise-order-routing.module';
import { CruiseOrderListComponent } from './cruise-order-list/cruise-order-list.component';
import { CruiseOrderDetailComponent } from './cruise-order-detail/cruise-order-detail.component';
import { CruiseOrderListService } from './cruise-order-list/cruise-order-list.service';
import { CruiseOrderDetailService } from './cruise-order-detail/cruise-order-detail.service';
import { CommonService } from 'src/app/core/services/common.service';
import { CruiseOrderItemComponent } from './cruise-order-item/cruise-order-item.component';
import { CruiseOrderItemWarehouseComponent } from './cruise-order-item/cruise-order-item-warehouse/cruise-order-item-warehouse.component';
import { CruiseOrderItemNoteListComponent } from './cruise-order-item/cruise-order-item-note-list/cruise-order-item-note-list.component';
import { CruiseOrderItemDetailComponent } from './cruise-order-item/cruise-order-item-detail/cruise-order-item-detail.component';
import { NoteService } from 'src/app/core/notes/note.service';
import { CruiseOrderItemService } from './cruise-order-item/cruise-order-item.service';
import { CruiseOrderItemNotePopupComponent } from './cruise-order-item/popups/cruise-order-item-note-popup/cruise-order-item-note-popup.component';
import { CruiseOrderSupplementaryComponent } from './cruise-order-supplementary/cruise-order-supplementary.component';
import { CruiseOrderItemCopyPopupComponent } from './cruise-order-item/popups/cruise-order-item-copy-popup/cruise-order-item-copy-popup.component';

@NgModule({
    imports: [
        CruiseOrderRoutingModule,
        UiModule
    ],
    exports: [
    ],
    declarations: [
        CruiseOrderListComponent,
        CruiseOrderDetailComponent,
        CruiseOrderItemComponent,
        CruiseOrderItemWarehouseComponent,
        CruiseOrderItemNoteListComponent,
        CruiseOrderItemDetailComponent,
        CruiseOrderItemNotePopupComponent,
        CruiseOrderSupplementaryComponent,
        CruiseOrderItemCopyPopupComponent
    ],
    providers: [CruiseOrderListService, CruiseOrderDetailService, NoteService, CruiseOrderItemService, CommonService]
})

export class CruiseOrderModule { }
