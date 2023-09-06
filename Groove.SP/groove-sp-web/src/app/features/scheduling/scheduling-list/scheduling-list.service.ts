import { Injectable } from '@angular/core';
import { HttpService, ListService } from 'src/app/core';
import { environment } from 'src/environments/environment';

@Injectable()
export class SchedulingListService extends ListService {

    constructor(httpService: HttpService) {

        super(httpService, `${environment.apiUrl}/Schedulings`);
        this.defaultState.sort = [
        ];

    }
}
