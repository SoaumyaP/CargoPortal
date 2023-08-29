import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { faPencilAlt, faPlus } from '@fortawesome/free-solid-svg-icons';
import { ListComponent, UserContextService } from 'src/app/core';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { WarehouseLocationListService } from './warehouse-location-list.service';
import { Location } from '@angular/common';
import { Subscription } from 'rxjs';

@Component({
    selector: 'app-warehouse-location-list',
    templateUrl: './warehouse-location-list.component.html',
    styleUrls: ['./warehouse-location-list.component.scss']
})
export class WarehouseLocationListComponent extends ListComponent implements OnInit, OnDestroy {
    listName = 'warehouse-locations';
    readonly AppPermissions = AppPermissions;
    faPlus = faPlus;
    faPencilAlt = faPencilAlt;

    isWarehouseLocationViewAllow: boolean = false;
    isOrganizationViewAllow: boolean = false;
    private _currentUser: any;
    private _subscriptions: Array<Subscription> = [];

    constructor(
        public service: WarehouseLocationListService,
        route: ActivatedRoute,
        location: Location,
        private _userContextService: UserContextService
    ) {
        super(service, route, location);
    }

    ngOnInit() {
        super.ngOnInit();

        this._userContextService.isGranted(AppPermissions.Organization_WarehouseLocation_Detail).subscribe(
            result => {
                this.isWarehouseLocationViewAllow = result;
        });

        this._userContextService.isGranted(AppPermissions.Organization_Detail).subscribe(
            result => {
                this.isOrganizationViewAllow = result;
        });
    }

    ngOnDestroy(): void {
        this._subscriptions.map(x => x.unsubscribe());
    }

}
