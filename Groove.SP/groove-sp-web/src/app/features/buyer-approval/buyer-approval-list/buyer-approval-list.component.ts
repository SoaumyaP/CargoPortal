import { Component, OnInit} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { ListComponent } from 'src/app/core/list/list.component';
import { FormModeType, FulfillmentType, StringHelper, UserContextService } from '../../../core';
import { BuyerApprovalListService } from './buyer-approval-list.service';
import { map } from 'rxjs/operators';

@Component({
    selector: 'app-buyer-approval-list',
    templateUrl: './buyer-approval-list.component.html',
    styleUrls: ['./buyer-approval-list.component.scss']
})

export class BuyerApprovalListComponent extends ListComponent implements OnInit {
    listName = 'buyer-approvals';
    readonly fulfillmentType = FulfillmentType;
    formModeType = FormModeType;

    constructor(service: BuyerApprovalListService, route: ActivatedRoute, location: Location,
        private _userContext: UserContextService, private router: Router) {
        super(service, route, location);
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                if (!user.isInternal) {
                    this.service.affiliates = user.affiliates;
                    this.service.otherQueryParams.organizationId = user.organizationId;
                    this.service.state = Object.assign({}, this.service.defaultState);
                }
            }
        });
    }

    ngOnInit() {
        this.route.paramMap.pipe(map(() => window.history.state))
            .subscribe(
                (state) => {
                    if (StringHelper.isNullOrEmpty(state.statisticKey)) {
                        this.route.queryParams.subscribe(params => {
                            this.service.otherQueryParams.statisticKey
                                = this.service.statisticKey
                                = params.statistic;

                            this.service.otherQueryParams.userRole
                                = this.service.userRole
                                = params.userRole;
                        });
                    } else {
                        this.service.otherQueryParams.statisticKey
                            = this.service.statisticKey
                            = state.statisticKey;

                        this.service.otherQueryParams.userRole
                            = this.service.userRole
                            = state.userRole;
                    }
                }
            );

        super.ngOnInit();
    }
}
