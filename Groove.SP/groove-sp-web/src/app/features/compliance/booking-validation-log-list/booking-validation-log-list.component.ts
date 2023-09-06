import { Component, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { ListComponent } from 'src/app/core/list/list.component';
import { BookingValidationLogListService } from './booking-validation-log-list.service';
import { DATE_FORMAT, FormModeType } from 'src/app/core';
import { BookingValidationLogPopupComponent } from '../booking-validation-log-popup/booking-validation-log-popup.component';

@Component({
    selector: 'app-booking-validation-log-list',
    templateUrl: './booking-validation-log-list.component.html',
    styleUrls: ['./booking-validation-log-list.component.scss']
})
export class BookingValidationLogListComponent extends ListComponent {

    DATE_FORMAT = DATE_FORMAT;
    formModeType = FormModeType;
    listName = 'compliances/booking-validation-logs';
    complianceId = null;
    isChildListComponent = true;
    @ViewChild(BookingValidationLogPopupComponent, { static: true }) formPopup: BookingValidationLogPopupComponent;
    constructor(service: BookingValidationLogListService, route: ActivatedRoute, location: Location) {
        super(service, route, location);
        this.route.queryParams.subscribe((params) => {
            this.service.parentId = params.parentid;
            this.service.state = Object.assign({}, this.service.defaultState);
        });
    }
}
