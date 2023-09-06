import { StringHelper } from 'src/app/core';
import { NoteModel } from 'src/app/core/models/note.model';

/** Model data for note of Purchase order */
export class OrderNoteModel extends NoteModel {

    static defaultCategory = 'General';

    purchaseOrderId: number;
    poItems?: Array<string>;
    isMasterNote: boolean =  false;


    constructor(owner?: string, createdBy?: string, createdDate?: Date, poItems?: Array<string>, category?: string) {
        super(owner, createdBy, createdDate);

        // Set default value for poItems options
        if (StringHelper.isNullOrEmpty(poItems)) {
            this.poItems = [];
        } else {
            this.poItems = poItems;
        }

        // Set default value for category options
        if (StringHelper.isNullOrEmpty(category)) {
            this.category = OrderNoteModel.defaultCategory;
        } else {
            this.category = category;
        }
    }

    /** Convert from based Note model to Purchase order note */
    public MapFrom(source: NoteModel) {
        this.id = source.id;
        this.category = source.category;
        this.noteText = source.noteText;
        this.poItems = StringHelper.isNullOrEmpty(source.extendedData) ? [] : JSON.parse(source.extendedData);
        this.extendedData = source.extendedData;
        this.createdDate = source.createdDate;
        this.createdBy = source.createdBy;
        this.owner = source.owner;
    }

    public MapFromMasterNote(source) {
        const {id, category, message, createdDate, createdBy, owner} = source.masterDialog;
        this.isMasterNote = true;
        this.id = id;
        this.category = category;
        this.noteText = message;
        this.poItems = StringHelper.isNullOrEmpty(source.extendedData) ? [] : JSON.parse(source.extendedData);
        this.extendedData = source.extendedData;
        this.createdDate = createdDate;
        this.createdBy = createdBy;
        this.owner = owner;
    }
}
