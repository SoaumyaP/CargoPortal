import { AttachmentType, EntityType } from './enums/enums';

export interface AttachmentModel {
    id: number;
    fileName: string;
    shipmentId?: number;
    poFulfillmentId?: number;
    consignmentId?: number;
    containerId?: number;
    billOfLadingId?: number;
    masterBillOfLadingId?: number;
    attachmentType?: AttachmentType;
    blobId?: string;
    description?: string;
    referenceNo?: string;

    /**
     * To front-end logic: fetching document type selections by entity type (current page)
     */
    entityType: EntityType;
    /**
     * To show waning message as user is going to re-upload
     */
    otherDocumentTypes: Array<AttachmentKeyPair>;
    /**
     * To determine where the attachment belongs
     */
    documentLevel: string;

    files?: any;

    rowVersion?: string;
    createdBy?: string;
    createdDate?: string;
    updatedBy?: string;
    updatedDate?: string;
    uploadedBy?: string;
    uploadedDate?: string;
    uploadedDateTime?: string;
}

/**Key pair to Distinguish attachment */
export class AttachmentKeyPair {
    attachmentType: string;
    refNumber: string;

    constructor(attachmentType: string, refNumber: string) {
        this.attachmentType = attachmentType;
        this.refNumber = refNumber;
    }
}