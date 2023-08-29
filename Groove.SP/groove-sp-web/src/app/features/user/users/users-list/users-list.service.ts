import { Injectable } from '@angular/core';
import { HttpService, ListService } from 'src/app/core';
import { environment } from 'src/environments/environment';

@Injectable()
export class UsersListService extends ListService {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/users`);
        this.defaultState.sort = [
            { field: 'name', dir: 'asc' },
            { field: 'organizationName', dir: 'asc' }
        ];
    }
}