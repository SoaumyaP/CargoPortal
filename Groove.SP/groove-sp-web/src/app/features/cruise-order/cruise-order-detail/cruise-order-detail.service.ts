import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { HttpService, FormService } from 'src/app/core';
import { IntegrationData } from 'src/app/core/models/forms/integration-data';
import { environment } from 'src/environments/environment';

@Injectable()
export class CruiseOrderDetailService extends FormService<any> {

    /**Used to interact between components in the cruise order module
     * by sending or listening for events and processing them by key name */
    public integration$: Subject<IntegrationData> = new Subject();

    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/cruiseorders`);
    }

    getActivities(cruiseOrderId: number): Observable<any[]> {
        return this.httpService.get(`${environment.apiUrl}/cruiseOrders/${cruiseOrderId}/activities`);
    }

    createActivity(cruiseOrderId: number, model) {
        return this.httpService.create(`${environment.apiUrl}/cruiseOrders/${cruiseOrderId}/activities`,
        model);
    }

    updateActivity(cruiseOrderId: number, activityId: number, model) {
        return this.httpService.update(`${environment.apiUrl}/cruiseOrders/${cruiseOrderId}/activities/${activityId}`,
        model);
    }

    deleteActivity(cruiseOrderId: number, activityId: number) {
        return this.httpService.delete(`${environment.apiUrl}/cruiseOrders/${cruiseOrderId}/activities/${activityId}`);
    }
}
