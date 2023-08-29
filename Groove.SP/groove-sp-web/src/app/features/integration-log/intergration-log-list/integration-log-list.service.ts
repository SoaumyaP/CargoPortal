import { Injectable } from '@angular/core';
import { HttpService } from 'src/app/core';
import { ListService } from 'src/app/core/list/list.service';
import { environment } from 'src/environments/environment';

@Injectable()
export class IntegrationLogListService extends ListService {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/integrationLogs`);
        this.defaultState.sort = [
            { field: 'postingDate', dir: 'desc' },
        ];
        this.defaultState.filter = { logic: 'and', filters: [{
            field: 'postingDate',
            operator: 'gte',
            value: new Date()
        }]};
    }
}
