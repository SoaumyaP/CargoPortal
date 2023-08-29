import { AttachmentModel } from "src/app/core/models/attachment.model";

export interface POFulfillmentAttachmentModel extends AttachmentModel {
    /**
     * As attachments will be saved within Booking POFulfillment, it is to detect how to store attachment
     */
    state?: POFulfillmentAttachmentState;

    /**
     * To link with index of original attachment list for editing case
     */
    sourceIndex?: number;
}

export enum POFulfillmentAttachmentState {
    edited = 0,
    added = 1,
    deleted = -1
}
