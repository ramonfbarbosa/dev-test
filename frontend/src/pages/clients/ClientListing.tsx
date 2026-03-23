import React, { Suspense, useRef, useState } from "react";
import { Button, Card } from "react-bootstrap";
import { Link } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import { Trash2 } from "react-feather";
import DataTable, { GlobalFilterType } from "@/components/DataTable";
import Pagination from "@/components/Pagination";
import Loader from "@/components/Loader";
import { NAVIGATION_PATH } from "@/constants";
import { ReactQueryKeys } from "@/constants/ReactQueryKeys";
import { Client } from "@/types/api/Client";
import { PaginationMeta } from "@/types/api/PagedList";
import ClientService from "@/services/ClientService";
import { format } from "@/helpers/format";
import { errorHandling } from "@/utils/errorHandling";
import { toastr } from "@/utils/toastr";
import { useDialog } from "@/contexts/DialogContext";

const DEFAULT_PAGE_SIZE = 10;

const ClientListing = () => {
    const fileInputRef = useRef<HTMLInputElement>(null);
    const { showDialog } = useDialog();
    const queryClient = useQueryClient();

    const [documentInput, setDocumentInput] = useState("");
    const [isImporting, setIsImporting] = useState(false);
    const [isExporting, setIsExporting] = useState(false);
    const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);
    const [sortBy, setSortBy] = useState("firstName");
    const [sortDirection, setSortDirection] = useState<"asc" | "desc">("asc");

    const [filters, setFilters] = useState<GlobalFilterType[]>([]);
    const [pagination, setPagination] = useState<PaginationMeta | null>(null);

    // --- Formatters ---

    function formatPhoneNumber(phoneNumber?: string) {
        const digits = phoneNumber?.replace(/\D/g, "") ?? "";
        const mask = digits.length > 10 ? "(##) #####-####" : "(##) ####-####";
        return format.toMask(digits, mask);
    }

    function formatDocumentNumber(documentNumber?: string) {
        const digits = documentNumber?.replace(/\D/g, "") ?? "";

        if (digits.length === 14) return format.toMask(digits, "##.###.###/####-##");
        if (digits.length === 11) return format.toMask(digits, "###.###.###-##");
        return documentNumber ?? "";
    }

    // --- CSV import ---

    function handleImportClick() {
        if (!fileInputRef.current) return;
        fileInputRef.current.value = "";
        fileInputRef.current.click();
    }

    async function handleImportChange(event: React.ChangeEvent<HTMLInputElement>) {
        const file = event.target.files?.[0];
        if (!file) return;

        try {
            setIsImporting(true);
            const response = await ClientService.importCsv(file);
            await toastr({
                title: "Importação iniciada",
                text: response.message ?? response.Message ?? "Arquivo enviado com sucesso. A importação será processada em background.",
                icon: "success",
            });
        } catch (error) {
            errorHandling(error);
        } finally {
            setIsImporting(false);
            event.target.value = "";
        }
    }

    async function handleExportClick() {
        try {
            setIsExporting(true);
            await ClientService.exportCsv();
        } catch (error) {
            errorHandling(error);
        } finally {
            setIsExporting(false);
        }
    }

    // --- Filter helpers ---

    function applyFilters(overrides: Record<string, unknown> = {}) {
        const params: Record<string, unknown> = {
            document: documentInput.trim() || undefined,
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

    // --- Event handlers ---

    function handleSearch() {
        applyFilters();
    }

    function handleClear() {
        const hasFilters = documentInput.trim() !== "";
        if (!hasFilters) return;

        setDocumentInput("");
        setPageSize(DEFAULT_PAGE_SIZE);
        setSortBy("firstName");
        setSortDirection("asc");
        applyFilters({
            document: undefined,
            page: 1,
            pageSize: DEFAULT_PAGE_SIZE,
            sortBy: "firstName",
            sortDirection: "asc",
        });
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

    function handleDeleteClient(client: Client) {
        if (!client.id) return;

        showDialog({
            title: "Excluir cliente",
            message: `Deseja realmente excluir o cliente ${client.firstName} ${client.lastName}?`,
            variant: "danger",
            icon: Trash2,
            actions: [
                { label: "Cancelar", variant: "light" },
                {
                    label: "Excluir",
                    variant: "danger",
                    onClick: async () => {
                        await ClientService.delete(client.id!);
                        await queryClient.invalidateQueries({ queryKey: [ReactQueryKeys.CLIENT, "listing"] });
                        queryClient.removeQueries({ queryKey: [ReactQueryKeys.CLIENT, client.id] });
                        await toastr({ title: "Cliente excluído com sucesso", icon: "success" });
                    },
                },
            ],
        });
    }

    // --- Query ---

    async function fetchClients(globalFilters: GlobalFilterType[]): Promise<Client[]> {
        const get = (name: string) => globalFilters.find((f) => f.name === name)?.value;

        const result = await ClientService.getAll({
            documentNumber: get("document")?.toString().trim() || undefined,
            page: (get("page") as number) || 1,
            pageSize: (get("pageSize") as number) || pageSize,
            sortBy: (get("sortBy") as string) || sortBy,
            sortDirection: (get("sortDirection") as string) || sortDirection,
        });

        const { values, ...meta } = result;
        setPagination(meta);
        return values;
    }

    // --- Render ---

    return <>
        <input
            ref={fileInputRef}
            type="file"
            accept=".csv,text/csv"
            style={{ display: "none" }}
            onChange={handleImportChange}
        />
        <Card>
            <Card.Title></Card.Title>
            <Card.Header style={{ display: "flex", justifyContent: "space-between", alignItems: "center", gap: 10 }}>
                <Card.Title>Clientes</Card.Title>
                <div style={{ display: "flex", gap: 10, flexWrap: "wrap", justifyContent: "flex-end" }}>
                    <Button
                        variant="outline-secondary"
                        onClick={handleExportClick}
                        disabled={isExporting || !pagination?.totalCount}
                    >
                        {isExporting ? "Exportando..." : "Exportar CSV"}
                    </Button>
                    <Button
                        variant="outline-secondary"
                        onClick={handleImportClick}
                        disabled={isImporting}
                    >
                        {isImporting ? "Enviando CSV..." : "Importar CSV"}
                    </Button>
                    <Link to={NAVIGATION_PATH.CLIENTS.CREATE.ABSOLUTE}>
                        <Button>Adicionar</Button>
                    </Link>
                </div>
            </Card.Header>

            <div style={{ padding: 20, display: "flex", gap: 10, alignItems: "center" }}>
                <input
                    type="text"
                    className="form-control"
                    placeholder="Buscar por documento"
                    value={documentInput}
                    onChange={(e) => setDocumentInput(e.target.value)}
                    onKeyDown={(e) => e.key === "Enter" && handleSearch()}
                    style={{ maxWidth: 250 }}
                />
                <Button variant="primary" onClick={handleSearch}>Buscar</Button>
                <Button variant="secondary" onClick={handleClear}>Limpar</Button>
            </div>

            <Suspense fallback={<><Loader /><br /><br /></>}>
                <DataTable<Client, any>
                    thin
                    columns={[
                        { Header: "Nome", accessor: "firstName" },
                        { Header: "Sobrenome", accessor: "lastName" },
                        { Header: "Data de Nascimento", accessor: "birthDate" },
                        { Header: "Email", accessor: "email" },
                        {
                            Header: "Telefone",
                            accessor: "phoneNumber",
                            Cell: ({ row }) => formatPhoneNumber(row.original.phoneNumber),
                        },
                        {
                            Header: "Documento",
                            accessor: "documentNumber",
                            Cell: ({ row }) => formatDocumentNumber(row.original.documentNumber),
                        },
                        {
                            Header: "Ações",
                            accessor: "id",
                            disableSortBy: true,
                            Cell: ({ row }: any) => (
                                <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
                                    <Link to={`/clientes/edit/${row.original.id}`}>
                                        <Button variant="outline-primary" size="sm">Editar</Button>
                                    </Link>
                                    <Button
                                        variant="outline-danger"
                                        size="sm"
                                        onClick={() => handleDeleteClient(row.original)}
                                    >
                                        Excluir
                                    </Button>
                                </div>
                            ),
                        } as any,
                    ]}
                    query={fetchClients}
                    fetchButton={false}
                    cleanButton={false}
                    filters={[]}
                    queryName={[ReactQueryKeys.CLIENT, "listing", JSON.stringify(filters)]}
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
    </>;
};

export default ClientListing;
