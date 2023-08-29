import { Pipe, PipeTransform } from '@angular/core';
import moment from 'moment';
import { StringHelper } from '../helpers/string.helper';

/**
 * List of available format. They are moment styles
 */
export enum MomentUTCFormat {
    Date = 'MM/DD/YYYY',
    Date_Time = 'MM/DD/YYYY kk:mm',
    Date_Time_Second = 'MM/DD/YYYY kk:mm:ss'
}

/**
 * To get string of UTC value of provided date-time
 */
@Pipe({ name: 'utcDateTimeFormat', pure: true })
export class UTCDateTimeFormatPipe implements PipeTransform {

    transform(value?: Date, format?: MomentUTCFormat): any {
        if (!StringHelper.isNullOrEmpty(value)) {
            const utcDateTime = moment.utc(value);
            if (!StringHelper.isNullOrEmpty(format)) {
               return utcDateTime.format(format);
            } else {
                return utcDateTime.format(MomentUTCFormat.Date_Time_Second) + ' GMT';
            }
        } else {
            return '';
        }

    }
}
