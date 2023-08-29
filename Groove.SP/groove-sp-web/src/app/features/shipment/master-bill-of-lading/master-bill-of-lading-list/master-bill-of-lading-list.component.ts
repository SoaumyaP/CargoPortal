import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { ListComponent, UserContextService } from 'src/app/core';
import { MasterBillOfLadingListService } from './master-bill-of-lading-list.service';

@Component({
    selector: 'app-user-list',
    templateUrl: './master-bill-of-lading-list.component.html',
    styleUrls: ['./master-bill-of-lading-list.component.scss']
})
export class MasterBillOfLadingListComponent extends ListComponent implements OnInit {
    listName = 'master-bill-of-ladings';

    constructor(public service: MasterBillOfLadingListService, route: ActivatedRoute, location: Location
    , private _userContext: UserContextService) {
        super(service, route, location);
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                if (!user.isInternal) {
                    this.service.affiliates = user.affiliates;
                    this.service.state = Object.assign({}, this.service.defaultState);
                }
            }
        });
    }

    ngOnInit() {
        super.ngOnInit();
    }
}
