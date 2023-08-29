import { NgModule } from '@angular/core';
import { UiModule } from 'src/app/ui';
import { OrderRoutingModule } from './order-routing.module';
import { PurchaseOrderListService } from './purchase-order-list/purchase-order-list.service';
import { PurchaseOrderDetailService } from './purchase-order-detail/purchase-order-detail.service';
import { PurchaseOrderListComponent } from './purchase-order-list/purchase-order-list.component';
import { PurchaseOrderDetailComponent } from './purchase-order-detail/purchase-order-detail.component';
import { ExcelModule } from '@progress/kendo-angular-grid';
import { PODelegationModalComponent } from './po-delegation-modal/po-delegation-modal.component';
import { PODelegationService } from './po-delegation-modal/po-delegation-modal.service';
import { NoteService } from 'src/app/core/notes/note.service';
import { OrderNoteListComponent } from './order-note-list/order-note-list.component';
import { OrderNotePopupComponent } from './order-note-popup/order-note-popup.component';
import { POActivityFormComponent } from './purchase-order-activity-popup/purchase-order-activity-popup.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
    imports: [
        OrderRoutingModule,
        UiModule,
        ExcelModule,
        ReactiveFormsModule
    ],
    exports: [
    ],
    declarations: [
        PurchaseOrderListComponent,
        PurchaseOrderDetailComponent,
        PODelegationModalComponent,
        OrderNoteListComponent,
        OrderNotePopupComponent,
        POActivityFormComponent
    ],
    providers: [PurchaseOrderListService, PurchaseOrderDetailService, PODelegationService, NoteService]
})

export class OrderModule { }
