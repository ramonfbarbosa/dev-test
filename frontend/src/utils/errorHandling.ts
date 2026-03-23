import { ApiError, ApiErrorItem } from "@/types/ApiError";
import { toastr } from "./toastr";

type NormalizedApiErrorItem = {
    key: string;
    value: string;
}

type NormalizedApiError = {
    message?: string;
    errors: NormalizedApiErrorItem[];
}

export function getApiErrorDetails(err: unknown): NormalizedApiError {
    const apiError = err as ApiError;
    const rawErrors = apiError?.errors ?? apiError?.Errors ?? apiError?.items ?? [];
    const errors = rawErrors
        .map(normalizeApiErrorItem)
        .filter(item => !!item.key || !!item.value);

    return {
        message: apiError?.message ?? apiError?.Message ?? errors[0]?.value,
        errors,
    };
}

export function normalizeApiErrorFieldPath(key: string): string {
    return key
        .replace(/^\$\./, "")
        .split(".")
        .filter((item, index) => !(index === 0 && item.toLowerCase() === "request"))
        .map(item => item ? `${item.charAt(0).toLowerCase()}${item.slice(1)}` : item)
        .join(".");
}

export function errorHandling(err: any, message?: string, title?: string) {
    const apiError = getApiErrorDetails(err);
    let errorMessage = message ?? apiError.message;
    if (!errorMessage)
        errorMessage = typeof err === "string" ? err : "Ocorreu um erro inesperado. Por favor tente novamente"

    const timer = calculateToastTime(errorMessage);

    void toastr({
        title: title ?? "A requisição falhou!",
        text: message ?? errorMessage,
        icon: "error",
        timer
    });
}

function calculateToastTime(text: string): number {
    const palavras = text.trim().split(/\s+/).length;
    const tempoCalculado = palavras * 267;

    return Math.min(Math.max(tempoCalculado, 3000), 10000);
}

function normalizeApiErrorItem(item: ApiErrorItem): NormalizedApiErrorItem {
    return {
        key: item?.key ?? item?.Key ?? "",
        value: item?.value ?? item?.Value ?? "",
    };
}
