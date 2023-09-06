import { Injectable } from '@angular/core';
import { ListService } from 'src/app/core/list/list.service';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { ActivatedRoute } from '@angular/router';
import { HttpService, StringHelper } from 'src/app/core';

@Injectable()
export class SupplierRelationshipListService extends ListService {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.commonApiUrl}/supplierRelationships`);
        this.defaultState = {
            skip: 0,
            take: 10,
            sort: [
                { field: 'code', dir: 'asc' }
            ]
        };
    }

    getSuppliers(customerId: number): Observable<any[]> {
        return this.httpService.get(`${environment.commonApiUrl}/organizations/${customerId}/suppliers`);
    }

    getCountries(): Observable<any[]> {
        return this.httpService.get(`${environment.commonApiUrl}/countries/dropdown`);
    }

    getLocations(countryId: number): Observable<any[]> {
        return this.httpService.get(
            `${environment.commonApiUrl}/countries/${countryId}/locations/dropdown`);
    }

    addSupplierRelationship(
        customerId: number,
        supplierId: number,
        supplier: any
    ): Observable<null> {
        return this.httpService.update(`${environment.commonApiUrl}/organizations/${customerId}/suppliers/${supplierId}`, supplier);
    }

    removeSupplierRelationship(
        customerId: number,
        supplierId: number,
    ): Observable<null> {
        return this.httpService.delete(`${environment.commonApiUrl}/organizations/${customerId}/suppliers/${supplierId}`);
    }

    addSupplier(
        customerId: number,
        supplier: any
    ): Observable<null> {
        return this.httpService.create(`${environment.commonApiUrl}/organizations/${customerId}/suppliers`, supplier);
    }

    getSuppliersBy(name,
    contactEmail,
    contactNumber,
    websiteDomain): Observable<any> {
        let filter = `name~eq~'${name}'~or~contactEmail~eq~'${contactEmail}'`;

        if (!StringHelper.isNullOrEmpty(contactNumber)) {
            filter += `~or~contactNumber~eq~'${contactNumber}'`;
        }

        if (!StringHelper.isNullOrEmpty(websiteDomain)) {
            filter += `~or~websiteDomain~eq~'${websiteDomain}'`;
        }

        return this.httpService.get(`${environment.commonApiUrl}/organizations/search?
        filter=(${filter})`);
    }

    resendConnectionToSupplier(supplierId, customerId) {
        return this.httpService.update(`${environment.commonApiUrl}/organizations/${supplierId}/resendConnectionToSupplier/${customerId}`);
    }
}
