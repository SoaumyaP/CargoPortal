import { Injectable } from '@angular/core';
import { HttpService } from 'src/app/core';
import { ListService } from 'src/app/core/list/list.service';
import { environment } from 'src/environments/environment';

@Injectable()
export class LocationListService extends ListService {
    affiliates: any;
    constructor(httpService: HttpService) {
        super(httpService, `${environment.commonApiUrl}/locations`);
        this.defaultState.sort = [
            { field: 'locationDescription', dir: 'asc' }
        ];
    }

    getLocations() {
        return this.httpService.get(`${environment.commonApiUrl}/locations`);
    }
}