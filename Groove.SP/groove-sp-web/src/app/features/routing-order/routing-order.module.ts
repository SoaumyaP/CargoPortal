import { NgModule } from '@angular/core';
import { UiModule } from 'src/app/ui';
import { NoteService } from 'src/app/core/notes/note.service';
import { ReactiveFormsModule } from '@angular/forms';
import { RoutingOrderListComponent } from './routing-order-list/routing-order-list.component';
import { RoutingOrderRoutingModule } from './routing-order-routing';
import { RoutingOrderListService } from './routing-order-list/routing-order-list.service';
import { RoutingOrderFormComponent } from './routing-order-form/routing-order-form.component';
import { RoutingOrderFormService } from './routing-order-form/routing-order-form.service';
import { RoutingOrderGeneralComponent } from './routing-order-general/routing-order-general.component';
import { CommonService } from 'src/app/core/services/common.service';
import { RoutingOrderInvoiceComponent } from './routing-order-invoice/routing-order-invoice.component';
import { RoutingOrderAdditionalInfoComponent } from './routing-order-additional-info/routing-order-additional-info.component';
import { RoutingOrderContactComponent } from './routing-order-contact/routing-order-contact.component';
import { OrgContactPreferenceService } from '../org-contact-preference/org-contact-preference.service';
import { RoutingOrderContainerComponent } from './routing-order-container/routing-order-container.component';
import { RoutingOrderItemComponent } from './routing-order-item/routing-order-item.component';
import { RoutingOrderItemDialogComponent } from './routing-order-item-dialog/routing-order-item-dialog.component';
import { RoutingOrderNoteListComponent } from './routing-order-note-list/routing-order-note-list.component';
import { AddRoutingOrderNoteComponent } from './add-routing-order-note/add-routing-order-note.component';
import { ExcelModule } from '@progress/kendo-angular-grid';

@NgModule({
    imports: [
        RoutingOrderRoutingModule,
        UiModule,
        ExcelModule,
        ReactiveFormsModule
    ],
    exports: [
    ],
    declarations: [
        RoutingOrderListComponent,
        RoutingOrderFormComponent,
        RoutingOrderGeneralComponent,
        RoutingOrderInvoiceComponent,
        RoutingOrderAdditionalInfoComponent,
        RoutingOrderContactComponent,
        RoutingOrderContainerComponent,
        RoutingOrderItemComponent,
        RoutingOrderItemDialogComponent,
        RoutingOrderNoteListComponent,
        AddRoutingOrderNoteComponent
    ],
    providers: [NoteService, RoutingOrderListService, RoutingOrderFormService, CommonService, OrgContactPreferenceService]
})

export class RoutingOrderModule { }