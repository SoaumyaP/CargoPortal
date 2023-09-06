import { Injectable } from '@angular/core';
import { HttpService, StringHelper } from 'src/app/core';
import { Observable, of } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable()
export class AssignPOsFormService {

    constructor(private _httpService: HttpService) {
    }

    getSourcePOsDataSource(principalOrganizationId: number,
        supplierOrganizationId: number,
        skip: number,
        take: number,
        searchTeam: string): Observable<any> {
        if ((StringHelper.isNullOrEmpty(principalOrganizationId) || principalOrganizationId === 0) &&
            (StringHelper.isNullOrEmpty(supplierOrganizationId) || supplierOrganizationId === 0)) {
            return of([]);
        } else {
            const searchTermQueryString = !StringHelper.isNullOrEmpty(searchTeam) ? `&searchTerm=${encodeURIComponent(searchTeam.trim())}` : '';
            let url = `${environment.apiUrl}/purchaseorders/unmappedPurchaseOrderSelections?principalId=${principalOrganizationId || 0}&supplierId=${supplierOrganizationId || 0}`;
            url += `&skip=${skip}&take=${take}${searchTermQueryString}`;
            return this._httpService.get<any>(url);
        }
    }

    getTotalCount(principalOrganizationId: number,
        supplierOrganizationId: number) {
        
            if ((StringHelper.isNullOrEmpty(principalOrganizationId) || principalOrganizationId === 0) &&
            (StringHelper.isNullOrEmpty(supplierOrganizationId) || supplierOrganizationId === 0)) {
            return of(0);
        } else {
            let url = `${environment.apiUrl}/purchaseorders/unmappedPurchaseOrderTotalCount?principalId=${principalOrganizationId || 0}&supplierId=${supplierOrganizationId || 0}`;
            return this._httpService.get<any>(url);
        }
    }

    assignPOs(purchaseOrderIds: Array<number>,
        organizationId: number,
        role: string) {
        return this._httpService.create(`${environment.apiUrl}/purchaseorders/assignPurchaseOrders`, { 
            purchaseOrderIds, organizationId, organizationRole: role
        });
    }
}
