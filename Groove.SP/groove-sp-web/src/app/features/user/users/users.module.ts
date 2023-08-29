import { NgModule } from '@angular/core';
import { UsersListComponent } from './users-list/users-list.component';
import { UiModule } from 'src/app/ui';
import { UsersRoutingModule } from './users-routing.module';
import { UsersListService } from './users-list/users-list.service';
import { UserFormService } from './user-form/user-form.service';
import { UserFormComponent } from './user-form/user-form.component';
import { UsersService } from './users.service';

@NgModule({
  declarations: [UsersListComponent, UserFormComponent],
    imports: [
        UiModule,
        UsersRoutingModule
    ],
    providers: [UsersListService, UserFormService, UsersService]
})
export class UsersModule { }
