import { Injectable } from '@angular/core';
import { HttpService } from 'src/app/core';
import { ListService } from 'src/app/core/list/list.service';
import { environment } from 'src/environments/environment';

@Injectable()
export class WarehouseLocationListService extends ListService {
    affiliates: any;
    constructor(httpService: HttpService) {
        super(httpService, `${environment.commonApiUrl}/warehouselocations`);
        this.defaultState.sort = [
            { field: 'code', dir: 'asc' }
        ];
    }
}
