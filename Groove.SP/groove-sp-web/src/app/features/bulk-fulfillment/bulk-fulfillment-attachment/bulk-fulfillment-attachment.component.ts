import { Component, Input, OnChanges, OnInit, SimpleChanges, ViewChild } from '@angular/core';
import { faCloudDownloadAlt, faCloudUploadAlt, faEllipsisV, faInfoCircle, faPencilAlt, faPlus, faPowerOff, faShare, faTrashAlt } from '@fortawesome/free-solid-svg-icons';
import { AttachmentKeyPair } from 'src/app/core/models/attachment.model';
import { EntityType, ValidationDataType } from 'src/app/core/models/enums/enums';
import { AttachmentFormModeType, AttachmentUploadPopupComponent } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.component';
import { AttachmentUploadPopupService } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.service';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { POFulfillmentAttachmentModel, POFulfillmentAttachmentState } from '../../po-fulfillment/models/po-fulfillment-attachment.model';
import * as appConstants from 'src/app/core/models/constants/app-constants';
import { StringHelper } from 'src/app/core';
import { RowArgs } from '@progress/kendo-angular-grid';
import { ValidationData } from 'src/app/core/models/forms/validation-data';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { GAEventCategory } from 'src/app/core/models/constants/app-constants';

@Component({
  selector: 'app-bulk-fulfillment-attachment',
  templateUrl: './bulk-fulfillment-attachment.component.html',
  styleUrls: ['./bulk-fulfillment-attachment.component.scss']
})
export class BulkFulfillmentAttachmentComponent implements OnInit, OnChanges {
  @Input() bookingModel: any;
  @Input() isViewMode: boolean;
  @Input() isEditMode: boolean;
  @Input() isAddMode: boolean;
  @Input() saveAsDraft: boolean;

  @ViewChild(AttachmentUploadPopupComponent, { static: false }) attachmentPopupComponent: AttachmentUploadPopupComponent;

  faShare = faShare;
  faCloudUploadAlt = faCloudUploadAlt;
  faInfoCircle = faInfoCircle;
  attachmentUploadPopupOpened = false;
  faCloudDownloadAlt = faCloudDownloadAlt;
  faPlus = faPlus;
  faEllipsisV = faEllipsisV;
  faPencilAlt = faPencilAlt;
  faTrashAlt = faTrashAlt;
  faPowerOff = faPowerOff;

  /** Data for attachment upload popup */
  attachmentModel: POFulfillmentAttachmentModel = null;
  attachmentFormMode: AttachmentFormModeType = AttachmentFormModeType.add;
  /** Selected attachments by checkboxes */
  selectedAttachments = [];
  private readonly DocumentLevel = appConstants.DocumentLevel;
  stringHelper = StringHelper;
  ImportStepState = {
    Selecting: 0,
    Selected: 1
  };
  errorMessages = {
    requireMSDSFile: null
  }


  constructor(
    private _attachmentService: AttachmentUploadPopupService,
    public notification: NotificationPopup,
    private _gaService: GoogleAnalyticsService
  ) { }

