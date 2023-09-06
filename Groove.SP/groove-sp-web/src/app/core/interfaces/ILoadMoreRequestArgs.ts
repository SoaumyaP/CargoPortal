interface ILoadMoreRequestArgs {
    skip: number;
    take: number;
    loadedRecordCount: number;
    maximumRecordCount: number;
    loadingData: boolean;
}
