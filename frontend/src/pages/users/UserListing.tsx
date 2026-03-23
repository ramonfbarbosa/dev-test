import React, { Suspense, useEffect, useState } from "react";
import { Helmet } from "react-helmet-async";
import { Button, Card, Form } from "react-bootstrap";
import { useQueryClient } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { Mail } from "react-feather";
import DataTable, { GlobalFilterType } from "@/components/DataTable";
import Pagination from "@/components/Pagination";
import { useDialog } from "@/contexts/DialogContext";
import Loader from "@/components/Loader";
import { NAVIGATION_PATH } from "@/constants";
import { ReactQueryKeys } from "@/constants/ReactQueryKeys";
import UserService from "@/services/UserService";
import { UserListItem } from "@/types/api/User";
import { getUserProfileLabel, userProfileOptions } from "@/types/api/enums/UserProfile";
import { toastr } from "@/utils/toastr";
import { PaginationMeta } from "@/types/api/PagedList";

const DEFAULT_PAGE_SIZE = 10;
const RESEND_COOLDOWN_KEY = "resendEmailCooldowns";
const RESEND_COOLDOWN_MS = 60_000;

function getStoredCooldowns(): Record<string, number> {
  try {
    return JSON.parse(localStorage.getItem(RESEND_COOLDOWN_KEY) || "{}");
  } catch {
    return {};
  }
}

function getActiveCooldownIds(): Set<string> {
  const now = Date.now();
  const active = new Set<string>();
  for (const [id, expiresAt] of Object.entries(getStoredCooldowns())) {
    if (expiresAt > now) active.add(id);
  }
  return active;
}

const ACTIVE_STATUS_OPTIONS = [
  { value: "", label: "Todos" },
  { value: "true", label: "Ativos" },
  { value: "false", label: "Inativos" },
];

