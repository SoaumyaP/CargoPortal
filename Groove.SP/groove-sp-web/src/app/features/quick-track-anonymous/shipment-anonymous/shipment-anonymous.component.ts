import { Component, ViewChild, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { MilestoneComponent } from 'src/app/ui/milestone/milestone.component';
import { DATE_FORMAT, FormComponent, MilestoneType, ActivityType } from 'src/app/core';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { ShipmentAnonymousService } from './shipment-anonymous.service';

@Component({
    selector: 'app-shipment-anonymous',
    templateUrl: './shipment-anonymous.component.html',
    styleUrls: ['./shipment-anonymous.component.scss']
})
export class ShipmentAnonymousComponent extends FormComponent {
    modelName = 'shipments';
    milestoneType = MilestoneType;
    @ViewChild('milestone', { static: true }) milestone: MilestoneComponent;
    DATE_FORMAT = DATE_FORMAT;

    constructor(protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: ShipmentAnonymousService,
        public translateService: TranslateService) {
        super(route, service, notification, translateService, router);
    }

    onInitDataLoaded(data): void {
        if (this.model !== null) {
            this.milestone.data = this.model.activities.filter(a => a.activityType === ActivityType.FreightShipment);
            this.milestone.reload();
        }
    }
}
