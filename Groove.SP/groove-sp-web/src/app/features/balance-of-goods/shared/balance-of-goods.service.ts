import { Injectable } from '@angular/core';
import { GridDataResult } from '@progress/kendo-angular-grid';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { HttpService, ListService } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { BalanceOfGoodModel } from './balance-of-goods.model';
import { ListResultModel } from './list-result.model';

@Injectable()
export class BalanceOfGoodsService extends ListService  {
    constructor(httpService: HttpService) {
        super(httpService, environment.supplementalApiUrl);
    }

    query2 (state: any, principles: any): Observable<ListResultModel<BalanceOfGoodModel>> {
        const baseUrl = `${environment.supplementalApiUrl}/balance-of-goods/search`;
        if (principles && principles.length > 0) {
            state.principles = JSON.parse(principles);
        }

        const s = JSON.stringify(state);
        const encoded = encodeURIComponent(s);
        const url = baseUrl + `?query=${encoded}`;
        return this.httpService.get<ListResultModel<BalanceOfGoodModel>>(url)
            .pipe(
                tap(x => {
                    this.dataCount = x.totalRecords;
                    this.serviceData = x.records;
                    const g = <GridDataResult>{
                        data: x.records, total: x.totalRecords
                    };
                    super.next(g);
                    this.gridLoading = false;
                })
            );
    }

    query = (state: any): void => {};
}

const getResult = (): BalanceOfGoodModel[] => {
    const j = `[
        {
           "principleId":3,
           "PrincipleCode":"CASE0021",
           "PrincipleName":"CARGO SERVICES FAR EAST LIMITED",
           "warehouseId":3,
           "warehouseCode":"GROHNWH002",
           "warehouseName":"Groove warehouse 002",
           "locationId":22,
           "locationName":"AUSYD",
           "articleId":55945,
           "articleCode":"30034827                                          ",
           "articleName":"STORAGE CHEST CANVAS BUTTERFLY SMALL",
           "availableQuantity":-1405,
           "receivedQuantity":0,
           "shippedQuantity":1000,
           "adjustQuantity":-305,
           "damageQuantity":-100
        },
        {
           "PrincipleId":4,
           "PrincipleCode":"ORG0001",
           "PrincipleName":"INTERNATIONAL ART ENTERPRISE",
           "warehouseId":4,
           "warehouseCode":"GROHNWH003",
           "warehouseName":"Groove Hai Nguyen warehouse 003",
           "locationId":22,
           "locationName":"AUSYD",
           "articleId":55945,
           "articleCode":"30034827                                          ",
           "articleName":"STORAGE CHEST CANVAS BUTTERFLY SMALL",
           "availableQuantity":1000,
           "receivedQuantity":1000,
           "shippedQuantity":0,
           "adjustQuantity":0,
           "damageQuantity":0
        },
        {
           "PrincipleId":1,
           "PrincipleCode":"PUGL0002",
           "PrincipleName":"PURE GLOBAL LOGISTICS PTY LTD",
           "warehouseId":1,
           "warehouseCode":"LAFASA01",
           "warehouseName":"LAU FAU SHAN warehouse A",
           "locationId":306,
           "locationName":"HKHKG",
           "articleId":55927,
           "articleCode":"30034809                                          ",
           "articleName":"PLAY MAT WITH 2 CARS",
           "availableQuantity":-2000,
           "receivedQuantity":0,
           "shippedQuantity":2000,
           "adjustQuantity":0,
           "damageQuantity":0
        },
        {
           "PrincipleId":2,
           "PrincipleCode":"THRE0003",
           "PrincipleName":"THE REJECT SHOP LIMITED",
           "warehouseId":2,
           "warehouseCode":"GROHNWH001",
           "warehouseName":"Groove warehouse 001",
           "locationId":22,
           "locationName":"AUSYD",
           "articleId":55927,
           "articleCode":"30034809                                          ",
           "articleName":"PLAY MAT WITH 2 CARS",
           "availableQuantity":2000,
           "receivedQuantity":2000,
           "shippedQuantity":0,
           "adjustQuantity":0,
           "damageQuantity":0
        }
     ]`;
     const obj = <Array<BalanceOfGoodModel>>JSON.parse(j);
     return obj;
};
