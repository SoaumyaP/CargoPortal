import { Pipe, PipeTransform } from '@angular/core';
import { StringHelper } from '../helpers/string.helper';

@Pipe({ name: 'convertUTCDateToLocalDate', pure: true })
export class ConvertUTCDateToLocalDatePipe implements PipeTransform {

    /**
     * @param value Date time string with accepted format 'YYYY-mm-ddTHH:mm:ss'
     */
    transform(value: string): any {

        const datetimeISOFormatRegex = /^([\+-]?\d{4}(?!\d{2}\b))((-?)((0[1-9]|1[0-2])(\3([12]\d|0[1-9]|3[01]))?|W([0-4]\d|5[0-2])(-?[1-7])?|(00[1-9]|0[1-9]\d|[12]\d{2}|3([0-5]\d|6[1-6])))([T\s]((([01]\d|2[0-3])((:?)[0-5]\d)?|24\:?00)([\.,]\d+(?!:))?)?(\17[0-5]\d([\.,]\d+)?)?([zZ]|([\+-])([01]\d|2[0-3]):?([0-5]\d)?)?)?)?$/g;

        if (!StringHelper.isNullOrEmpty(value) && datetimeISOFormatRegex.test(value)) {

            // Replace 'z' chars to make sure inputed datetime string is not a UTC format before the datetime will be converted.
            value = value.toString().replace(/z/ig, '');

            // Convert inputted datetime string to UTC format
            const utc = new Date(value);

            // Transform UTC datetime to local datetime
            // The UTC time zone offset is the difference in hours and minutes from Coordinated Universal Time (UTC) for a particular place and date
            const localDate = new Date(utc.getTime() - utc.getTimezoneOffset() * 60 * 1000);

            return localDate;
        } else {

            return value;

        }
    }
}
