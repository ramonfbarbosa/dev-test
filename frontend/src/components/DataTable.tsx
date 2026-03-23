import React, { ReactNode, useCallback, useEffect, useState } from "react";
import { Column, TableInstance, Row as RowType, useTable, useGlobalFilter, Cell, useRowSelect } from "react-table";
import { Button, Card, Col, Form, Row, Table } from "react-bootstrap";
import { useSuspenseQuery } from "@tanstack/react-query";
import Loader from "./Loader";
import { Link, useNavigate, useSearchParams } from "react-router-dom";

import "@/styles/components/datatable.scss"
import { TextFormField, TextFormFieldProps } from "./form/TextFormField/TextFormField";
import { BaseFilter } from "@/types/api/filters/BaseFilter";
import { toastr } from "@/utils/toastr";
import { errorHandling } from "@/utils/errorHandling";
import { SpeedDial, SpeedDialAction } from "@mui/material";
import { SlOptionsVertical } from "react-icons/sl";

export interface GlobalFilterType {
    name: string,
    value: unknown
}

interface RowSelectionProps<T extends {}> {
    label: string,
    handle: (selectedItems: T[]) => Promise<void>
    icon: ReactNode
}

interface DataTableProps<T extends {}, TFilter> {
    title?: string,
    description?: string,
    thin?: boolean,
    columns: Column<T>[],
    queryName?: string | (string | Date | undefined)[],
    query: (filters: GlobalFilterType[], signal?: AbortSignal) => Promise<T[]>,
    createRouterLink?: string
    hover?: boolean,
    filters?: TextFormFieldProps<TFilter>[];
    summary?: ReactNode;
    reloadButton?: boolean,
    autoReloadTimerInSeconds?: number,
    fetchButton?: boolean,
    fetchButtonVariant?: string,
    fetchButtonName?: string,
    cleanButton?: boolean,
    exportButton?: boolean,
    exportButtonText?: string,
    rowSelectionProps?: RowSelectionProps<T>[],
    rowSelectionShowIfFn?: (row: T) => boolean,
    exportFn?: (filters: any[]) => Promise<{ url: string }>;
    cleanFn?: () => Promise<void>;
    globalFilters?: GlobalFilterType[];
    sortBy?: string;
    sortDirection?: "asc" | "desc";
    onSortChange?: (accessor: string, direction: "asc" | "desc") => void;
}

const IndeterminateCheckbox = React.forwardRef(
    //@ts-ignore
    ({ indeterminate, ...rest }, ref) => {
        const defaultRef = React.useRef()
        const resolvedRef = ref || defaultRef

        React.useEffect(() => {
            //@ts-ignore
            resolvedRef.current.indeterminate = indeterminate
        }, [resolvedRef, indeterminate])

        return (
            //@ts-ignore
            <> <input type="checkbox" ref={resolvedRef} {...rest} /> </>
        )
    }
)

