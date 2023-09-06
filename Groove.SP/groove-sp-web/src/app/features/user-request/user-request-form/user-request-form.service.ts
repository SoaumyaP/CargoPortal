import { Injectable } from '@angular/core';
import { HttpService } from 'src/app/core';
import { FormService } from 'src/app/core/form';
import { environment } from 'src/environments/environment';

@Injectable()
export class UserRequestFormService extends FormService<any> {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/userrequests`);
    }

    approveUserRequest(data: any) {
        return this.httpService.update(
            `${environment.apiUrl}/userrequests/${data.id}/approve`, data);
    }

    rejectUserRequest(data: any) {
        return this.httpService.update(
            `${environment.apiUrl}/userrequests/${data.id}/reject`, data);
    }
}
