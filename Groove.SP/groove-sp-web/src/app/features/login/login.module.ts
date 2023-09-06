import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { LoginComponent } from './login.component';


const route = [
    {
        path: '',
        component: LoginComponent,
        data: {
            pageName: 'login'
        }
    }
];

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(route),
        TranslateModule
    ],
    declarations: [
        LoginComponent
    ],
    providers: [
    ]
})
export class LoginModule { }