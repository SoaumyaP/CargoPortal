import { Component, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormComponent, MilestoneType, ActivityType, DATE_FORMAT } from '../../../core';
import { NotificationPopup } from '../../../ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { MilestoneComponent } from '../../../ui/milestone/milestone.component';
import { ContainerAnonymousService } from './container-anonymous.service';

@Component({
    selector: 'app-container-anonymous',
    templateUrl: './container-anonymous.component.html',
    styleUrls: ['./container-anonymous.component.scss']
})
export class ContainerAnonymousComponent extends FormComponent {
    milestoneType = MilestoneType;
    modelName = 'containers';
    @ViewChild('milestone', { static: true }) milestone: MilestoneComponent;
    DATE_FORMAT = DATE_FORMAT;

    constructor(protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public service: ContainerAnonymousService,
        public router: Router,
        public translateService: TranslateService) {
        super(route, service, notification, translateService, router);
    }

    onInitDataLoaded(data: void) {
        if (this.model != null) {
            this.milestone.data = this.model.activities.filter(a => a.activityType === ActivityType.Container);
            this.milestone.reload();
        }
    }
}
