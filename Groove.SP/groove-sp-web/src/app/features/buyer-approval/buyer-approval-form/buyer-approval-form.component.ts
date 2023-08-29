import { Component } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormComponent, UserContextService, DropDowns, StringHelper, DATE_HOUR_FORMAT, FulfillmentType, FormModeType } from 'src/app/core';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { BuyerApprovalFormService } from './buyer-approval-form.service';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { faCheck, faBan, faTimes } from '@fortawesome/free-solid-svg-icons';
import { ApproverSetting, BuyerApprovalStage, POFulfillmentStageType } from 'src/app/core';
import { WarehouseFulfillmentFormService } from '../../warehouse-fulfillment/warehouse-fulfillment-form/warehouse-fulfillment-form.service';
import { GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
@Component({
    selector: 'app-buyer-approval-form',
    templateUrl: './buyer-approval-form.component.html',
    styleUrls: ['./buyer-approval-form.component.scss']
})
export class BuyerApprovalFormComponent extends FormComponent {
    modelName = 'buyerapprovals';
    faCheck = faCheck;
    faBan = faBan;
    faTimes = faTimes;
    buyerApprovalStage = DropDowns.BuyerApprovalStage;
    buyerApprovalStageType = BuyerApprovalStage;
    poFulfillmentStage = POFulfillmentStageType;
    approverSettingType = ApproverSetting;
    formModeType = FormModeType;
    locations = [];
    readonly AppPermissions = AppPermissions;
    readonly fulfillmentType = FulfillmentType;
    DATE_HOUR_FORMAT = DATE_HOUR_FORMAT;

    // Control saving message popup
    saveBookingFailed = false;
    saveBookingErrors: Array<string> = [];

    constructor(protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: BuyerApprovalFormService,
        public translateService: TranslateService,
        private warehouseFulfillmentFormService: WarehouseFulfillmentFormService,
        private _userContext: UserContextService,
        private _gaService: GoogleAnalyticsService) {
        super(route, service, notification, translateService, router);
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                if (!user.isInternal) {
                    this.service.affiliateCodes = user.affiliates;
                }
            }
        });
    }

    returnValue(field) {
        return this.model[field] ? this.model[field] : '--';
    }

    onInitDataLoaded(data): void {
        this.service.getLocations().subscribe((locations: any) => {
            this.locations = locations;
        });
    }

    getLocationName(locationId) {
        const item = this.locations.find(x => x.id === locationId);
        return item ? item.locationDescription : '--';
    }

    backList() {
        this.router.navigate(['/buyer-approvals']);
    }

    onApprove() {
        const confirmDlg = this.notification.showConfirmationDialog('msg.confirmApprovePOFF', 'label.poFulfillment');

        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    if (this.model.poFulfillment.stage === this.poFulfillmentStage.ForwarderBookingRequest) {
                        switch (this.model.poFulfillment.fulfillmentType) {
                            case FulfillmentType.Warehouse:
                                this.warehouseFulfillmentFormService.approve(this.model).subscribe(data => {
                                    this.notification.showSuccessPopup('msg.bookingHasApproved', 'label.poFulfillment');
                                    this._gaService.emitEvent('approve', GAEventCategory.BookingApproval, 'Approve');
                                    this.ngOnInit();
                                },
                                (err) => {
                                    if (err.error) {
                                        const errors = err.error.errors.split(/(?=,\d+:\/)/);
                                        this.saveBookingErrors = [];
                                        if (errors) {
                                            this.saveBookingErrors = errors.map(x => x.replace(',', '', 1));
                                        }
                                        this.saveBookingFailed = true;
                                        this.saveBookingErrors.unshift(this.translateService.instant('msg.submittedBookingFailedDetails'));
                                    } else {
                                        this.notification.showErrorPopup('msg.cannotApprovePOFF', 'label.poFulfillment');
                                    }
                                });
                                break;
                            default:
                                this.service.approve(this.model).subscribe(data => {
                                    this.notification.showSuccessPopup('msg.bookingHasApproved', 'label.poFulfillment');
                                    this._gaService.emitEvent('approve', GAEventCategory.BookingApproval, 'Approve');
                                    this.ngOnInit();
                                },
                                err => {
                                    if (err.error) {
                                        const errors = err.error.errors.split(/(?=,\d+:\/)/);
                                        this.saveBookingErrors = [];
                                        if (errors) {
                                            this.saveBookingErrors = errors.map(x => x.replace(',', '', 1));
                                        }
                                        this.saveBookingFailed = true;
                                        this.saveBookingErrors.unshift(this.translateService.instant('msg.submittedBookingFailedDetails'));
                                    } else {
                                        this.notification.showErrorPopup('msg.cannotApprovePOFF', 'label.poFulfillment');
                                    }
                                });
                                break;
                        }
                    } else {
                        this.notification.showErrorPopup('msg.cannotApprovePOFF', 'label.poFulfillment');
                    }
                }
            });
    }

    get hiddenBtn() {
        if (this.model.stage !== BuyerApprovalStage.Pending) {
            return true;
        }

        return false;
    }

    onReject() {
        if (StringHelper.isNullOrEmpty(this.model.exceptionDetail)) {
            this.notification.showErrorPopup('msg.invalidRejectPOFF', 'label.poFulfillment');
            return;
        }
        const confirmDlg = this.notification.showConfirmationDialog('msg.confirmRejectPOFF', 'label.poFulfillment');

        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    switch (this.model.poFulfillment.fulfillmentType) {
                        case FulfillmentType.Warehouse:
                            this.warehouseFulfillmentFormService.reject(this.model).subscribe(data => {
                                this.notification.showSuccessPopup('msg.bookingHasRejected', 'label.poFulfillment');
                                this._gaService.emitEvent('reject', GAEventCategory.BookingApproval, 'Reject');
                                this.ngOnInit();
                            });
                            break;

                        default:
                            this.service.reject(this.model).subscribe(data => {
                                this.notification.showSuccessPopup('msg.bookingHasRejected', 'label.poFulfillment');
                                this._gaService.emitEvent('reject', GAEventCategory.BookingApproval, 'Reject');
                                this.ngOnInit();
                            });
                            break;
                    }
                }
            });
    }
}
