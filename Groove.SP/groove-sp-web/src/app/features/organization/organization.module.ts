import { NgModule } from '@angular/core';

import { UiModule } from 'src/app/ui';

import { OrganizationListComponent } from './organization-list/organization-list.component';
import { OrganizationFormComponent } from './organization-form/organization-form.component';
import { OrganizationListService } from './organization-list/organization-list.service';
import { OrganizationsRoutingModule } from './organization-routing.module';
import { OrganizationFormService } from './organization-form/organization-form.service';
import { CustomerRelationshipComponent } from './customer-relationship/customer-relationship.component';
import { AddUserFormComponent } from './add-user-form/add-user-form.component';
import { SupplierRelationshipListComponent } from './supplier-relationship-list/supplier-relationship-list.component';
import { SupplierRelationshipListService } from './supplier-relationship-list/supplier-relationship-list.service';
import { AddSupplierRelationshipFormComponent } from './add-supplier-relationship-form/add-supplier-relationship-form.component';
import { WarehouseAssignmentComponent } from './warehouse-assignment/warehouse-assignment.component';
import { EmailNotificationComponent } from './email-notification/email-notification.component';
import { EmailNotificationService } from './email-notification/email-notification.service';
import { AddAffiliateFormComponent } from './add-affiliate-form/add-affiliate-form.component';

@NgModule({
    imports: [
        OrganizationsRoutingModule,
        UiModule
    ],
    declarations: [
        OrganizationListComponent,
        OrganizationFormComponent,
        CustomerRelationshipComponent,
        AddUserFormComponent,
        SupplierRelationshipListComponent,
        AddSupplierRelationshipFormComponent,
        WarehouseAssignmentComponent,
        EmailNotificationComponent,
        AddAffiliateFormComponent
    ],
    providers: [OrganizationListService, OrganizationFormService, SupplierRelationshipListService, EmailNotificationService]
})

export class OrganizationModule { }