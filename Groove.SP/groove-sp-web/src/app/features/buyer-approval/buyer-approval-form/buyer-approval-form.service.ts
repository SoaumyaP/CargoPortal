import { Injectable } from '@angular/core';
import { HttpService, FormService } from 'src/app/core';
import { environment } from 'src/environments/environment';

@Injectable()
export class BuyerApprovalFormService extends FormService<any> {

    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/buyerApprovals`);
    }

    getLocations() {
        return this.httpService.getWithCache(
            `${environment.commonApiUrl}/locations`);
    }

    approve(model) {
        return this.httpService.update(`${environment.apiUrl}/buyerApprovals/${model.id}/approve`, model);
    }

    reject(model) {
        return this.httpService.update(`${environment.apiUrl}/buyerApprovals/${model.id}/reject`, model);
    }
}
