import { Pipe, PipeTransform } from '@angular/core';
import { StringHelper } from '../helpers/string.helper';

@Pipe({ name: 'linebreak', pure: true })
export class LineBreakPipe implements PipeTransform {
    /**
     * Using to break a string into multiple lines based on the '\n' char
     * Please use output inside innerHTML.
     * @param value the string contains '\n' char
     */
    transform(value: string): any {
        if (StringHelper.isNullOrEmpty(value)) {
            return '';
        }
        var lineArr = value.split("\\n");
        var lineElmts = '';
        lineArr.forEach(line => {
            lineElmts += `<p>${line}</p>`;
        });
        return lineElmts;
    }
}
