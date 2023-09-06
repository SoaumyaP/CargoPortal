import { Component, OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { UserContextService } from 'src/app/core';
import { BalanceOfGoodsService } from '../shared/balance-of-goods.service';
import { GridComponent,
    GridDataResult,
    PageChangeEvent
} from '@progress/kendo-angular-grid';
import { BehaviorSubject, Observable  } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { WindowService,
    WindowRef,
    DialogRef,
    DialogService
} from '@progress/kendo-angular-dialog';
import { BalanceOfGoodsTransactionsComponent } from '../balance-of-goods-transactions/balance-of-goods-transactions.component';
import { TranslateService } from '@ngx-translate/core';

@Component({
    selector: 'app-balance-of-goods-enquiry',
    templateUrl: './balance-of-goods-enquiry.component.html',
    styleUrls: ['./balance-of-goods-enquiry.component.scss'],
})
export class BalanceOfGoodsEnquiryComponent implements OnInit {
    constructor(
        private _userContext: UserContextService,
        private service: BalanceOfGoodsService,
        private dialogService: DialogService,
        private translateService: TranslateService
    ) {
    }

    public pagerTypes = ['numeric', 'input'];
    public buttonCount = 5;
    public info = true;
    public pageSizes = true;
    public previousNext = true;
    public position = 'bottom';
    public type = 'numeric';

    @ViewChild(GridComponent, { static: false }) grid: GridComponent;
    @ViewChild('container', { read: ViewContainerRef, static: false }) public containerRef: ViewContainerRef;

    isInternal = false;
    showPrinciple = false;
    affiliates = '[]';

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

    gridLoading: boolean = false;

    view: BehaviorSubject<GridDataResult> = new BehaviorSubject<GridDataResult>(null);

    ngOnInit(): void {
        this._userContext.getCurrentUser().subscribe((user) => {
            if (user) {
                if (!user.isInternal) {
                    const affiliates = user.affiliates ?? JSON.parse(user.affiliates);
                    if (affiliates && affiliates.length > 0) {
                        if (affiliates.includes(0) || affiliates.length > 1) {
                            this.showPrinciple = true;
                            this.affiliates = user.affiliates;
                        }
                    }
                } else {
                    this.isInternal = true;
                }
            }
        });
        this.loadData();
    }

    gridStateChange(e: any): void {
        this.state = e;
        this.loadData();
    }

    loadData() {
        this.gridLoading = true;
        this.service.query2(this.state, this.affiliates).pipe(
            tap((r) => {
                this.view.next({
                    data: r.records,
                    total: r.totalRecords
                });
                this.gridLoading = false;
            }),
            catchError(this.requestErrorHandler)
        ).subscribe();
    }

    requestErrorHandler (error: any): Observable<void> {
        this.gridLoading = false;
        console.log(error.message);
        return Observable.throwError(error.message || 'server error');
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

    openArticleTransactions(item: any) {
        // Article Transaction: Article {articleName} of {principleCode}
        const title = this.translateService
            .instant('label.articleTransactionTitle', {
                articleName: item.articleName,
                principleCode: item.principleCode
            });

        const dialogRef: DialogRef = this.dialogService.open({
            title: title,
            content: BalanceOfGoodsTransactionsComponent,
            width: 1000,
            height: 800
        });

        const instance = dialogRef.content.instance;
        instance.articleId = item.articleId;
        instance.principleId = item.principleId;
        instance.mode = 'article';
    }

    openWarehouseTransactions(item: any) {
        // Warehouse Transaction: Warehouse {warehouseCode} of {principleCode}
        const title = this.translateService
            .instant('label.warehouseTransactionTitle', {
                warehouseCode: item.warehouseCode,
                principleCode: item.principleCode
            });

        const dialogRef: DialogRef = this.dialogService.open({
            title: title,
            content: BalanceOfGoodsTransactionsComponent,
            width: 1000,
            height: 800
        });

        const instance = dialogRef.content.instance;
        instance.articleId = item.articleId;
        instance.principleId = item.principleId;
        instance.warehouseId = item.warehouseId;
        instance.mode = 'warehouse';
    }
}
