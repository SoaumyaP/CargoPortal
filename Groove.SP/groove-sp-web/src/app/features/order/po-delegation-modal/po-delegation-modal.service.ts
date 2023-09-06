import { Injectable } from '@angular/core';
import { HttpService, FormService } from 'src/app/core';
import { environment } from 'src/environments/environment';

@Injectable()
export class PODelegationService extends FormService<any> {

    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/purchaseorders`);
    }

    getOrganizations() {
        return this.httpService.getWithCache<Array<any>>(`${environment.commonApiUrl}/organizations/activeCodes`);
    }

    getUsersByOrganizationId(organizationId: number) {
        return this.httpService.get<Array<any>>(`${environment.apiUrl}/users/byorganization/${organizationId}`);
    }

    delegatePO(model) {
        return this.httpService.update(`${environment.apiUrl}/purchaseorders/${model.id}/delegate`, model);
    }
}
