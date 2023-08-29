import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { ListComponent } from 'src/app/core/list/list.component';
import { IntegrationLogListService } from './integration-log-list.service';
import { IntegrationLogStatus } from 'src/app/core';
import { IntegrationLogFormComponent } from '../intergration-log-form/integration-log-form.component';

@Component({
    selector: 'app-integration-log-list',
    templateUrl: './integration-log-list.component.html',
    styleUrls: ['./integration-log-list.component.scss']
})
export class IntegrationLogListComponent extends ListComponent implements OnInit {

    listName = 'integration-logs';

    integrationLogStatus = IntegrationLogStatus;

    @ViewChild(IntegrationLogFormComponent, { static: true }) formPopup: IntegrationLogFormComponent;

    constructor(service: IntegrationLogListService, route: ActivatedRoute, location: Location) {
        super(service, route, location);
    }
}
