import { NgModule } from '@angular/core';

import { UiModule } from 'src/app/ui';
import { UserRequestRoutingModule } from './user-request-routing.module';
import { UserRequestListComponent } from './user-request-list/user-request-list.component';
import { UserRequestListService } from './user-request-list/user-request-list.service';
import { UserRequestFormComponent } from './user-request-form/user-request-form.component';
import { UserRequestFormService } from './user-request-form/user-request-form.service';
import { UsersService } from '../user/users/users.service';
import { CommonService } from 'src/app/core/services/common.service';

@NgModule({
    declarations: [UserRequestListComponent, UserRequestFormComponent],
    imports: [
        UiModule,
        UserRequestRoutingModule
    ],
    providers: [UserRequestListService, UserRequestFormService, UsersService, CommonService]
})
export class UserRequestModule { }