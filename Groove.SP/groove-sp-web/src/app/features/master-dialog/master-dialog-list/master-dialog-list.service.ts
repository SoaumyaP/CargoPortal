import { Injectable } from '@angular/core';
import { HttpService, ListService } from 'src/app/core';
import { environment } from 'src/environments/environment';

@Injectable()
export class MasterDialogListService extends ListService {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/masterDialogs`);
        this.defaultState.sort = [
            { field: 'createdDate', dir: 'desc' },
        ];
    }

    delete(id: number) {
        return this.httpService.delete(`${environment.apiUrl}/masterdialogs/${id}`);
    }
}