import React, { useEffect, useState } from "react";
import { Helmet } from "react-helmet-async";
import { Card, Col, Row, Table, Badge, Spinner } from "react-bootstrap";
import Chart from "react-apexcharts";
import { Users, UserCheck, MapPin, Briefcase, Calendar, Mail } from "react-feather";
import DashboardService from "@/services/DashboardService";
import { DashboardData } from "@/types/api/Dashboard";
import usePalette from "@/hooks/usePalette";
import { dateUtils } from "@/helpers/date";

// --- KPI Card ---

interface KpiCardProps {
    title: string;
    value: string | number;
    subtitle?: string;
    icon: React.ReactNode;
    color: string;
    iconColor?: string;
}

const KpiCard: React.FC<KpiCardProps> = ({ title, value, subtitle, icon, color, iconColor }) => (
    <Card className="flex-fill">
        <Card.Body>
            <Row>
                <Col className="mt-0">
                    <h5 className="card-title text-muted">{title}</h5>
                </Col>
                <Col xs="auto">
                    <div
                        className="stat"
                        style={{
                            backgroundColor: color,
                            color: iconColor ?? "#fff",
                            width: 48,
                            height: 48,
                            borderRadius: "50%",
                            display: "flex",
                            alignItems: "center",
                            justifyContent: "center",
                        }}
                    >
                        {icon}
                    </div>
                </Col>
            </Row>
            <h1 className="mt-1 mb-1">{value}</h1>
            {subtitle && <div className="mb-0 text-muted"><small>{subtitle}</small></div>}
        </Card.Body>
    </Card>
);

// --- Chart configs ---

type Palette = ReturnType<typeof usePalette>;

function buildClientsByStateChart(data: DashboardData, palette: Palette) {
    const categories = data.clientsByState.map((item) => item.state);
    const values = data.clientsByState.map((item) => item.count);

    return {
        options: {
            chart: { id: "clients-by-state", toolbar: { show: false }, foreColor: palette.black },
            plotOptions: { bar: { horizontal: true, borderRadius: 4 } },
            xaxis: { categories },
            colors: [palette.primary],
            dataLabels: { enabled: true },
        },
        series: [{ name: "Clientes", data: values }],
    };
}

function buildUsersByProfileChart(data: DashboardData, palette: Palette) {
    const labels = data.usersByProfile.map((item) =>
        item.profile === "Administrator" ? "Administrador" : "Operador",
    );
    const values = data.usersByProfile.map((item) => item.count);

    return {
        options: {
            chart: { id: "users-by-profile", foreColor: palette.black },
            labels,
            colors: [palette["primary-dark"], palette.info],
            legend: { position: "bottom" as const },
        },
        series: values,
    };
}

function buildNewClientsPerMonthChart(data: DashboardData, palette: Palette) {
    const categories = data.newClientsPerMonth.map((item) => {
        const [year, month] = item.month.split("-");
        const monthNames = ["Jan", "Fev", "Mar", "Abr", "Mai", "Jun", "Jul", "Ago", "Set", "Out", "Nov", "Dez"];
        return `${monthNames[parseInt(month) - 1]}/${year.slice(2)}`;
    });
    const values = data.newClientsPerMonth.map((item) => item.count);

    return {
        options: {
            chart: { id: "new-clients-per-month", toolbar: { show: false }, foreColor: palette.black },
            xaxis: { categories },
            colors: [palette.success],
            stroke: { curve: "smooth" as const, width: 3 },
            dataLabels: { enabled: false },
            fill: {
                type: "gradient",
                gradient: { shadeIntensity: 1, opacityFrom: 0.45, opacityTo: 0.05, stops: [50, 100] },
            },
        },
        series: [{ name: "Novos Clientes", data: values }],
    };
}

// --- Formatters ---

function formatDocument(doc: string): string {
    const digits = doc?.replace(/\D/g, "") ?? "";
    if (digits.length === 11) return digits.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, "$1.$2.$3-$4");
    if (digits.length === 14) return digits.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, "$1.$2.$3/$4-$5");
    return doc;
}

