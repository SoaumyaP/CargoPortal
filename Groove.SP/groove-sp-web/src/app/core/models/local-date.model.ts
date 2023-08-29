
/**
 * Get value date exactly as stored, not matter on time-zone.
 * Time part will be set to zero.
 * Use it for date picker.
 */
export class LocalDate extends Date {

    hasValue: boolean = true;

    constructor(date: Date) {
        super(date);
        if (date) {
        } else {
            this.hasValue = false;
        }
    }

    /** Returns a date as a string value in ISO format. */
    /** Used by the JSON.stringify method to enable the transformation of an object's data for JavaScript Object Notation (JSON) serialization. */
    toISOString(): string {
        if (!this.hasValue) {
            return null;
        }
        const date = this.getDate();
        const month = this.getMonth() + 1;
        const year = this.getFullYear();
        return `${year}-${month.toString().padStart(2, '0')}-${date.toString().padStart(2, '0')}T00:00:00.000`;
    }
}
