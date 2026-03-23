import React from "react";
import { matchPath, useLocation } from "react-router-dom";

import { SidebarItemsType } from "../../types/sidebar";
import SidebarNavListItem from "./SidebarNavListItem";
import SidebarNavList from "./SidebarNavList";
import useAppSelector from "@/hooks/useAppSelector";
import { UserProfile } from "@/types/api/enums/UserProfile";

interface ReduceChildRoutesProps {
  depth: number;
  page: SidebarItemsType;
  items: JSX.Element[];
}

const reduceChildRoutes = (props: ReduceChildRoutesProps) => {
  const { items, page, depth } = props;
  const router = useLocation();
  const currentRoute = router.pathname;
  const profile = useAppSelector(state => state.auth.user?.profile)

  let open = page.href
    ? currentRoute?.includes(page.href)
    : false;

  if (!page.permission || page.permission?.find(item => item === profile)) {

    if (page.children) {
      open = page.children.some(pg => currentRoute?.includes(pg.href));

      items.push(
        <SidebarNavListItem
          depth={depth}
          icon={page.icon}
          key={page.title}
          badge={page.badge}
          open={!!open}
          active={!!open}
          title={page.title}
          href={page.href}
        >
          <SidebarNavList depth={depth + 1} pages={page.children} />
        </SidebarNavListItem>
      );
    }
    else {
      items.push(
        <SidebarNavListItem
          depth={depth}
          href={page.href}
          icon={page.icon}
          key={page.title}
          badge={page.badge}
          title={page.title}
          active={!!open}
          open={!!open}
        />
      );
    }
  }

  return items;
};

export default reduceChildRoutes;
