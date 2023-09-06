import { Injectable } from '@angular/core';
import { HttpService, FormService } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';

@Injectable()
export class ConsignmentFormDialogService extends FormService<any> {

    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/consignments`);
    }

    getOrganizations() {
        return this.httpService.getWithCache<Array<any>>(`${environment.commonApiUrl}/organizations/activeCodes`);
    }

    getOrganization(id): Observable<any> {
        return this.httpService.get(`${environment.commonApiUrl}/organizations/${id}`);
    }

    getAllLocations(): any {
        return this.httpService.getWithCache<any[]>(`${environment.commonApiUrl}/locations`);
    }
}
