import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { ListComponent } from 'src/app/core/list/list.component';
import { ComplianceListService } from './compliance-list.service';
import { BuyerComplianceStatus } from 'src/app/core';
import { faPlus } from '@fortawesome/free-solid-svg-icons';
import { AppPermissions } from 'src/app/core/auth/auth-constants';

@Component({
    selector: 'app-compliance-list',
    templateUrl: './compliance-list.component.html',
    styleUrls: ['./compliance-list.component.scss']
})
export class ComplianceListComponent extends ListComponent implements OnInit {

    listName = 'compliances';
    faPlus = faPlus;
    buyerComplianceStatus = BuyerComplianceStatus;
    readonly AppPermissions = AppPermissions;
    constructor(service: ComplianceListService, route: ActivatedRoute, location: Location) {
        super(service, route, location);
    }
}
