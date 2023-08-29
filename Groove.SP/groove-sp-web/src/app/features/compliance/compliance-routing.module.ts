import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { ComplianceListComponent } from './compliance-list/compliance-list.component';
import { ComplianceFormComponent } from './compliance-form/compliance-form.component';
import { BookingValidationLogListComponent } from './booking-validation-log-list/booking-validation-log-list.component';

const routes: Routes = [
    {
        path: '', component: ComplianceListComponent,
        data:
        {
            pageName: 'compliances',
            permission: AppPermissions.Organization_Compliance_List
        }
    },
    {
        path: 'booking-validation-logs', component: BookingValidationLogListComponent,
        data:
        {
            pageName: 'bookingValidationLogs',
            permission: AppPermissions.Organization_Compliance_Detail
        }
    },
    {
        path: ':mode/:id', component: ComplianceFormComponent,
        data:
        {
            pageName: 'complianceDetail',
            permission: {
                'view': AppPermissions.Organization_Compliance_Detail,
                'add': AppPermissions.Organization_Compliance_Detail_Edit,
                'edit': AppPermissions.Organization_Compliance_Detail_Edit }
        }
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class ComplianceRoutingModule { }