import { NgModule, Optional, SkipSelf } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { UrlSerializer } from '@angular/router';

import { AuthInterceptor } from './auth';
import { LowerCaseUrlSerializer } from './helpers';

import { throwIfAlreadyLoaded } from './guards/module-import.guard';

import { TranslationModule } from './translation/translation.module';

/*
    Here is the place to put Global and Singleton Service
*/
@NgModule({
    declarations: [
    ],
    imports: [
        HttpClientModule,
        TranslationModule
    ],
    providers: [
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AuthInterceptor,
            multi: true,
        },
        {
            provide: UrlSerializer,
            useClass: LowerCaseUrlSerializer
        }
    ],
    exports: [
        HttpClientModule
    ]
})
export class CoreModule {
    constructor(@Optional() @SkipSelf() parentModule: CoreModule) {
        throwIfAlreadyLoaded(parentModule, 'CoreModule');
    }
}