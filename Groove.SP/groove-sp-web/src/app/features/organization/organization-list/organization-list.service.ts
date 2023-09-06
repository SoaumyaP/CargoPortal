import { Injectable } from '@angular/core';
import { HttpService } from 'src/app/core';
import { ListService } from 'src/app/core/list/list.service';
import { environment } from 'src/environments/environment';

@Injectable()
export class OrganizationListService extends ListService {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.commonApiUrl}/organizations`);
        this.defaultState.sort = [
            { field: 'createdDate', dir: 'asc' },
            { field: 'name', dir: 'asc' }
        ];
    }
}
