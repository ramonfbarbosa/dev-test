import { SidebarItemsType } from "@/types/sidebar";
import { NAVIGATION_PATH } from "@/constants";
import { UserProfile } from "@/types/api/enums/UserProfile";
import { FaChartBar, FaRegAddressBook, FaUsers, FaFileImport } from "react-icons/fa";

// PAGES
const DASHBOARD_PAGE: SidebarItemsType = { href: NAVIGATION_PATH.DASHBOARD.OVERVIEW.ABSOLUTE, title: "Dashboard", icon: FaChartBar }
const CLIENTS_PAGE: SidebarItemsType = {
    href: NAVIGATION_PATH.CLIENTS.LISTING.ABSOLUTE,
    title: "Clientes",
    icon: FaRegAddressBook,
    children: [
        { href: NAVIGATION_PATH.CLIENTS.LISTING.ABSOLUTE, title: "Listagem", icon: FaRegAddressBook },
        { href: NAVIGATION_PATH.CLIENTS.IMPORTS.ABSOLUTE, title: "Importações", icon: FaFileImport },
    ]
}
const USERS_PAGE: SidebarItemsType = { href: NAVIGATION_PATH.USERS.LISTING.ABSOLUTE, title: "Usuários", icon: FaUsers }

export const SIDEBAR = {
    [UserProfile.Administrator]: [
        {
            title: "Geral",
            pages: [DASHBOARD_PAGE]
        },
        {
            title: "Gestão",
            pages: [CLIENTS_PAGE, USERS_PAGE]
        }
    ],
    [UserProfile.Operator]: [
        {
            title: "Geral",
            pages: [DASHBOARD_PAGE]
        },
        {
            title: "Gestão",
            pages: [CLIENTS_PAGE]
        }
    ],
}
