import { NgModule } from '@angular/core';

import { UiModule } from 'src/app/ui';
import { ComplianceRoutingModule } from './compliance-routing.module';
import { ComplianceListComponent } from './compliance-list/compliance-list.component';
import { ComplianceListService } from './compliance-list/compliance-list.service';
import { ComplianceFormComponent } from './compliance-form/compliance-form.component';
import { ComplianceFormService } from './compliance-form/compliance-form.service';
import { BookingValidationLogListComponent } from './booking-validation-log-list/booking-validation-log-list.component';
import { BookingValidationLogListService } from './booking-validation-log-list/booking-validation-log-list.service';
import { BookingValidationLogPopupComponent } from './booking-validation-log-popup/booking-validation-log-popup.component';
import { AgentAssignmentComponent } from './compliance-form/agent-assignment/agent-assignment.component';
import { EmailSettingComponent } from './email-setting/email-setting.component';
@NgModule({
    imports: [
        ComplianceRoutingModule,
        UiModule
    ],
    declarations: [
        ComplianceListComponent,
        ComplianceFormComponent,
        BookingValidationLogListComponent,
        BookingValidationLogPopupComponent,
        AgentAssignmentComponent,
        EmailSettingComponent
    ],
    providers: [ComplianceListService, ComplianceFormService, BookingValidationLogListService]
})

export class ComplianceModule { }
