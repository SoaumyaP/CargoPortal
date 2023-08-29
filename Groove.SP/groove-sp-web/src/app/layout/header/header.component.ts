import { Component } from '@angular/core';
import { DropDowns } from 'src/app/core';
import { CustomTranslationService } from 'src/app/core/translation/custom-translation.service';

@Component({
    selector: 'app-header',
    templateUrl: './header.component.html',
    styleUrls: ['./header.component.scss']
})

export class HeaderComponent {
    public allLanguages = DropDowns.AllLanguages;
    public currentLanguage: string;
    constructor(public customTranslationService: CustomTranslationService) {
        this.currentLanguage = customTranslationService.getCachedLang();
    }

    valueChange(value: any) {
        localStorage.setItem('lang', value);
        location.reload();
    }
}
