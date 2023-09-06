import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { DropDowns } from '../models/dropDowns/dropDowns';
import { MessageService } from '@progress/kendo-angular-l10n';
import { CldrIntlService, IntlService } from '@progress/kendo-angular-intl';
import { CustomMessageService } from './custom-message-service';
import { ActivatedRoute, Params } from '@angular/router';
import { StringHelper } from '../helpers';
import { registerLocaleData } from '@angular/common';

import ngZhHans from '@angular/common/locales/zh-Hans';
import ngZhHant from '@angular/common/locales/zh-Hant';
@Injectable({
    providedIn: 'root'
})
export class CustomTranslationService {
    public allLanguages = DropDowns.AllLanguages;
    public defaultLang = this.allLanguages.find(l => l.default === true);
    private messageService: CustomMessageService;

    constructor(public translateService: TranslateService, messageService: MessageService,
                public intlService: IntlService, protected route: ActivatedRoute) {
        this.messageService = <CustomMessageService>messageService;

        const langs = this.allLanguages.map(l => l.value);
        translateService.addLangs(langs);

        const cachedLang = this.getCachedLang();
        this.useLang(cachedLang);

        // quick track
        this.route.queryParams.subscribe((queryParams: Params) => {
            const lang = queryParams['lang'];
            if (!StringHelper.isNullOrEmpty(lang) && cachedLang !== lang) {
                localStorage.setItem('lang', lang);
                this.useLang(lang);
            }
        });

        translateService.onLangChange.subscribe(language => {
            this.messageService.setLanguage(language.lang, language.translations);
            (<CldrIntlService>this.intlService).localeId = language.lang;
            localStorage.setItem('lang', language.lang);
            this.importLocaleLibrary(language.lang);
        });
    }

    getCachedLang() {
        let cachedLang = localStorage.getItem('lang');
        // In case there is no local storage before
        if (!cachedLang) {
            // Get the first language that is supportive from browser langs
            const browserLangs = navigator.languages;
            if (browserLangs && browserLangs.length > 0) {
                const supportiveLang = browserLangs.find(x => this.allLanguages.some(y => StringHelper.caseIgnoredCompare(x, y.browserCultureLang) || StringHelper.caseIgnoredCompare(x, y.browserLang)));
                if (StringHelper.isNullOrEmpty(supportiveLang)) {
                    // In case there is no supportive lang, get default from portal
                    cachedLang = this.defaultLang.value;
                } else {
                    cachedLang = this.allLanguages.find(x => StringHelper.caseIgnoredCompare(supportiveLang, x.browserCultureLang) || StringHelper.caseIgnoredCompare(supportiveLang, x.browserLang)).value;
                }
            } else {
                // In case there is no browser langs, get default from portal
                cachedLang = this.defaultLang.value;
            }
        }
        return cachedLang;
    }

    useLang(lang) {
        this.translateService.use(lang);
    }

    importLocaleLibrary(localeId: string) {
        switch (localeId) {
            case 'zh-hans':
                import ('./kendo/kendo-zh-hans');
                registerLocaleData(ngZhHans, localeId);
                break;
            case 'zh-hant':
                import('./kendo/kendo-zh-hant');
                registerLocaleData(ngZhHant, localeId);
                break;
            default:
                break;
        }

    }
}

