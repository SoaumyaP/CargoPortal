import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { BalanceOfGoodsComponent } from './balance-of-goods.component';


const routes: Routes = [
    {
        path: '',
        component: BalanceOfGoodsComponent,
        data:
        {
            permission: AppPermissions.Organization_ArticleMaster_List,
            pageName: 'balance-of-goods'
        }
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class BalanceOfGoodsRoutingModule { }
