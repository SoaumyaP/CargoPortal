import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { DropDownListItemModel, HttpService } from '..';
import { CarrierModel } from '../models/carrier.model';
import { CurrencyModel } from '../models/currency.model';
import { OrganizationReferenceDataModel } from '../models/organization.model';
import { VesselModel } from '../models/vessel.model';

/**
 * Applying caching for every request
 */
@Injectable()
export class CommonService {
    constructor(private httpService: HttpService) { }

    getAllLocations(): Observable<any[]> {
        return this.httpService.getWithCache<any[]>(`${environment.commonApiUrl}/locations`);
    }

    getEvents(eventTypes: string): Observable<any[]> {
        return this.httpService.getWithCache<any[]>(`${environment.commonApiUrl}/eventCodes/eventByTypes?types=${eventTypes}`);
    }

    getCountries(): Observable<any[]> {
        return this.httpService.getWithCache(`${environment.commonApiUrl}/countries`);
    }

    getCountryDropdown(): Observable<any[]> {
        return this.httpService.getWithCache(`${environment.commonApiUrl}/countries/dropdown`);
    }

    getCountryDropdownByCode(code: string): Observable<any> {
        return this.httpService.getWithCache(`${environment.commonApiUrl}/countries/dropdownCode/${code}`);
    }

    getCurrencies(): Observable<CurrencyModel[]> {
        return this.httpService.getWithCache(`${environment.commonApiUrl}/currencies`);
    }

    getCarriers(): Observable<CarrierModel[]> {
        return this.httpService.getWithCache<CarrierModel[]>(`${environment.commonApiUrl}/carriers`);
    }

    getRealActiveVessels(): Observable<VesselModel[]> {
        return this.httpService.getWithCache<CarrierModel[]>(`${environment.commonApiUrl}/vessels/internal?filterType=realactive`);
    }

    searchRealActiveVessels(name: string): Observable<DropDownListItemModel<string>[]> {
        return this.httpService.get<DropDownListItemModel<string>[]>(`${environment.commonApiUrl}/vessels/internal/searchRealActive?name=${name}`);
    }

    getSupplierByCustomerId(customerId: number): Observable<any[]> {
        return this.httpService.get<any[]>(`${environment.commonApiUrl}/organizations/${customerId}/supplierSelections`);
    }

    getAllLocationSelections(): Observable<any[]> {
        return this.httpService.get<any[]>(`${environment.commonApiUrl}/countries/allLocationSelections`);
    }

    getOrganizations(): Observable<OrganizationReferenceDataModel[]> {
        return this.httpService.get(`${environment.commonApiUrl}/organizations/orgReferenceData/active`);
    }
}
