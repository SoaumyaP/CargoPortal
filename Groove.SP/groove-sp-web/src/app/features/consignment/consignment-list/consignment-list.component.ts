import { Component} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { ListComponent } from 'src/app/core/list/list.component';
import { UserContextService } from '../../../core';
import { ConsignmentListService } from './consignment-list.service';

@Component({
    selector: 'app-consignment-list',
    templateUrl: './consignment-list.component.html',
    styleUrls: ['./consignment-list.component.scss']
})

export class ConsignmentListComponent extends ListComponent {
    listName = 'consignments';

    constructor(service: ConsignmentListService, route: ActivatedRoute, location: Location,
        private _userContext: UserContextService, private router: Router) {
        super(service, route, location);
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                if (!user.isInternal) {
                    this.service.affiliates = user.affiliates;
                }
            }
        });
    }
}
