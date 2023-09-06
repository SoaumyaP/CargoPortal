import { Component, ElementRef, HostListener, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { faBan, faCheck, faClipboardList, faPencilAlt, faCopy, faRedo, faUserCog } from '@fortawesome/free-solid-svg-icons';
import { TranslateService } from '@ngx-translate/core';
import { forkJoin, Observable, of, Subject } from 'rxjs';
import { GAEventCategory, MovementTypes } from 'src/app/core/models/constants/app-constants';
import { ModeOfTransportType, POFulfillmentStageType, POFulfillmentStatus, Roles, EquipmentType, OrganizationNameRole, POFulfillmentLoadStatus, DialogActionType, ViewSettingModuleIdType, FormModeType } from 'src/app/core/models/enums/enums';
import { DateHelper, FormComponent, StringHelper } from 'src/app/core';
import { IntegrationData } from 'src/app/core/models/forms/integration-data';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { BulkFulfillmentCargoDetailComponent } from '../bulk-fulfillment-cargo-detail/bulk-fulfillment-cargo-detail.component';
import { BulkFulfillmentContactComponent } from '../bulk-fulfillment-contact/bulk-fulfillment-contact.component';
import { BulkFulfillmentTabModel } from '../models/bulk-fulfillment-tab.model';
import { BulkFulfillmentLoadModel, BulkFulfillmentModel } from '../models/bulk-fulfillment.model';
import { BulkFulfillmentFormService } from './bulk-fulfillment-form.service';
import { DatePipe } from '@angular/common';
import { BulkFulfillmentGeneralComponent } from '../bulk-fulfillment-general/bulk-fulfillment-general.component';
import { BulkFulfillmentAttachmentComponent } from '../bulk-fulfillment-attachment/bulk-fulfillment-attachment.component';
import { first, map, take, tap } from 'rxjs/operators';
import { BulkFulfillmentNoteModel } from '../models/bulk-fulfillment-note.model';
import { UserOrganizationProfileModel } from 'src/app/core/models/organization.model';
import { BulkFulfillmentLoadDetailComponent } from '../bulk-fulfillment-load-detail/bulk-fulfillment-load-detail.component';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { OrganizationFormService } from '../../organization/organization-form/organization-form.service';
import { ViewSettingModel } from 'src/app/core/models/viewSetting.model';
import { FormHelper } from 'src/app/core/helpers/form.helper';

@Component({
    selector: 'app-bulk-fulfillment-form',
    templateUrl: './bulk-fulfillment-form.component.html',
    styleUrls: ['./bulk-fulfillment-form.component.scss']
})

export class BulkFulfillmentFormComponent extends FormComponent {
    modelName = "bulkFulfillments";
    viewSettingModuleIdType = ViewSettingModuleIdType;
    formHelper = FormHelper;
    isOpenDuplicatedCompanyDialog: boolean;
    integration$: Subject<IntegrationData> = new Subject();
    allLocationOptions = [];
    POFulfillmentStatus = POFulfillmentStatus;
    EquipmentType = EquipmentType;
    MovementTypes = MovementTypes;
    ModeOfTransportType = ModeOfTransportType;
    tabs: Array<BulkFulfillmentTabModel> = this.service.createNavigation(true);

    private isManualScroll: boolean = true;

    model: BulkFulfillmentModel = new BulkFulfillmentModel();
    copyingBooking: BulkFulfillmentModel;
    /**Owner's Organization info of the booking */
    createdByOrganization: UserOrganizationProfileModel;

    // Data for Dialog Tab
    noteList: BulkFulfillmentNoteModel[];

    faPencilAlt = faPencilAlt;
    faBan = faBan;
    faCheck = faCheck;
    faRedo = faRedo;
    faUserCog = faUserCog;
    faClipboardList = faClipboardList;
    faCopy = faCopy;
    stringHelper = StringHelper;

    saveAsDraft: boolean = true;
    /**Re-load means plan to ship again. */
    isReloadMode: boolean = false;

    isCancelling: boolean = false;
    inBookingProgress: boolean = false;
    ineSIProgress: boolean = false;
    isSubmitting: boolean = false;
    isAmending: boolean = false;
    isContinueBookingSaveProcess: boolean;
    copyingBookingId: number;

    cancelBulkFulfillmentDialog: boolean = false;
    cancelReason = "";

    // Control saving message popup
    saveBookingFailed = false;
    saveBookingErrors: Array<string> = [];
    duplicatedCompanies: any[] = [];
    consigneeContact: any;

    @ViewChild('headerBar', { static: false }) headerBarElement: ElementRef;
    @ViewChild('stickyBar', { static: false }) stickyBarElement: ElementRef;
    @ViewChild('sectionContainer', { static: false }) sectionContainerElement: ElementRef;
    @ViewChild('general', { static: false }) generalElement: ElementRef;
    @ViewChild('plannedSchedule', { static: false }) plannedScheduleElement: ElementRef;
    @ViewChild('contact', { static: false }) contactElement: ElementRef;
    @ViewChild('cargoDetails', { static: false }) cargoDetailsElement: ElementRef;
    @ViewChild('loadDetails', { static: false }) loadDetailsElement: ElementRef;
    @ViewChild('activity', { static: false }) activityElement: ElementRef;
    @ViewChild('attachment', { static: false }) attachmentElement: ElementRef;
    @ViewChild('dialog', { static: false }) dialogElement: ElementRef;

    @ViewChild(BulkFulfillmentContactComponent, { static: false }) bulkFulfillmentContactComponent: BulkFulfillmentContactComponent;
    @ViewChild(BulkFulfillmentCargoDetailComponent, { static: false }) bulkFulfillmentCargoDetailComponent: BulkFulfillmentCargoDetailComponent;
    @ViewChild(BulkFulfillmentGeneralComponent, { static: false }) bulkFulfillmentGeneralComponent: BulkFulfillmentGeneralComponent;
    @ViewChild(BulkFulfillmentAttachmentComponent, { static: false }) bulkFulfillmentAttachmentComponent: BulkFulfillmentAttachmentComponent;
    @ViewChild(BulkFulfillmentLoadDetailComponent, { static: false }) bulkFulfillmentLoadDetailComponent: BulkFulfillmentLoadDetailComponent;

    constructor(
        protected route: ActivatedRoute,
        public service: BulkFulfillmentFormService,
        public notification: NotificationPopup,
        public translateService: TranslateService,
        private orgService: OrganizationFormService,
        public router: Router,
        private _gaService: GoogleAnalyticsService) {
        super(route, service, notification, translateService, router);
        this.getAllLocationOptions().subscribe();
        const currentUser = service.currentUser;
        this.currentUser = currentUser;
        if (currentUser) {
            this.model.owner = currentUser.companyName;
        }

    }

    // Please use this method to make sure data is already initialized
    getAllLocationOptions(): Observable<any> {
        // not initialized data
        if (this.allLocationOptions.length === 0) {
            return this.service.getAllLocations().map(locations => {
                this.allLocationOptions = locations;
                return this.allLocationOptions;
            });
        } else {
            // after data initialized
            return of(this.allLocationOptions);
        }
    }

    get isGeneralTabEditable() {
        if (this.model.stage > POFulfillmentStageType.Draft) {
            return false;
        }

        if (this.isViewMode) {
            return false;
        }

        return true;
    }

    get isLoadTabEditable() {
        if (this.model.stage > POFulfillmentStageType.Draft) {
            return false;
        }

        if (this.isViewMode) {
            return false;
        }

        return true;
    }

    get isLoadDetailTabEditable() {
        if (this.model.stage < POFulfillmentStageType.ForwarderBookingConfirmed) {
            return false;
        }
        // Load Details only displays on Sticky menu if Mode of Transport is Sea and Movement Type is CY
        if (!(this.model.modeOfTransport === ModeOfTransportType.Sea && this.model.movementType === MovementTypes.CYUnderscoreCY)) {
            return false;
        }

        return true;
    }

    onInitDataLoaded() {
        if (!this.isAddMode) {
            this.model = this.convertToModel(this.model);
        }
        else {
            this.model = new BulkFulfillmentModel();
            this.route.queryParams
                .pipe(
                    take(1),
                    tap(params => {
                        this.copyingBookingId = params['copyfrom'];
                        if (params['formType'] === FormModeType.Copy) {
                            this._fetchCopyingBookingSource();
                        }
                    })
                ).subscribe();
        }

        // init default value
        this.formErrors = {};
        // this.isReloadMode = false;
        this.saveAsDraft = true;

        this.tabs = this.service.createNavigation(false, this.isAddMode, this.isEditMode, this.model.stage, this.model.modeOfTransport, this.model.movementType);
        setTimeout(() => {
            this.initTabLink();
        }, 500); // make sure UI has been rendered

        this.bindingNoteTab();
        this.service.getOwnerOrgInfo(this.model.createdBy).subscribe(
            response => this.createdByOrganization = response
        )
    }

    private convertToModel({ ...data }): BulkFulfillmentModel {
        let model = new BulkFulfillmentModel(data); // convert to data model
        // map data after convert
        model.createdBy = data.createdBy;
        model.createdDate = data.createdDate;
        return model;
    }

    private _fetchCopyingBookingSource() {
        if (!this.copyingBookingId) {
            return;
        }

        this.service.getPOFulfillment(this.copyingBookingId)
            .pipe(
                tap((x: BulkFulfillmentModel) => this.copyingBooking = x)
            )
            .pipe(
                tap(() => this.copyBooking())
            )
            .subscribe();
    }

    private copyBooking() {
        let copyModel = new BulkFulfillmentModel(this.copyingBooking);

        if (copyModel.status === POFulfillmentStatus.Inactive) {
            return;
        }
        // to keep some fields.
        copyModel.stage = this.model.stage;
        copyModel.createdBy = this.model.createdBy;
        copyModel.createdDate = this.model.createdDate;
        copyModel.updatedBy = this.model.updatedBy;
        copyModel.updatedDate = this.model.updatedDate;
        copyModel.id = this.model.id;
        copyModel.number = this.model.number;
        copyModel.owner = this.model.owner;
        copyModel.status = this.model.status;

        // do not copy
        copyModel['attachments'] = [];
        delete copyModel['shipments'];
        delete copyModel['itineraries'];
        let destinationAgentIndex = copyModel.contacts?.findIndex(c => StringHelper.caseIgnoredCompare(c.organizationRole, OrganizationNameRole.DestinationAgent));
        if (destinationAgentIndex && destinationAgentIndex !== -1) {
            copyModel.contacts.splice(destinationAgentIndex, 1);
        }

        // reset to default value
        copyModel.orders?.forEach(el => {
            el.id = 0;
            el.status = 1;
        })
        copyModel.loads?.map(x => x.id = 0);
        copyModel.contacts?.map(x => x.id = 0);

        // copy starting...
        this.model = copyModel;
    }

    /**Link UI element to tabs object
    Must make sure that it is correct order */
    private initTabLink(): void {
        this.tabs.map(tab => {
            switch (tab.sectionId) {
                case 'general':
                    tab.sectionElementRef = this.generalElement;
                    break;
                case 'plannedSchedule':
                    tab.sectionElementRef = this.plannedScheduleElement;
                    break;
                case 'contact':
                    tab.sectionElementRef = this.contactElement;
                    break;
                case 'cargoDetails':
                    tab.sectionElementRef = this.cargoDetailsElement;
                    break;
                case 'loadDetails':
                    tab.sectionElementRef = this.loadDetailsElement;
                    break;
                case 'attachment':
                    tab.sectionElementRef = this.attachmentElement;
                    break;
                case 'activity':
                    tab.sectionElementRef = this.activityElement;
                    break;
                case 'dialog':
                    tab.sectionElementRef = this.dialogElement;
                    break;
            }
        });
    }

    onClickStickyBar(event, tab: BulkFulfillmentTabModel) {
        this.isManualScroll = false;
        for (let i = 0; i < this.tabs.length; i++) {
            const element = this.tabs[i];
            if (element.sectionId === tab.sectionId) {
                element.selected = true;
            } else {
                element.selected = false;
            }
        }
        // If the first section, move to the top
        if (tab.text === 'label.general') {
            window.scrollTo({ behavior: 'smooth', top: 0 });
        } else {
            const headerHeight = this.headerBarElement?.nativeElement?.clientHeight;
            window.scrollTo({ behavior: 'smooth', top: tab.sectionElementRef?.nativeElement?.offsetTop - headerHeight - 36 });
        }

        // After 1s, reset isManualScroll = true -> it scrolls to target position
        setTimeout(() => {
            this.isManualScroll = true;
        }, 1000);
    }

    @HostListener('window:scroll', ['$event'])
    onScroll(event) {

        const currentYPosition = window.scrollY;
        const headerHeight = this.headerBarElement?.nativeElement?.clientHeight;

        // Make header sticky
        if (currentYPosition >= headerHeight - 30) {
            this.headerBarElement?.nativeElement?.style.setProperty('position', 'sticky');
            this.headerBarElement?.nativeElement?.style.setProperty('top', '60px');
        } else {
            this.headerBarElement?.nativeElement?.style.setProperty('position', 'relative');
            this.headerBarElement?.nativeElement?.style.removeProperty('top');
        }

        // Make sticky bar

        if (currentYPosition >= headerHeight + 30) {
            this.stickyBarElement?.nativeElement?.style.setProperty('position', 'sticky');
            this.stickyBarElement?.nativeElement?.style.setProperty('top', headerHeight + 60 + 'px');
            this.stickyBarElement?.nativeElement?.style.removeProperty('display');
        } else {
            this.stickyBarElement?.nativeElement?.style.setProperty('display', 'none');
        }

        //#region Auto update sticky bar status

        // If user clicks on sticky menu, do not update status
        if (!this.isManualScroll) {
            return;
        }

        this.tabs.forEach(c => {
            c.selected = false;
        });

        for (let i = 0; i < this.tabs.length; i++) {
            const element = this.tabs[i];
            // adding 240px to make update sticky bar earlier
            if (currentYPosition + headerHeight + 40 <= element.sectionElementRef?.nativeElement?.offsetTop + element.sectionElementRef?.nativeElement?.clientHeight) {
                element.selected = true;
                break;
            }
        }
        //#endregion
    }

    onEditButtonClick(): void {
        this.router.navigate([
            `/bulk-fulfillments/edit/${this.model.id}`
        ], { queryParams: { formType: FormModeType.Edit } });
    }

    onCopyButtonClick(): void {
        this.router.navigate(['/bulk-fulfillments/add/0'], { queryParams: { formType: FormModeType.Copy, isAddNew: true, copyfrom: this.model.id } });
    }

    onCancel(): void {
        const confirmDlg = this.notification.showConfirmationDialog(
            "edit.cancelConfirmation",
            "label.poFulfillment"
        );

        confirmDlg.result.subscribe((result: any) => {
            if (result.value) {
                if (this.isAddMode) {
                    this.backToList();
                } else {
                    if (this.isEditMode) {
                        this.router.navigate([
                            `/bulk-fulfillments/view/${this.model.id}`
                        ], { queryParams: { formType: FormModeType.View } });
                        this.isReloadMode = false;
                    }
                }
            }
        });
    }

    get isContactTabEditable() {
        if (this.model.stage > POFulfillmentStageType.Draft) {
            return false;
        }

        if (this.isViewMode) {
            return false;
        }

        return true;
    }

    get isHiddenLoads() {
        if (this.model.modeOfTransport !== ModeOfTransportType.Sea) {
            return true;
        }

        if (this.model.movementType !== MovementTypes.CYUnderscoreCY) {
            return true;
        }

        return false;
    }

    get hiddenBtnEdit() {
        const currentUser = this.service.currentUser;
        if (
            !currentUser.isInternal &&
            this.model.stage === POFulfillmentStageType.ForwarderBookingRequest
        ) {
            return true;
        }

        if (this.model.stage === POFulfillmentStageType.Closed) {
            return true;
        }

        if (this.model.status === POFulfillmentStatus.Inactive) {
            return true;
        }

        if (!currentUser.isInternal) {
            if (currentUser.role.id !== Roles.Shipper && currentUser.role.id !== Roles.Factory) {
                return true;
            }
            if (currentUser.organizationId !== this.createdByOrganization?.organizationId ?? 0) {
                return true;
            }
        }
        return false;
    }

    get hiddenBtnCopy() {
        if (this.model.status === POFulfillmentStatus.Inactive) {
            return true;
        }
        return false;
    }

    get hiddenBtnCancel() {
        const currentUser = this.service.currentUser;
        if (this.model.stage > POFulfillmentStageType.ForwarderBookingRequest) {
            return true;
        }

        if (this.model.status === POFulfillmentStatus.Inactive) {
            return true;
        }

        if (!currentUser.isInternal) {
            if (currentUser.role.id !== Roles.Shipper && currentUser.role.id !== Roles.Factory) {
                return true;
            }
            if (currentUser.organizationId !== this.createdByOrganization?.organizationId ?? 0) {
                return true;
            }
        }
        return false;
    }

    get hiddenBtnBook() {
        const currentUser = this.service.currentUser;
        if (this.model.stage !== POFulfillmentStageType.Draft) {
            return true;
        }

        if (this.model.status === POFulfillmentStatus.Inactive) {
            return true;
        }

        if (!currentUser.isInternal) {
            if (currentUser.role.id !== Roles.Shipper && currentUser.role.id !== Roles.Factory) {
                return true;
            }
            if (currentUser.organizationId !== this.createdByOrganization?.organizationId ?? 0) {
                return true;
            }
        }
        return false;
    }

    get hiddenBtnAmend() {
        const currentUser = this.service.currentUser;
        if (this.model.stage !== POFulfillmentStageType.ForwarderBookingRequest) {
            return true;
        }

        if (this.model.status === POFulfillmentStatus.Inactive) {
            return true;
        }

        if (!currentUser.isInternal) {
            if (currentUser.role.id !== Roles.Shipper && currentUser.role.id !== Roles.Factory) {
                return true;
            }
            if (currentUser.organizationId !== this.createdByOrganization?.organizationId ?? 0) {
                return true;
            }
        }
        return false;
    }

    get hiddenBtnPlanToShip() {
        const currentUser = this.service.currentUser;
        if (this.model.status === POFulfillmentStatus.Inactive) {
            return true;
        }

        if (
            this.model.stage !==
            POFulfillmentStageType.ForwarderBookingConfirmed
        ) {
            return true;
        }

        if (this.model.isGeneratePlanToShip) {
            return true;
        }

        if (!currentUser.isInternal) {
            if (currentUser.role.id !== Roles.Shipper && currentUser.role.id !== Roles.Factory) {
                return true;
            }
            if (currentUser.organizationId !== this.createdByOrganization?.organizationId ?? 0) {
                return true;
            }
        }

        if (this.model.movementType === MovementTypes.CFSUnderscoreCY) {
            return true;
        }

        return false;
    }

    get hiddenBtnReload() {
        const currentUser = this.service.currentUser;
        if (![POFulfillmentStageType.ForwarderBookingConfirmed, POFulfillmentStageType.ShipmentDispatch].includes(this.model.stage)) {
            return true;
        }

        if (!this.model.isGeneratePlanToShip) {
            return true;
        }

        if (this.model.status === POFulfillmentStatus.Inactive) {
            return true;
        }

        if (!currentUser.isInternal) {
            if (currentUser.role.id !== Roles.Shipper && currentUser.role.id !== Roles.Factory) {
                return true;
            }
            if (currentUser.organizationId !== this.createdByOrganization?.organizationId ?? 0) {
                return true;
            }
        }

        return false;
    }

    /**
     * Get tab details/ settings
     * @param sectionId Id of section
     * @returns
     */
    getTabDetails(sectionId: string): BulkFulfillmentTabModel {
        const result = this.tabs.find(x => x.sectionId === sectionId);
        return result;
    }

    onCancelBulkFulfillmentClick() {
        this.cancelBulkFulfillmentDialog = true;
    }

    onYesOfCancelDialogClick() {
        this.isCancelling = true;
        this.cancelBulkFulfillmentDialog = false;
        if (
            this.model.stage < POFulfillmentStageType.ForwarderBookingConfirmed
        ) {
            this.service
                .cancelBulkFulfillment(this.model.id, this.cancelReason)
                .subscribe(
                    () => {
                        this.cancelReason = "";
                        this._gaService.emitAction('Cancel', GAEventCategory.BulkBooking);
                        this.notification.showSuccessPopup(
                            "confirmation.cancelSuccessfully",
                            "label.poFulfillment"
                        );
                        this.router.navigate([
                            `/bulk-fulfillments/view/${this.model.id}`
                        ], { queryParams: { formType: FormModeType.View } });
                        this.isCancelling = false;
                        this.reloadData();
                    },
                    (err) => {
                        this.cancelReason = '';
                        this.isCancelling = false;

                        if (err.error) {
                            const errorsDetails = err.error.errors;
                            const errors = errorsDetails.split(/(?=,\d+:\/)/);
                            this.saveBookingErrors = [];
                            if (errors) {
                                this.saveBookingErrors = errors.map(x => x.replace(',', '', 1));
                            }
                            this.saveBookingFailed = true;
                            this.saveBookingErrors.unshift(this.translateService.instant('msg.cancelledBookingFailedDetails'));
                        }
                    });
        }
    }

    onNoOfCancelDialogClick() {
        this.cancelBulkFulfillmentDialog = false;
        this.cancelReason = "";
    }

    onBookButtonClick() {
        let isValid: boolean = true;
        let firstErrorSectionId = '';

        /*
        Validate general section
        */
        let validationResult = this.bulkFulfillmentGeneralComponent.validateBeforeSubmitting();
        if (validationResult.length > 0) {
            isValid = false;
            firstErrorSectionId = 'general'
        }

        /*
        Validate contact section
        */
        validationResult = this.bulkFulfillmentContactComponent.validateBeforeSubmitting();
        if (validationResult.length > 0) {
            isValid = false;
            if (StringHelper.isNullOrEmpty(firstErrorSectionId)) {
                firstErrorSectionId = 'contact'
            }
        }

        /*
        Validate cargo detail section
        */
        validationResult = this.bulkFulfillmentCargoDetailComponent.validateBeforeSubmitting();
        if (validationResult.length > 0) {
            isValid = false;
            if (StringHelper.isNullOrEmpty(firstErrorSectionId)) {
                firstErrorSectionId = 'cargoDetails'
            }
        }

        /*
        Validate attachment section
        */
        validationResult = this.bulkFulfillmentAttachmentComponent.validateBeforeSubmitting();
        if (validationResult.length > 0) {
            isValid = false;
            if (StringHelper.isNullOrEmpty(firstErrorSectionId)) {
                firstErrorSectionId = 'attachment'
            }
        }

        if (!isValid) {
            this.notification.showErrorPopup(
                "msg.validateCreateBulkBookingRequestFailed",
                "label.poFulfillment"
            );
            this.tabs = this.service.createNavigation(false, false, true, this.model.stage, this.model.modeOfTransport, this.model.movementType);
            this.initTabLink();
            this.router.navigate([
                `/bulk-fulfillments/edit/${this.modelId}`
            ], { queryParams: { formType: FormModeType.Edit } });
            this.saveAsDraft = false;

            setTimeout(() => {
                this.validateAllFields(false);
                const firstTabIndex = this.tabs.findIndex(x => x.sectionId === firstErrorSectionId);
                this.onClickStickyBar(null, this.tabs[firstTabIndex]);
                return;
            }, 1);
        } else {
            this.inBookingProgress = true;
            this.service.submitBooking(this.model.id).subscribe(
                success => {
                    this._gaService.emitAction('Submit', GAEventCategory.BulkBooking);
                    this.notification.showSuccessPopup(
                        "msg.bookingAcceptedResult",
                        "label.poFulfillment"
                    );
                    this.inBookingProgress = false;
                    this.reloadData();
                },
                err => {
                    this.inBookingProgress = false;
                    if (err.error) {
                        const errorsDetails = err.error.errors;
                        const errors = errorsDetails.split(/(?=,\d+:\/)/);
                        this.saveBookingErrors = [];
                        if (errors) {
                            this.saveBookingErrors = errors.map(x => x.replace(',', '', 1));
                        }
                        this.saveBookingFailed = true;
                        this.saveBookingErrors.unshift(this.translateService.instant('msg.submittedBookingFailedDetails'));

                    }
                }
            );
        }
    }

    onAmendPOFulfillmentClick() {
        const amendMessage = "confirmation.amendPOFulfillment";
        const confirmDlg = this.notification.showConfirmationDialog(
            amendMessage,
            "label.poFulfillment",
            false,
            true,
            475
        );

        confirmDlg.result.subscribe((result: any) => {
            if (result.value) {
                this.isAmending = true;
                this.service.amendBooking(this.model.id).subscribe(
                    () => {
                        this._gaService.emitAction('Amend', GAEventCategory.BulkBooking);
                        this.notification.showSuccessPopup(
                            "msg.amendPOFulfillmentSuccessfully",
                            "label.poFulfillment"
                        );
                        this.router.navigate([
                            `/bulk-fulfillments/view/${this.model.id}`
                        ], { queryParams: { formType: FormModeType.View } });
                        this.isAmending = false;
                        this.reloadData();
                    },
                    err => {
                        this.isAmending = false;
                        if (err.error) {
                            const errorsDetails = err.error.errors;
                            const errors = errorsDetails.split(/(?=,\d+:\/)/);
                            this.saveBookingErrors = [];
                            if (errors) {
                                this.saveBookingErrors = errors.map(x => x.replace(',', '', 1));
                            }
                            this.saveBookingFailed = true;
                            this.saveBookingErrors.unshift(this.translateService.instant('msg.amendedBookingFailedDetails'));

                        }
                    }
                );
            }
        });
    }

    /**Note: Open in edit mode to re-load cargo items and plan to ship again. */
    onReloadButtonClick() {
        this.router.navigate([`/bulk-fulfillments/edit/${this.model.id}`], { queryParams: { formType: FormModeType.Edit } });
        this.isReloadMode = true;
    }

    onPlanToShipButtonClick() {
        let validationResult = this.bulkFulfillmentLoadDetailComponent.validateBeforeSaving();

        let isValid = validationResult.length <= 0;
        if (!isValid) {
            return;
        }

        isValid = this.validateBeforePlanToShip();
        if (!isValid) {
            return;
        }
        this.ineSIProgress = true;
        this.service.planToShip(this.model.id).subscribe(
            () => {
                this._gaService.emitAction('Plan to ship', GAEventCategory.BulkBooking);
                this.notification.showSuccessPopup(
                    "msg.planToShipSuccessfully",
                    "label.poFulfillment"
                );
                this.ineSIProgress = false;
                this.reloadData();
            },
            err => {
                switch (err.status) {
                    case 400:
                        this.notification.showErrorPopup(
                            "validation.planToShip",
                            "label.poFulfillment"
                        );
                        break;
                    default:
                        this.notification.showErrorPopup(
                            "save.failureNotification",
                            "label.poFulfillment"
                        );
                        break;
                }
                this.ineSIProgress = false;
            }
        );
    }

    async onSubmit() {
        if (this.isReloadMode) {

            let validationResult = this.bulkFulfillmentLoadDetailComponent.validateBeforeSaving();
            let isValid = validationResult.length <= 0;
            if (!isValid) {
                return;
            }

            isValid = this.validateBeforePlanToShip();
            if (!isValid) {
                return;
            }

            let tempModel: BulkFulfillmentModel = new BulkFulfillmentModel({ ...this.model });

            tempModel.loads = tempModel.loads?.map(
                load => DateHelper.formatDate(load)) || [];

            const confirmDlg = this.notification.showConfirmationDialog(
                "confirmation.resubmittingTheLoadPlan",
                "label.poFulfillment"
            );

            confirmDlg.result.subscribe((result: any) => {
                if (result.value) {
                    this.isSubmitting = true;
                    this.service.reload(this.modelId, tempModel).subscribe(
                        data => {
                            this._gaService.emitAction('Re-load', GAEventCategory.BulkBooking);
                            this.notification.showSuccessPopup(
                                "save.sucessNotification",
                                "label.poFulfillment"
                            );
                            this.router.navigate([
                                `/bulk-fulfillments/view/${this.model.id}`
                            ], { queryParams: { formType: FormModeType.View } });
                            this.isReloadMode = false;
                            this.isSubmitting = false;
                        },
                        err => {
                            this.isSubmitting = false;
                            this.notification.showErrorPopup(
                                "save.failureNotification",
                                "label.poFulfillment"
                            );
                        }
                    )
                }
            });
        }
        else if (this.isAddMode) {

            let isValid: boolean = true;
            if (!this.mainForm.valid) {
                isValid = false;
                this.validateAllFields(false);
            }

            if (!this.validateBookingBeforeSaving()) {
                isValid = false;
            }

            if (!isValid) {
                return;
            }

            // In case there is any error but not belonging to any tab
            const errors = Object.keys(this.formErrors);
            errors.map((key) => {
                const err = Reflect.get(this.formErrors, key);
                if (err && !StringHelper.isNullOrEmpty(err)) {
                    return;
                }
            });

            let tempModel: BulkFulfillmentModel = new BulkFulfillmentModel({ ...this.model });

            this.mappingContacts(tempModel);
            this.mappingLoads(tempModel);

            this.isSubmitting = true;

            this.consigneeContact = tempModel.contacts.find(c => c.organizationRole === OrganizationNameRole.Consignee);
            if (!StringHelper.isNullOrWhiteSpace(this.consigneeContact?.companyName) && !this.isContinueBookingSaveProcess) {
                const equalSearchOrg = await this.orgService.getOrgByName(this.consigneeContact.companyName).toPromise()
                    .catch(
                        error => {
                            this.notification.showErrorPopup(
                                "save.failureNotification",
                                "label.poFulfillment"
                            );
                        }
                    );
                if (!equalSearchOrg) {
                    const fulltextSearchOrgs: any = await this.orgService.getOrgsWithFulltextSearchByName(this.consigneeContact.companyName).toPromise()
                        .catch(
                            error => {
                                this.notification.showErrorPopup(
                                    "save.failureNotification",
                                    "label.poFulfillment"
                                );
                            }
                        );
                    if (fulltextSearchOrgs.length > 0) {
                        this.isOpenDuplicatedCompanyDialog = true;
                        this.duplicatedCompanies = fulltextSearchOrgs;
                        return;
                    }
                }

                this.createBooking(tempModel);
                return;
            }
            this.createBooking(tempModel);

        } else if (this.isEditMode) {

            let isValid: boolean = true;
            if (!this.mainForm.valid) {
                isValid = false;
                this.validateAllFields(false);
            }

            if (!this.validateBookingBeforeSaving()) {
                isValid = false;
            }

            if (!isValid) {
                return;
            }

            // In case there is any error but not belonging to any tab
            const errors = Object.keys(this.formErrors);
            errors.map((key) => {
                const err = Reflect.get(this.formErrors, key);
                if (err && !StringHelper.isNullOrEmpty(err)) {
                    return;
                }
            });

            let tempModel: any = new BulkFulfillmentModel({ ...this.model });

            this.mappingContacts(tempModel);
            this.mappingLoads(tempModel);

            tempModel.loads = tempModel.loads?.map(
                load => DateHelper.formatDate(load)) || [];

            this.isSubmitting = true;

            this.consigneeContact = tempModel.contacts.find(c => c.organizationRole === OrganizationNameRole.Consignee);
            if (!StringHelper.isNullOrWhiteSpace(this.consigneeContact?.companyName) && !this.isContinueBookingSaveProcess) {
                const equalSearchOrg = await this.orgService.getOrgByName(this.consigneeContact.companyName).toPromise()
                    .catch(
                        error => {
                            this.notification.showErrorPopup(
                                "save.failureNotification",
                                "label.poFulfillment"
                            );
                        }
                    );
                if (!equalSearchOrg) {
                    const fulltextSearchOrgs: any = await this.orgService.getOrgsWithFulltextSearchByName(this.consigneeContact.companyName).toPromise()
                        .catch(
                            error => {
                                this.notification.showErrorPopup(
                                    "save.failureNotification",
                                    "label.poFulfillment"
                                );
                            }
                        );
                    if (fulltextSearchOrgs.length > 0) {
                        this.isOpenDuplicatedCompanyDialog = true;
                        this.duplicatedCompanies = fulltextSearchOrgs;
                        return;
                    }
                }

                this.updateBooking(tempModel);
                return;
            }
            this.updateBooking(tempModel);
        }
    }

    updateBooking(tempModel: any) {
        this.service.update(this.modelId, tempModel).subscribe(
            res => {
                this.isSubmitting = false;
                this.isContinueBookingSaveProcess = false;
                this.notification.showSuccessPopup(
                    "save.sucessNotification",
                    "label.poFulfillment"
                );
                this.router.navigate([
                    `/bulk-fulfillments/view/${this.modelId}`
                ], { queryParams: { formType: FormModeType.View } });
            },
            err => {
                this.isSubmitting = false;
                this.isContinueBookingSaveProcess = false;
                this.notification.showErrorPopup(
                    "save.failureNotification",
                    "label.poFulfillment"
                );
            }
        )
    }

    createBooking(tempModel: any) {
        this.service.createNew(tempModel).subscribe(
            data => {
                this.isSubmitting = false;
                this.isContinueBookingSaveProcess = false;
                this.notification.showSuccessPopup(
                    "save.sucessNotification",
                    "label.poFulfillment"
                );
                if (this.copyingBookingId > 0) {
                    this._gaService.emitAction('Copy', GAEventCategory.BulkBooking);
                } else {
                    this._gaService.emitAction('Add', GAEventCategory.BulkBooking);

                }
                this.router.navigate([
                    `/bulk-fulfillments/view/${data.id}`
                ], { queryParams: { formType: FormModeType.View } });
            },
            err => {
                this.isSubmitting = false;
                this.isContinueBookingSaveProcess = false;
                this.notification.showErrorPopup(
                    "save.failureNotification",
                    "label.poFulfillment"
                );
            }
        )
    }

    duplicatedCompanyDialogChanged(event: { data: any, dialogActionType: DialogActionType }) {
        this.isOpenDuplicatedCompanyDialog = false;
        switch (event.dialogActionType) {
            case DialogActionType.Submit:
                this.isSubmitting = false;
                this.consigneeContact.companyName = event.data.name;
                this.consigneeContact.address = this.concatenateAddressLines(event.data.address, event.data.addressLine2, event.data.addressLine3, event.data.addressLine4);
                this.consigneeContact.contactName = event.data.contactName;
                this.consigneeContact.contactEmail = event.data.contactEmail;
                this.consigneeContact.contactNumber = event.data.contactNumber;
                this.consigneeContact.weChatOrWhatsApp = event.data.weChatOrWhatsApp;
                break;

            case DialogActionType.Cancel:
                this.isContinueBookingSaveProcess = true;
                this.onSubmit();
                break;

            case DialogActionType.Close:
                this.isSubmitting = false;
                break;
            default:
                break;
        }
    }

    private validateBookingBeforeSaving(): boolean {
        let isValid: boolean = true;
        let firstErrorSectionId = '';

        // Validate general
        let validationResult = this.bulkFulfillmentGeneralComponent.validateBeforeSaving();
        if (validationResult.length > 0) {
            firstErrorSectionId = 'general';
            isValid = false;
        }
        // Validate contact
        validationResult = this.bulkFulfillmentContactComponent.validateBeforeSaving();
        if (validationResult.length > 0) {
            if (StringHelper.isNullOrEmpty(firstErrorSectionId)) {
                firstErrorSectionId = 'contact'
            }
            isValid = false;
        }
        // Validate cargo detail
        validationResult = this.bulkFulfillmentCargoDetailComponent.validateBeforeSaving();
        if (validationResult.length > 0) {
            if (StringHelper.isNullOrEmpty(firstErrorSectionId)) {
                firstErrorSectionId = 'cargoDetails'
            }
            isValid = false;
        }
        // Validate load detail
        if (this.model.stage >= POFulfillmentStageType.ForwarderBookingConfirmed) {
            validationResult = this.bulkFulfillmentLoadDetailComponent?.validateBeforeSaving();
            if (validationResult?.length > 0) {
                if (StringHelper.isNullOrEmpty(firstErrorSectionId)) {
                    firstErrorSectionId = 'loadDetails'
                }
                isValid = false;
            }
        }

        // Validate attachment
        validationResult = this.bulkFulfillmentAttachmentComponent?.validateBeforeSaving();
        if (validationResult.length > 0) {
            if (StringHelper.isNullOrEmpty(firstErrorSectionId)) {
                firstErrorSectionId = 'attachment'
            }
            isValid = false;
        }

        if (!isValid) {
            const firstTabIndex = this.tabs.findIndex(x => x.sectionId === firstErrorSectionId);
            this.onClickStickyBar(null, this.tabs[firstTabIndex]);
        }
        return isValid;
    }

    private validateBeforePlanToShip(): boolean {
        const isLoadEmpty =
            this.model.loads &&
            this.model.loads.find(x => x.details && x.details.length === 0);
        if (isLoadEmpty) {
            this.notification.showErrorPopup(
                "validation.oneLoadDetail",
                "label.poFulfillment"
            );
            return false;
        }

        const isMissingContainerNo =
            this.model.loads?.some(x => StringHelper.isNullOrEmpty(x.containerNumber)) || false;
        if (isMissingContainerNo) {
            this.notification.showErrorPopup(
                "validation.provideContainerNoBeforeeSI",
                "label.poFulfillment"
            );
            return false;
        }
        return true;
    }

    /**
     * To update data of contact list
     * @param model
     */
    private mappingContacts(model: BulkFulfillmentModel): void {
        const { contactList, isShipperPickup, isNotifyPartyAsConsignee } = this.bulkFulfillmentContactComponent;
        // remove all record with removed marked
        model.contacts = contactList?.filter((value) => StringHelper.isNullOrEmpty(value.removed) || !value.removed);

        // If Notify party same as consignee = True, clone consignee to notify party.
        if (isNotifyPartyAsConsignee) {
            const consignee = model.contacts.find(x => x.organizationRole === OrganizationNameRole.Consignee);
            const notifyParty = model.contacts.find(x => x.organizationRole === OrganizationNameRole.NotifyParty);
            notifyParty.organizationId = consignee.organizationId;
            notifyParty.companyName = consignee.companyName;
            notifyParty.contactName = consignee.contactName;
            notifyParty.contactNumber = consignee.contactNumber;
            notifyParty.contactEmail = consignee.contactEmail;
            notifyParty.address = consignee.address;
        }
        model.isShipperPickup = isShipperPickup;
        model.isNotifyPartyAsConsignee = isNotifyPartyAsConsignee;
    }

    mappingLoads(model: BulkFulfillmentModel) {
        if (!this.isAddMode && model.stage !== POFulfillmentStageType.Draft) {
            return;
        }

        const groupedLoads = this.bulkFulfillmentCargoDetailComponent.groupedLoads?.filter(
            (value) => StringHelper.isNullOrEmpty(value.removed) || !value.removed) || [];

        if (this.model.modeOfTransport === ModeOfTransportType.Air) {
            model.loads = [];
            let newLoad = new BulkFulfillmentLoadModel();
            newLoad.equipmentType = EquipmentType.AirShipment;
            newLoad.status = POFulfillmentLoadStatus.Active;

            model.loads.push(newLoad);
            return;
        }

        if (this.model.modeOfTransport === ModeOfTransportType.Sea
            && this.model.movementType === MovementTypes.CFSUnderscoreCY) {

            model.loads = [];
            let newLoad = new BulkFulfillmentLoadModel();
            newLoad.equipmentType = EquipmentType.LCLShipment;
            newLoad.status = POFulfillmentLoadStatus.Active;

            model.loads.push(newLoad);
            return;
        }

        if (this.model.modeOfTransport === ModeOfTransportType.Sea
            && this.model.movementType === MovementTypes.CYUnderscoreCY) {

            model.loads = [];
            for (let item of groupedLoads) {
                for (let index = 0; index < item.quantity; index++) {
                    let newLoad = new BulkFulfillmentLoadModel();
                    newLoad.equipmentType = item.equipmentType;
                    newLoad.status = POFulfillmentLoadStatus.Active;
                    model.loads.push(newLoad);
                }
            }
            return;
        }
    }

    bindingNoteTab() {
        var noteObs$ = this.service.getNotes(this.model.id)
            .pipe(
                map((res: any) => {
                    return res.map(x => {
                        const bookingNoteModel = new BulkFulfillmentNoteModel();
                        bookingNoteModel.MapFrom(x);
                        return bookingNoteModel;
                    })
                })
            )

        var masterNote$ = this.service.getMasterNotes(this.model.id)
            .pipe(
                map((res: any) => {
                    return res.map(x => {
                        const newBulkFulfillmentNoteModel = new BulkFulfillmentNoteModel();
                        newBulkFulfillmentNoteModel.MapFromMasterNote(x);
                        return newBulkFulfillmentNoteModel;
                    })
                })
            )

        forkJoin([noteObs$, masterNote$]).subscribe(
            (note) => {
                this.noteList = note[0].concat(note[1]);
            });
    }

    onModeOfTransportChange(event): void {
        if (!this.isHiddenLoads
            && !this.model.loads?.some(l => l.equipmentType !== EquipmentType.LCLShipment && l.equipmentType !== EquipmentType.AirShipment)) {
            this.bulkFulfillmentCargoDetailComponent.deleteAllLoads();
            this.bulkFulfillmentCargoDetailComponent.addBlankLoad();
        }
    }

    onMovementChange(event): void {
        if (!this.isHiddenLoads
            && !this.model.loads?.some(l => l.equipmentType !== EquipmentType.LCLShipment && l.equipmentType !== EquipmentType.AirShipment)) {
            this.bulkFulfillmentCargoDetailComponent.deleteAllLoads();
            this.bulkFulfillmentCargoDetailComponent.addBlankLoad();
        }
    }

    onMixedCartonChange(value): void {
        this.model.isAllowMixedCarton = value;
    }

    backToList() {
        this.router.navigate(["/po-fulfillments"]);
    }

    get isCFSBooking() {
        return this.model.movementType === 'CFS_CY' || this.model.movementType === 'CFS_CFS';
    }

    concatenateAddressLines(address: string, addressLine2?: string, addressLine3?: string, addressLine4?: string) {
        let concatenatedAddress = address;
        if (!StringHelper.isNullOrEmpty(addressLine2)) {
            concatenatedAddress += '\n' + addressLine2;
        }
        if (!StringHelper.isNullOrEmpty(addressLine3)) {
            concatenatedAddress += '\n' + addressLine3;
        }
        if (!StringHelper.isNullOrEmpty(addressLine4)) {
            concatenatedAddress += '\n' + addressLine4;
        }
        return concatenatedAddress;
    }


}
