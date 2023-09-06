import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CommonService } from 'src/app/core/services/common.service';
import { UiModule } from 'src/app/ui';

import { BalanceOfGoodsRoutingModule } from './balance-of-goods-routing.module';
import { BalanceOfGoodsEnquiryComponent } from './balance-of-goods-enquiry/balance-of-goods-enquiry.component';
import { BalanceOfGoodsTransactionsComponent } from './balance-of-goods-transactions/balance-of-goods-transactions.component';
import { BalanceOfGoodsService } from './shared/balance-of-goods.service';
import { MockBalanceOfGoodsService, MockHttpService, MockListService } from './shared/mock-list-service';
import { HttpService } from 'src/app/core';
import { WindowModule } from '@progress/kendo-angular-dialog';
import { BalanceOfGoodsTransactionService } from './shared/balance-of-goods-transaction.service';


@NgModule({
  declarations: [BalanceOfGoodsEnquiryComponent,
    BalanceOfGoodsTransactionsComponent],
  imports: [
    CommonModule,
    BalanceOfGoodsRoutingModule,
    UiModule,
    WindowModule
  ],
  providers: [
    CommonService,
    BalanceOfGoodsService,
    BalanceOfGoodsTransactionService,
    // MockListService,
    // {
    //     provide: BalanceOfGoodsService, useClass: MockBalanceOfGoodsService
    // },
    // {
    //     provide: HttpService, useClass: MockHttpService
    // }
  ],
  entryComponents: [BalanceOfGoodsTransactionsComponent]
})
export class BalanceOfGoodsModule { }
