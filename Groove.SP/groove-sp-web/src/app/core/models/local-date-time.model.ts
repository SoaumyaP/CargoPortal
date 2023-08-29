
/**
 * Get value date and time exactly as stored, not matter on time-zone.
 * Use it for date time picker
 */
export class LocalDateTime extends Date {

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
        const hours = this.getHours();
        const minutes = this.getMinutes();
        const seconds = this.getSeconds();
        const milliseconds = this.getMilliseconds();
        return `${year}-${month.toString().padStart(2, '0')}-${date.toString().padStart(2, '0')}T${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}.${milliseconds.toString().padStart(3, '0')}`;
    }
}
