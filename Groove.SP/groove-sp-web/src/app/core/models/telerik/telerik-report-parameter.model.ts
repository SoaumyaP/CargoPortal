export interface TelerikReportParameterModel {
    allowBlank: boolean;
    allowNull: boolean;
    autoRefresh: boolean;
    /**
     * Raw data source returned from report server
     */
    availableValues?: Array<any>;
    /**
     * Data source used to filter
     */
    filteredAvailableValues?: Array<any>;
    childParameters?: Array<any>;
    hasChildParameters: boolean;
    id: string;
    isVisible: boolean;
    label: Array<any>|any;
    multivalue: boolean;
    name: string;
    text: string;
    type: string;
    value: Array<any>|any;
    isDisabled: boolean;
    isHidden: boolean;
}
