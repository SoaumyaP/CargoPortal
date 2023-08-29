import { Injectable } from '@angular/core';
import { HttpService, FormService, StringHelper, POType } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { take, tap } from 'rxjs/operators';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { NoteModel } from 'src/app/core/models/note.model';
import { CarrierModel } from 'src/app/core/models/carrier.model';
import { OrganizationReferenceDataModel } from 'src/app/core/models/organization.model';
import { VesselModel } from '../../models/vessel.model';

@Injectable()
export class VesselFormService extends FormService<any> {

    constructor(httpService: HttpService) {
        super(httpService, `${environment.commonApiUrl}/vessels/internal`);
    }

    addNewVessel(model: VesselModel) {
        return this.httpService.create(`${environment.commonApiUrl}/vessels/internal`, model);
    }

    updateVessel(model: VesselModel) {
        return this.httpService.update(`${environment.commonApiUrl}/vessels/internal/${model.id}`, model);
    }

    getAllVessels() {
        return this.httpService.get(`${environment.commonApiUrl}/vessels/internal`);
    }
}
