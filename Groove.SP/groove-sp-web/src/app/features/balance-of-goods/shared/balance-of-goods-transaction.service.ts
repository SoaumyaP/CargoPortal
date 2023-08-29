import { Injectable } from '@angular/core';
import { GridDataResult } from '@progress/kendo-angular-grid';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { HttpService, ListService } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { BalanceOfGoodsTransactionModel } from './balance-of-goods-transaction.model';
import { ListResultModel } from './list-result.model';

@Injectable()
export class BalanceOfGoodsTransactionService extends ListService {
    constructor(httpService: HttpService) {
        super(httpService, environment.supplementalApiUrl);
    }

    query = (state: any): void => {};

    queryTransaction(mode: string,
        principleId: number,
        articleId: number,
        warehouseId: number,
        state: any): Observable<ListResultModel<BalanceOfGoodsTransactionModel>> {
            const baseUrl = `${environment.supplementalApiUrl}/balance-of-goods/transaction`;
            const s = JSON.stringify(state);
            const encoded = encodeURIComponent(s);
            const url = baseUrl + `?mode=${mode}&principleId=${principleId ?? 0}&articleId=${articleId ?? 0}&warehouseId=${warehouseId ?? 0}&state=${encoded}`;
            return this.httpService.get<ListResultModel<BalanceOfGoodsTransactionModel>>(url)
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
}
