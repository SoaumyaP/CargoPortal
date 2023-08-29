import { Injectable } from '@angular/core';
import { HttpService, ListService } from 'src/app/core';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ShippedPurchaseOrderListService extends ListService {
    affiliates: Array<number> = [];
    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/PurchaseOrders/Shipped`);
        this.defaultState.sort = [
            { field: 'createdDate', dir: 'desc' }
        ];
    }
}
