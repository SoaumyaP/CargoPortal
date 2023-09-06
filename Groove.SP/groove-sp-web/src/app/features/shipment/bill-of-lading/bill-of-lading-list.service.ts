import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { FormService, HttpService, ListService } from '../../../core';
import { BillOfLadingModel } from './models/bill-of-lading.model';


@Injectable()
export class BillOfLadingListService extends ListService {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/billOfLadings`);
    }

    /**
     * To search house bill of ladings by number on sever-side
    */
    searchHouseBLsByNumber(searchTerm: string, isInternal: boolean, affiliates: string): Observable<Array<BillOfLadingModel>> {
        const affiliatesFilter = isInternal ? '' : `&affiliates=${affiliates}`;
        const result = this.httpService.get<Array<BillOfLadingModel>>(`${environment.apiUrl}/billOfLadings/searchByNumber?searchTerm=${encodeURIComponent(searchTerm)}${affiliatesFilter}`);
        return result;
    }
}
