import { Injectable } from '@angular/core';
import { HttpService } from 'src/app/core';
import { ListService } from 'src/app/core/list/list.service';
import { environment } from 'src/environments/environment';

@Injectable()
export class ShortshipListService extends ListService {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/shortships`);
        this.defaultState.sort = [
            { field: 'approvedDate', dir: 'desc' },
            { field: 'poFulfillmentNumber', dir: 'desc' }
        ];
    }

    readOrUnread(id: number, model: any) {
        return this.httpService.update(`${environment.apiUrl}/shortships/${id}/read-or-unread`, model);
    }
}