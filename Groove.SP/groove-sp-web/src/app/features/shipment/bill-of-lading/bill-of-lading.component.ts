import { Component, OnDestroy, ViewChild } from '@angular/core';
import { faShare, faCloudUploadAlt, faTrashAlt, faPencilAlt, faEllipsisV, faPlus, faCloudDownloadAlt } from '@fortawesome/free-solid-svg-icons';
import { DATE_FORMAT, DATE_HOUR_FORMAT_12, EntityType, FormComponent, ModeOfTransportType, OrganizationType, StringHelper, UserContextService } from '../../../core';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from '../../../ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { BillOfLadingFormService } from './bill-of-lading-form.service';
import { RowArgs } from '@progress/kendo-angular-grid';
import { AttachmentUploadPopupComponent } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.component';
import { AttachmentUploadPopupService } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.service';
import { ArrayHelper } from 'src/app/core/helpers/array.helper';
import { UserProfileModel } from 'src/app/core/models/user/user-profile.model';
import { AttachmentKeyPair, AttachmentModel } from 'src/app/core/models/attachment.model';
import { BLAddMasterBLPopupComponentMetadata } from './bl-add-master-bl-popup/bl-add-master-bl-popup.component';
import { BillOfLadingModel } from './models/bill-of-lading.model';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { ConsignmentModel } from '../../consignment/models/consignment.model';
import { Subscription } from 'rxjs';
import { DefaultValue2Hyphens, DocumentLevel, GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';

/**
 * Component to display House BOL and Master BOL details which contains tabs
 */
@Component({
    selector: 'app-bill-of-lading',
    templateUrl: './bill-of-lading.component.html',
    styleUrls: ['./bill-of-lading.component.scss']
})
export class BillOfLadingComponent extends FormComponent implements OnDestroy {
    model: BillOfLadingModel;
    public shipments = [];
    public contacts = [];
    public containers = [];
    public itineraries = [];
    /**available attachments on the grid bill of lading */
    public attachments: Array<AttachmentModel> = [];
    /**selected attachments by checkboxes */
    public selectedAttachments: Array<AttachmentModel> = [];
    modelName = 'billOfLadings';
    faShare = faShare;
    StringHelper = StringHelper;

    DATE_FORMAT = DATE_FORMAT;
    DATE_HOUR_FORMAT_12 = DATE_HOUR_FORMAT_12;

    faCloudUploadAlt = faCloudUploadAlt;
    faCloudDownloadAlt = faCloudDownloadAlt;
    faTrashAlt = faTrashAlt;
    faPencilAlt = faPencilAlt;
    faEllipsisV = faEllipsisV;
    faPlus = faPlus;

    importFormOpened = false;
    /**Data for attachment upload popup */
    attachmentModel: AttachmentModel = null;
    attachmentFormMode = 0;
    currentUser: UserProfileModel;
    AttachmentFormModeType = {
        add: 0,
        edit: 1
    };

    masterBLPopupMetaData: BLAddMasterBLPopupComponentMetadata = {
        isFormOpened: false,
        itineraryDataSource: [],
        houseBLId: 0,
        modeOfTransport: null,
        executionAgentId: null
    };
    isOpenLinkToShipmentPopup: boolean;

    readonly AppPermissions = AppPermissions;
    documentLevel = DocumentLevel;

    @ViewChild(AttachmentUploadPopupComponent, { static: false }) attachmentPopupComponent: AttachmentUploadPopupComponent;
    ImportStepState = {
        Selecting: 0,
        Selected: 1,
    };
    subscriptions = new Subscription();
    isCanClickMasterBLNo: boolean;
    defaultValue = DefaultValue2Hyphens;
    modeOfTransport = ModeOfTransportType;

    constructor(protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public _billOfLadingService: BillOfLadingFormService,
        public router: Router,
        public translateService: TranslateService,
        private _userContext: UserContextService,
        public attachmentService: AttachmentUploadPopupService,
        private _gaService: GoogleAnalyticsService) {
        super(route, _billOfLadingService, notification, translateService, router);
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
                if (!user.isInternal) {
                    this.service.affiliateCodes = user.affiliates;
                }
                this.isCanClickMasterBLNo = user.permissions?.some(c => c.name === AppPermissions.BillOfLading_MasterBLDetail);
            }
        });
    }

    onInitDataLoaded(data: void) {
        if (this.model !== null) {
            if (new Date(this.model.issueDate) <= new Date(1900, 1, 1)) {
                this.model.issueDate = null;
            }
            this.getShipmentsByBOL(this.model.id);
            this.getContactsByBOL(this.model.id, this.model.contacts);
            this.getContainersByBOL(this.model.id);
            this.getItinerariesByBOL(this.model.id);
            this.getAttachmentTab();
        }
    }

    public selectAttachment(context: RowArgs): string {
        return context.dataItem;
    }

    getShipmentsByBOL(id: number) {
        this._billOfLadingService.getShipmentsByBOL(id).subscribe(res => {
            if (res) {
                this.shipments = res;
            }
        },
            () => {

            });
    }

    getContactsByBOL(id: number, contacts: any[] = null) {
        if (contacts != null) {
            return this.contacts = contacts;
        }

        this._billOfLadingService.getContactsByBOL(id).subscribe(res => {
            if (res) {
                this.contacts = res;
            }
        },
            () => {

            });
    }

    getContainersByBOL(id: number) {
        this._billOfLadingService.getContainersByBOL(id).subscribe(res => {
            if (res) {
                this.containers = res;
            }
        },
            () => {

            });
    }

    getItinerariesByBOL(id: number) {
        this._billOfLadingService.getItinerariesByBOL(id).subscribe(res => {
            if (res) {
                this.itineraries = res;

                const itinerariesLinkedFreightScheduler = this.itineraries.filter(c => c.scheduleId);
                const itinerariesNotDuplicate = ArrayHelper.uniqueBy(itinerariesLinkedFreightScheduler, 'scheduleId');

                // Get itineraries not link to FreightScheduler
                this.itineraries = this.itineraries.filter(c => !c.scheduleId);
                this.itineraries.push(...itinerariesNotDuplicate);
                ArrayHelper.sortBy(this.itineraries, 'sequence');
            }
        },
            () => {

            });
    }

    getAttachmentTab() {
        this._billOfLadingService.getAttachmentsByBOL(this.model.id).subscribe(res => {
            if (res) {
                this.attachments = res;
            }
        },
            () => {

            });
    }

    private getDocumentLevelText(documentLevel: string) : string {
        return this.attachmentService.translateDocumentLevel(documentLevel);
    }

    downloadFile(id, fileName) {
        this.attachmentService.downloadFile(id, fileName).subscribe();
    }

    backList() {
        this.router.navigate(['/bill-of-ladings']);
    }

    uploadAttachment() {
        this.attachmentFormMode = this.AttachmentFormModeType.add;
        this.attachmentPopupComponent.updateStyle(this.ImportStepState.Selecting);
        this.importFormOpened = true;
        this.attachmentModel = {
            id: 0,
            fileName: '',
            billOfLadingId: this.model.id,
            entityType: EntityType.BillOfLading,
            documentLevel: DocumentLevel.BillOfLading,
            otherDocumentTypes: this.attachments?.map(x => new AttachmentKeyPair(x.attachmentType, x.referenceNo))
        };
    }

    attachmentAddHandler(attachment: AttachmentModel) {
        this.importFormOpened = false;
        this.attachmentService.create(attachment).subscribe(
            data => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.attachment');
                this.getAttachmentTab();
                this._gaService.emitAction('Upload Attachment', GAEventCategory.HouseBill);
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
                this._gaService.emitAction('Edit Attachment', GAEventCategory.HouseBill);
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
            this.attachmentModel.billOfLadingId = this.model.id;
            this.attachmentModel.entityType = EntityType.BillOfLading;
            this.attachmentModel.otherDocumentTypes = this.attachments?.filter(x => x.id !== this.attachmentModel.id)?.map(x => new AttachmentKeyPair(x.attachmentType, x.referenceNo));

            this.attachmentPopupComponent.updateStyle(this.ImportStepState.Selected);
            this.attachmentFormMode = this.AttachmentFormModeType.edit;
            this.importFormOpened = true;
        }
    }

    deleteAttachment(attachmentId) {
        const confirmDlg = this.notification.showConfirmationDialog('msg.deleteAttachmentConfirmation', 'label.attachment');
        const globalId = `${EntityType.BillOfLading}_${this.model.id}`;

        confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.attachmentService.deleteAttachment(globalId, attachmentId).subscribe(
                        data => {
                            this.notification.showSuccessPopup('msg.attachmentDeleteSuccessfullyNotification', 'label.attachment');
                            this.getAttachmentTab();
                            this._gaService.emitAction('Delete Attachment', GAEventCategory.HouseBill);
                        },
                        error => {
                            this.notification.showErrorPopup('save.failureNotification', 'label.attachment');
                        });
                }
            });
    }

    downloadAttachments() {
        this.attachmentService.downloadAttachments(`HouseBL ${this.model.billOfLadingNo} Documents` , this.selectedAttachments).subscribe();
    }

    /* Handler for Add/Assign Master BOL */

    onBtnAddMasterBLClick() {
        let itineraries = Object.assign([], this.itineraries || []);
        // filter on Mode of transport = Sea
        itineraries = itineraries.filter(x => StringHelper.caseIgnoredCompare(x.modeOfTransport, ModeOfTransportType.Sea));
        if (StringHelper.isNullOrEmpty(itineraries) || itineraries.length === 0) {
            this.notification.showInfoDialog('msg.pleaseScheduleTheShipmentItinerary', 'label.billOfLading');
        } else {
            this.masterBLPopupMetaData = {
                isFormOpened: true,
                itineraryDataSource: itineraries,
                houseBLId: this.model.id,
                modeOfTransport: this.model.modeOfTransport,
                executionAgentId: this.model.executionAgentId
            };
        }
    }

    onMasterBOLPopupClosed(masterBLId?: number) {
        this.masterBLPopupMetaData = { ...this.masterBLPopupMetaData, isFormOpened: false };
        // call request to assign master bl to current house bl
        if (!StringHelper.isNullOrEmpty(masterBLId) && masterBLId > 0) {
            this._billOfLadingService.assignMasterBillOfLading(this.model.id, masterBLId).subscribe(
                data => {
                    this._gaService.emitAction('Assign Master BL', GAEventCategory.HouseBill);
                    this.notification.showSuccessPopup('save.sucessNotification', 'label.billOfLading');
                    this.ngOnInit();
                },
                error => {
                    this.notification.showErrorPopup('save.failureNotification', 'label.billOfLading');
                }
            );
        }
    }

    get isHiddenBtnAddMasterBL() {
        return !StringHelper.isNullOrEmpty(this.model.masterBillOfLadingId) || this.model.masterBillOfLadingId > 0 || StringHelper.caseIgnoredCompare(this.model.modeOfTransport,ModeOfTransportType.Air);
    }

    get dateTimeFormat() {
        const isAirMode = StringHelper.caseIgnoredCompare(ModeOfTransportType.Air, this.model?.modeOfTransport);
        return isAirMode ? DATE_HOUR_FORMAT_12 : DATE_FORMAT;
    }

    /* End Handler for Add/Assign Master BOL */

    onLinkToShipment() {
        if (!this.currentUser.isInternal) {
            const affiliates = JSON.parse(this.currentUser.affiliates);
            const isUserHasConsignment = affiliates.some(x => x === this.model.executionAgentId);
            if (!isUserHasConsignment) {
                this.notification.showInfoDialog('msg.createConsignmentHouseBL', 'label.houseBL');
                return;
            }
        }
        this.isOpenLinkToShipmentPopup = true;
    }
    onLinkHouseBLSuccessfully() {
        this.isOpenLinkToShipmentPopup = false;
        this.ngOnInit();
        this._gaService.emitAction('Add Shipment', GAEventCategory.HouseBill);
    }

    onCloseLinkToShipmentPopup() {
        this.isOpenLinkToShipmentPopup = false;
    }

    unlinkShipment(shipmentId: number) {
        const confirmDlg = this.notification.showConfirmationDialog('msg.removeShipment', 'label.houseBL');
        const sub = confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    const isTheLastLinkedShipment = this.shipments.length === 1 ? 1 : 0;
                    this._billOfLadingService.unlinkShipment(shipmentId, this.model.id, isTheLastLinkedShipment).subscribe(
                        r => {
                            this.notification.showSuccessPopup('save.sucessNotification', 'label.houseBL');
                            this.ngOnInit();
                            this._gaService.emitAction('Delete Shipment', GAEventCategory.HouseBill);
                        },
                        err => {
                            this.notification.showErrorPopup('save.failureNotification', 'label.houseBL')
                        }
                    );
                }
            });

        this.subscriptions.add(sub);
    }

    ngOnDestroy(): void {
        this.subscriptions.unsubscribe();
    }
}
