import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService, FormService, DropdownListModel } from 'src/app/core';
import { environment } from 'src/environments/environment';

@Injectable()
export class ShipmentItineraryConfirmPopupService {

    constructor(private httpService: HttpService) {
    }

    getTerminalDataSource() : Observable<DropdownListModel<string>[]> {
        return this.httpService.getWithCache(`${environment.commonApiUrl}/terminals/dropdown`);
    }

    getWarehouseDataSource() : Observable<DropdownListModel<string>[]> {
        return this.httpService.getWithCache(`${environment.commonApiUrl}/warehouses/dropdown`);
    }
}