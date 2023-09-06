import { Injectable } from '@angular/core';
import { HttpService } from 'src/app/core';
import { FormService } from 'src/app/core/form';
import { environment } from 'src/environments/environment';

@Injectable()
export class RoleFormService extends FormService<any> {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/roles`);
    }
}
