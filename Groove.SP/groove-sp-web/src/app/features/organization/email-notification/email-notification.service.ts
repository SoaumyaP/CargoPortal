import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { DropDownListItemModel, HttpService } from 'src/app/core';

@Injectable()
export class EmailNotificationService {

    constructor(public _httpService: HttpService) {

    }

    getPrincipalDataSource(roleId: number, organizationId: number, affiliates: string): Observable<Array<DropDownListItemModel<number>>> {
        const url = `${environment.commonApiUrl}/organizations/principals?roleId=${roleId}&affiliates=${affiliates || '[]'}&organizationId=${organizationId || 0}&checkIsBuyer=false`;
        return this._httpService.get<Array<DropDownListItemModel<number>>>(url);
    }

    getCountries() {
        return this._httpService.get(`${environment.commonApiUrl}/countries/DropDown`);
    }

    getAllLocations() {
        return this._httpService.get(`${environment.commonApiUrl}/countries/AllLocations`);
    }

    createEmailNotification(model) {
        return this._httpService.create(`${environment.commonApiUrl}/emailNotifications`, model);
    }

    updateEmailNotification(model, id) {
        return this._httpService.update(`${environment.commonApiUrl}/emailNotifications/${id}`, model);
    }

    deleteEmailNotification(id) {
        return this._httpService.delete(`${environment.commonApiUrl}/emailNotifications/${id}`);
    }
}