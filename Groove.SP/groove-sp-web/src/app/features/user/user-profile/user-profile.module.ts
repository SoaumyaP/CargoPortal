import { NgModule } from '@angular/core';

import { UiModule } from 'src/app/ui';
import { UserProfileFormComponent } from './user-profile.component';
import { UserProfileFormService } from './user-profile.service';
import { UserProfileRoutingModule } from './user-profile-routing.module';

@NgModule({
    declarations: [UserProfileFormComponent],
    imports: [
        UiModule,
        UserProfileRoutingModule
    ],
    providers: [UserProfileFormService]
})
export class UserProfileModule { }
