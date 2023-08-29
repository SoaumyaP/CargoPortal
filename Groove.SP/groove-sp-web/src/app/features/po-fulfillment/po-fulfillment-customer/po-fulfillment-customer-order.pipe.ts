import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'poFulfillmentCustomerOrder' })
export class POFulfillmentCustomerOrderPipe implements PipeTransform {

    /**Order by customerPONumber and productCode
    */
    transform(value: Array<any>): Array<any> {
        return value.sort((a, b) => {
            if (a['customerPONumber'] === b['customerPONumber']) {
                return a['productCode'] >= b['productCode'] ? 1 : -1;
            }
            return a['customerPONumber'] >= b['customerPONumber'] ? 1 : -1;
        });
    }
}
