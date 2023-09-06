import moment from "moment";
import { StringHelper } from "src/app/core";

export class POProgressCheckModel {
    id: number;
    poNumber: string;
    cargoReadyDate: string | null;
    productionStarted: boolean;
    qcRequired: boolean;
    shortShip: boolean;
    splitShipment: boolean;
    proposeDate: string | null;
    remark: string;
}

export class POProgressCheckQueryModel {
    SelectedCustomerId: number;
    SelectedSupplierId: number | null;
    PONoFrom: string | null;
    PONoTo: string | null;
    CargoReadyDateFrom: string | null;
    CargoReadyDateTo: string | null;
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

    public convertToQueryDateString(date: Date) {
        return moment(date).format('YYYY-MM-DD');
    }
}

export class POProgressCheckFilterModel {
    selectedSupplier: string;
    selectedCustomerId: number;
    selectedSupplierId: number;
    poNoFrom: string;
    poNoTo: string;
    cargoReadyDateFrom: Date;
    cargoReadyDateTo: Date;
}