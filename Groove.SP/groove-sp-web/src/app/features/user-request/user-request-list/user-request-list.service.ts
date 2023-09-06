import { Injectable } from '@angular/core';
import { ListService } from 'src/app/core/list/list.service';
import { environment } from 'src/environments/environment';
import { DataStateChangeEvent } from '@progress/kendo-angular-grid';
import { HttpService } from 'src/app/core';

@Injectable()
export class UserRequestListService extends ListService {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/userrequests`);
        this.defaultState.sort = [
            { field: 'createdDate', dir: 'asc' },
            { field: 'name', dir: 'asc' }
        ];
    }

    public dataStateChange(state: DataStateChangeEvent): void {
        if (state.sort == undefined || state.sort[0].dir == undefined || state.sort[0].field == undefined) {
            state.sort = this.defaultState.sort;
        }
        super.dataStateChange(state);
    }
}
