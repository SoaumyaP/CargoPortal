import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { LOCALE_ID, NgModule } from '@angular/core';
import { CreateTranslationLoader } from './create-translation-loader';

import { MessageService } from '@progress/kendo-angular-l10n';
import { load, IntlModule } from '@progress/kendo-angular-intl';

import * as likelySubtags from 'cldr-data/supplemental/likelySubtags.json';
import * as weekData from 'cldr-data/supplemental/weekData.json';
import * as currencyData from 'cldr-data/supplemental/currencyData.json';
import * as numbers from 'cldr-data/main/de/numbers.json';
import * as calendar from 'cldr-data/main/de/ca-gregorian.json';
import * as currencies from 'cldr-data/main/de/currencies.json';

import './kendo/kendo-zh-hans';
import './kendo/kendo-zh-hant';
import { CustomMessageService } from './custom-message-service';
import { HttpService } from '../services/http.service';
import { StringHelper } from '../helpers';
import { DefaultEnUsLang } from '../models/constants/app-constants';

declare var require: any;
const dateFields = require('cldr-data/main/en/dateFields.json');
const fields = dateFields.main.en.dates.fields;
fields.day.displayName = 'DD';
fields.month.displayName = 'MM';
fields.year.displayName = 'YYYY';

load(
    likelySubtags,
    weekData,
    currencyData,
    numbers,
    currencies,
    calendar,
    dateFields
);

@NgModule({
    imports: [
        IntlModule,
        TranslateModule.forRoot({
            loader: {
                provide: TranslateLoader,
                useFactory: (CreateTranslationLoader),
                deps: [HttpService]
            }
        })
    ],
    exports: [
        IntlModule,
        TranslateModule
    ],
    providers: [
        {
            provide: MessageService,
            useClass: CustomMessageService
        },
        {
            provide: LOCALE_ID,
            useValue: StringHelper.isNullOrEmpty(localStorage.getItem('lang')) ? DefaultEnUsLang : localStorage.getItem('lang')
        }
    ],
    declarations: []
})

export class TranslationModule {}
