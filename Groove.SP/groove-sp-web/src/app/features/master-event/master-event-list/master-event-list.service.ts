import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { EventCodeStatus, HttpService } from 'src/app/core';
import { ListService } from 'src/app/core/list/list.service';
import { CarrierModel } from 'src/app/core/models/carrier.model';
import { environment } from 'src/environments/environment';

@Injectable()
export class MasterEventListService extends ListService {
    affiliates: any;
    constructor(httpService: HttpService) {
        super(httpService, `${environment.commonApiUrl}/eventcodes`);

        this.defaultState.sort = [
            { field: 'createdDate', dir: 'desc' }
        ];
    }

    enableSortMode() {
        let state = { ...this.defaultState };
        state.sort = [
            { field: 'sortSequence', dir: 'asc' }
        ];
        delete state['take'];
        this.query(state);
    }

    updateSortSequences(model: any) {
        return this.httpService.update(`${environment.commonApiUrl}/eventcodes/sequenceUpdates`, model);
    }

    updateStatus(evenCodeId: number, status: EventCodeStatus) {
        return this.httpService.update(`${environment.commonApiUrl}/eventcodes/${evenCodeId}/statusUpdate`, { status });
    }
}
