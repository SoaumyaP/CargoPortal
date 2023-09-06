import { TranslateLoader } from '@ngx-translate/core';
import { Observable } from 'rxjs';
import { mergeMap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { HttpService } from '../services/http.service';

export class CustomTranslationLoader implements TranslateLoader {
    constructor(private httpService: HttpService) {}
    getTranslation(lang: string): Observable<any> {
        return this.httpService.get(`${environment.apiUrl}/translations/version`).pipe(
            mergeMap(version => this.httpService.get(`${environment.apiUrl}/translations/${lang}.json?version=${version}`))
        );
    }
}
