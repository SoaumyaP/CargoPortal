import { Injectable } from '@angular/core';
import { FormService } from 'src/app/core/form';
import { environment } from 'src/environments/environment';
import { HttpService, UserStatus } from 'src/app/core';

@Injectable()
export class UserFormService extends FormService<any> {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/users`);
    }

    getOrganizationList(status) {
        if (status === UserStatus.Active) {
            return this.httpService.get(`${environment.commonApiUrl}/organizations/orgReferenceData/active`);
        } else {
            return this.httpService.get(`${environment.commonApiUrl}/organizations/orgReferenceData`);
        }
    }

    sendActivationEmail(userId: number) {
        return this.httpService.create(`${this.apiUrl}/${userId}/send-activation-email`);
    }
}
