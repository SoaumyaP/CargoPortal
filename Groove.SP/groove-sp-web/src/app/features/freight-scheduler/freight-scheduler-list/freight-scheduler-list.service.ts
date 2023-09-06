import { Injectable } from '@angular/core';
import { HttpService, ListService } from 'src/app/core';
import { environment } from 'src/environments/environment';

@Injectable()
export class FreightSchedulerListService extends ListService {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/freightSchedulers`);
        this.defaultState.sort = [
            { field: 'etdDate', dir: 'desc' },
        ];
    }

    deleteFreightScheduler(id) {
        return this.httpService.delete(`${environment.apiUrl}/freightSchedulers/${id}`);
    }
}