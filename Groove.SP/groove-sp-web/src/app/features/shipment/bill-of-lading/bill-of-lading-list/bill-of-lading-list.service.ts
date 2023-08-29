import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService, ListService } from 'src/app/core';
import { environment } from 'src/environments/environment';

@Injectable()
export class BillOfLadingListService extends ListService {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/billOfLadings`);
        this.defaultState.sort = [
            { field: 'issueDate', dir: 'desc' },
        ];
    }
}
