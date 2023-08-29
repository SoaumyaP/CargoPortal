import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService, FormService } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { WarehouseLocationModel } from '../models/warehouse-location.model';

@Injectable()
export class WarehouseLocationFormService extends FormService<any> {

    constructor(httpService: HttpService) {
        super(httpService, `${environment.commonApiUrl}/warehouseLocations`);
    }

    getCountries(): Observable<any[]> {
        return this.httpService.get(`${environment.commonApiUrl}/countries/dropdown`);
    }

    getAgentOrganizations(): Observable<any[]> {
        return this.httpService.get(`${environment.commonApiUrl}/organizations/agents/dropdown`);
    }

    getLocations(countryId: number): Observable<any[]> {
        return this.httpService.get(
            `${environment.commonApiUrl}/countries/${countryId}/locations/dropdown`);
    }

    createWarehouseLocation(model: WarehouseLocationModel): Observable<WarehouseLocationModel> {
        return this.httpService.create<WarehouseLocationModel>(`${environment.commonApiUrl}/warehouseLocations`, model);
    }

    updateWarehouseLocation(model: WarehouseLocationModel): Observable<WarehouseLocationModel> {
        return this.httpService.update<WarehouseLocationModel>(`${environment.commonApiUrl}/warehouseLocations/${model.id}`, model);
    }

    getLocationSameCountryDropDown(locationId: number): Observable<any[]> {
        return this.httpService.get(`${environment.commonApiUrl}/locations/${locationId}/samecountry/dropdown`);
    }

    getCustomerList(warehouseLocationId: number): Observable<any[]> {
        return this.httpService.get(`${environment.commonApiUrl}/warehouseLocations/${warehouseLocationId}/customers`);
    }
}