function formatPhone(phone: string): string {
    const digits = phone?.replace(/\D/g, "") ?? "";
    if (digits.length === 11) return digits.replace(/(\d{2})(\d{5})(\d{4})/, "($1) $2-$3");
    if (digits.length === 10) return digits.replace(/(\d{2})(\d{4})(\d{4})/, "($1) $2-$3");
    return phone;
}

// --- Page ---

const DashboardPage: React.FC = () => {
    const [data, setData] = useState<DashboardData | null>(null);
    const [loading, setLoading] = useState(true);
    const palette = usePalette();

    useEffect(() => {
        DashboardService.getData()
            .then(setData)
            .finally(() => setLoading(false));
    }, []);

    if (loading) {
        return (
            <div className="text-center py-5">
                <Spinner animation="border" role="status" />
                <p className="mt-2 text-muted">Carregando dashboard...</p>
            </div>
        );
    }

    if (!data) {
        return <p className="text-muted text-center py-5">Não foi possível carregar os dados do dashboard.</p>;
    }

    const activePercent = data.totalUsers > 0 ? Math.round((data.activeUsers / data.totalUsers) * 100) : 0;
    const confirmedPercent = data.totalUsers > 0 ? Math.round((data.usersWithConfirmedEmail / data.totalUsers) * 100) : 0;

    const clientsByState = buildClientsByStateChart(data, palette);
    const usersByProfile = buildUsersByProfileChart(data, palette);
    const newClientsPerMonth = buildNewClientsPerMonthChart(data, palette);

    return (
        <React.Fragment>
            <Helmet title="Dashboard" />
            <h1 className="h3 mb-3">Dashboard</h1>

            {/* --- KPI Cards --- */}
            <Row>
                <Col sm={6} lg={4} className="d-flex">
                    <KpiCard
                        title="Total de Clientes"
                        value={data.totalClients}
                        subtitle={`${data.clientsThisMonth} cadastrados este mês`}
                        icon={<Briefcase size={24} />}
                        color={palette.primary}
                    />
                </Col>
                <Col sm={6} lg={4} className="d-flex">
                    <KpiCard
                        title="Clientes este Mês"
                        value={data.clientsThisMonth}
                        subtitle="Últimos 30 dias"
                        icon={<Calendar size={24} />}
                        color={palette.success}
                    />
                </Col>
                <Col sm={6} lg={4} className="d-flex">
                    <KpiCard
                        title="Estados Distintos"
                        value={data.distinctStates}
                        subtitle="Distribuição geográfica"
                        icon={<MapPin size={24} />}
                        color={palette.info}
                    />
                </Col>
            </Row>
            <Row>
                <Col sm={6} lg={4} className="d-flex">
                    <KpiCard
                        title="Total de Usuários"
                        value={data.totalUsers}
                        icon={<Users size={24} />}
                        color={palette["gray-100"]}
                        iconColor={palette.secondary}
                    />
                </Col>
                <Col sm={6} lg={4} className="d-flex">
                    <KpiCard
                        title="Usuários Ativos"
                        value={data.activeUsers}
                        subtitle={`${activePercent}% do total`}
                        icon={<UserCheck size={24} />}
                        color={palette.warning}
                    />
                </Col>
                <Col sm={6} lg={4} className="d-flex">
                    <KpiCard
                        title="Emails Confirmados"
                        value={data.usersWithConfirmedEmail}
                        subtitle={`${confirmedPercent}% do total`}
                        icon={<Mail size={24} />}
                        color={palette.danger}
                    />
                </Col>
            </Row>

            {/* --- Charts --- */}
            <Row>
                <Col lg={8} className="d-flex">
                    <Card className="flex-fill">
                        <Card.Header><Card.Title>Novos Clientes por Mês</Card.Title></Card.Header>
                        <Card.Body>
                            {data.newClientsPerMonth.length > 0 ? (
                                <Chart
                                    options={newClientsPerMonth.options}
                                    series={newClientsPerMonth.series}
                                    type="area"
                                    height={300}
                                />
                            ) : (
                                <p className="text-muted text-center">Nenhum dado disponível</p>
                            )}
                        </Card.Body>
                    </Card>
                </Col>
                <Col lg={4} className="d-flex">
                    <Card className="flex-fill">
                        <Card.Header><Card.Title>Usuários por Perfil</Card.Title></Card.Header>
                        <Card.Body>
                            {data.usersByProfile.length > 0 ? (
                                <Chart
                                    options={usersByProfile.options}
                                    series={usersByProfile.series}
                                    type="donut"
                                    height={300}
                                />
                            ) : (
                                <p className="text-muted text-center">Nenhum dado disponível</p>
                            )}
                        </Card.Body>
                    </Card>
                </Col>
            </Row>
            <Row>
                <Col xs={12}>
                    <Card>
                        <Card.Header><Card.Title>Clientes por Estado (Top 10)</Card.Title></Card.Header>
                        <Card.Body>
                            {data.clientsByState.length > 0 ? (
                                <Chart
                                    options={clientsByState.options}
                                    series={clientsByState.series}
                                    type="bar"
                                    height={Math.max(200, data.clientsByState.length * 40)}
                                />
                            ) : (
                                <p className="text-muted text-center">Nenhum dado disponível</p>
                            )}
                        </Card.Body>
                    </Card>
                </Col>
            </Row>

            {/* --- Tables --- */}
            <Row>
                <Col lg={7}>
                    <Card>
                        <Card.Header><Card.Title>Últimos Clientes Cadastrados</Card.Title></Card.Header>
                        <Card.Body style={{ overflowX: "auto" }}>
                            <Table striped bordered hover size="sm">
                                <thead>
                                    <tr>
                                        <th>Nome</th>
                                        <th>Email</th>
                                        <th>Documento</th>
                                        <th>Telefone</th>
                                        <th>Cidade/UF</th>
                                        <th>Cadastrado em</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {data.recentClients.length === 0 && (
                                        <tr><td colSpan={6} className="text-center text-muted">Nenhum cliente cadastrado</td></tr>
                                    )}
                                    {data.recentClients.map((client) => (
                                        <tr key={client.id}>
                                            <td>{client.fullName}</td>
                                            <td>{client.email}</td>
                                            <td>{formatDocument(client.documentNumber)}</td>
                                            <td>{formatPhone(client.phoneNumber)}</td>
                                            <td>{client.cityState}</td>
                                            <td>{dateUtils.formatDate(client.createdAt)}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </Table>
                        </Card.Body>
                    </Card>
                </Col>
                <Col lg={5}>
                    <Card>
                        <Card.Header><Card.Title>Últimos Usuários Cadastrados</Card.Title></Card.Header>
                        <Card.Body style={{ overflowX: "auto" }}>
                            <Table striped bordered hover size="sm">
                                <thead>
                                    <tr>
                                        <th>Usuário</th>
                                        <th>Email</th>
                                        <th>Perfil</th>
                                        <th>Email</th>
                                        <th>Status</th>
                                        <th>Cadastrado em</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {data.recentUsers.length === 0 && (
                                        <tr><td colSpan={6} className="text-center text-muted">Nenhum usuário cadastrado</td></tr>
                                    )}
                                    {data.recentUsers.map((user) => (
                                        <tr key={user.id}>
                                            <td>{user.username}</td>
                                            <td>{user.email}</td>
                                            <td>{user.profile === "Administrator" ? "Admin" : "Operador"}</td>
                                            <td>
                                                <Badge bg={user.emailConfirmed ? "success" : "warning"}>
                                                    {user.emailConfirmed ? "Confirmado" : "Pendente"}
                                                </Badge>
                                            </td>
                                            <td>
                                                <Badge bg={user.active ? "success" : "danger"}>
                                                    {user.active ? "Ativo" : "Inativo"}
                                                </Badge>
                                            </td>
                                            <td>{dateUtils.formatDate(user.createdAt)}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </Table>
                        </Card.Body>
                    </Card>
                </Col>
            </Row>
        </React.Fragment>
    );
};

export default DashboardPage;
