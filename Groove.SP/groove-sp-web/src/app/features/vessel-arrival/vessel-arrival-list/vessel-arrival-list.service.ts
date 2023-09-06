import { Injectable } from '@angular/core';
import { ListService } from 'src/app/core/list/list.service';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { HttpService } from 'src/app/core';

@Injectable({
    providedIn: 'root'
})
export class VesselArrivalListService extends ListService {
    affiliates: any;
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/vesselArrivals`);
        this.defaultState.sort = [
            { field: 'etaDate', dir: 'desc' },
            { field: 'poNumber', dir: 'desc' }
        ];
    }
}
