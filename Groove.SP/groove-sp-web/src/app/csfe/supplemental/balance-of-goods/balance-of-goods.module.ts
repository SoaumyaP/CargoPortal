import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UiModule } from 'src/app/ui';
import { CommonService } from 'src/app/core/services/common.service';
import { BalanceOfGoodsRoutingModule } from './balance-of-goods-routing.module';
import { BalanceOfGoodsService } from './balance-of-goods.service';
import { BalanceOfGoodsComponent } from './balance-of-goods.component';

@NgModule({
    declarations: [
        BalanceOfGoodsComponent
    ],
    imports: [
        CommonModule,
        UiModule,
        BalanceOfGoodsRoutingModule
    ],
    providers: [
        CommonService,
        BalanceOfGoodsService
    ]
})
export class BalanceOfGoodsModule { }
