import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { HttpService } from 'src/app/core/services/http.service';
import { UserContextService } from 'src/app/core';
import { POProgressCheckModel } from '../po-progress-check.model';

@Injectable()
export class POProgressCheckFormService {
    public currentUser: any;
    constructor(protected httpService: HttpService, private _userContext: UserContextService) {
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
            }
        });
    }

    searchPOProgressCheck(
        queryString: string
    ): Observable<Array<POProgressCheckModel>> {
        if (!this.currentUser.isInternal) {
            queryString += `&Affiliates=${this.currentUser.affiliates}`
        }
        return this.httpService.get(`${environment.apiUrl}/purchaseOrders/progressCheck/search?${queryString}`);
    }

    searchPOProgressCheckFromEmail(buyerComplianceId, poIds: string): Observable<Array<POProgressCheckModel>> {
        return this.httpService.get(`${environment.apiUrl}/purchaseOrders/progressCheck/searchFromEmail?buyerComplianceId=${buyerComplianceId}&poIds=${poIds}`);
    }

    savePOProgressCheck(poProgressCheckModels: POProgressCheckModel[]) {
        return this.httpService.update(`${environment.apiUrl}/purchaseOrders/progressCheck`, poProgressCheckModels);
    }
}