import { Injectable } from '@angular/core';
import { HttpService } from 'src/app/core';
import { ListService } from 'src/app/core/list/list.service';
import { environment } from 'src/environments/environment';

@Injectable()
export class ArticleMasterListService extends ListService {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/articlemasters/internal`);
        this.defaultState.sort = [
            { field: 'itemDesc', dir: 'asc' },
        ];
    }
}
