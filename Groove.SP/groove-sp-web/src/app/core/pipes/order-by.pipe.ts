import { Pipe, PipeTransform } from '@angular/core';
import { StringHelper } from '../helpers/string.helper';

@Pipe({ name: 'orderBy' })
export class OrderByPipe implements PipeTransform {

    /**
     * Order by property names:
     * Ex: 'propertyA'=>asc; '-propertyA'=>desc; 'propertyA|date'=>date comparison only (ignore time part)
     * */
    transform(value: Array<any>, propertyNames: Array<string> | string | number): Array<any> {

        // Transform propertyNames into array if needed
        if (!Array.isArray(propertyNames)) {
            const propName: string = propertyNames.toString();
            propertyNames = [];
            propertyNames.push(propName);
        }
        // Return empty array if source is null
        if (StringHelper.isNullOrEmpty(value)) {
            return [];
        }

        /**
         * 1 -> asc, -1 -> desc
         */
        const sortingOrder = 1;
        const currentPropIndex = 0;
        const currentPropName = propertyNames[currentPropIndex];

        // It is list of primitive data types
        if (StringHelper.isNullOrEmpty(currentPropName)) {
            return value.sort((a, b) => {
                if (a === b) {
                    return 0;
                }
                return sortingOrder * ((a > b) ? 1 : -1);
            });
        } else {
            // It is list on complex objects
            return value.sort((a, b) => {
               return this.sorting(a, b, propertyNames as string[], currentPropIndex);
            });
        }
    }

    public sorting(a, b, propertyNames: Array<string>, propertyIndex): number {
        let sortOrder = 1;
        let propIndex = propertyIndex || 0;
        let propertyName = propertyNames[propIndex];
        let nameOfPart: string = null;
        if (propertyName[0] === '-') {
            sortOrder = -1;
            propertyName = propertyName.substr(1);
            const indexOfPart = propertyName.indexOf('|');
            if (indexOfPart >= 0) {
                nameOfPart = propertyName.substr(indexOfPart + 1, propertyName.length - indexOfPart - 1);
                propertyName = propertyName.substr(0, indexOfPart);
            }
        }
        // extend logic on comparison here
        // if comparison on date part -> convert to ISOString without time
        const aValue = nameOfPart === OrderPipeComparison.date ?  a[propertyName].toISOString().split('T')[0] : a[propertyName];
        const bValue = nameOfPart === OrderPipeComparison.date ?  b[propertyName].toISOString().split('T')[0] : b[propertyName];

        // values of current property are equal, move to next property to compare
        if (aValue === bValue && propertyNames.length > propIndex + 1) {
            propIndex = propIndex + 1;
            return this.sorting(a, b, propertyNames, propIndex);
        }
        // return 0 if equal
        if (aValue === bValue) {
            return 0;
        }
        // return comparison result
        // null/undefined/empty will be on the top of asc sorting
        return sortOrder * (((aValue || (-1 * Infinity)) > (bValue || (-1 * Infinity))) ? 1 : -1);
    }

}

export enum OrderPipeComparison {
    date = 'date'
}

