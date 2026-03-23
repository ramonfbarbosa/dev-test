export type PaginationMeta = {
    currentPage: number;
    totalPages: number;
    pageSize: number;
    totalCount: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
};

export type PagedList<T> = PaginationMeta & {
    values: T[];
};

export type PagedListWithSummary<T, S> = PaginationMeta & {
    values: T[];
    summary: S;
};