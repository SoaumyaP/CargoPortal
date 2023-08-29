import { StringHelper } from 'src/app/core';
import { NoteModel } from 'src/app/core/models/note.model';

/** Model data for note of Purchase order */
export class CruiseOrderLineItemNoteModel extends NoteModel {

    cruiseOrderId?: number;
    cruiseOrderItemId?: number;
    cruiseOrderLineItems?: Array<string>;

    constructor(owner?: string, cruiseOrderLineItems?: Array<string>) {
        super(owner);
        // Set default value for poItems options
        if (StringHelper.isNullOrEmpty(cruiseOrderLineItems)) {
            this.cruiseOrderLineItems = [];
        } else {
            this.cruiseOrderLineItems = cruiseOrderLineItems;
        }
    }

    /** Convert from based Note model to Purchase order note */
    public MapFrom(source: NoteModel) {
        this.id = source.id;
        this.category = source.category;
        this.noteText = source.noteText;
        this.cruiseOrderLineItems = JSON.parse(source.extendedData);
        this.extendedData = source.extendedData;
        this.createdDate = source.createdDate;
        this.createdBy = source.createdBy;
        this.owner = source.owner;
    }
}