export default function DataTable<T extends {}, TFilter extends BaseFilter>(props: DataTableProps<T, TFilter>) {
    const [searchParams, setSearchParams] = useSearchParams();
    const [internalGlobalFilters, setInternalGlobalFilters] = useState<GlobalFilterType[]>(searchParams.size > 0 ? [...searchParams.entries()].filter(item => item[1]).map(([name, value]) => ({ name, value })) : []);
    const globalFilters = props.globalFilters !== undefined ? props.globalFilters : internalGlobalFilters;

    const [exportLoading, setExportLoading] = useState<boolean>(false);
    const navigate = useNavigate();

    const queryAsync = useCallback(async (globalFilters: GlobalFilterType[], signal?: AbortSignal) => {
        let response = await props.query(globalFilters);
        return response;
    }, [globalFilters])

    const queryKey = [
        ...(props.queryName
            ? Array.isArray(props.queryName)
                ? props.queryName
                : [props.queryName]
            : ["default-query-name"]),
        { globalFilters } as any
    ];

    const handleChangeGlobalFilter = (name: string, value: unknown) => {
        const filters = [...internalGlobalFilters];
        const index = filters.findIndex(f => f.name === name);
        if (index > -1) {
            filters[index].value = value;
        }
        else {
            filters.push({ name, value });
        }
        setInternalGlobalFilters(filters);
        const urlSearchParams = filters
            .filter(item => !!item.value)
            .reduce((acc, curr) => {
                acc[curr.name] = `${curr.value}`;
                return acc;
            }, {} as any);

        setSearchParams(`?${new URLSearchParams(urlSearchParams)}`)
    }

    const { isLoading, isRefetching, refetch, data } = useSuspenseQuery<T[]>({
        queryKey: queryKey,
        meta: { fetchFn: async () => await queryAsync(globalFilters) },
        gcTime: 0,
    });
    if (props.reloadButton && props.autoReloadTimerInSeconds) {
        useEffect(() => {
            if (props.autoReloadTimerInSeconds && props.autoReloadTimerInSeconds > 0) {
                const interval = setInterval(async () => { await refetch() }, props.autoReloadTimerInSeconds * 1000);
                return () => clearInterval(interval);
            }
        }, [props.autoReloadTimerInSeconds]);
    }

    const { selectedFlatRows, ...tableProps } = useTable<T>(
        {
            columns: props.columns as Column<T>[],
            data: data ?? [],
            manualFilters: true,
        },
        useGlobalFilter,
        useRowSelect,
        hooks => {
            props.rowSelectionProps && props.rowSelectionProps.length > 0 &&
                hooks.visibleColumns.push(columns => [
                    {
                        id: 'selection',
                        Header: ({ getToggleAllRowsSelectedProps }) => (
                            //@ts-ignore
                            <div> <IndeterminateCheckbox {...getToggleAllRowsSelectedProps()} /> </div>
                        ),
                        //@ts-ignore
                        Cell: ({ row }) => {
                            if (!props.rowSelectionShowIfFn || props.rowSelectionShowIfFn(row.original)) {
                                return (
                                    <>
                                        <div>
                                            <IndeterminateCheckbox {...row.getToggleRowSelectedProps()} />
                                        </div>
                                    </>
                                )
                            }
                            return (<></>)
                        },

                    },
                    ...columns,
                ])
        }
    );


    const {
        getTableProps,
        getTableBodyProps,
        headerGroups,
        prepareRow,
        rows,
    } = tableProps;

    return (
        <>
            {
                props.createRouterLink &&
                <Row style={{ justifyContent: "end", margin: "10px 0" }}>
                    <Link to={props.createRouterLink}>
                        <Button style={{ maxWidth: "fit-content", float: "right" }}>Adicionar</Button>
                    </Link>
                </Row>
            }
            <Card style={{ boxShadow: "none" }}>
                {(props.title || props.description) && (
                    <Card.Header>
                        {props.title && <Card.Title>{props.title}</Card.Title>}
                        {props.description && <h6 className="card-subtitle text-muted">{props.description}</h6>}
                    </Card.Header>
                )}
                {props.filters && props.filters.filter(item => item.renderIf !== false) && (
                    <Row className="datatable-filters">
                        {props.filters
                            .filter(item => item.renderIf !== false)
                            .map((filter, key) => {
                                return (
                                    <Col md={3} key={key}>
                                        <TextFormField
                                            {...filter}
                                            defaultValue={searchParams.get(filter.name.toString()) ?? filter.defaultValue}
                                            value={globalFilters.find(f => f.name === filter.name)?.value as any ?? null}
                                            handleChange={(event) => {
                                                if (event.hasOwnProperty("target")) {
                                                    handleChangeGlobalFilter(event.target.name, event.target.value)
                                                }
                                                else {
                                                    handleChangeGlobalFilter(event.name, event.value)
                                                }
                                            }}
                                        />
                                    </Col>
                                )
                            })}
                    </Row>
                )}
                {props.filters && (props.cleanButton || props.fetchButton || props.exportButton || props.reloadButton) && (
                    <div style={{ display: "flex", justifyContent: "flex-end", paddingBottom: 15, paddingRight: 20, gap: 10 }}>
                        {props.exportButton && <Button variant="success" onClick={async () => {
                            if (props.exportFn) {

                                try {
                                    setExportLoading(true);

                                    let filters = Array.from(searchParams.entries()).map(([key, value]) => ({
                                        name: key,
                                        value: value
                                    }));

                                    const response = await props.exportFn(filters);
                                    const link = document.createElement('a');
                                    link.href = response.url;
                                    document.body.appendChild(link);
                                    link.click();
                                    link.parentNode?.removeChild(link);
                                    toastr({ icon: 'success', title: 'Exportação realizada com sucesso!' })
                                }
                                catch (error) {
                                    errorHandling(error)
                                }
                                finally {
                                    setExportLoading(false)
                                }
                            }
                            else {
                                toastr({ icon: 'error', title: 'Exportação não implementada!' })

                            }
                        }} disabled={exportLoading} >{exportLoading ? "Exportando..." : props.exportButtonText ?? "Exportar"} </Button>}
                        {props.cleanButton && <Button variant="secondary" onClick={() => {
                            setInternalGlobalFilters([]);
                            setSearchParams("");
                            if (props.cleanFn)
                                props.cleanFn();
                        }}>Limpar</Button>}
                        {props.fetchButton && <Button className={"fetchButton"} variant={props.fetchButtonVariant} onClick={() => refetch()}>{props.fetchButtonName ?? "Buscar"}</Button>}
                        {props.reloadButton && <Button className={"reloadButton"} onClick={() => refetch()}>Atualizar {props.autoReloadTimerInSeconds ? `(${props.autoReloadTimerInSeconds}s)` : ""}</Button>}
                    </div>
                )}
                {props.summary}
                <Table style={{ maxWidth: "100%", overflowX: "auto", whiteSpace: "nowrap" }} responsive bordered striped hover={props.hover ?? true} {...getTableProps()} {...(props.thin ? { size: 'sm' } : {})}>
                    <thead>
                        {headerGroups.map((headerGroup, index) => (
                            <tr {...headerGroup.getHeaderGroupProps()} key={index}>
                                {headerGroup.headers.map((column, columnIndex) => {
                                    const columnId = column.id;
                                    const isSortable = !!props.onSortChange && !(column as any).disableSortBy;
                                    const isSorted = props.sortBy === columnId;

                                    function handleHeaderClick() {
                                        if (!isSortable) return;
                                        const nextDirection = isSorted && props.sortDirection === "asc" ? "desc" : "asc";
                                        props.onSortChange!(columnId, nextDirection);
                                    }

                                    return (
                                        <th
                                            {...column.getHeaderProps()}
                                            key={columnIndex}
                                            onClick={isSortable ? handleHeaderClick : undefined}
                                            style={{
                                                cursor: isSortable ? "pointer" : "default",
                                                userSelect: isSortable ? "none" : undefined,
                                            }}
                                        >
                                            <span style={{ display: "inline-flex", alignItems: "center", gap: 4 }}>
                                                {column.render("Header")}
                                                {isSortable && (
                                                    <span style={{ opacity: isSorted ? 1 : 0.3, fontSize: "0.75em" }}>
                                                        {isSorted && props.sortDirection === "desc" ? "▼" : "▲"}
                                                    </span>
                                                )}
                                            </span>
                                        </th>
                                    );
                                })}
                            </tr>
                        ))}
                    </thead>
                    <tbody {...getTableBodyProps()}>
                        {(isLoading || isRefetching)
                            ? (
                                <tr>
                                    <td colSpan={props.rowSelectionProps ? props.columns.length + 1 : props.columns.length}>
                                        <Loader small />
                                    </td>
                                </tr>
                            )
                            : rows.length === 0 ? (
                                <tr>
                                    <td colSpan={props.rowSelectionProps ? props.columns.length + 1 : props.columns.length} style={{ textAlign: "center" }}>Nenhum registro encontrado</td>
                                </tr>
                            ) : rows.map((row: RowType<any>, index: number) => {
                                prepareRow(row);
                                return (
                                    <tr {...row.getRowProps()} key={index}>
                                        {row.cells.map((cell: Cell<object>, cellIndex) => {
                                            return (
                                                <td {...cell.getCellProps()} key={cellIndex}>{cell.render("Cell")}</td>
                                            );
                                        })}
                                    </tr>
                                );
                            })}
                    </tbody>
                </Table>
            </Card>
            {props.rowSelectionProps
                && props.rowSelectionProps.length > 0
                && selectedFlatRows
                && selectedFlatRows.length > 0
                && <SelectRowActions actions={props.rowSelectionProps} selectedIds={selectedFlatRows.map(row => (row.original as any))} />
            }
        </>
    )
}

const SelectRowActions = ({ actions, selectedIds }: { actions: RowSelectionProps<any>[], selectedIds: string[] }) => {
    return (
        <SpeedDial
            ariaLabel="Row Selection"
            sx={{ position: "fixed", bottom: "1.5rem", right: "1.5rem" }}
            icon={<SlOptionsVertical size={20} />}
            FabProps={{
                sx: {
                    bgcolor: '#4F46E5',
                    '&:hover': {
                        bgcolor: '#4F46E5',
                    }
                }
            }}
        >
            {actions.map((action, index) => <SpeedDialAction
                tooltipOpen
                style={{ whiteSpace: "nowrap" }}
                key={index}
                icon={action.icon}
                tooltipTitle={action.label}
                onClick={() => { action.handle(selectedIds) }}
            />)}


        </SpeedDial>
    );
}

export { IndeterminateCheckbox, SelectRowActions };
export type DataTableType = typeof DataTable;
