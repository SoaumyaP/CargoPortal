import { Injectable } from '@angular/core';
import { HttpService, VesselStatus } from 'src/app/core';
import { ListService } from 'src/app/core/list/list.service';
import { environment } from 'src/environments/environment';
import { VesselModel } from '../models/vessel.model';

@Injectable()
export class VesselListService extends ListService {
    affiliates: any;
    constructor(httpService: HttpService) {
        super(httpService, `${environment.commonApiUrl}/vessels/internal`);
        this.defaultState.sort = [
            { field: 'name', dir: 'asc' }
        ];
    }

    updateStatus(id: number, model: VesselModel) {
        return this.httpService.update(`${environment.commonApiUrl}/vessels/internal/${id}/updateStatus`, model);
    }
}
