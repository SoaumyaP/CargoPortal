import { StringHelper } from 'src/app/core';
import { NoteModel } from 'src/app/core/models/note.model';
import { BulkFulfillmentNoteItemModel } from './bulk-fulfillment-note-item.model';
import { BulkFulfillmentOrderModel } from './bulk-fulfillment.model';

export class BulkFulfillmentNoteModel extends NoteModel {
    static defaultCategory = 'General';

    poFulfillmentId: number;
    itemsSelected: Array<BulkFulfillmentNoteItemModel>;
    isMasterNote: boolean = false;

    constructor(owner?: string, createdBy?: string, createdDate?: Date, items?: Array<BulkFulfillmentNoteItemModel>, category?: string) {
        super(owner, createdBy, createdDate);

        // Set default value for poItems options
        if (StringHelper.isNullOrEmpty(items)) {
            this.itemsSelected = [];
        } else {
            this.itemsSelected = items;
        }

        // Set default value for category options
        if (StringHelper.isNullOrEmpty(category)) {
            this.category = BulkFulfillmentNoteModel.defaultCategory;
        } else {
            this.category = category;
        }
    }

    public MapFrom(source: NoteModel) {
        this.id = source.id;
        this.category = source.category;
        this.noteText = source.noteText;
        this.itemsSelected = JSON.parse(source.extendedData);
        this.extendedData = source.extendedData;
        this.createdDate = source.createdDate;
        this.createdBy = source.createdBy;
        this.owner = source.owner;
    }

    public MapFromMasterNote(source) {
        const { id, category, message, createdDate, createdBy, owner } = source.masterDialog;
        this.isMasterNote = true;
        this.id = id;
        this.category = category;
        this.noteText = message;
        this.itemsSelected = StringHelper.isNullOrEmpty(source.extendedData) ? [] : JSON.parse(source.extendedData);
        this.extendedData = source.extendedData;
        this.createdDate = createdDate;
        this.createdBy = createdBy;
        this.owner = owner;
    }
}

