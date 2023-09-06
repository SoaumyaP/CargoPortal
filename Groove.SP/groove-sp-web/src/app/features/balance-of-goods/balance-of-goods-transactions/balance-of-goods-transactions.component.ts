import { Component, OnInit, ViewChild, Input } from '@angular/core';
import { GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import { BehaviorSubject, Observable } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { UserContextService } from 'src/app/core';
import { BalanceOfGoodsTransactionService } from '../shared/balance-of-goods-transaction.service';

@Component({
    selector: 'app-balance-of-goods-transactions',
    templateUrl: './balance-of-goods-transactions.component.html',
    styleUrls: ['./balance-of-goods-transactions.component.scss'],
})
export class BalanceOfGoodsTransactionsComponent implements OnInit {
    constructor(private _userContext: UserContextService,
        private service: BalanceOfGoodsTransactionService) {

        }

    @Input() public articleId: number;
    @Input() public principleId: number;
    @Input() public warehouseId: number;
    @Input() public mode: string;

    view: BehaviorSubject<GridDataResult> = new BehaviorSubject<GridDataResult>({data: [], total: 0});

    state = {
        filter: {
            logic: 'and',
            filters: []
        },
        group: [],
        sort: [
            {
                dir: 'asc',
                field: 'articleCode'
            }
        ],
        take: 20,
        skip: 0
    };

    ngOnInit(): void {
        this.loadData();
    }

    loadData() {
        this.service.queryTransaction(this.mode,
            this.principleId,
            this.articleId,
            this.warehouseId,
            this.state)
            .pipe(
                tap(x => {
                    this.view.next({total: x.totalRecords, data: x.records});
                }),
                catchError(this.requestErrorHandler)
            ).subscribe();
    }

    requestErrorHandler (error: any): Observable<void> {
        console.log(error.message);
        return Observable.throwError(error.message || 'server error');
    }

    gridStateChange(e: any): void {
        this.state = e;
        this.loadData();
    }

    pageChange({ skip, take }: PageChangeEvent): void {
        this.state.skip = skip;
        this.state.take = take;
    }

    gridPageSizeChange(e: any): void {
        this.state.take = e;
        this.state.skip = 0;
        this.loadData();
    }

    filterChange(e: any): void {
        this.state.filter = e;
        this.state.skip = 0;
    }

    sortChange(e: any): void {
        this.state.skip = 0;
        this.state.sort = e;
    }
}
