import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { FormService, HttpService } from '../../../core';

@Injectable()
export class ContainerAnonymousService extends FormService<any> {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/containers`);
    }
}
