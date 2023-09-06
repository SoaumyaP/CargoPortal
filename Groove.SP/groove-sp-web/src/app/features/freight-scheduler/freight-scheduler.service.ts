import { Injectable } from '@angular/core';
import { HttpService, FormService } from 'src/app/core';
import { environment } from 'src/environments/environment';

@Injectable()
export class FreightSchedulerService extends FormService<any> {

    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/freightschedulers`);
    }

    public edit(id, model) {
        return this.httpService.update(`${this.apiUrl}/edit/${id}`, model);
    }

    getFreightScheduler(id: number) {
        return this.httpService.get(`${this.apiUrl}/${id}`);
    }

    deleteFreightScheduler(id: number) {
        return this.httpService.delete(`${this.apiUrl}/${id}`);
    }
}
