import moment from 'moment';

export const DATE_FORMAT = 'MM/dd/yyyy';
export const DATE_HOUR_FORMAT = 'MM/dd/yyyy HH:mm';
/**
 * Display date and time in 12 hour format with AM/PM. */
export const DATE_HOUR_FORMAT_12 = "MM/dd/yyyy hh:mm' 'a";

export const DATE_HOUR_PLACEHOLDER = 'MM/DD/YYYY HH:mm';
export class DateHelper {
    public static formatDate(object: any): any {
        try {
            const result = Object.assign({}, object);
            for (const item in result) {
                if (result[item] && result[item] !== null) {
                    if (typeof result[item].getMonth === 'function') {
                        result[item] = moment(object[item]).format('YYYY-MM-DDTHH:mm:ss.SSS');
                    } else if (typeof (result[item]) === 'object') {
                        this.formatDate(result[item]);
                    }
                }
            }

            return result;
        } catch (ex) {
            console.log(ex);
        }
    }

    public static isValidDate(date: Date) {
        if (!date) return false;
        var dateFormat = moment(date).format('YYYY-MM-DD');
        const date_regex = /([12]\d{3}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01]))/;
        return (date_regex.test(dateFormat));
    }
}