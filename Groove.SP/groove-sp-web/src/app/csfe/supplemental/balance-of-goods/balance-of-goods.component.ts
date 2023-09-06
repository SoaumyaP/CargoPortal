import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { ArticleMasterStatus, FormComponent } from 'src/app/core';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { UserContextService } from 'src/app/core';
import { BalanceOfGoodsService } from './balance-of-goods.service';

@Component({
  selector: 'app-balance-of-goods',
  templateUrl: './balance-of-goods.component.html',
  styleUrls: ['./balance-of-goods.component.scss']
})
export class BalanceOfGoodsComponent implements OnInit {

  constructor(protected route: ActivatedRoute,
    public notification: NotificationPopup,
    public router: Router,
    private _userContext: UserContextService,
    public service: BalanceOfGoodsService) {

  }

  ngOnInit(): void {
    this._userContext.getCurrentUser().subscribe(user => {
        if (user) {
            if (!user.isInternal) {
                this.service.affiliates = user.affiliates;
            }
        }
    });
  }
}
