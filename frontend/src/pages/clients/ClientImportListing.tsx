import React, { Suspense, useState } from "react";
import { Badge, Button, Card, Table, Spinner } from "react-bootstrap";
import { Link } from "react-router-dom";
import { AlertCircle } from "react-feather";
import DataTable, { GlobalFilterType } from "@/components/DataTable";
import Pagination from "@/components/Pagination";
import CustomModal from "@/components/CustomModal";
import Loader from "@/components/Loader";
import { NAVIGATION_PATH } from "@/constants";
import { ReactQueryKeys } from "@/constants/ReactQueryKeys";
import { ClientImport, ClientImportErrors } from "@/types/api/ClientImport";
import { PaginationMeta } from "@/types/api/PagedList";
import ClientService from "@/services/ClientService";
import { dateUtils } from "@/helpers/date";

const DEFAULT_PAGE_SIZE = 10;

function statusBadge(status: number, statusText: string) {
    const variants: Record<number, string> = {
        0: "warning",
        1: "info",
        2: "success",
        3: "danger",
        4: "warning",
    };
    return <Badge bg={variants[status] ?? "secondary"}>{statusText}</Badge>;
}

const ClientImportListing = () => {
    const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);
    const [sortBy, setSortBy] = useState("createdAt");
    const [sortDirection, setSortDirection] = useState<"asc" | "desc">("desc");
    const [filters, setFilters] = useState<GlobalFilterType[]>([]);
    const [pagination, setPagination] = useState<PaginationMeta | null>(null);

    const [showErrorModal, setShowErrorModal] = useState(false);
    const [errorDetails, setErrorDetails] = useState<ClientImportErrors | null>(null);
    const [loadingErrors, setLoadingErrors] = useState(false);

    function applyFilters(overrides: Record<string, unknown> = {}) {
        const params: Record<string, unknown> = {
            page: 1,
            pageSize,
            sortBy,
            sortDirection,
            ...overrides,
        };
        setFilters(
            Object.entries(params)
                .filter(([, v]) => v !== undefined)
                .map(([name, value]) => ({ name, value })),
        );
    }

    function handlePageChange(page: number) {
        applyFilters({ page });
    }

    function handlePageSizeChange(newPageSize: number) {
        setPageSize(newPageSize);
        applyFilters({ page: 1, pageSize: newPageSize });
    }

    function handleSortChange(column: string, direction: "asc" | "desc") {
        setSortBy(column);
        setSortDirection(direction);
        applyFilters({ page: 1, sortBy: column, sortDirection: direction });
    }

    async function fetchImports(globalFilters: GlobalFilterType[]): Promise<ClientImport[]> {
        const get = (name: string) => globalFilters.find((f) => f.name === name)?.value;

        const result = await ClientService.getImports({
            page: (get("page") as number) || 1,
            pageSize: (get("pageSize") as number) || pageSize,
            sortBy: (get("sortBy") as string) || sortBy,
            sortDirection: (get("sortDirection") as string) || sortDirection,
        });

        const { values, ...meta } = result;
        setPagination(meta);
        return values;
    }

    async function handleViewErrors(importItem: ClientImport) {
        setShowErrorModal(true);
        setLoadingErrors(true);
        setErrorDetails(null);
        try {
            const details = await ClientService.getImportErrors(importItem.id);
            setErrorDetails(details);
        } catch {
            setErrorDetails(null);
        } finally {
            setLoadingErrors(false);
        }
    }

    return <>
        <Card>
            <Card.Header style={{ display: "flex", justifyContent: "space-between", alignItems: "center", gap: 10 }}>
                <Card.Title>Importações de clientes</Card.Title>
                <Link to={NAVIGATION_PATH.CLIENTS.LISTING.ABSOLUTE}>
                    <Button variant="outline-secondary">Voltar para clientes</Button>
                </Link>
            </Card.Header>

            <Suspense fallback={<><Loader /><br /><br /></>}>
                <DataTable<ClientImport, any>
                    thin
                    columns={[
                        {
                            Header: "Arquivo",
                            accessor: "originalFileName",
                        },
                        {
                            Header: "Status",
                            accessor: "statusText",
                            Cell: ({ row }: any) => statusBadge(row.original.status, row.original.statusText),
                        },
                        {
                            Header: "Enviado por",
                            accessor: "uploadedByUserName",
                        },
                        {
                            Header: "Enviado em",
                            accessor: "createdAt",
                            Cell: ({ row }: any) => dateUtils.formatDateTime(row.original.createdAt),
                        },
                        {
                            Header: "Total",
                            accessor: "totalRows",
                            disableSortBy: true,
                        },
                        {
                            Header: "Importados",
                            accessor: "importedRows",
                            disableSortBy: true,
                        },
                        {
                            Header: "Erros",
                            accessor: "failureCount",
                            disableSortBy: true,
                            Cell: ({ row }: any) => {
                                const item = row.original as ClientImport;
                                if (item.failureCount > 0) {
                                    return (
                                        <Button
                                            variant="link"
                                            className="text-danger fw-bold p-0"
                                            onClick={() => handleViewErrors(item)}
                                        >
                                            {item.failureCount} <AlertCircle size={14} />
                                        </Button>
                                    );
                                }
                                if (item.status === 3 || item.status === 4 /* Failed or ProcessedWithErrors */) {
                                    return (
                                        <Button
                                            variant="link"
                                            className="text-danger p-0"
                                            onClick={() => handleViewErrors(item)}
                                        >
                                            Ver erro
                                        </Button>
                                    );
                                }
                                return <span>{item.failureCount}</span>;
                            },
                        },
                        {
                            Header: "Finalizado em",
                            accessor: "finishedAt",
                            disableSortBy: true,
                            Cell: ({ row }: any) => dateUtils.formatDateTime(row.original.finishedAt),
                        },
                    ]}
                    query={fetchImports}
                    fetchButton={false}
                    cleanButton={false}
                    filters={[]}
                    queryName={[ReactQueryKeys.CLIENT_IMPORT, "listing", JSON.stringify(filters)]}
                    globalFilters={filters}
                    sortBy={sortBy}
                    sortDirection={sortDirection}
                    onSortChange={handleSortChange}
                />
            </Suspense>

            {pagination && (
                <Pagination
                    currentPage={pagination.currentPage}
                    totalPages={pagination.totalPages}
                    totalCount={pagination.totalCount}
                    pageSize={pagination.pageSize}
                    onPageChange={handlePageChange}
                    onPageSizeChange={handlePageSizeChange}
                />
            )}
        </Card>

        <CustomModal
            show={showErrorModal}
            size="lg"
            onHide={() => setShowErrorModal(false)}
            header={{ title: errorDetails ? `Erros — ${errorDetails.originalFileName}` : "Detalhes dos erros" }}
            footer={{ actions: [{ label: "Fechar", variant: "secondary", handler: () => setShowErrorModal(false) }] }}
        >
            {loadingErrors && (
                <div className="text-center py-4">
                    <Spinner animation="border" />
                    <p className="mt-2 text-muted">Carregando erros...</p>
                </div>
            )}

            {!loadingErrors && errorDetails && (
                <>
                    <div className="d-flex gap-4 mb-3">
                        <span><strong>Status:</strong> {errorDetails.statusText}</span>
                        <span><strong>Total:</strong> {errorDetails.totalRows}</span>
                        <span><strong>Importados:</strong> {errorDetails.importedRows}</span>
                        <span className="text-danger"><strong>Erros:</strong> {errorDetails.failureCount}</span>
                    </div>

                    {errorDetails.errors.length > 0 ? (
                        <div style={{ maxHeight: 400, overflowY: "auto" }}>
                            <Table striped bordered size="sm">
                                <thead>
                                    <tr>
                                        <th style={{ width: 80 }}>Linha</th>
                                        <th>Mensagem</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {errorDetails.errors.map((err, idx) => (
                                        <tr key={idx}>
                                            <td>{err.lineNumber > 0 ? err.lineNumber : "—"}</td>
                                            <td>{err.message}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </Table>
                        </div>
                    ) : (
                        <p className="text-muted">Nenhum detalhe de erro disponível.</p>
                    )}
                </>
            )}

            {!loadingErrors && !errorDetails && (
                <p className="text-muted">Não foi possível carregar os detalhes dos erros.</p>
            )}
        </CustomModal>
    </>;
};

export default ClientImportListing;
