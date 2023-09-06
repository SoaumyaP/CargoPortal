import { MessageService } from '@progress/kendo-angular-l10n';
import { Injectable } from '@angular/core';
import { DefaultEnUsLang } from '../models/constants/app-constants';

@Injectable({
    providedIn: 'root'
})
export class CustomMessageService extends MessageService {

    private localeId =  DefaultEnUsLang;
    private translations: any = {};

    public setLanguage(lang: string, translations: any) {
        this.localeId = lang;
        this.translations = translations;
        this.notify(false);
    }

    public get language(): string {
        return this.localeId;
    }

    private get messages(): any {
        return this.translations;
    }

    public get(key: string): string {
        if (this.messages != null) {
            const msg = this.messages[key];
            return msg == null ? key : msg;
        }
        return key;
    }
}

