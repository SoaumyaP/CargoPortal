import { OnInit, OnDestroy } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { DropDowns, StringHelper, UserContextService } from 'src/app/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Subscription, timer } from 'rxjs';
import { ReportCriteriaFormService } from './report-criteria-form.service';
import {
    faFileExport,
    faRedo
} from "@fortawesome/free-solid-svg-icons";
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';

export class ReportFormBase implements OnInit, OnDestroy {
    poStageTypeOptions = DropDowns.POStageType;
    bookingStageTypeOptions = DropDowns.BookingStageType;
    incotermTypeOptions = DropDowns.IncotermCustomStringType;
    faFileExport = faFileExport;
    faRedo = faRedo;

    isInitDataLoaded: boolean;
    isReadyForExport: boolean = true;

    selectedCustomerId: any;
    selectedReportId: any;
    // Store all subscriptions, then should un-subscribe at the end
    protected subscriptions: Array<Subscription> = [];

    mainForm: FormGroup;
    formErrors = {};

    constructor(
        protected reportCriteriaFormService: ReportCriteriaFormService,
        protected _userContext: UserContextService,
        protected router: Router,
        protected activatedRoute: ActivatedRoute,
        protected notification: NotificationPopup

    ) { }

    ngOnInit() {
        this.isInitDataLoaded = false;

        this.activatedRoute.queryParams.subscribe(params => {
            this.selectedCustomerId = params['customer'];
            this.selectedReportId = params['report'];
            if (StringHelper.isNullOrEmpty(this.selectedCustomerId) || StringHelper.isNullOrEmpty(this.selectedReportId)) {
                this.router.navigate(['/error/404']);
            } else {
                this.reportCriteriaFormService.checkExportPermission(this.selectedReportId, this.selectedCustomerId).subscribe(
                    success => {
                        this.ngOnInitDataLoaded();
                    },
                    err => {
                        // return 403 Forbid status code: Handled by http interceptor.
                    }
                )
            }
        });
    }

    protected ngOnInitDataLoaded() { }

    protected onFormInit() { }

    onResetClick() {
        this.onFormInit();
        this.formErrors = {};
    }

    protected onExportClick() {
        this.isReadyForExport = false;

        if (!this.isFormValid) {
            return;
        }

        // Every by 20 seconds, popup to UI to inform
        const sub = timer(20000, 20000).subscribe(() => {
            this.notification.showInfoPopup(
                'label.stillWorking',
                'label.result'
            );
        });
        this.subscriptions.push(sub);
    }

    protected fieldValue(name) {
        return this.mainForm.controls[`${name}`].value;
    }

    protected setValueForControl(controlName, value) {
        return this.mainForm.controls[`${controlName}`].setValue(value);
    }

    protected get isFormValid() {
        for (var key in this.formErrors) {
            if (this.formErrors.hasOwnProperty(key))
                return false;
        }
        return true;
    }

    backList() {
        this.router.navigate(["/reports"]);
    }

    ngOnDestroy(): void {
        this.subscriptions.map(x => x.unsubscribe());
    }

    protected resetAfterDownload() {
        this.isReadyForExport = true;
        this.subscriptions.map(x => x.unsubscribe());
    }
}
