
/** Base model data for note */
export class NoteModel {
    id?: number;
    category?: string;
    noteText?: string;
    extendedData?: string;
    createdDate?: Date;
    /**Email of user who created*/
    createdBy?: string;
    /**Name of user who created*/
    owner?: string;

    constructor(owner?: string, createdBy?: string, createdDate?: Date) {
        this.owner = owner;
        this.createdBy = createdBy;
        this.createdDate = createdDate;
    }
}
