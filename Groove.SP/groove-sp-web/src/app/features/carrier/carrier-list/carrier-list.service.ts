import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService } from 'src/app/core';
import { ListService } from 'src/app/core/list/list.service';
import { CarrierModel } from 'src/app/core/models/carrier.model';
import { environment } from 'src/environments/environment';

@Injectable()
export class CarrierListService extends ListService {
    affiliates: any;
    constructor(httpService: HttpService) {
        super(httpService, `${environment.commonApiUrl}/carriers`);
        this.defaultState.sort = [
            { field: 'name', dir: 'asc' }
        ];
    }

    createNewCarrier(model: CarrierModel): Observable<CarrierModel> {
        return this.httpService.create<CarrierModel>(`${environment.commonApiUrl}/carriers`, model);
    }

    editCarrier(model: CarrierModel): Observable<CarrierModel> {
        return this.httpService.update<CarrierModel>(`${environment.commonApiUrl}/carriers/${model.id}`, model);
    }

    updateStatus(id: number, model: CarrierModel) {
        return this.httpService.update(`${environment.commonApiUrl}/carriers/${id}/updateStatus`, model);
    }
}
