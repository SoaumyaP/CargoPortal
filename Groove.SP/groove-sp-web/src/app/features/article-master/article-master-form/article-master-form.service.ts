import { Injectable } from '@angular/core';
import { HttpService, FormService } from 'src/app/core';
import { environment } from 'src/environments/environment';

@Injectable()
export class ArticleMasterFormService extends FormService<any> {

    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/articlemasters/internal`);
    }
}