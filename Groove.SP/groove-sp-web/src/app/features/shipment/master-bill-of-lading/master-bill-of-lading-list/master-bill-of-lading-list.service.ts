import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { DropDownListItemModel, HttpService, ListService } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { MasterBillOfLadingModel } from '../models/master-bill-of-lading-model';

@Injectable()
export class MasterBillOfLadingListService extends ListService {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/masterBillOfLadings`);
        this.defaultState.sort = [
            { field: 'onBoardDate', dir: 'desc' },
        ];
    }

    /**
     * To search for master bill of ladings by number on sever-side
     * @param searchTerm
     * @param isDirectMaster
     * @param isInternal
     * @param affiliates
     * @returns
     */
    searchMasterBLsByNumber(searchTerm: string, isDirectMaster: boolean, isInternal: boolean, affiliates: string): Observable<Array<MasterBillOfLadingModel>> {
        const affiliatesFilter = isInternal ? '' : `&affiliates=${affiliates}`;
        const result = this.httpService.get<Array<MasterBillOfLadingModel>>(`${environment.apiUrl}/masterBillOfLadings/searchByNumber?searchTerm=${encodeURIComponent(searchTerm)}&isDirectMaster=${isDirectMaster}${affiliatesFilter}`);
        return result;
    }

    /**
     * To search contract master on server-side
     * @param searchTerm Text to search
     * @param carrierCode Carrier code/SCAC to search
     * @param currentDate Current date without time
     * @returns Observable<Array<DropDownListItemModel<string>>>
     */
    getContractMasterOptions(searchTerm: string, carrierCode: string, currentDate: Date): Observable<Array<DropDownListItemModel<string>>> {
        const currentDateOnlyString = currentDate.toISOString().substring(0, 10);
        const result = this.httpService.get<Array<DropDownListItemModel<string>>>(`${environment.apiUrl}/contractMasters/masterBOLContractMasterOptions`
                        + `?searchTerm=${encodeURIComponent(searchTerm)}&carrierCode=${carrierCode}&currentDate=${currentDateOnlyString}`);
        return result;
    }
}
