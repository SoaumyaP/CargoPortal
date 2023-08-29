import { Component, OnInit } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute } from '@angular/router';

import { ListComponent } from 'src/app/core/list/list.component';

import { UserRequestListService } from './user-request-list.service';
import { UserStatus } from 'src/app/core';

@Component({
    selector: 'app-user-request-list',
    templateUrl: './user-request-list.component.html',
    styleUrls: ['./user-request-list.component.scss']
})
export class UserRequestListComponent extends ListComponent implements OnInit {
    userStatus = UserStatus;
    listName = 'user-requests';

    public dataItems: Array<{ text: string, value: boolean }> = [
        { text: 'label.internalUser', value: true },
        { text: 'label.externalUser', value: false}
    ];

    constructor(service: UserRequestListService, route: ActivatedRoute, location: Location) {
        super(service, route, location);
    }
}
