import { Pipe, PipeTransform } from '@angular/core';
import { StringHelper } from '../helpers/string.helper';

@Pipe({ name: 'showless', pure: true })
export class ShowLessPipe implements PipeTransform {
    /**
     * To display the fully string to the show less mode with '...' chars
     * @param value the fully string
     * @param limit the limit of string that you want to display "..."
     */
    transform(value: string, limit: number, hintText: string = "..."): any {

        if (StringHelper.isNullOrEmpty(value)) {
            return '';
        }

        if (value.length <= limit) {
            return value;
        }

        var r = value.substring(0, limit);

        return (value[limit] === " "
            ? r : r.substring(0, r.lastIndexOf(" "))) + hintText;
    }
}
