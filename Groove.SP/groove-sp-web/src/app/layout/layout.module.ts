import { NgModule } from '@angular/core';

import { LayoutModule as KendoLayoutModule } from '@progress/kendo-angular-layout';

import { UiModule } from '../ui';
import { LayoutRoutingModule } from './layout-routing.module';
import { SearchComponent } from './header/search/search.component';
import { NavigationComponent } from './navigation/navigation.component';
import { FooterComponent } from './footer/footer.component';
import { LayoutComponent } from './layout.component';
import { HeaderComponent } from './header/header.component';;
import { UserRoleSwitchComponent } from './header/user-role-switch/user-role-switch.component'
import { UserRoleSwitchServer } from './header/user-role-switch/user-role-switch.service';
import { NotificationComponent } from './header/notification/notification.component'
import { NotificationService } from './header/notification/notification.service';

@NgModule({
    declarations: [
        LayoutComponent,
        HeaderComponent,
        SearchComponent,
        NavigationComponent,
        FooterComponent,
        UserRoleSwitchComponent,
        NotificationComponent
    ],
    imports: [
        KendoLayoutModule,
        UiModule,
        LayoutRoutingModule
    ],
    providers: [
        UserRoleSwitchServer,
        NotificationService
    ]
})

export class LayoutModule { }
