import { Injectable } from '@angular/core';
import { HttpService } from 'src/app/core';
import { ListService } from 'src/app/core/list/list.service';
import { environment } from 'src/environments/environment';

@Injectable()
export class ComplianceListService extends ListService {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/buyercompliances`);
        this.defaultState.sort = [
            { field: 'createdDate', dir: 'desc' },
        ];
    }
}