const UserListing = () => {
  const { showDialog } = useDialog();
  const queryClient = useQueryClient();

  const [searchInput, setSearchInput] = useState("");
  const [profileInput, setProfileInput] = useState("");
  const [activeInput, setActiveInput] = useState("");
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);
  const [sortBy, setSortBy] = useState("username");
  const [sortDirection, setSortDirection] = useState<"asc" | "desc">("asc");

  const [filters, setFilters] = useState<GlobalFilterType[]>([]);
  const [pagination, setPagination] = useState<PaginationMeta | null>(null);
  const [resendCooldowns, setResendCooldowns] = useState<Set<string>>(() => getActiveCooldownIds());

  useEffect(() => {
    const now = Date.now();
    const stored = getStoredCooldowns();
    for (const [id, expiresAt] of Object.entries(stored)) {
      const remaining = expiresAt - now;
      if (remaining > 0) {
        setTimeout(() => {
          const s = getStoredCooldowns();
          delete s[id];
          localStorage.setItem(RESEND_COOLDOWN_KEY, JSON.stringify(s));
          setResendCooldowns((prev) => {
            const next = new Set(prev);
            next.delete(id);
            return next;
          });
        }, remaining);
      }
    }
  }, []);

  // --- Filter helpers ---

  /** Monta os filtros para o DataTable a partir do estado atual + overrides para valores que acabaram de mudar via setState. */
  function applyFilters(overrides: Record<string, unknown> = {}) {
    const params: Record<string, unknown> = {
      search: searchInput.trim() || undefined,
      profile: profileInput || undefined,
      isActive: activeInput || undefined,
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
    const hasFilters = searchInput.trim() !== "" || profileInput !== "" || activeInput !== "";
    if (!hasFilters) return;

    setSearchInput("");
    setProfileInput("");
    setActiveInput("");
    setPageSize(DEFAULT_PAGE_SIZE);
    setSortBy("username");
    setSortDirection("asc");
    applyFilters({
      search: undefined,
      profile: undefined,
      isActive: undefined,
      page: 1,
      pageSize: DEFAULT_PAGE_SIZE,
      sortBy: "username",
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

  function handleToggleUserActive(user: UserListItem) {
    if (!user.id) return;

    const isActivating = !user.active;

    showDialog({
      title: isActivating ? "Ativar usuário" : "Desativar usuário",
      message: `Deseja realmente ${isActivating ? "ativar" : "desativar"} o usuário ${user.username}?`,
      variant: isActivating ? "success" : "danger",
      actions: [
        { label: "Cancelar", variant: "light" },
        {
          label: isActivating ? "Ativar" : "Desativar",
          variant: isActivating ? "success" : "danger",
          onClick: async () => {
            await UserService.toggleActive(user.id!);
            await queryClient.invalidateQueries({ queryKey: [ReactQueryKeys.USER, "listing"] });
            queryClient.removeQueries({ queryKey: [ReactQueryKeys.USER, user.id] });
            await toastr({
              title: `Usuário ${isActivating ? "ativado" : "desativado"} com sucesso`,
              icon: "success",
            });
          },
        },
      ],
    });
  }

  function handleResendConfirmationEmail(user: UserListItem) {
    if (!user.id) return;

    showDialog({
      title: "Reenviar email de confirmação",
      message: `Deseja reenviar o email de confirmação para ${user.username}?`,
      variant: "warning",
      icon: Mail,
      actions: [
        { label: "Cancelar", variant: "light" },
        {
          label: "Reenviar",
          variant: "warning",
          onClick: async () => {
            await UserService.resendConfirmationEmail(user.id!);
            const stored = getStoredCooldowns();
            stored[user.id!] = Date.now() + RESEND_COOLDOWN_MS;
            localStorage.setItem(RESEND_COOLDOWN_KEY, JSON.stringify(stored));
            setResendCooldowns((prev) => new Set(prev).add(user.id!));
            setTimeout(() => {
              const s = getStoredCooldowns();
              delete s[user.id!];
              localStorage.setItem(RESEND_COOLDOWN_KEY, JSON.stringify(s));
              setResendCooldowns((prev) => {
                const next = new Set(prev);
                next.delete(user.id!);
                return next;
              });
            }, RESEND_COOLDOWN_MS);
            await toastr({
              title: "Email de confirmação reenviado com sucesso",
              icon: "success",
            });
          },
        },
      ],
    });
  }

  // --- Query ---

  async function fetchUsers(globalFilters: GlobalFilterType[]): Promise<UserListItem[]> {
    const get = (name: string) => globalFilters.find((f) => f.name === name)?.value;

    const result = await UserService.getAll({
      search: (get("search") as string) || undefined,
      profile: get("profile") ? Number(get("profile")) : undefined,
      isActive: get("isActive") != null && get("isActive") !== ""
        ? get("isActive") === "true" || get("isActive") === true
        : undefined,
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

  return (
    <React.Fragment>
      <Helmet title="Usuários" />
      <Card>
        <Card.Title></Card.Title>
        <Card.Header style={{ display: "flex", justifyContent: "space-between", alignItems: "center", gap: 10 }}>
          <Card.Title>Usuários</Card.Title>
          <Link to={NAVIGATION_PATH.USERS.CREATE.ABSOLUTE}>
            <Button>Adicionar</Button>
          </Link>
        </Card.Header>

        <div style={{ padding: 20, display: "flex", gap: 10, alignItems: "center", flexWrap: "wrap" }}>
          <input
            type="text"
            className="form-control"
            placeholder="Buscar por nome ou email"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && handleSearch()}
            style={{ maxWidth: 250 }}
          />
          <Form.Select
            value={profileInput}
            onChange={(e) => setProfileInput(e.target.value)}
            style={{ maxWidth: 180 }}
            size="sm"
          >
            <option value="">Todos os perfis</option>
            {userProfileOptions().map((opt) => (
              <option key={opt.id} value={opt.id}>{opt.name}</option>
            ))}
          </Form.Select>
          <Form.Select
            value={activeInput}
            onChange={(e) => setActiveInput(e.target.value)}
            style={{ maxWidth: 160 }}
            size="sm"
          >
            {ACTIVE_STATUS_OPTIONS.map((opt) => (
              <option key={opt.value} value={opt.value}>{opt.label}</option>
            ))}
          </Form.Select>
          <Button variant="primary" onClick={handleSearch}>Buscar</Button>
          <Button variant="secondary" onClick={handleClear}>Limpar</Button>
        </div>

        <Suspense fallback={<><Loader /><br /><br /></>}>
          <DataTable<UserListItem, any>
            thin
            columns={[
              { Header: "Usuário", accessor: "username" },
              { Header: "Email", accessor: "email" },
              {
                Header: "Perfil",
                accessor: "profile",
                Cell: ({ row }) => getUserProfileLabel(row.original.profile),
              },
              {
                Header: "Email confirmado",
                accessor: "emailConfirmed",
                Cell: ({ row }) => (row.original.emailConfirmed ? "Sim" : "Não"),
              },
              {
                Header: "Acesso",
                accessor: "active",
                Cell: ({ row }) => (row.original.active ? "Ativo" : "Inativo"),
              },
              {
                Header: "Ações",
                accessor: "id",
                disableSortBy: true,
                Cell: ({ row }: any) => (
                  <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
                    <Link to={`/usuarios/edit/${row.original.id}`}>
                      <Button variant="outline-primary" size="sm">Editar</Button>
                    </Link>
                    {!row.original.emailConfirmed && !resendCooldowns.has(row.original.id) && (
                      <Button
                        variant="outline-warning"
                        size="sm"
                        onClick={() => handleResendConfirmationEmail(row.original)}
                      >
                        Reenviar Email
                      </Button>
                    )}
                    {(row.original.active || row.original.emailConfirmed) && (
                      <Button
                        variant={row.original.active ? "outline-danger" : "outline-success"}
                        size="sm"
                        onClick={() => handleToggleUserActive(row.original)}
                      >
                        {row.original.active ? "Desativar" : "Ativar"}
                      </Button>
                    )}
                  </div>
                ),
              } as any,
            ]}
            query={fetchUsers}
            fetchButton={false}
            cleanButton={false}
            filters={[]}
            queryName={[ReactQueryKeys.USER, "listing", JSON.stringify(filters)]}
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
    </React.Fragment>
  );
};

export default UserListing;
