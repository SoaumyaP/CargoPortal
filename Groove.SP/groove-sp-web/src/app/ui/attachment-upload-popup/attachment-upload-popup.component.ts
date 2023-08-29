import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, OnChanges, SimpleChanges, SimpleChange } from '@angular/core';
import { FileRestrictions, SuccessEvent, UploadEvent, SelectEvent } from '@progress/kendo-angular-upload';
import { TranslateService } from '@ngx-translate/core';
import { FormComponent, UserContextService, DropdownListModel, StringHelper, AttachmentType, EntityType } from 'src/app/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { environment } from 'src/environments/environment';
import { AttachmentUploadPopupService } from './attachment-upload-popup.service';
import { Subscription } from 'rxjs';
import { AttachmentModel } from 'src/app/core/models/attachment.model';
import { faInfoCircle } from '@fortawesome/free-solid-svg-icons';
@Component({
    selector: 'app-attachment-upload-popup',
    templateUrl: './attachment-upload-popup.component.html',
    styleUrls: ['./attachment-upload-popup.component.scss']
})
export class AttachmentUploadPopupComponent extends FormComponent implements OnInit, OnChanges, OnDestroy {

    @Input() public model: AttachmentModel;
    @Input() public formMode: AttachmentFormModeType;
    @Input() public formOpened: boolean = false;
    @Output() add: EventEmitter<AttachmentModel> = new EventEmitter<AttachmentModel>();
    @Output() edit: EventEmitter<AttachmentModel> = new EventEmitter<AttachmentModel>();
    @Output() close: EventEmitter<AttachmentModel> = new EventEmitter();

    faInfoCircle = faInfoCircle;
    uploadSaveUrl = `${environment.apiUrl}/attachments/ImportFile`;
    importRestrictions: FileRestrictions = {
        allowedExtensions: ['.pdf', '.doc', '.docx', '.png', '.jpg', '.xls', '.xlsx', '.csv']
    };

    // form validation
    validationRules = {
        'attachmentType': {
            'required': 'label.attachmentType'
        },
    };

    documentTypeOptions: Array<DropdownListModel<string>> = [];
    public importStep: any;
    ImportStepState = {
        Selecting: 0,
        Selected: 1,
    };
    AttachmentFormModeType = AttachmentFormModeType;
    isDropEnteredFile: boolean = false;
    isNotAllow = false;
    /**a flag to define if the warning re-uploading message should be shown */
    reUploadWarningVisible: boolean = false;
    reUploadWarningMessage: string = '';
    ignoreUploadWarningAttachmentTypes: Array<string> = [AttachmentType.Others, AttachmentType.Miscellaneous];


    private _subscriptions: Array<Subscription> = [];


    constructor(protected route: ActivatedRoute,
        public notification: NotificationPopup,
        public router: Router,
        public service: AttachmentUploadPopupService,
        public translateService: TranslateService,
        private _userContext: UserContextService) {
        super(route, service, notification, translateService, router);
        this._userContext.getCurrentUser().subscribe((user) => {
            if (user != null) {
                this.currentUser = user;
            }
        });
    }

    ngOnInit() {
        this.updateStyle(this.ImportStepState.Selecting);

    }

    ngOnChanges(changes: SimpleChanges) {
        // to fetch data for attachment type options
        const model: SimpleChange  = changes['model'];
        const previousEntityType = (model?.previousValue as AttachmentModel)?.entityType;
        const currentEntityType = (model?.currentValue as AttachmentModel)?.entityType;
        var entityId = 0;
        switch (currentEntityType) {
            case EntityType.POFulfillment:
                entityId = (model?.currentValue as AttachmentModel)?.poFulfillmentId ?? 0;
                break;
            case EntityType.Shipment:
                entityId = (model?.currentValue as AttachmentModel)?.shipmentId ?? 0;
                break;
            case EntityType.Container:
                entityId = (model?.currentValue as AttachmentModel)?.containerId ?? 0;
                break;
            case EntityType.BillOfLading:
                entityId = (model?.currentValue as AttachmentModel)?.billOfLadingId ?? 0;
                break;
            case EntityType.MasterBill:
                entityId = (model?.currentValue as AttachmentModel)?.masterBillOfLadingId ?? 0;
                break;
            default:
                break;
        }
        if (!StringHelper.isNullOrEmpty(currentEntityType) && previousEntityType !== currentEntityType) {

            // fire request to server to get document types allowed to current user role and entity type (current page)
            const sub = this.service.getAccessibleDocumentTypeOptions(this.currentUser.role.id, this.model.entityType, entityId).subscribe(
                data => {
                    this.documentTypeOptions = data;
                }
            );
            this._subscriptions.push(sub);
        }
        this.isNotAllow = false;
        this.updateWarningMessageStatus();
    }

    ngOnDestroy() {
        this._subscriptions.map(x => x.unsubscribe());
    }

    referenceNoChange() {
        this.updateWarningMessageStatus();
    }

    updateStyle(state) {
        this.importStep = state;
        this.isDropEnteredFile = false;
    }

    public getFileName(fileName: string) {
        const ext = fileName.split('.').pop();
        if (this.importRestrictions.allowedExtensions.indexOf('.' + ext) < 0) {
            return 'default';
        }
        return ext;
    }

    selectEventHandler(e: SelectEvent) {
        this.isNotAllow = e.files[0].validationErrors && e.files[0].validationErrors.length > 0;
        this.model.fileName =  e.files[0].name;
        this.updateStyle(this.ImportStepState.Selected);
    }

    onAdd() {
        this.validateAllFields(false);
        if (this.isSelectingFile || !this.mainForm.valid || this.isNotAllow) {
            return;
        }

        if (this.mainForm.valid) {
            this.model.uploadedDateTime = (new Date()).toISOString();
            switch (this.formMode) {
                case AttachmentFormModeType.add:
                    this.add.emit(this.model);
                    break;
                case AttachmentFormModeType.edit:
                    this.edit.emit(this.model);
                    break;
            }
            this.initPopup();
        }
    }

    onFormClosed() {
        this.resetCurrentForm();
        this.formOpened = false;
        this.close.emit();
    }

    initPopup() {
        this.updateStyle(this.ImportStepState.Selecting);
    }

    onSelectFile() {
        const uploadFile: HTMLElement = document.querySelector('input[kendofileselect]');
        uploadFile.click();
    }

    uploadEventHandler(e: UploadEvent) {
    }

    uploadSuccess(event: SuccessEvent) {
        const response = event.response;
        const data = response.body;
        this.model.fileName = data.fileName;
        this.model.blobId = data.blobId;
        this.model.uploadedBy = this.currentUser.username;
    }

    uploadError(event: SuccessEvent) {
        console.log(event);
    }

    updateWarningMessageStatus() {
        if (this.model) {
            // Show warning message
            this.reUploadWarningVisible = this.model.otherDocumentTypes?.some(x => this.ignoreUploadWarningAttachmentTypes.indexOf(x.attachmentType) < 0 
                && x.attachmentType === this.model.attachmentType 
                && StringHelper.caseIgnoredCompare(x.refNumber, this.model.referenceNo)) || false;

            if (this.reUploadWarningVisible) {
                this.reUploadWarningMessage = this.translateService.instant('message.overrideDocumentTypeWarning',
                {
                    attachmentType: this.model?.attachmentType
                });
            }
        }
    }

    get isSelectingFile() {
        return this.importStep === this.ImportStepState.Selecting;
    }

    get isSelectedFile() {
        return this.importStep === this.ImportStepState.Selected;
    }
}

export enum AttachmentFormModeType {
    add = 0,
    edit = 1
}
