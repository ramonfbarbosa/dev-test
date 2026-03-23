export interface DashboardData {
    totalClients: number;
    clientsThisMonth: number;
    distinctStates: number;
    totalUsers: number;
    activeUsers: number;
    usersWithConfirmedEmail: number;
    clientsByState: ClientsByStateItem[];
    usersByProfile: UsersByProfileItem[];
    newClientsPerMonth: NewClientsPerMonthItem[];
    recentClients: RecentClientItem[];
    recentUsers: RecentUserItem[];
}

export interface ClientsByStateItem {
    state: string;
    count: number;
}

export interface UsersByProfileItem {
    profile: string;
    count: number;
}

export interface NewClientsPerMonthItem {
    month: string;
    count: number;
}

export interface RecentClientItem {
    id: string;
    fullName: string;
    email: string;
    documentNumber: string;
    phoneNumber: string;
    cityState: string;
    createdAt: string;
}

export interface RecentUserItem {
    id: string;
    username: string;
    email: string;
    profile: string;
    emailConfirmed: boolean;
    active: boolean;
    createdAt: string;
}
