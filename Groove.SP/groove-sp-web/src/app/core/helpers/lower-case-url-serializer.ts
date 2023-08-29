import { DefaultUrlSerializer, UrlTree } from '@angular/router';

export class LowerCaseUrlSerializer extends DefaultUrlSerializer {
    parse(url: string): UrlTree {
        // Optional Step: Do some stuff with the url if needed.

        // If you lower it in the optional step
        // you don't need to use "toLowerCase"
        // when you pass it down to the next function

        // DO NOT update url if it contains id_token or po-progress-check/ telerik-report/ scheduling
        const exceptionUrls = [
            '#id_token',
            'po-progress-check',
            'warehouse-bookings-confirm',
            'telerik-report',
            'scheduling',
            'purchase-orders',
            'shipments',
            'bulk-fulfillments',
            'po-fulfillments',
            'missing-po-fulfillments'
        ]
        if (url && exceptionUrls.some(c => url.includes(c))) {
            return super.parse(url);
        } else {
            return super.parse(url.toLowerCase());
        }
    }
}
