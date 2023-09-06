import { Pipe, PipeTransform } from '@angular/core';
import { StringHelper } from '../helpers/string.helper';

@Pipe({ name: 'mawbNumberFormat', pure: true })
export class MAWBNumberFormatPipe implements PipeTransform {

    transform(mawbNo: string): any {
        let result = '';

        if (!StringHelper.isNullOrEmpty(mawbNo)) {
            if (mawbNo.length > 4) {
                result = mawbNo.substring(0, 3) + "-" + mawbNo.substring(3, mawbNo.length);
            } else {
                result = mawbNo;
            }
        }

        return result;
    }
}