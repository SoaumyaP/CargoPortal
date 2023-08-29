import { ChangeDetectorRef, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { CruiseOrderStatus, FormComponent, MilestoneType, Roles, UserContextService } from 'src/app/core';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { CruiseOrderDetailService } from './cruise-order-detail.service';
import { faInfo } from '@fortawesome/free-solid-svg-icons';
import { MilestoneComponent } from 'src/app/ui/milestone/milestone.component';
import { Subject, Subscription } from 'rxjs';
import { IntegrationData } from 'src/app/core/models/forms/integration-data';
import { CruiseOrderModel } from '../models/cruise-order.model';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { filter } from 'rxjs/operators';

@Component({
    selector: 'app-cruise-order-detail',
    templateUrl: './cruise-order-detail.component.html',
    styleUrls: ['./cruise-order-detail.component.scss'],
})
export class CruiseOrderDetailComponent extends FormComponent implements OnInit, OnDestroy {

    @ViewChild('milestone', { static: false }) milestone: MilestoneComponent;
    modelName = 'cruiseOrders';
    model: CruiseOrderModel;
    cruiseOrderStatus = CruiseOrderStatus;
    faInfo = faInfo;
    milestoneType = MilestoneType;
    currentUser: any = null;
    defaultValue = '--';

    readonly AppPermissions = AppPermissions;
    isReadyForSubmit: boolean = true;
    // Store all subscriptions, then should un-subscribe at the end
    private _subscriptions: Array<Subscription> = [];
    integration$: Subject<IntegrationData> = new Subject();

    constructor(
        protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public translateService: TranslateService,
        public service: CruiseOrderDetailService,
        private _userContext: UserContextService
    ) {
        super(route, service, notification, translateService, router);

        this._userContext.getCurrentUser().subscribe((user) => {
            if (user) {
                this.currentUser = user;
            }
        });

        this._registerMilestoneChangedHandler();
    }

    convertModelType(data: any): CruiseOrderModel {
        if (data !== null) {
            return new CruiseOrderModel(data);
        }
        return data;
    }

    onInitDataLoaded(data): void {
        this._registerEventHandlers();
    }

    _registerMilestoneChangedHandler() {
        // Handler activity list changed fired from cruise-order-activity
        const sub = this.integration$.pipe(
                filter((event: IntegrationData) =>
                    event.name === '[cruise-order-activity]activityListChanged'
                )).subscribe((event: IntegrationData) => {
                        if (this.milestone != null) {
                            const activityList = event.content.activityList;
                            this.milestone.data = activityList;
                            this.milestone.reload();
                        }
                });
        this._subscriptions.push(sub);
    }

    private _registerEventHandlers(): void {
        // to update model as cruise order items have been changed
        // emit event to cruise order details (root level) because of load on demand on items level
        const sub = this.service.integration$
            .pipe(
                filter(
                    (eventContent: IntegrationData) => eventContent.name === '[cruise-order-item]onItemDetailsUpdated'
                )
            )
            .subscribe((eventContent: IntegrationData) => {
                const cruiseOrderItems = eventContent.content['lineItems'];
                this.model.items =  Object.assign([], cruiseOrderItems);
            });
        this._subscriptions.push(sub);
    }

    backList() {
        this.router.navigate(['/cruise-orders']);
    }

    returnValue(field) {
        return this.model[field] ? this.model[field] : '--';
    }

    editCruiseOrder() {
        this.router.navigate([`/cruise-orders/edit/${this.model.id}`]);
        this.ngOnInit();
    }

    onCancel() {
        const confirmDlg = this.notification.showConfirmationDialog(
            'edit.cancelConfirmation',
            'label.cruiseOrder'
        );

        confirmDlg.result.subscribe((result: any) => {
            if (result.value) {
                if (this.isAddMode) {
                    this.backList();
                } else {
                    if (this.isEditMode) {
                        this.router.navigate([
                            `/cruise-orders/view/${this.model.id}`,
                        ]);
                        this.ngOnInit();
                    }
                }
            }
        });
    }

    ngOnDestroy(): void {
        this._subscriptions.map((x) => x.unsubscribe());
    }
}
