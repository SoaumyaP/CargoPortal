import { Component, OnDestroy, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { faShare, faCloudUploadAlt, faTrashAlt, faPencilAlt, faEllipsisV, faPlus, faCloudDownloadAlt } from '@fortawesome/free-solid-svg-icons';
import { FormComponent, UserContextService, MilestoneType, ActivityType as ActivityType, DropDowns, StringHelper, DateHelper, Roles, EntityType } from '../../../core';
import { ContainerFormService } from './container-form.service';
import { TranslateService } from '@ngx-translate/core';
import { RowArgs } from '@progress/kendo-angular-grid';
import { AttachmentUploadPopupService } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.service';
import { AttachmentUploadPopupComponent } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.component';
import { MilestoneComponent } from 'src/app/ui/milestone/milestone.component';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { Observable, Subscription } from 'rxjs';
import { ContainerHelper } from 'src/app/core/helpers/container-helper';
import * as cloneDeep from 'lodash/cloneDeep';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { AttachmentKeyPair, AttachmentModel } from 'src/app/core/models/attachment.model';
import { UserProfileModel } from 'src/app/core/models/user/user-profile.model';
import { DefaultValue2Hyphens, DocumentLevel, GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { of } from 'rxjs';
import { map } from 'rxjs/operators';
import { groupBy} from 'lodash';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';

@Component({
    selector: 'app-container-tracking',
    templateUrl: './container-tracking.component.html',
    styleUrls: ['./container-tracking.component.scss']
})
export class ContainerTrackingComponent extends FormComponent implements OnDestroy {
    milestoneType = MilestoneType;
    AppPermissions = AppPermissions;
    defaultValue = DefaultValue2Hyphens;
    documentLevel = DocumentLevel;
    activityFormMode = 'view';
    activityDetails: any = {};
    activityFormOpened: boolean;
    heightActivity = 530;
    allEventOptions: any;
    public cargoDetails = [];
    public activities = [];
    groupedActivityList = [];
    public itineraries = [];
    isShowNestedActivityGrid: boolean = false;
    /** available attachments on grid of container */
    public attachments: Array<AttachmentModel> = [];
    /** selected attachment by checkboxes */
    public selectedAttachments: Array<AttachmentModel> = [];
    public containerTypeOptions = DropDowns.ContainerType;
    modelName = 'container';
    @ViewChild('milestone', { static: false }) milestone: MilestoneComponent;

    faCloudUploadAlt = faCloudUploadAlt;
    faCloudDownloadAlt = faCloudDownloadAlt;
    faTrashAlt = faTrashAlt;
    faPencilAlt = faPencilAlt;
    faEllipsisV = faEllipsisV;
    faShare = faShare;
    faPlus = faPlus;

    importFormOpened = false;
    /**data for attachment upload popup */
    attachmentModel: AttachmentModel = null;
    attachmentFormMode = 0;
    currentUser: UserProfileModel;
    AttachmentFormModeType = {
        add: 0,
        edit: 1
    };

    openCargoDescriptionDetailPopup: boolean = false;
    cargoDescriptionDetail: string;

    private _subscriptions: Array<Subscription> = [];

    @ViewChild(AttachmentUploadPopupComponent, { static: false }) attachmentPopupComponent: AttachmentUploadPopupComponent;
    ImportStepState = {
        Selecting: 0,
        Selected: 1,
    };

    private editingContainerNumber: string;

    validationRules = {
        'containerNumber': {
            'required': 'label.containerNo',
            'duplicateContainer': 'validation.duplicateContainer',
            'containerNumberInvalid': 'validation.containerNumberInvalid'
        },
        'carrierSONumber': {
            'duplicatedWithContainerNumber': 'validation.duplicatedOnCarrierSONoAndContainerNo'
        }
    };

    constructor(protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public _containerService: ContainerFormService,
        public router: Router,
        public translateService: TranslateService,
        private _userContext: UserContextService,
        public attachmentService: AttachmentUploadPopupService,
        private _gaService: GoogleAnalyticsService) {
        super(route, _containerService, notification, translateService, router);
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
                if (!user.isInternal) {
                    this.service.affiliateCodes = user.affiliates;
                }
            }
        });

        this._containerService.getEvents().subscribe(data => {
            this.allEventOptions = data;
        });
    }

    checkInvalidContainerNumber() {
        if (StringHelper.isNullOrEmpty(this.model.containerNo)) {
            this.setInvalidControl('containerNumber', 'required');
            return;
        }
        if (this.model.containerNo.length !== 11) {
            this.setInvalidControl('containerNumber', 'containerNumberInvalid');
            return;
        }
        if (!ContainerHelper.checkDigitContainer(this.model.containerNo)) {
            this.setInvalidControl('containerNumber', 'containerNumberInvalid');
            return;
        }
        this.setValidControl('containerNumber');
    }

    onContainerNumberFocusout() {
        this.checkInvalidContainerNumber();
        this.checkDuplicateContainer().subscribe();
    }

    private checkDuplicateContainer(): Observable<boolean> {
        if (!StringHelper.isNullOrEmpty(this.model.carrierSONo) && !StringHelper.isNullOrEmpty(this.model.containerNo)) {
            return this._containerService.isDuplicatedContainer(this.model.id, this.model.containerNo, this.model.carrierSONo).pipe(
              map(x => {
                if (x) {
                  this.setInvalidControl('carrierSONumber', 'duplicatedWithContainerNumber');
                } else {
                  this.setValidControl('carrierSONumber');
                }
                return x;
              }));
          } else {
            return of(false);
          }
    }

    public selectAttachment(context: RowArgs): string {
        return context.dataItem;
    }

    onInitDataLoaded(data: void) {
        if (this.model !== null) {
            if (!this.model.isFCL && this.isEditMode) {
                this.router.navigate(['/error/404']);
                return;
            }
            this.getActivitiesByContainer(this.model.id);
            this.getItinerariesByContainer(this.model.id);
            this.getCargodetailsByContainer(this.model.id);
            this.getAttachmentTab();
            this.editingContainerNumber = this.model.containerNo;
            this.checkDuplicateContainer().subscribe();
        }
    }

    getActivitiesByContainer(id) {
        this._containerService.getActivitiesByContainer(id).subscribe(res => {
            if (res) {
                this.activities = res;
                this.groupActivity();
                if (this.milestone != null) {
                    this.milestone.data = res.filter(a => a.activityType === ActivityType.Container ||  a.activityType === ActivityType.VesselActivity);
                    this.milestone.reload();
                }
            }
        },
        err => {

        });
    }

    getItinerariesByContainer(id) {
        this._containerService.getItinerariesByContainer(id).subscribe(res => {
            if (res) {
                this.itineraries = res;
            }
        },
            err => {

            });
    }

    getCargodetailsByContainer(id) {
        this._containerService.getCargodetailsByContainer(id).subscribe(res => {
            if (res) {
                this.cargoDetails = res;
            }
        },
            err => {

            });
    }

    getAttachmentTab() {
        this._containerService.getAttachmentsByContainers(this.model.id).subscribe(res => {
            if (res) {
                this.attachments = res;
            }
        },
            err => {

            });
    }


    private getDocumentLevelText(documentLevel: string) : string {
        return this.attachmentService.translateDocumentLevel(documentLevel);
    }

    downloadFile(id, fileName) {
        this.attachmentService.downloadFile(id, fileName).subscribe();
    }

    backList() { }

    public async testReport(): Promise<void> {
        this._containerService.testReport('3').subscribe();
    }

    //#region attachment
    uploadAttachment() {
        this.attachmentFormMode = this.AttachmentFormModeType.add;
        this.attachmentPopupComponent.updateStyle(this.ImportStepState.Selecting);
        this.importFormOpened = true;
        this.attachmentModel = {
            id: 0,
            fileName: '',
            containerId: this.model.id,
            entityType: EntityType.Container,
            documentLevel: DocumentLevel.Container,
            otherDocumentTypes: this.attachments?.map(x => new AttachmentKeyPair(x.attachmentType, x.referenceNo))
        };
    }

    attachmentAddHandler(attachment: AttachmentModel) {
        this.importFormOpened = false;
        this.attachmentService.create(attachment).subscribe(
            data => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.attachment');
                this.getAttachmentTab();
                this._gaService.emitAction('Upload Attachment', GAEventCategory.Container);
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.attachment');
            });
    }

    attachmentEditHandler(attachment: AttachmentModel) {
        this.importFormOpened = false;
        this.attachmentService.update(attachment.id, attachment).subscribe(
            data => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.attachment');
                this.getAttachmentTab();
                this._gaService.emitAction('Edit Attachment', GAEventCategory.Container);
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.attachment');
            });
    }

    editAttachment(id) {
        const result = this.attachments.find(x => x.id === id);
        if (result) {
            // clone object to dis-coupling on data reference from current page to Attachment popup
            this.attachmentModel = Object.assign({}, result);
            this.attachmentModel.containerId = this.model.id;
            this.attachmentModel.entityType = EntityType.Container;
            this.attachmentModel.otherDocumentTypes = this.attachments?.filter(x => x.id !== this.attachmentModel.id)?.map(x => new AttachmentKeyPair(x.attachmentType, x.referenceNo));

            this.attachmentPopupComponent.updateStyle(this.ImportStepState.Selected);
            this.attachmentFormMode = this.AttachmentFormModeType.edit;
            this.importFormOpened = true;
        }
    }

    deleteAttachment(attachmentId) {
        const confirmDlg = this.notification.showConfirmationDialog('msg.deleteAttachmentConfirmation', 'label.attachment');
        const globalId = `${EntityType.Container}_${this.model.id}`;
        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.attachmentService.deleteAttachment(globalId, attachmentId).subscribe(
                        data => {
                            this.notification.showSuccessPopup('msg.attachmentDeleteSuccessfullyNotification', 'label.attachment');
                            this.getAttachmentTab();
                            this._gaService.emitAction('Delete Attachment', GAEventCategory.Container);
                        },
                        error => {
                            this.notification.showErrorPopup('save.failureNotification', 'label.attachment');
                        });
                }
            });
    }

    downloadAttachments() {
        this.attachmentService.downloadAttachments(`Container ${this.editingContainerNumber} Documents` , this.selectedAttachments).subscribe();
    }
    //#endregion

    saveContainer() {
        // Validate container before saving
        this.validateAllFields(false);
        this.checkInvalidContainerNumber();

        if (!this.mainForm.valid) {
            return;
        }

        let updatingModel = cloneDeep(this.model);
        let tmpModel = DateHelper.formatDate(updatingModel);
        this._containerService.updateContainer(this.modelId, tmpModel).subscribe(
            success => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.container');
                this._gaService.emitAction('Edit', GAEventCategory.Container);
                this.router.navigate([`/containers/${this.model.id}`]);
                this.ngOnInit();
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.container');
            }
        );
    }

    cancelEditingContainer() {
        const confirmDlg = this.notification.showConfirmationDialog('edit.cancelConfirmation', 'label.container');

        const sub = confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.router.navigate([`/containers/${this.model.id}`]);
                    this.ngOnInit();
                }
            });
        this._subscriptions.push(sub);
    }

    //#region activity
    get canAddActivity() {
        if (!this.currentUser) {
            return false;
        }
        if (!this.currentUser.isInternal && !this.isAgentUser) {
            return false;
        }
        return true;
    }

    groupActivity() {
        this.groupedActivityList = [];

        // group by activity code & location
        this.activities.map(
            x => {x.activityCodeLocation =  `${x.activityCode}_${x.location}`;
            }
        );
        const groupedByCodeObj = groupBy([...this.activities], 'activityCodeLocation');

        for (const property in groupedByCodeObj) {
            let groupedActivities = groupedByCodeObj[property];
            if (groupedActivities.length > 1) {
                groupedActivities.sort((a, b) =>
                    Date.parse(a.activityDate) - Date.parse(b.activityDate) || Date.parse(a.createdDate) - Date.parse(b.createdDate)
                );
                let activity = groupedActivities[0];
                activity.nestedList = groupedActivities.filter(a => a.id !== activity.id);
                this.groupedActivityList.push(activity);
            } else {
                let activity = groupedActivities[0];
                activity.nestedList = [];
                this.groupedActivityList.push(activity);
            }
        }
        this.groupedActivityList = this.sortActivity(this.groupedActivityList);
        this.isShowNestedActivityGrid = this.groupedActivityList.some(x => x.nestedList.length > 0);
    }

    /**Sort by activityDate descending, if it is equal then sort by activityCode. */
    sortActivity([...activityList]) {
        if (activityList?.length > 0) {
            activityList.sort((a, b) => {
                if (new Date(Date.parse(a.activityDate)).setHours(0,0,0,0) < new Date(Date.parse(b.activityDate)).setHours(0,0,0,0)) {
                    return 1;
                } else if (new Date(Date.parse(a.activityDate)).setHours(0,0,0,0) > new Date(Date.parse(b.activityDate)).setHours(0,0,0,0)) {
                    return -1;
                }
                // Else go to compare sortSequence
                if (a.sortSequence < b.sortSequence) {
                    return 1;
                } else if (a.sortSequence > b.sortSequence) {
                    return -1;
                } else { // nothing to split them
                    return 0;
                }
            });
        }
        return activityList;
    }

    isExceptionEventType(activityType) {
        return !StringHelper.isNullOrEmpty(activityType) && activityType[activityType.length - 1] === 'E';
    }

    onAddActivityClick() {
        this.activityFormMode = 'add';
        this.activityFormOpened = true;
        this.heightActivity = 530;
        this.activityDetails = {
            eventName: null,
            activityDate: new Date()
        };
    }

    onEditActivityClick(activity) {
        this.activityFormMode = 'edit';
        this.activityFormOpened = true;
        this.activityDetails = Object.assign({}, activity);
        this.activityDetails.eventName = activity.activityCode + ' - ' + activity.activityDescription;
        this.activityDetails.activityTypeDescription = this.allEventOptions.find(x => x.activityCode === activity.activityCode).activityTypeDescription;
        if (this.isExceptionEventType(activity.activityType)) {
            this.heightActivity = 710;
        } else {
            this.heightActivity = 530;
        }
    }

    onActivityAdded(activity) {
        this.activityFormOpened = false;
        activity.containerId = this.model.id;
        activity.createdBy = this.currentUser.username;
        this._containerService.createActivity(this.model.id, DateHelper.formatDate(activity)).subscribe(
            data => {
                this.notification.showSuccessPopup('save.activityAddedNotification', 'label.activity');
                this.getActivitiesByContainer(this.model.id);
                this._gaService.emitAction('Add Activity', GAEventCategory.Container);
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.activity');
            });
    }

    onActivityEdited(activity) {
        this.activityFormOpened = false;
        activity.updatedBy = this.currentUser.username;
        this._containerService.updateActivity(this.model.id, activity.id, DateHelper.formatDate(activity)).subscribe(
            data => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.activity');
                this.getActivitiesByContainer(this.model.id);
                this._gaService.emitAction('Edit Activity', GAEventCategory.Container);
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.activity');
            });
    }

    onDeleteActivityClick(activityId) {
        const confirmDlg = this.notification.showConfirmationDialog('msg.deleteActivityConfirm', 'label.activity');

        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this._containerService.deleteActivity(this.model.id, activityId).subscribe(
                        data => {
                            this.notification.showSuccessPopup('msg.deleteActivitySuccessfully', 'label.activity');
                            this.getActivitiesByContainer(this.model.id);
                            this._gaService.emitAction('Delete Activity', GAEventCategory.Container);
                        },
                        error => {
                            this.notification.showErrorPopup('save.failureNotification', 'label.activity');
                        });
                }
            });
    }

    onActivityFormClosed() {
        this.activityFormOpened = false;
    }

    openActivityPopup(activity) {
        this.onEditActivityClick(activity);
        this.activityFormMode = 'view';
    }

    updateMilestone() {
        if (this.milestone != null) {
            this.milestone.data = this.activities.filter(a => a.activityType === ActivityType.Container ||  a.activityType === ActivityType.VesselActivity);
            this.milestone.reload();
        }
    }

    get isExceptionContainer() {
        return (this.activities?.findIndex(a => this.isExceptionEventType(a.activityType) && !a.resolved) !== -1) ?? false;
    }

    public rowCallback(args) {
        return {
            'expandable': args.dataItem.nestedList.length > 0
        };
    }

    //#endregion

    get isAgentUser() {
        return this.currentUser && this.currentUser.userRoles.find(x => x.role.id === Roles.Agent || x.role.id === Roles.CruiseAgent) != null;
    }

    seeMoreCargoDescription(description: string) {
        this.openCargoDescriptionDetailPopup = true;
        this.cargoDescriptionDetail = description;
    }
    onCargoDescriptionDetailPopupClosed() {
        this.openCargoDescriptionDetailPopup = false;
    }

    backToList() {
        this.router.navigate(['/containers']);
    }

    editContainer() {
        this.router.navigate([`/containers/edit/${this.model.id}`]);
    }

    ngOnDestroy() {
        this._subscriptions.map(x => x.unsubscribe());
    }
}
