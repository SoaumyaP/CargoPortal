import { NgModule } from '@angular/core';
import { RolesListComponent } from './roles-list/roles-list.component';
import { UiModule } from 'src/app/ui';
import { RolesListService } from './roles-list/roles-list.service';
import { RolesRoutingModule } from './roles-routing.module';
import { RoleFormComponent } from './role-form/role-form.component';
import { RoleFormService } from './role-form/role-form.service';

@NgModule({
    declarations: [RolesListComponent, RoleFormComponent],
    imports: [
      UiModule,
      RolesRoutingModule
    ],
    providers: [RolesListService, RoleFormService]
})
export class RolesModule { }
