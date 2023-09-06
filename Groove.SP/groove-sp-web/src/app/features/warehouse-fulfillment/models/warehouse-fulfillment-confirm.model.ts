import { StringHelper } from "src/app/core";

export class WarehouseFulfillmentConfirmQueryModel {
    selectedCustomerId: number;
    selectedSupplier: string | null;
    bookingNoFrom: string | null;
    bookingNoTo: string | null;
    expectedHubArrivalDateFrom: string | null;
    expectedHubArrivalDateTo: string | null;

    /**
       * Build filter-data model to URI Query Params
       */
     public get buildToQueryParams() {
        let queryStr = '';
        let esc = encodeURIComponent;
        queryStr += Object.keys(this)
            .filter(k => this[k] != null && this[k] != "")
            .map(k => esc(StringHelper.toUpperCaseFirstLetter(k, true)) + '=' + esc(this[k]))
            .join('&');
        return queryStr;
    }
}