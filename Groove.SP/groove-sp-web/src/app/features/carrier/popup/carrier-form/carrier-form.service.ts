import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CarrierModel } from 'src/app/core/models/carrier.model';
import { environment } from 'src/environments/environment';

@Injectable()
export class CarrierFormService {
    constructor(protected _httpClient: HttpClient) {
        
    }

    checkDuplicateCarrierCode(model: CarrierModel): Observable<boolean> {
        return this._httpClient.post<boolean>(`${environment.commonApiUrl}/carriers/checkDuplicateCarrierCode`, model);
    }

    checkDuplicateCarrierName(model: CarrierModel): Observable<boolean> {
        return this._httpClient.post<boolean>(`${environment.commonApiUrl}/carriers/checkDuplicateCarrierName`, model);
    }

    checkDuplicateCarrierNumber(model: CarrierModel): Observable<boolean> {
        return this._httpClient.post<boolean>(`${environment.commonApiUrl}/carriers/checkDuplicateCarrierNumber`, model);
    }
}
