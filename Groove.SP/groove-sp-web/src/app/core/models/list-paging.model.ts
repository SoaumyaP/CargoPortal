export interface ListPagingViewModel<T> {
    skip: number;
    take: number;
    totalItem: number;
    pageSize: number;
    page: number;
    items: T[];
}