import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { BalanceOfGoodsEnquiryComponent } from './balance-of-goods-enquiry/balance-of-goods-enquiry.component';


const routes: Routes = [
  {
      path: '',
      component: BalanceOfGoodsEnquiryComponent,
      data:
      {
          permission: AppPermissions.BalanceOfGoods,
          pageName: 'balanceOfGoodsEnquiry'
      }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class BalanceOfGoodsRoutingModule { }
