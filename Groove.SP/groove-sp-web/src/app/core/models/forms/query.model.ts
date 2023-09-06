import { DateHelper } from '../../helpers';
import moment from 'moment';

export class QueryModel {

    //#region filter-props: Please set the shared filter-props here!

    //#endregion

    /**
     * CONSTRUCTOR
     */
    constructor() { }

    /**
     * Build filter-data model to URI Query Params
     */
    public get buildToQueryParams(): string {
        let queryStr = '';
        let esc = encodeURIComponent;
        queryStr += Object.keys(this)
            .filter(k => this[k] != null)
            .map(k => esc(k) + '=' + esc(this[k]))
            .join('&');
        return queryStr;
    }

    protected convertToQueryDateString(date: Date) {
        return moment(date).format('YYYY-MM-DD');
    }

}
