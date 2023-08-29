import { Component, OnDestroy, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { FormComponent } from 'src/app/core/form/form.component';
import { environment } from 'src/environments/environment';
import { TranslateService } from '@ngx-translate/core';
import { MasterBillOfLadingFormService } from './master-bill-of-lading-form.service';
import { DateHelper, DATE_FORMAT, DropDownListItemModel, DropdownListModel, EntityType, ModeOfTransportType, StringHelper, UserContextService } from 'src/app/core';
import { faShare, faCloudUploadAlt, faTrashAlt, faPencilAlt, faEllipsisV, faPlus, faCloudDownloadAlt } from '@fortawesome/free-solid-svg-icons';
import { RowArgs } from '@progress/kendo-angular-grid';
import { AttachmentUploadPopupComponent } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.component';
import { AttachmentUploadPopupService } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.service';
import { UserProfileModel } from 'src/app/core/models/user/user-profile.model';
import { AttachmentKeyPair, AttachmentModel } from 'src/app/core/models/attachment.model';
import { MasterBillOfLadingModel } from '../models/master-bill-of-lading-model';
import { DefaultValue2Hyphens, DocumentLevel, GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { MasterBLAddBLPopupComponentMetadata } from '../master-bl-add-bl-popup/master-bl-add-bl-popup.component';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { EMPTY, of, Subject, Subscription } from 'rxjs';
import { ItineraryModel } from 'src/app/core/models/itinerary.model';
import { BillOfLadingModel } from '../../bill-of-lading/models/bill-of-lading.model';
import moment from 'moment';
import { ComboBoxComponent } from '@progress/kendo-angular-dropdowns';
import { delay, switchMap, tap } from 'rxjs/operators';
import { MasterBillOfLadingListService } from '../master-bill-of-lading-list/master-bill-of-lading-list.service';
import { MAWBNumberFormatPipe } from 'src/app/core/pipes/mawb-number-format.pipe';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';

@Component({
    selector: 'app-master-bill-of-lading-form',
    templateUrl: './master-bill-of-lading-form.component.html',
    styleUrls: ['./master-bill-of-lading-form.component.scss']
})
export class MasterBillOfLadingFormComponent extends FormComponent implements OnDestroy {

    modelName = 'masterBOL';
    DATE_FORMAT = DATE_FORMAT;
    defaultValue = DefaultValue2Hyphens;
    modeOfTransportType = ModeOfTransportType;
    documentLevel = DocumentLevel;
    model: MasterBillOfLadingModel;
    tmpMasterBillOfLadingNo: string;
    readonly AppPermissions = AppPermissions;

    initData = [
        { sourceUrl: `${environment.commonApiUrl}/countries/dropDown` },
    ];

    validationRules = {
        'masterBillOfLadingNo': {
            'required': 'label.masterBLNo'
        },
        'carrierName': {
            'required': 'label.carrier'
        },
        'vesselVoyage': {
            'required': 'label.lastVesselVoyage'
        },
        'placeOfIssue': {
            'required': 'label.placeOfIssue'
        },
        'portOfLoading': {
            'required': 'label.lastLoadingPort'
        },
        'issueDate': {
            'required': 'label.issueDates',
            'dateWithin90DaysToday': 'validation.dateWithin90DaysToday'
        },
        'portOfDischarge': {
            'required': 'label.lastDischarge'
        },
        'onBoardDate': {
            'required': 'label.onBoardDates',
            'dateWithin90DaysToday': 'validation.dateWithin90DaysToday'
        },
        'carrierContractNo': {
            'required': 'label.contractNo'
        }
    };

    literalLabels = {
        'masterBill': 'label.masterBillOfLading',
        'masterBillDetail': 'label.masterBillOfLadingDetail',
        'masterBillNo': 'label.masterBLNo',
        'houseBill': 'label.houseBillOfLading',
        'houseBillNo': 'label.houseBLNo'
    }

    /**
     * Data source for grid of house bill of ladings
     */
    billOfLadingList: Array<BillOfLadingModel>;
    containerList: any;
    shipmentList: any;
    contactList: any;
    /**available attachments on the grid of master bill */
    attachments: Array<AttachmentModel>;
    /**selected attachments by checkboxes */
    selectedAttachments: Array<AttachmentModel> = [];
    faShare = faShare;

    faCloudUploadAlt = faCloudUploadAlt;
    faCloudDownloadAlt = faCloudDownloadAlt;
    faTrashAlt = faTrashAlt;
    faPencilAlt = faPencilAlt;
    faEllipsisV = faEllipsisV;
    faPlus = faPlus;
    stringHelper = StringHelper;

    importFormOpened = false;
    attachmentModel: AttachmentModel = null;
    attachmentFormMode = 0;
    currentUser: UserProfileModel;
    AttachmentFormModeType = {
        add: 0,
        edit: 1
    };

    @ViewChild(AttachmentUploadPopupComponent, { static: false }) attachmentPopupComponent: AttachmentUploadPopupComponent;
    ImportStepState = {
        Selecting: 0,
        Selected: 1,
    };

    fileList = [];

    /* Data source for drop-down lists */
    /**
     * To map from Carrier name to SCAC
     */
    carriers: Array<DropdownListModel<string>> = [];
    /**
     * Data source for drop-down list carrier
     */
    carrierNameDataSource: Array<string> = [];
    /**
     * Data source for location auto-complete
     */
    locations: Array<string> = [];
    locationDataSource: Array<string> = [];
    private _subscriptions: Array<Subscription> = [];

    /**
     * Metadata for Add House bill of lading popup
    */
    houseBLPopupMetaData: MasterBLAddBLPopupComponentMetadata = {
        isFormOpened: false,
        masterBLId: null
    };

    /**
     * Data source for carrier contract no
     */
    carrierContractNoDataSource: Array<DropDownListItemModel<string>> = [];
    carrierContractNoKeyUp$ = new Subject<string>();
    // Master BL number
    @ViewChild('carrierContractNoComboBox', { static: false })
    public carrierContractNoComboBox: ComboBoxComponent;
    isCarrierContractNoLoading: boolean = false;

    constructor(protected route: ActivatedRoute,
        public service: MasterBillOfLadingFormService,
        private _listService: MasterBillOfLadingListService,
        public notification: NotificationPopup,
        public router: Router,
        public translateService: TranslateService,
        private _userContext: UserContextService,
        public attachmentService: AttachmentUploadPopupService,
        private mawbNumberFormatPipe: MAWBNumberFormatPipe,
        private _gaService: GoogleAnalyticsService) {
        super(route, service, notification, translateService, router);
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
                if (!user.isInternal) {
                    this.service.affiliateCodes = user.affiliates;
                }
            }
            this._registerEventHandlers();
        });
    }

    ngOnDestroy() {
        this._subscriptions.map(x => x.unsubscribe());
    }

    public selectAttachment(context: RowArgs): string {
        return context.dataItem;
    }

    onInitDataLoaded(data): void {
        if (this.model !== null) {
            if (new Date(this.model.issueDate) <= new Date(1900, 1, 1)) {
              this.model.issueDate = null;
            }
            if (this.stringHelper.caseIgnoredCompare(this.model.modeOfTransport, ModeOfTransportType.Air)) {
                this.tmpMasterBillOfLadingNo = this.model.masterBillOfLadingNo;
                this.model.masterBillOfLadingNo = this.mawbNumberFormatPipe.transform(this.model.masterBillOfLadingNo);
            }
            this.updateLiteralLabels(this.model.modeOfTransport);
        }
        // If it is edit mode, need to fetch some order data sources
        if (this.isEditMode) {
            this._fetchDataSources();
        } else {
            // It is in view/add mode

            let sub: Subscription;

            // If it is not direct master (linking to house bl)
            if (!this.model.isDirectMaster) {
                sub = this.service.getHouseBillOfLadings(this.model.id).subscribe((list: Array<BillOfLadingModel>) => {
                if (list) {
                    this.billOfLadingList = list;
                }
                });
                this._subscriptions.push(sub);
            }

            sub = this.service.getContainers(this.model.id, this.model.isDirectMaster).subscribe(list => {
                if (list) {
                    this.containerList = list;
                }
            });
            this._subscriptions.push(sub);

            sub = this.service.getShipments(this.model.id, this.model.isDirectMaster).subscribe(list => {
                if (list) {
                    this.shipmentList = list;
                }
            });
            this._subscriptions.push(sub);

            sub = this.service.getContacts(this.model.id).subscribe(list => {
                if (list) {
                    this.contactList = list;
                }
            });
            this._subscriptions.push(sub);
            this.getAttachmentTab();
        }

    }

    getAttachmentTab() {
        const sub =  this.service.getAttachments(this.model.id).subscribe((list: Array<AttachmentModel>) => {
            if (list) {
                this.attachments = list;
            }
        });
        this._subscriptions.push(sub);
    }

    private getDocumentLevelText(documentLevel: string): string {
        return this.attachmentService.translateDocumentLevel(documentLevel);
    }

    idKey(context: RowArgs): string {
        return context.dataItem.id;
    }

    downloadFile(id, fileName) {
        const sub = this.attachmentService.downloadFile(id, fileName).subscribe();
        this._subscriptions.push(sub);
    }

    backList() {
        this.router.navigate(['/master-bill-of-ladings']);
    }

    uploadAttachment() {
        this.attachmentFormMode = this.AttachmentFormModeType.add;
        this.attachmentPopupComponent.updateStyle(this.ImportStepState.Selecting);
        this.importFormOpened = true;
        this.attachmentModel = {
            id: 0,
            fileName: '',
            masterBillOfLadingId: this.model.id,
            entityType: EntityType.MasterBill,
            documentLevel: DocumentLevel.MasterBill,
            otherDocumentTypes: this.attachments?.map(x => new AttachmentKeyPair(x.attachmentType, x.referenceNo))
        };
    }

    attachmentAddHandler(attachment: AttachmentModel) {
        this.importFormOpened = false;
        const sub = this.attachmentService.create(attachment).subscribe(
            data => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.attachment');
                this.getAttachmentTab();
                this._gaService.emitAction('Upload Attachment', GAEventCategory.MasterBill);
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.attachment');
            });
        this._subscriptions.push(sub);
    }

    attachmentEditHandler(attachment: AttachmentModel) {
        this.importFormOpened = false;
        const sub = this.attachmentService.update(attachment.id, attachment).subscribe(
            data => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.attachment');
                this.getAttachmentTab();
                this._gaService.emitAction('Edit Attachment', GAEventCategory.MasterBill);
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.attachment');
            });
        this._subscriptions.push(sub);
    }

    editAttachment(id) {
        const result = this.attachments.find(x => x.id === id);
        if (result) {

            // clone object to dis-coupling on data reference from current page to Attachment popup
            this.attachmentModel = Object.assign({}, result);
            this.attachmentModel.masterBillOfLadingId = this.model.id;
            this.attachmentModel.entityType = EntityType.MasterBill;
            this.attachmentModel.otherDocumentTypes = this.attachments?.filter(x => x.id !== this.attachmentModel.id)?.map(x => new AttachmentKeyPair(x.attachmentType, x.referenceNo));

            this.attachmentPopupComponent.updateStyle(this.ImportStepState.Selected);
            this.attachmentFormMode = this.AttachmentFormModeType.edit;
            this.importFormOpened = true;
        }
    }

    deleteAttachment(attachmentId) {
        const confirmDlg = this.notification.showConfirmationDialog('msg.deleteAttachmentConfirmation', 'label.attachment');
        const globalId = `${EntityType.MasterBill}_${this.model.id}`;
        const sub = confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.attachmentService.deleteAttachment(globalId, attachmentId).subscribe(
                        data => {
                            this.notification.showSuccessPopup('msg.attachmentDeleteSuccessfullyNotification', 'label.attachment');
                            this.getAttachmentTab();
                            this._gaService.emitAction('Delete Attachment', GAEventCategory.MasterBill);
                        },
                        error => {
                            this.notification.showErrorPopup('save.failureNotification', 'label.attachment');
                        });
                }
            });
        this._subscriptions.push(sub);
    }

    downloadAttachments() {
        this.attachmentService.downloadAttachments(`MasterBL ${this.model.masterBillOfLadingNo} Documents` , this.selectedAttachments).subscribe();
    }

    private _fetchDataSources(): void {
        this._fetchLocations();
        this._fetchItineraries();
        this._fetchCarrierContractNumbers();
    }

    /* Handler for Add/Remove House BOL */
    onBtnAddHouseBLClick() {
        this.houseBLPopupMetaData = {
            isFormOpened : true,
            masterBLId: this.model.id,
            houseBLList: this.billOfLadingList
        };
    }

    onHouseBLPopupClosed(houseBLId?: number) {
        this.houseBLPopupMetaData = {...this.houseBLPopupMetaData, isFormOpened : false};
        // call request to assign master bl to current house bl
        if (!StringHelper.isNullOrEmpty(houseBLId) && houseBLId > 0) {
            const sub = this.service.assignHouseBillOfLading(this.model.id, houseBLId).subscribe(
                data => {
                    this._gaService.emitAction('Add House BL', GAEventCategory.MasterBill);
                    this.notification.showSuccessPopup('save.sucessNotification', 'label.masterBOL');
                    this.ngOnInit();
                },
                error => {
                    this.notification.showErrorPopup('save.failureNotification', 'label.masterBOL');
                }
            );
            this._subscriptions.push(sub);
        }
    }

    onBtnRemoveHouseBLClick(data) {

        const sub = this.notification.showConfirmationDialog(
            'confirmation.unlinkHouseBL',
            'label.masterBOL'
        ).result.subscribe((result: any) => {
            if (result.value) {
                const sub1 = this.service.removeHouseBillOfLading(this.model.id, data.id).subscribe(
                    () => {
                        this._gaService.emitAction('Delete House BL', GAEventCategory.MasterBill);
                        this.notification.showSuccessPopup('save.sucessNotification', 'label.masterBOL');
                        this.ngOnInit();
                    },
                    () => {
                        this.notification.showErrorPopup('save.failureNotification', 'label.masterBOL');
                    }
                );
                this._subscriptions.push(sub1);
            }
        });
        this._subscriptions.push(sub);
    }

    /* End Handler for Add/Remove House BOL */

    /* Handler for edit Master bill of lading */

    private _fetchLocations() {
        const sub = this.service.getAllLocations().subscribe(data => {
            this.locations = data?.map(x => x.locationDescription) || [];
            this.locationDataSource = this.locations;
        });
        this._subscriptions.push(sub);
    }

    private _fetchItineraries() {

        // Fetch data for Itineraries, then pre-populate for drop-down lists
        const sub = this.service.getItineraries(this.model.id).subscribe(list => {
            // Seed data source with current values from master bill of lading
            this.carriers = [];
            this.carrierNameDataSource = [];

            // Seed more data from itineraries if available
            list?.map(itinerary => {
                if (!this.carrierNameDataSource.some(x => x === itinerary.carrierName)) {
                    this.carrierNameDataSource.push(itinerary.carrierName);
                    let carrierCode = StringHelper.caseIgnoredCompare(itinerary.modeOfTransport, ModeOfTransportType.Air) ? itinerary.airlineCode : itinerary.scac;
                    this.carriers.push(new DropdownListModel<string>(itinerary.carrierName, carrierCode));
                }
            });
            if (!this.carrierNameDataSource.some(x => x === this.model.carrierName)) {
                this.carrierNameDataSource.push(this.model.carrierName);
                let carrierCode = StringHelper.caseIgnoredCompare(this.model.modeOfTransport, ModeOfTransportType.Air) ? this.model.airlineCode : this.model.scac;
                this.carriers.push(new DropdownListModel<string>(this.model.carrierName, carrierCode));
            }

        });
        this._subscriptions.push(sub);
    }

    private _registerEventHandlers(): void {
        const sub = this.carrierContractNoKeyUp$.pipe(
            tap(() => {
                this.carrierContractNoComboBox.toggle(false);
            }),
            switchMap((searchTerm: string) => {
                if (!StringHelper.isNullOrEmpty(searchTerm) && searchTerm.length >= 3) {
                    return of(searchTerm).pipe(delay(1000));
                } else {
                    return EMPTY;
                }
            }
        )).subscribe((searchTerm: string) => {
            this._carrierContractNoFilterChange(searchTerm);
        });
        this._subscriptions.push(sub);
    }

    placeOfIssueFilterChange(value: string) {
        this.locationDataSource = [];
        if (value.length >= 3) {
            this.locationDataSource = this.locations.filter((s) => s.toLowerCase().indexOf(value.toLowerCase()) > -1);
        }
    }

    onIssueDateValueChanged(value) {
        const currentDate = new Date().toISOString().split('T')[0];
        const dayDiff = moment(value).diff(moment(currentDate), 'days', true);

        if (dayDiff < -90 || 90 < dayDiff) {
            this.setInvalidControl('issueDate', 'dateWithin90DaysToday');
        } else {
            this.setValidControl('issueDate', 'dateWithin90DaysToday');
        }
    }

    onBoardDateValueChanged(value) {
        const currentDate = new Date().toISOString().split('T')[0];
        const dayDiff = moment(value).diff(moment(currentDate), 'days', true);

        if (dayDiff < -90 || 90 < dayDiff) {
            this.setInvalidControl('onBoardDate', 'dateWithin90DaysToday');
        } else {
            this.setValidControl('onBoardDate', 'dateWithin90DaysToday');
        }
    }

    onBtnEditMasterBLClick() {
        this.router.navigate([`/master-bill-of-ladings/edit/${this.model.id}`]);
    }

    onBtnCancellingMasterBLEditClick() {
        const confirmDlg = this.notification.showConfirmationDialog('edit.cancelConfirmation', 'label.masterBOL');

        const sub = confirmDlg.result.subscribe(
            (result: any) => {
                if (result.value) {
                    this.router.navigate([`/master-bill-of-ladings/${this.model.id}`]);
                    this.ngOnInit();
                }
            });
        this._subscriptions.push(sub);
    }

    onBtnSavingMasterBLClick() {
        if (!this.mainForm.valid) {
            return;
        }
        let updatingModel = {...this.model};
        delete updatingModel.contractMaster;
        updatingModel = DateHelper.formatDate(updatingModel);

        // Need to fulfill some other properties
        updatingModel.scac = this.carriers?.find(x => x.label === this.model.carrierName).value || null;

        // Revert mawb number format before saving
        if (this.stringHelper.caseIgnoredCompare(updatingModel.modeOfTransport, ModeOfTransportType.Air)) {
            updatingModel.masterBillOfLadingNo = this.tmpMasterBillOfLadingNo;
        }

        this.service.updateMasterBOL(this.model.id, updatingModel).subscribe(
            success => {
                this._gaService.emitAction('Edit', GAEventCategory.MasterBill);
                this.notification.showSuccessPopup('save.sucessNotification', 'label.masterBOL');
                this.router.navigate([`/master-bill-of-ladings/${this.model.id}`]);
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.masterBOL');
            }
        );

    }

    /* End Handler for edit Master bill of lading */

    /* Handler for Carrier Contract No */

    private _fetchCarrierContractNumbers() {
        this.carrierContractNoDataSource = [];
        if (this.model?.contractMaster) {
            this.carrierContractNoDataSource.push(
                {
                    text : this.model.contractMaster.realContractNo,
                    value : this.model.carrierContractNo
                }
            );
        }
    }

    private _carrierContractNoFilterChange(searchTerm: string) {
        this.isCarrierContractNoLoading = true;
        const carrierCode =  this.carriers?.find(x => x.label === this.model.carrierName).value || null;
        this._listService.getContractMasterOptions(searchTerm, carrierCode, new Date()).subscribe(
            (data) => {
                this.carrierContractNoDataSource = data;
                this.carrierContractNoComboBox.toggle(true);
                this.isCarrierContractNoLoading = false;
            }
        );
    }

    /* End Handler for Carrier Contract No */

    private updateLiteralLabels(modeOfTransport: string): void {
        if (!modeOfTransport) {
            return;
        }
        switch (modeOfTransport.toLowerCase()) {
            case ModeOfTransportType.Sea.toLowerCase():
                this.literalLabels.masterBill = 'label.masterBillOfLading';
                this.literalLabels.masterBillDetail = 'label.masterBillOfLadingDetail';
                this.literalLabels.masterBillNo = 'label.masterBLNo';
                this.literalLabels.houseBill = 'label.houseBillOfLading';
                this.literalLabels.houseBillNo = 'label.houseBLNo';
                break;
            case ModeOfTransportType.Air.toLowerCase():
                this.literalLabels.masterBill = 'label.mawb';
                this.literalLabels.masterBillDetail = 'label.mawbDetail';
                this.literalLabels.masterBillNo = 'label.mawbNo';
                this.literalLabels.houseBill = 'label.hawb';
                this.literalLabels.houseBillNo = 'label.hawbNo';
                break;
            default:
                this.literalLabels.masterBill = 'label.masterBillOfLading';
                this.literalLabels.masterBillDetail = 'label.masterBillOfLadingDetail';
                this.literalLabels.masterBillNo = 'label.masterBLNo';
                this.literalLabels.houseBill = 'label.houseBillOfLading';
                this.literalLabels.houseBillNo = 'label.houseBLNo';
                break;
        }
    }

}
