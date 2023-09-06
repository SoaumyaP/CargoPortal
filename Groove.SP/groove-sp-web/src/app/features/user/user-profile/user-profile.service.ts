import { Injectable } from '@angular/core';
import { HttpService } from 'src/app/core';
import { FormService } from 'src/app/core/form';
import { environment } from 'src/environments/environment';

@Injectable()
export class UserProfileFormService extends FormService<any> {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/users`);
    }

    getOrganization(id) {
        return this.httpService.get(`${environment.commonApiUrl}/organizations/${id}`);
    }

    updateCurrentUser(model) {
        return this.httpService.update(`${environment.apiUrl}/users/current`, model);
    }
}
