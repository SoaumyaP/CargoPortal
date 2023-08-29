import { EventEmitter, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DataStateChangeEvent, GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import { SortDescriptor } from '@progress/kendo-data-query';
import { Observable, of } from 'rxjs';
import { HttpService, ListService } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { BalanceOfGoodsService } from './balance-of-goods.service';
import { ConstructorBase } from 'src/app/core/models/model-base.model';
import { AttachmentModel } from 'src/app/core/models/attachment.model';
import { BalanceOfGoodModel } from './balance-of-goods.model';
import { ListResultModel } from './list-result.model';
import { map, tap } from 'rxjs/operators';

@Injectable()
export class MockListService extends ListService {
    constructor(public http: HttpService) {
        super(http, environment.supplementalApiUrl);
        console.log('Mock List Service Loaded');
        super.serviceData = getResult();
    }

    public pageChanged: EventEmitter<PageChangeEvent> = new EventEmitter<PageChangeEvent>();
    public pageSizeChangd: EventEmitter<any> = new EventEmitter<any>();

    pageSizeChange(e: any): void {
        this.pageSizeChangd.emit(e);
    }

    pageChange(event: PageChangeEvent): void {
        this.pageChanged.emit(event);
    }

    queryToExport = (): Observable<GridDataResult> => of(null);
    query = (state: any): void => {};
    updateData = (x: any): void => {};
    queryQuickTracking = (state: any): Observable<GridDataResult> => of(null);
    dataStateChange = (state: DataStateChangeEvent): void => {};
    sortChange = (sort: SortDescriptor[]): void => {};
    fetch = (_apiUrl: string, state: any, isExport = false): Observable<GridDataResult> => of(null);
    checkAffiliateApiPrefix = (api: string): boolean => false;
    checkChildrenApiPrefix = (api: string): boolean => false;

}

@Injectable()
export class MockHttpService extends HttpService {
    constructor(private http: HttpClient) {
        super(http);
    }

    get = <T>(api: string, params?: any): Observable<T> => of(null);
    getWithCache<T>(api: string, params?: any): Observable<T> {
        return of(null);
    }
    create<T>(api: string, payload?: any, isSilent: boolean = true): Observable<T> {
        return of(null);
    }
    update<T>(api: string, payload?: any): Observable<T> {
        return of(null);
    }
    convertModelThenUpdate<InputModel>(type: ConstructorBase, api: string,  payload?: InputModel): Observable<any> {
        return of(null);
    }
    delete<T>(api: string): Observable<T> {
        return of(null);
    }
    uploadFile(api: string, file: File, type: string): Observable<any> {
        return of(null);
    }
    downloadFile(url: string, fileName): Observable<any> {
        return of(null);
    }
    downloadAttachments(
        fileName: string,
        attachments: Array<AttachmentModel>): Observable<any> {
            return of(null);
        }
}

@Injectable()
export class MockBalanceOfGoodsService extends BalanceOfGoodsService {
    constructor(private http: HttpService) {
        super(http);
    }

    query2(state: any, principles: any): Observable<ListResultModel<BalanceOfGoodModel>> {
        const a = [];
        a.push(...getResult(), ...getResult(), ...getResult(), ...getResult(), ...getResult());


        return of({
            totalRecords: 1000,
            records: a
        }).pipe(
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
    // fetch = (_apiUrl: string, state: any, isExport = false): Observable<GridDataResult> => of(<GridDataResult>{total: 0, data: []});
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
