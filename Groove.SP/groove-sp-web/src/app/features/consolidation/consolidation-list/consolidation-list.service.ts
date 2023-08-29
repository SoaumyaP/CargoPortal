import { Injectable } from '@angular/core';
import { HttpService, ListService } from 'src/app/core';
import { environment } from 'src/environments/environment';

@Injectable()
export class ConsolidationListService extends ListService {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/consolidations/internal`);
        this.defaultState.sort = [
            { field: 'cfsCutoffDate', dir: 'desc' },
        ];
    }
}