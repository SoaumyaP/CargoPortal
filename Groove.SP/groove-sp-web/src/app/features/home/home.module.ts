import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { HomeComponent } from './home.component';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { UiModule } from 'src/app/ui';
import { HomeService } from './home.service';
import { CategorizedPOComponent } from '../categorized-po/categorized-po.component';
import { SurveyFormService } from '../survey/survey-form/survey-form.service';

const route = [
    {
        path: '',
        component: HomeComponent,
        data:
        {
            permission: AppPermissions.Dashboard,
            pageName: 'dashboard'
        }
    }
];

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(route),
        TranslateModule,
        UiModule
    ],
    declarations: [
        HomeComponent,
        CategorizedPOComponent
    ],
    providers: [
        HomeService,
        SurveyFormService
    ]
})
export class HomeModule { }
