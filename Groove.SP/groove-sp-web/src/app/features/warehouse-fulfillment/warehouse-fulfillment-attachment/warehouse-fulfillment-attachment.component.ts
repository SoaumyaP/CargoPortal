import {
  Component,
  Input,
  OnInit,
  ViewChild
} from '@angular/core';
import {
  faCloudDownloadAlt,
  faCloudUploadAlt,
  faEllipsisV,
  faShare
} from '@fortawesome/free-solid-svg-icons';
import { RowArgs } from '@progress/kendo-angular-grid';
import {
  EntityType,
  StringHelper
} from 'src/app/core';
import { AttachmentKeyPair } from 'src/app/core/models/attachment.model';
import { DocumentLevel, GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import {
  AttachmentFormModeType,
  AttachmentUploadPopupComponent
} from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.component';
import { AttachmentUploadPopupService } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.service';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import {
  POFulfillmentAttachmentModel,
  POFulfillmentAttachmentState
} from '../../po-fulfillment/models/po-fulfillment-attachment.model';
import { WarehouseFulfillmentModel } from '../models/warehouse-fulfillment.model';

@Component({
  selector: 'app-warehouse-fulfillment-attachment',
  templateUrl: './warehouse-fulfillment-attachment.component.html',
  styleUrls: ['./warehouse-fulfillment-attachment.component.scss']
})
export class WarehouseFulfillmentAttachmentComponent implements OnInit {
  @Input() warehouseFulfillmentModel: WarehouseFulfillmentModel;
  @Input() isViewMode: boolean;

  faCloudUploadAlt = faCloudUploadAlt;
  faCloudDownloadAlt = faCloudDownloadAlt;
  faShare = faShare;
  faEllipsisV = faEllipsisV;

  private readonly DocumentLevel = DocumentLevel;
  attachmentUploadPopupOpened = false;
  /** Data for attachment upload popup */
  attachmentModel: POFulfillmentAttachmentModel = null;
  attachmentFormMode: AttachmentFormModeType = AttachmentFormModeType.add;

  @ViewChild(AttachmentUploadPopupComponent, { static: false })
  attachmentPopupComponent: AttachmentUploadPopupComponent;

  /** Selected attachments by checkboxes */
  selectedAttachments = [];

  ImportStepState = {
    Selecting: 0,
    Selected: 1
  };

  constructor(
    private _attachmentService: AttachmentUploadPopupService,
    public _notification: NotificationPopup,
    private _gaService: GoogleAnalyticsService) { }

  ngOnInit() {
  }

  public selectAttachment(context: RowArgs): string {
    return context.dataItem;
  }

  private getDocumentLevelText(documentLevel: string): string {
    return this._attachmentService.translateDocumentLevel(documentLevel);
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
      poFulfillmentId: this.warehouseFulfillmentModel.id,
      entityType: EntityType.POFulfillment,
      documentLevel: this.DocumentLevel.POFulfillment,
      otherDocumentTypes: this.warehouseFulfillmentModel.attachments?.map(x => new AttachmentKeyPair(x.attachmentType, x.referenceNo))
    };
  }

  downloadAttachments() {
    this._attachmentService.downloadAttachments(`Booking ${this.warehouseFulfillmentModel.number} Documents`, this.selectedAttachments).subscribe();
  }

  editAttachment(attachment: POFulfillmentAttachmentModel) {
    // get index of current attachment
    const index = this.warehouseFulfillmentModel.attachments.indexOf(attachment);
    if (index >= 0) {
      this.attachmentModel = Object.assign({}, attachment);
      this.attachmentModel.otherDocumentTypes = this.warehouseFulfillmentModel.attachments?.filter(x => this.warehouseFulfillmentModel.attachments.indexOf(x) !== index)?.map(x => new AttachmentKeyPair(x.attachmentType, x.referenceNo));
      this.attachmentModel.entityType = EntityType.POFulfillment;
      // store link to original attachments
      this.attachmentModel.sourceIndex = index;
      this.attachmentModel.poFulfillmentId = this.warehouseFulfillmentModel.id;
      this.attachmentFormMode = AttachmentFormModeType.edit;
      this.attachmentUploadPopupOpened = true;
      this.attachmentPopupComponent.updateStyle(
        this.ImportStepState.Selected
      );
    }
  }

  attachmentClosedHandler() {
    this.attachmentUploadPopupOpened = false;
  }

  attachmentAddedHandler(attachment: POFulfillmentAttachmentModel) {
    const ignoreAttachmentTypes = this.attachmentPopupComponent.ignoreUploadWarningAttachmentTypes;
    attachment.state = POFulfillmentAttachmentState.added;
    let attachments: Array<POFulfillmentAttachmentModel> = this.warehouseFulfillmentModel.attachments;
    attachments = attachments.filter(x => ignoreAttachmentTypes.indexOf(x.attachmentType) >= 0 || x.attachmentType !== attachment.attachmentType || !StringHelper.caseIgnoredCompare(x.referenceNo, attachment.referenceNo));
    attachments.unshift(attachment);
    this.warehouseFulfillmentModel.attachments = attachments;
    this.attachmentUploadPopupOpened = false;
    this._gaService.emitAction('Upload Attachment', GAEventCategory.WarehouseBooking);
  }

  attachmentEditedHandler(attachment: POFulfillmentAttachmentModel) {
    const ignoreAttachmentTypes = this.attachmentPopupComponent.ignoreUploadWarningAttachmentTypes;
    attachment.state = attachment.state || POFulfillmentAttachmentState.edited;
    attachment.uploadedDateTime = (new Date()).toISOString();
    let attachments: Array<POFulfillmentAttachmentModel> = this.warehouseFulfillmentModel.attachments;
    // get link to original attachment
    const index = attachment.sourceIndex;
    if (index >= 0) {
      attachments = attachments.filter(x => this.warehouseFulfillmentModel.attachments.indexOf(x) !== index && (ignoreAttachmentTypes.indexOf(x.attachmentType) >= 0 || x.attachmentType !== attachment.attachmentType || !StringHelper.caseIgnoredCompare(x.referenceNo, attachment.referenceNo)));
      // push at the top of grid
      attachments.unshift(attachment);
      this.warehouseFulfillmentModel.attachments = attachments;
    }
    this.attachmentUploadPopupOpened = false;
    this._gaService.emitAction('Edit Attachment', GAEventCategory.WarehouseBooking);

  }

  attachmentDeletedHandler(attachment: POFulfillmentAttachmentModel) {
    const confirmDlg = this._notification.showConfirmationDialog(
      "msg.deleteAttachmentConfirmation",
      "label.poFulfillment"
    );

    confirmDlg.result.subscribe((result: any) => {
      if (result.value) {
        const index = this.warehouseFulfillmentModel.attachments.indexOf(attachment);
        if (index >= 0) {
            this.warehouseFulfillmentModel.attachments.splice(index, 1);
            this._gaService.emitAction('Delete Attachment', GAEventCategory.WarehouseBooking);

        }
      }
    });
  }

  downloadFile(id, fileName) {
    this._attachmentService.downloadFile(id, fileName).subscribe();
  }
}
