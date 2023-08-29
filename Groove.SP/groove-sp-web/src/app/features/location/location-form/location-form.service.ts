import { Injectable } from '@angular/core';
import { FormService, HttpService } from 'src/app/core';
import { LocationModel } from 'src/app/core/models/location.model';
import { environment } from 'src/environments/environment';

@Injectable()
export class LocationFormService extends FormService<any> {
    constructor(httpService: HttpService) {
        super(httpService, `${environment.commonApiUrl}/locations`);
    }

    addNewLocation(model: LocationModel) {
        return this.httpService.create(`${environment.commonApiUrl}/locations/internal`, model);
    }

    editLocation(model: LocationModel) {
        return this.httpService.update(`${environment.commonApiUrl}/locations/internal/${model.id}`, model);
    }
}