export interface ListResultModel<T> {
    totalRecords?: number;
    page?: number;
    pageSize?: number;
    order?: string;
    direction?: string;
    records?: T[];
}
