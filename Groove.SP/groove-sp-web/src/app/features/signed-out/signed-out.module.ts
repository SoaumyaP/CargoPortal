import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { SignedOutComponent } from './signed-out.component';


const route = [
    { path: '', component: SignedOutComponent }
];

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(route),
        TranslateModule
    ],
    declarations: [
        SignedOutComponent
    ],
    providers: [
    ]
})
export class SignedOutModule { }