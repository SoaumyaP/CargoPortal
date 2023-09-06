import { NgModule } from '@angular/core';
import { UiModule } from 'src/app/ui';
import { ConsolidationRoutingModule } from './consolidation-routing.module';
import { ConsolidationDetailsComponent } from './consolidation-details/consolidation-details.component';
import { ConsolidationFormComponent } from './consolidation-form/consolidation-form.component';
import { ConsolidationFormService } from './consolidation-form/consolidation-form.service';
import { ConsolidationService } from './consolidation.service';
import { ConsolidationListComponent } from './consolidation-list/consolidation-list.component';
import { ConsolidationListService } from './consolidation-list/consolidation-list.service';
import { CommonService } from 'src/app/core/services/common.service';
import { ConsolidationConsignmentFormComponent } from './consolidation-consignment-form/consolidation-consignment-form.component';
import { ConsolidationConsignmentFormService } from './consolidation-consignment-form/consolidation-consignment-form.service';
import { ConsolidationCargoDetailListComponent } from './consolidation-cargo-detail-list/consolidation-cargo-detail-list.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { LoadCargoDetailPopupComponent } from './load-cargo-detail-popup/load-cargo-detail-popup.component';

@NgModule({
    imports: [
        ConsolidationRoutingModule,
        UiModule,
        FormsModule,
        ReactiveFormsModule
    ],
    exports: [
    ],
    declarations: [
        ConsolidationDetailsComponent,
        ConsolidationFormComponent,
        ConsolidationListComponent,
        ConsolidationConsignmentFormComponent,
        ConsolidationCargoDetailListComponent,
        LoadCargoDetailPopupComponent
    ],
    providers: [CommonService, ConsolidationFormService, ConsolidationConsignmentFormService, ConsolidationService, ConsolidationListService]
})

export class ConsolidationModule { }
