export type ApiError = {
    message?: string;
    Message?: string;
    errors?: ApiErrorItem[];
    Errors?: ApiErrorItem[];
    items?: ApiErrorItem[];
    status?: number;
}

export type ApiErrorItem = {
    key?: string;
    Key?: string;
    value?: string;
    Value?: string;
}