  ngOnInit() {
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes?.isViewMode?.currentValue) {
      this.resetErrorMessages();
    }
  }

  uploadAttachment() {
    this.attachmentFormMode = AttachmentFormModeType.add;
    this.attachmentPopupComponent.updateStyle(
      this.ImportStepState.Selecting
    );
    this.attachmentUploadPopupOpened = true;
    this.attachmentModel = {
      id: 0,
      fileName: '',
      poFulfillmentId: this.bookingModel.id,
      entityType: EntityType.POFulfillment,
      documentLevel: this.DocumentLevel.POFulfillment,
      otherDocumentTypes: this.bookingModel.attachments?.map(x => new AttachmentKeyPair(x.attachmentType, x.referenceNo))
    };
    this._gaService.emitAction('Upload Attachment', GAEventCategory.BulkBooking);
  }

  downloadAttachments() {
    this._attachmentService.downloadAttachments(`Booking ${this.bookingModel.number} Documents`, this.selectedAttachments).subscribe();
  }

  editAttachment(attachment: POFulfillmentAttachmentModel) {
    // get index of current attachment
    const index = this.bookingModel.attachments.indexOf(attachment);
    if (index >= 0) {
      this.attachmentModel = Object.assign({}, attachment);
      this.attachmentModel.otherDocumentTypes = this.bookingModel.attachments?.filter(x => this.bookingModel.attachments.indexOf(x) !== index)?.map(x => new AttachmentKeyPair(x.attachmentType, x.referenceNo));
      this.attachmentModel.entityType = EntityType.POFulfillment;
      // store link to original attachments
      this.attachmentModel.sourceIndex = index;
      this.attachmentModel.poFulfillmentId = this.bookingModel.id;
      this.attachmentFormMode = AttachmentFormModeType.edit;
      this.attachmentUploadPopupOpened = true;
      this.attachmentPopupComponent.updateStyle(
        this.ImportStepState.Selected
      );
      this._gaService.emitAction('Edit Attachment', GAEventCategory.BulkBooking);
    }
  }

  attachmentClosedHandler() {
    this.attachmentUploadPopupOpened = false;
  }

  attachmentAddedHandler(attachment: POFulfillmentAttachmentModel) {
    
    const ignoreAttachmentTypes = this.attachmentPopupComponent.ignoreUploadWarningAttachmentTypes;
    attachment.state = POFulfillmentAttachmentState.added;
    let attachments: Array<POFulfillmentAttachmentModel> = this.bookingModel.attachments;
    attachments = attachments.filter(x => ignoreAttachmentTypes.indexOf(x.attachmentType) >= 0 || x.attachmentType !== attachment.attachmentType || !this.stringHelper.caseIgnoredCompare(x.referenceNo, attachment.referenceNo));
    attachments.unshift(attachment);
    this.bookingModel.attachments = attachments;
    this.attachmentUploadPopupOpened = false;

    if (!this.saveAsDraft) {
      if (attachment.attachmentType === 'MSDS') {  
        this.errorMessages.requireMSDSFile = null;
      }
    }
  }

  attachmentEditedHandler(attachment: POFulfillmentAttachmentModel) {
    const ignoreAttachmentTypes = this.attachmentPopupComponent.ignoreUploadWarningAttachmentTypes;
    attachment.state = attachment.state || POFulfillmentAttachmentState.edited;
    attachment.uploadedDateTime = (new Date()).toISOString();
    let attachments: Array<POFulfillmentAttachmentModel> = this.bookingModel.attachments;
    // get link to original attachment
    const index = attachment.sourceIndex;
    if (index >= 0) {
      attachments = attachments.filter(x => this.bookingModel.attachments.indexOf(x) !== index && (ignoreAttachmentTypes.indexOf(x.attachmentType) >= 0 || x.attachmentType !== attachment.attachmentType || !this.stringHelper.caseIgnoredCompare(x.referenceNo, attachment.referenceNo)));
      // push at the top of grid
      attachments.unshift(attachment);
      this.bookingModel.attachments = attachments;
    }
    this.attachmentUploadPopupOpened = false;
  }

  attachmentDeletedHandler(attachment: POFulfillmentAttachmentModel) {
    const confirmDlg = this.notification.showConfirmationDialog(
      "msg.deleteAttachmentConfirmation",
      "label.poFulfillment"
    );

    confirmDlg.result.subscribe((result: any) => {
      if (result.value) {
        const index = this.bookingModel.attachments.indexOf(attachment);
        if (index >= 0) {
          this.bookingModel.attachments.splice(index, 1);
          this._gaService.emitAction('Delete Attachment', GAEventCategory.BulkBooking);
        }
      }
    });
  }

  downloadFile(id, fileName) {
    this._attachmentService.downloadFile(id, fileName).subscribe();
  }

  getDocumentLevelText(documentLevel: string): string {
    return this._attachmentService.translateDocumentLevel(documentLevel);
  }
  selectAttachment(context: RowArgs): string {
    return context.dataItem;
  }

  private resetErrorMessages(): void {
    this.errorMessages.requireMSDSFile = null;
  }

  private validate(): Array<ValidationData> {
    let result: Array<ValidationData> = [];
    if (this.bookingModel.isContainDangerousGoods) {
      let msdsFileIndex = this.bookingModel.attachments?.findIndex(a => a.attachmentType === 'MSDS') ?? -1;

      if (msdsFileIndex === -1) {
        this.errorMessages.requireMSDSFile = 'validation.requireMSDSWithDangerousGoods';
        result.push(new ValidationData(
          ValidationDataType.Business, false, this.errorMessages.requireMSDSFile
        ))
      }
    }

    return result;
  }

  validateBeforeSubmitting(): Array<ValidationData> {
    this.resetErrorMessages();

    let result: Array<ValidationData> = [];
    result = this.validate();
    return result;
  }

  validateBeforeSaving(): Array<ValidationData> {
    this.resetErrorMessages()
    
    let result: Array<ValidationData> = [];
    if (!this.saveAsDraft) {
      result = this.validate();
    }
    return result;
  }
}
