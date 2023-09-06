import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { HttpService } from 'src/app/core/services/http.service';
import { UserContextService } from 'src/app/core';

@Injectable()
export class WarehouseFulfillmentConfirmFormService {
    public currentUser: any;
    constructor(protected httpService: HttpService, private _userContext: UserContextService) {
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
            }
        });
    }

    searchWarehouseBookingConfirm(queryString: string) {
        if (!this.currentUser.isInternal) {
            queryString += `&Affiliates=${this.currentUser.affiliates}`
        }
        return this.httpService.get(`${environment.apiUrl}/warehouseFulfillments/confirm/search?${queryString}`);
    }

    confirmWarehouseBookings(warehouseBookings: any[]) {
        return this.httpService.update(`${environment.apiUrl}/warehouseFulfillments/confirm-by-patch`, warehouseBookings);
    }
}