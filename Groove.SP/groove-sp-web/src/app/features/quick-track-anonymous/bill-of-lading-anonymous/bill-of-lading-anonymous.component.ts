import { Component } from '@angular/core';
import { FormComponent, DATE_FORMAT } from '../../../core';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from '../../../ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { BillOfLadingAnonymousService } from './bill-of-lading-anonymous.service';


@Component({
    selector: 'app-bill-of-lading-anonymous',
    templateUrl: './bill-of-lading-anonymous.component.html',
    styleUrls: ['./bill-of-lading-anonymous.component.scss']
})
export class BillOfLadingAnonymousComponent extends FormComponent {
    DATE_FORMAT = DATE_FORMAT;
    modelName = 'billOfLadings';

    constructor(protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public _billOfLadingService: BillOfLadingAnonymousService,
        public router: Router,
        public translateService: TranslateService) {
        super(route, _billOfLadingService, notification, translateService, router);
    }

    onInitDataLoaded(data: void) {
    }
}
